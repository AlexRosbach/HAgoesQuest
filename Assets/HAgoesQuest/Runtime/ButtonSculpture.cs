using System.Collections.Generic;
using UnityEngine;

namespace HAgoesQuest
{
    // Original meshes and deterministic material maps; no third-party artwork.
    public sealed class ButtonSculpture : MonoBehaviour
    {
        WorldCard card;
        readonly List<Object> owned=new List<Object>();
        Material ceramic,metal,accent,dark,gold;
        public void Initialize(WorldCard owner,string kind)
        {
            card=owner;
            ceramic=Finish(new Color(.90f,.94f,1),false);
            metal=Finish(new Color(.58f,.72f,.87f),true);
            accent=Finish(kind=="map"?new Color(.34f,.85f,.65f):kind=="connection"?new Color(.35f,.72f,1):new Color(.70f,.57f,1),false);
            dark=Finish(new Color(.14f,.22f,.34f),false);
            gold=Finish(new Color(1,.77f,.30f),true);
            if(kind=="lamp")
            {
                Lathe(new[]{new Vector2(0,-27),new Vector2(11,-27),new Vector2(13,-22),new Vector2(13,-10),new Vector2(17,-3),new Vector2(25,7),new Vector2(27,18),new Vector2(22,29),new Vector2(12,36),new Vector2(0,38)},ceramic);
                for(int i=0;i<4;i++) Ring(new Vector3(0,-17-i*4,0),13,1.6f,metal,Quaternion.Euler(90,0,0));
                Part(PrimitiveType.Sphere,new Vector3(0,-33,0),new Vector3(16,8,16),gold);
                for(int i=0;i<3;i++) Part(PrimitiveType.Capsule,new Vector3(32+i*6,15+i*7,0),new Vector3(2,4,2),gold,Quaternion.Euler(0,0,-35));
            }
            else if(kind=="map")
            {
                Part(PrimitiveType.Cube,new Vector3(0,-17,0),new Vector3(76,5,57),metal);
                Part(PrimitiveType.Cube,new Vector3(0,-13,0),new Vector3(70,3,51),accent);
                Wall(new Vector3(-34,3,0),new Vector3(3,30,52));Wall(new Vector3(0,3,25),new Vector3(70,30,3));
                Wall(new Vector3(12,-2,9),new Vector3(3,20,31));Wall(new Vector3(-10,-2,0),new Vector3(43,20,3));
                Part(PrimitiveType.Cube,new Vector3(-13,-7,12),new Vector3(22,10,12),ceramic);
                Part(PrimitiveType.Cube,new Vector3(24,-8,11),new Vector3(12,8,17),gold);
                Part(PrimitiveType.Cylinder,new Vector3(22,-8,-15),new Vector3(12,5,12),dark);
                Part(PrimitiveType.Sphere,new Vector3(22,2,-15),new Vector3(15,18,15),accent);
                transform.localRotation=Quaternion.Euler(-32,-24,0);
            }
            else if(kind=="settings")
            {
                Ring(Vector3.zero,25,8,metal,Quaternion.identity);
                for(int i=0;i<12;i++) {float a=i*Mathf.PI/6;Part(PrimitiveType.Cube,new Vector3(Mathf.Sin(a)*32,Mathf.Cos(a)*32,0),new Vector3(10,13,13),metal,Quaternion.Euler(0,0,-i*30));}
                Ring(new Vector3(0,0,-7),15,3,accent,Quaternion.identity);
                Part(PrimitiveType.Sphere,new Vector3(0,0,-7),new Vector3(22,22,12),dark);
                Part(PrimitiveType.Sphere,new Vector3(0,0,-14),new Vector3(9,9,3),accent);
            }
            else
            {
                Part(PrimitiveType.Capsule,new Vector3(0,-3,0),new Vector3(43,23,35),ceramic,Quaternion.Euler(0,0,90));
                for(int i=0;i<7;i++)Part(PrimitiveType.Cube,new Vector3(-19+i*6,-5,-17),new Vector3(2,13,2),metal);
                for(int i=0;i<3;i++)Part(PrimitiveType.Sphere,new Vector3(-10+i*10,7,-19),new Vector3(4,4,2),i==0?accent:gold);
                for(int side=-1;side<=1;side+=2)Part(PrimitiveType.Capsule,new Vector3(side*25,27,6),new Vector3(4,14,4),dark,Quaternion.Euler(0,0,-side*12));
                Ring(new Vector3(0,32,0),10,2,accent,Quaternion.identity);
                Part(PrimitiveType.Sphere,new Vector3(0,32,0),Vector3.one*6,accent);
            }
            CombineSurfaces();
        }
        void CombineSurfaces()
        {
            var parts=GetComponentsInChildren<MeshFilter>();
            foreach(var material in new[]{ceramic,metal,accent,dark,gold})
            {
                var instances=new List<CombineInstance>();
                foreach(var part in parts)if(part.GetComponent<Renderer>().sharedMaterial==material)
                    instances.Add(new CombineInstance{mesh=part.sharedMesh,transform=transform.worldToLocalMatrix*part.transform.localToWorldMatrix});
                if(instances.Count==0)continue;
                var combined=new Mesh();combined.CombineMeshes(instances.ToArray());MeshPart(combined,material,Vector3.zero,Quaternion.identity);
            }
            foreach(var part in parts){part.gameObject.SetActive(false);Destroy(part.gameObject);}
        }
        Material Finish(Color color,bool brushed)
        {
            var tex=new Texture2D(128,128,TextureFormat.RGB24,true){wrapMode=TextureWrapMode.Repeat,filterMode=FilterMode.Bilinear};
            var pixels=new Color[128*128];
            for(int y=0;y<128;y++)for(int x=0;x<128;x++)
            {
                float grain=((x*73+y*151)%97)/96f;
                float tone=brushed?.82f+.10f*Mathf.Sin(y*2.1f)+.08f*grain:.94f+.06f*grain;
                // Fine engraved grid on ceramic; brushed bands on metal.
                if(!brushed&&(x%32==0||y%32==0))tone*=.88f;
                pixels[y*128+x]=new Color(tone,tone,tone);
            }
            tex.SetPixels(pixels);tex.Apply(true,true);owned.Add(tex);
            var material=new Material(Resources.Load<Shader>("SpatialButton")){color=color,mainTexture=tex,enableInstancing=true};
            material.SetFloat("_Metal",brushed?1:0);owned.Add(material);return material;
        }
        void Wall(Vector3 p,Vector3 s){Part(PrimitiveType.Cube,p,s,ceramic);Part(PrimitiveType.Cube,p+Vector3.up*s.y*.5f,new Vector3(s.x+1,2,s.z+1),metal);}
        void Part(PrimitiveType type,Vector3 p,Vector3 s,Material material,Quaternion? rotation=null)
        {
            var go=GameObject.CreatePrimitive(type);Destroy(go.GetComponent<Collider>());go.transform.SetParent(transform,false);
            go.transform.localPosition=p;go.transform.localScale=s;go.transform.localRotation=rotation??Quaternion.identity;go.GetComponent<Renderer>().sharedMaterial=material;
        }
        void MeshPart(Mesh mesh,Material material,Vector3 p,Quaternion rotation)
        {
            owned.Add(mesh);var go=new GameObject("Sculpted surface",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(transform,false);go.transform.localPosition=p;go.transform.localRotation=rotation;go.GetComponent<MeshFilter>().sharedMesh=mesh;go.GetComponent<MeshRenderer>().sharedMaterial=material;
        }
        void Lathe(Vector2[] profile,Material material)
        {
            const int segments=48;var vertices=new Vector3[profile.Length*(segments+1)];var uv=new Vector2[vertices.Length];var triangles=new List<int>();
            for(int y=0;y<profile.Length;y++)for(int x=0;x<=segments;x++){int i=y*(segments+1)+x;float a=x*Mathf.PI*2/segments;vertices[i]=new Vector3(Mathf.Sin(a)*profile[y].x,profile[y].y,Mathf.Cos(a)*profile[y].x);uv[i]=new Vector2(x/(float)segments,y/(float)(profile.Length-1));if(y>0&&x>0){triangles.AddRange(new[]{i,i-1,i-segments-2,i,i-segments-2,i-segments-1});}}
            var mesh=new Mesh{vertices=vertices,uv=uv,triangles=triangles.ToArray()};mesh.RecalculateNormals();MeshPart(mesh,material,Vector3.zero,Quaternion.identity);
        }
        void Ring(Vector3 position,float radius,float tube,Material material,Quaternion rotation)
        {
            const int n=48,m=12;var v=new Vector3[(n+1)*(m+1)];var uv=new Vector2[v.Length];var t=new List<int>();
            for(int a=0;a<=n;a++)for(int b=0;b<=m;b++){int i=a*(m+1)+b;float u=a*Mathf.PI*2/n,w=b*Mathf.PI*2/m;v[i]=new Vector3(Mathf.Cos(u)*(radius+tube*Mathf.Cos(w)),Mathf.Sin(u)*(radius+tube*Mathf.Cos(w)),tube*Mathf.Sin(w));uv[i]=new Vector2(a/(float)n,b/(float)m);if(a>0&&b>0)t.AddRange(new[]{i,i-m-2,i-1,i,i-m-1,i-m-2});}
            var mesh=new Mesh{vertices=v,uv=uv,triangles=t.ToArray()};mesh.RecalculateNormals();MeshPart(mesh,material,position,rotation);
        }
        void Update(){if(card)transform.localScale=Vector3.Lerp(transform.localScale,Vector3.one*(card.IsFocused?1.22f:1.15f),1-Mathf.Exp(-10*Time.unscaledDeltaTime));}
        void OnDestroy(){foreach(var item in owned)if(item)Destroy(item);}
    }
}
