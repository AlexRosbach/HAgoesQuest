using UnityEngine;

namespace HAgoesQuest
{
    // A translucent glow around the tracked real hand, never a replacement hand mesh.
    public sealed class HandReadinessGlow : MonoBehaviour
    {
        SpriteRenderer glow;
        Texture2D texture;
        Sprite sprite;
        public void Initialize()
        {
            texture=new Texture2D(96,96,TextureFormat.RGBA32,false){wrapMode=TextureWrapMode.Clamp};
            var pixels=new Color[96*96];
            for(int y=0;y<96;y++)for(int x=0;x<96;x++)
            {
                float r=new Vector2((x-47.5f)/47.5f,(y-47.5f)/47.5f).magnitude;
                float alpha=Mathf.Exp(-r*r*4)*.25f+Mathf.Exp(-Mathf.Pow((r-.62f)*10,2))*.35f;
                pixels[y*96+x]=new Color(.25f,1,.48f,alpha*Mathf.Clamp01((1-r)*8));
            }
            texture.SetPixels(pixels);texture.Apply(false,true);
            sprite=Sprite.Create(texture,new Rect(0,0,96,96),Vector2.one*.5f,640);
            glow=gameObject.AddComponent<SpriteRenderer>();glow.sprite=sprite;
            gameObject.SetActive(false);
        }
        public void Show(Transform wrist,Transform index,Transform little,Transform head,bool visible,bool closed)
        {
            visible=visible&&wrist&&index&&little&&head;
            gameObject.SetActive(visible);if(!visible)return;
            Vector3 center=(wrist.position+index.position+little.position)/3;
            transform.position=center+(head.position-center).normalized*.014f;
            transform.rotation=Quaternion.LookRotation(center-head.position,head.up);
            transform.localScale=Vector3.one*(closed?.85f:1);
            glow.color=new Color(1,1,1,(closed?.9f:.55f)+Mathf.Sin(Time.unscaledTime*4)*.08f);
        }
        public void Hide(){gameObject.SetActive(false);}
        void OnDestroy(){if(sprite)Destroy(sprite);if(texture)Destroy(texture);}
    }
}
