using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace HAgoesQuest
{
    public sealed class MediaFrame : MonoBehaviour
    {
        HomeAssistantConnection connection;
        string entityId;
        RawImage picture;
        Text caption;
        Texture2D texture;
        UnityWebRequest request;
        static int requests;
        bool slot;
        public void Initialize(HomeAssistantConnection ha,string id)
        {
            connection=ha;entityId=id;
            var image=new GameObject("HA image",typeof(RectTransform),typeof(RawImage));image.transform.SetParent(transform,false);
            picture=image.GetComponent<RawImage>();picture.raycastTarget=false;
            picture.rectTransform.sizeDelta=new Vector2(560,315);picture.rectTransform.anchoredPosition=new Vector2(0,22);
            caption=GetComponent<WorldCard>().Label;caption.rectTransform.anchorMin=caption.rectTransform.anchorMax=new Vector2(.5f,0);
            caption.rectTransform.pivot=new Vector2(.5f,0);caption.rectTransform.sizeDelta=new Vector2(560,55);caption.rectTransform.anchoredPosition=new Vector2(0,8);caption.fontSize=18;
            caption.text=L10n.T("Bild wird geladen …");
        }
        void OnEnable(){StartCoroutine(Refresh());}
        IEnumerator Refresh()
        {
            while(isActiveAndEnabled)
            {
                if(!connection||picture==null||requests>=2||!Camera.main||Vector3.Distance(Camera.main.transform.position,transform.position)>8||
                    Vector3.Dot(Camera.main.transform.forward,(transform.position-Camera.main.transform.position).normalized)<.3f)
                {yield return new WaitForSecondsRealtime(.5f);continue;}
                request=connection.MediaRequest(entityId);
                if(request==null){caption.text=L10n.T("Bild nicht verfügbar");yield return new WaitForSecondsRealtime(2);continue;}
                slot=true;requests++;
                yield return request.SendWebRequest();
                if(request.result==UnityWebRequest.Result.Success&&request.downloadHandler.data.Length<8*1024*1024)
                {
                    var next=new Texture2D(2,2,TextureFormat.RGB24,false);
                    if(next.LoadImage(request.downloadHandler.data,true)&&next.width<=4096&&next.height<=4096)
                    {
                        if(texture)Destroy(texture);texture=next;picture.texture=texture;
                        float aspect=(float)texture.width/texture.height;
                        picture.rectTransform.sizeDelta=aspect>560f/315?new Vector2(560,560/aspect):new Vector2(315*aspect,315);
                        string name=connection.Entities.TryGetValue(entityId,out var entity)?entity.Name:entityId;
                        caption.text=name+" · "+System.DateTime.Now.ToString("HH:mm:ss")+(entityId.StartsWith("camera.")?L10n.T("\nLive-Bilder · bis 2/s"):"");
                    }
                    else {Destroy(next);caption.text=L10n.T("Bildformat nicht unterstützt");}
                }
                else caption.text=L10n.T("Bild veraltet / nicht verfügbar");
                Release();
                yield return new WaitForSecondsRealtime(entityId.StartsWith("camera.")?.5f:10);
            }
        }
        void Release(){if(request!=null){request.Abort();request.Dispose();request=null;}if(slot){slot=false;requests--;}}
        void OnDisable(){StopAllCoroutines();Release();if(picture)picture.texture=null;if(texture)Destroy(texture);}
        void OnDestroy(){Release();if(texture)Destroy(texture);}
    }
}
