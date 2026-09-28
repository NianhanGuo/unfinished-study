Shader "Study/PhysicalSurface" {
 Properties { _MainTex("Albedo",2D)="white"{} _BumpMap("Normal",2D)="bump"{} _Color("Tint",Color)=(1,1,1,1) _Scale("Tiles per metre",Float)=1 _Glossiness("Smoothness",Range(0,1))=.3 _Metallic("Metallic",Range(0,1))=0 _BumpScale("Normal strength",Range(0,2))=.35 _Wear("Wear",Range(0,1))=.2 }
 SubShader { Tags {"RenderType"="Opaque"} LOD 300
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows addshadow
 #pragma target 3.0
 sampler2D _MainTex,_BumpMap;fixed4 _Color;half _Glossiness,_Metallic,_BumpScale,_Wear;float _Scale;
 struct Input {float3 worldPos;float3 worldNormal;INTERNAL_DATA};
 float noise(float3 p){return frac(sin(dot(p,float3(12.9898,78.233,37.719)))*43758.5453);}
 void surf(Input IN,inout SurfaceOutputStandard o){
  float3 p=IN.worldPos*_Scale;float3 nw=normalize(WorldNormalVector(IN,float3(0,0,1)));float3 w=pow(abs(nw),4);w/=max(dot(w,float3(1,1,1)),.001);
  float3 tex=tex2D(_MainTex,p.zy).rgb*w.x+tex2D(_MainTex,p.xz).rgb*w.y+tex2D(_MainTex,p.xy).rgb*w.z;
  float wear=sin(p.x*3.1+sin(p.z*4))*sin(p.y*2.3+p.z*.7)*.5+.5;
  o.Albedo=tex*_Color.rgb*(1-_Wear*.24*wear);
  float3 nx=UnpackNormal(tex2D(_BumpMap,p.zy));float3 ny=UnpackNormal(tex2D(_BumpMap,p.xz));float3 nz=UnpackNormal(tex2D(_BumpMap,p.xy));
  float3 perturb=float3(ny.x*w.y+nz.x*w.z,nx.y*w.x+nz.y*w.z,nx.x*w.x+ny.y*w.y);
  float3 n=normalize(nw+perturb*_BumpScale);float3 tx=WorldNormalVector(IN,float3(1,0,0)),ty=WorldNormalVector(IN,float3(0,1,0)),tz=WorldNormalVector(IN,float3(0,0,1));
  o.Normal=normalize(float3(dot(n,tx),dot(n,ty),dot(n,tz)));
  o.Metallic=_Metallic;o.Smoothness=saturate(_Glossiness*(.7+.3*tex.r)-wear*_Wear*.06);o.Occlusion=1-_Wear*.1*wear;
 }
 ENDCG
 } Fallback "Standard"
}
