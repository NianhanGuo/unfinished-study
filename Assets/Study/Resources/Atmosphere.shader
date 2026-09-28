Shader "Study/Atmosphere" {
Properties { _MainTex("Image",2D)="white"{} }
SubShader { Cull Off ZWrite Off ZTest Always Pass {
CGPROGRAM
#pragma vertex vert_img
#pragma fragment frag
#pragma target 3.0
#include "UnityCG.cginc"
sampler2D _MainTex,_CameraDepthTexture,_CameraDepthNormalsTexture;float4 _MainTex_TexelSize;float _TimeValue,_Title;float4x4 _InvProjection;
float3 position(float2 uv,float depth){float4 ray=mul(_InvProjection,float4(uv*2-1,1,1));return ray.xyz*(depth/max(.001,-ray.z));}
float random(float2 p){return frac(sin(dot(p,float2(12.9898,78.233)))*43758.5453);}
float4 frag(v2f_img i):SV_Target {
 float2 uv=i.uv;float3 c=tex2D(_MainTex,uv).rgb;
 float depth=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,uv));float dn;float3 normal;DecodeDepthNormal(tex2D(_CameraDepthNormalsTexture,uv),dn,normal);
 float3 pos=position(uv,depth);float ao=0;float angle=random(floor(uv/_MainTex_TexelSize.xy/3))*6.283;
 for(int j=0;j<12;j++){
  float a=angle+j*2.399;float r=lerp(3,28,(j+1)/12.0)*saturate(4/max(depth,.1));float2 q=uv+float2(cos(a),sin(a))*r*_MainTex_TexelSize.xy;
  float d=LinearEyeDepth(SAMPLE_DEPTH_TEXTURE(_CameraDepthTexture,q));float3 v=position(q,d)-pos;float len=length(v);
  ao+=max(0,dot(normal,v/max(len,.0001))-.16)*saturate(1-len/.42)*step(.008,len);
 }
 c*=1-saturate(ao/12*1.6)*.72*smoothstep(1.1,1.7,depth);
 // Restrained warm highlights, cool shadows and a soft photographic shoulder.
 float l=dot(c,float3(.2126,.7152,.0722));c*=lerp(float3(.90,.98,1.025),float3(1.035,1.0,.945),saturate(l*2));
 c=(c*(1.45*c+.13))/(c*(1.45*c+.52)+.14);c=pow(max(c,0),.96);
 float2 delta=uv-.5;float vignette=smoothstep(.82,.24,length(delta*float2(1,.86)));c*=lerp(.48,1,vignette);
 float grain=random(uv*float2(1440,900)+floor(_TimeValue*24))-.5;c+=grain*(.0022+.0018*_Title);
 return float4(c,1);
}
ENDCG
} } }
