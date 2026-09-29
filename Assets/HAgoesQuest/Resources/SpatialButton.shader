Shader "HAgoesQuest/SpatialButton" {
 Properties { _Color("Color",Color)=(1,1,1,1) _MainTex("Material texture",2D)="white" {} _Metal("Metal",Range(0,1))=0 }
 SubShader { Tags { "RenderType"="Opaque" } Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.5
 #pragma multi_compile_instancing
 #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
 #include "UnityCG.cginc"
 struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; float2 uv:TEXCOORD0; UNITY_VERTEX_INPUT_INSTANCE_ID };
 struct v2f { float4 position:SV_POSITION; float3 normal:TEXCOORD0; float2 uv:TEXCOORD1; UNITY_VERTEX_OUTPUT_STEREO };
 float4 _Color; sampler2D _MainTex; float _Metal;
 v2f vert(appdata v) { v2f o; UNITY_SETUP_INSTANCE_ID(v); UNITY_INITIALIZE_OUTPUT(v2f,o); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); o.position=UnityObjectToClipPos(v.vertex);o.normal=mul((float3x3)UNITY_MATRIX_V,UnityObjectToWorldNormal(v.normal));o.uv=v.uv;return o; }
 fixed4 frag(v2f i):SV_Target { UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i);
   float3 n=normalize(i.normal); float3 light=normalize(float3(-.4,.65,1));
   float diffuse=.72+.28*saturate(dot(n,light));
   float gloss=pow(saturate(dot(n,normalize(light+float3(0,0,1)))),lerp(18,64,_Metal));
   float rim=pow(1-abs(n.z),3)*.12;
   return fixed4(_Color.rgb*tex2D(_MainTex,i.uv).rgb*diffuse+gloss*lerp(.12,.38,_Metal)+rim,1);
 }
 ENDCG
 } }
}
