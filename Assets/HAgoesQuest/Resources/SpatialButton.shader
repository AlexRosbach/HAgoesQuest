Shader "HAgoesQuest/SpatialButton" {
 Properties { _Color("Color",Color)=(.77,.72,1,1) }
 SubShader { Tags { "RenderType"="Opaque" } Pass {
 CGPROGRAM
 #pragma vertex vert
 #pragma fragment frag
 #pragma target 3.5
 #pragma multi_compile_instancing
 #pragma multi_compile _ STEREO_INSTANCING_ON STEREO_MULTIVIEW_ON
 #include "UnityCG.cginc"
 struct appdata { float4 vertex:POSITION; float3 normal:NORMAL; UNITY_VERTEX_INPUT_INSTANCE_ID };
 struct v2f { float4 position:SV_POSITION; float3 normal:TEXCOORD0; UNITY_VERTEX_OUTPUT_STEREO };
 float4 _Color;
 v2f vert(appdata v) { v2f o; UNITY_SETUP_INSTANCE_ID(v); UNITY_INITIALIZE_OUTPUT(v2f,o); UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(o); o.position=UnityObjectToClipPos(v.vertex);o.normal=UnityObjectToWorldNormal(v.normal);return o; }
 fixed4 frag(v2f i):SV_Target { UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(i); float lighting=.38+.62*saturate(dot(normalize(i.normal),normalize(float3(-.3,.6,-1))));return fixed4(_Color.rgb*lighting,1); }
 ENDCG
 } }
}
