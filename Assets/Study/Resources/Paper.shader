Shader "Study/Paper" {
Properties{_MainTex("Paper",2D)="white"{} }
SubShader{Tags{"RenderType"="Opaque"} Pass{CGPROGRAM
#pragma vertex vert
#pragma fragment frag
#include "UnityCG.cginc"
sampler2D _MainTex;
struct appdata{float4 vertex:POSITION;float2 uv:TEXCOORD0;};struct v2f{float4 vertex:SV_POSITION;float2 uv:TEXCOORD0;};
v2f vert(appdata v){v2f o;o.vertex=UnityObjectToClipPos(v.vertex);o.uv=v.uv;return o;}
fixed4 frag(v2f i):SV_Target{float3 col=tex2D(_MainTex,i.uv).rgb;col*=.85-.16*length(i.uv-.5);return float4(col,1);}
ENDCG}}
}
