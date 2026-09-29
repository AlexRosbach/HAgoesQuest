using UnityEngine;
namespace HAgoesQuest
{
    // Spatial contours leave the tracked real hand unobscured.
    public sealed class HandReadinessGlow : MonoBehaviour
    {
        readonly LineRenderer[] arcs=new LineRenderer[6];
        Material material;
        public void Initialize()
        {
            material=new Material(Shader.Find("Sprites/Default"));
            for(int i=0;i<arcs.Length;i++)
            {
                var line=new GameObject(i%2==0?"Emerald contour":"Soft light falloff").AddComponent<LineRenderer>();
                line.transform.SetParent(transform,false);line.useWorldSpace=false;line.sharedMaterial=material;
                line.positionCount=33;line.numCapVertices=6;line.numCornerVertices=4;
                line.startWidth=line.endWidth=i%2==0?.0009f:.005f;
                line.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;line.receiveShadows=false;arcs[i]=line;
                float alpha=i%2==0?.7f:.075f;var color=new Color(.25f,.95f,.58f);
                var gradient=new Gradient();gradient.SetKeys(new[]{new GradientColorKey(color,0),new GradientColorKey(new Color(.7f,1,.82f),.5f),new GradientColorKey(color,1)},new[]{new GradientAlphaKey(0,0),new GradientAlphaKey(alpha,.18f),new GradientAlphaKey(alpha,.82f),new GradientAlphaKey(0,1)});line.colorGradient=gradient;
            }
            gameObject.SetActive(false);
        }
        public void Show(Transform wrist,Transform index,Transform little,Transform head,bool visible,bool closed)
        {
            visible=visible&&wrist&&index&&little&&head;
            gameObject.SetActive(visible);if(!visible)return;
            Vector3 knuckles=(index.position+little.position)*.5f;
            Vector3 across=(index.position-little.position).normalized;
            Vector3 along=(knuckles-wrist.position).normalized;
            Vector3 normal=Vector3.Cross(across,along).normalized;
            if(normal.sqrMagnitude<.1f)normal=head.forward;
            if(Vector3.Dot(normal,head.position-knuckles)<0)normal=-normal;
            transform.SetPositionAndRotation(Vector3.Lerp(wrist.position,knuckles,.65f)+normal*.012f,Quaternion.LookRotation(normal,along.sqrMagnitude>.1f?along:head.up));
            float width=Mathf.Clamp(Vector3.Distance(index.position,little.position)*.7f,.03f,.06f);
            float pulse=.5f+.5f*Mathf.Sin(Time.unscaledTime*2.5f);
            for(int j=0;j<3;j++)for(int layer=0;layer<2;layer++)
            {
                var line=arcs[j*2+layer];
                line.startWidth=line.endWidth=(layer==0?.0009f:.005f)*(closed?1.3f:1)*(1+pulse*.1f);
                for(int i=0;i<33;i++)
                {
                    float a=(j*120+15+i/32f*85+Mathf.Sin(Time.unscaledTime*.7f)*3)*Mathf.Deg2Rad;
                    line.SetPosition(i,new Vector3(Mathf.Cos(a)*width,Mathf.Sin(a)*width*1.18f,Mathf.Sin(a*2)*.003f));
                }
            }
        }
        public void Hide(){gameObject.SetActive(false);}
        void OnDestroy(){if(material)Destroy(material);}
    }
}
