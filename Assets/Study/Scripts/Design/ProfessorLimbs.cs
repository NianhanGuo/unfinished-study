using UnityEngine;
// Shared tapered cloth sections give the articulated silhouette wrists and elbows.
public class ProfessorLimbs:MonoBehaviour {
 Mesh upper,lower;
 Mesh Build(bool distal){
  const int rings=14,sides=16;var v=new Vector3[(rings+1)*(sides+1)];var uv=new Vector2[v.Length];var tri=new int[rings*sides*6];int k=0;
  for(int r=0;r<=rings;r++)for(int s=0;s<=sides;s++){
   float t=r/(float)rings,a=s*Mathf.PI*2/sides;
   float radius=distal?Mathf.Lerp(.22f,.53f,t):Mathf.Lerp(.39f,.60f,t);
   radius+=.035f*Mathf.Sin(t*7*Mathf.PI)*Mathf.Pow(t,4);
   int id=r*(sides+1)+s;v[id]=new Vector3(Mathf.Cos(a)*radius,t*2-1,Mathf.Sin(a)*radius);uv[id]=new Vector2(s/(float)sides,t*3);
   if(r<rings&&s<sides){int b=id+sides+1;tri[k++]=id;tri[k++]=b;tri[k++]=id+1;tri[k++]=id+1;tri[k++]=b;tri[k++]=b+1;}
  }
  var m=new Mesh{name=distal?"Tapered distal sleeve":"Gathered upper sleeve"};m.vertices=v;m.uv=uv;m.triangles=tri;m.RecalculateNormals();m.RecalculateTangents();m.RecalculateBounds();return m;
 }
 public Transform Segment(Transform root,int i,Material cloth){
  if(!upper){upper=Build(false);lower=Build(true);}
  var go=new GameObject("Tailored articulated limb "+i);go.transform.SetParent(root,false);go.AddComponent<MeshFilter>().sharedMesh=i%2==0?upper:lower;go.AddComponent<MeshRenderer>().sharedMaterial=cloth;return go.transform;
 }
 void OnDestroy(){if(upper)Destroy(upper);if(lower)Destroy(lower);}
}
