Shader "Study/StudentSkin" {
 Properties {_Color("Skin",Color)=(.62,.4,.29,1) _Glossiness("Smoothness",Range(0,1))=.3 _Dirt("Charcoal staining",Range(0,1))=1}
 SubShader {Tags {"RenderType"="Opaque"} LOD 300
 CGPROGRAM
 #pragma surface surf Standard fullforwardshadows vertex:vert addshadow
 #pragma target 3.0
 fixed4 _Color;half _Glossiness,_Dirt;
 struct Input {float3 localPos;};
 void vert(inout appdata_full v,out Input o){UNITY_INITIALIZE_OUTPUT(Input,o);o.localPos=v.vertex.xyz;}
 float hash(float3 p){return frac(sin(dot(p,float3(12.91,78.13,41.71)))*43758.5);}
 void surf(Input IN,inout SurfaceOutputStandard o){
  float3 p=IN.localPos;float pore=hash(floor(p*2700));float mottling=sin(p.x*161+sin(p.z*109))*sin(p.z*147+p.y*89)*.5+.5;
  float crease=pow(abs(sin(p.z*580+p.x*63)),26)*.014;
  float dirt=smoothstep(.026,.015,p.x)*smoothstep(-.065,-.017,p.z)*.45*_Dirt;
  o.Albedo=_Color.rgb*(.95+mottling*.04+pore*.012-crease);o.Albedo=lerp(o.Albedo,float3(.095,.088,.075),dirt);
  o.Albedo+=float3(.025,-.004,-.008)*mottling;o.Metallic=0;o.Smoothness=_Glossiness+(.5-pore)*.05;
  o.Occlusion=1-crease*.8;o.Emission=_Color.rgb*float3(.012,.006,.003);
 }
 ENDCG
 } Fallback "Standard"
}
