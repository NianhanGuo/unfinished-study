using UnityEngine;
// One continuous, sculpted head surface avoids a stack of floating cartoon primitives.
public static class DesignFace {
 static float Bell(float x,float center,float width){float a=(x-center)/width;return Mathf.Exp(-a*a);}
 public static void Build(Transform head,Material skin){
  const int rings=44,sectors=56;var vertices=new Vector3[(rings+1)*(sectors+1)];var uv=new Vector2[vertices.Length];var triangles=new int[rings*sectors*6];int n=0;
  for(int r=0;r<=rings;r++)for(int s=0;s<=sectors;s++){
   float t=r*Mathf.PI/rings,p=s*Mathf.PI*2/sectors,cy=Mathf.Cos(t),rad=Mathf.Sin(t),y=cy*(cy<0?.52f:.62f);
   float jaw=Mathf.Lerp(.9f,1,Mathf.SmoothStep(0,1,(cy+.9f)/1.1f));float x=Mathf.Cos(p)*rad*.295f*jaw,z=Mathf.Sin(p)*rad*.285f;
   float front=Mathf.Clamp01(Mathf.Sin(p)*5);
   float sockets=(Bell(x,-.125f,.065f)+Bell(x,.125f,.065f))*Bell(y,.035f,.07f);
   float bridge=Bell(x,0,.048f)*Bell(y,-.02f,.19f)*.095f;
   float tip=Bell(x,0,.067f)*Bell(y,-.12f,.06f)*.055f;
   float brow=(Bell(x,-.11f,.09f)+Bell(x,.11f,.09f))*Bell(y,.135f,.044f)*.035f;
   float cheeks=(Bell(x,-.18f,.05f)+Bell(x,.18f,.05f))*Bell(y,-.13f,.10f)*.027f;
   z+=front*(bridge+tip+brow+cheeks-sockets*.04f-Bell(x,0,.1f)*Bell(y,-.35f,.018f)*.02f);
   int i=r*(sectors+1)+s;vertices[i]=new Vector3(x,y,z);uv[i]=new Vector2(s/(float)sectors,r/(float)rings);
   if(r<rings&&s<sectors){int a=i,b=i+sectors+1;triangles[n++]=a;triangles[n++]=a+1;triangles[n++]=b;triangles[n++]=a+1;triangles[n++]=b+1;triangles[n++]=b;}
  }
  var mesh=new Mesh{name="Professor / continuous facial planes"};mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;mesh.RecalculateNormals();mesh.RecalculateBounds();
  var o=new GameObject("Sculpted bald head");o.transform.SetParent(head,false);o.AddComponent<MeshFilter>().sharedMesh=mesh;
  var material=new Material(skin);material.name="Uneven aged skin";material.color=new Color(.27f,.255f,.23f);material.SetFloat("_Glossiness",.19f);
  var texture=new Texture2D(128,128,TextureFormat.RGB24,false);var rng=new System.Random(734);
  for(int y=0;y<128;y++)for(int x=0;x<128;x++){float noise=(float)rng.NextDouble()*.09f;float mottling=Mathf.PerlinNoise(x*.11f,y*.09f)*.13f;float value=.79f+noise+mottling;texture.SetPixel(x,y,new Color(value,value*.985f,value*.95f));}texture.Apply();material.mainTexture=texture;material.mainTextureScale=Vector2.one;o.AddComponent<MeshRenderer>().sharedMaterial=material;
 }
}
