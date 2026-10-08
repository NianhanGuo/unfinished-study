using UnityEngine;
// One continuous deformed surface. No overlapping cylinders at the table-edge bend.
public class ProfessorNeck:MonoBehaviour {
 const int Rings=48,Sides=20;Mesh mesh;Vector3[] vertices;Vector2[] uv;
 public void Initialize(Material material){
  mesh=new Mesh{name="Professor / continuous extensible neck"};mesh.MarkDynamic();vertices=new Vector3[(Rings+1)*(Sides+1)];uv=new Vector2[vertices.Length];int[] triangles=new int[Rings*Sides*6];int k=0;
  for(int r=0;r<=Rings;r++)for(int s=0;s<=Sides;s++){uv[r*(Sides+1)+s]=new Vector2(s/(float)Sides,r/(float)Rings);if(r<Rings&&s<Sides){int a=r*(Sides+1)+s,b=a+Sides+1;triangles[k++]=a;triangles[k++]=b;triangles[k++]=a+1;triangles[k++]=a+1;triangles[k++]=b;triangles[k++]=b+1;}}
  mesh.vertices=vertices;mesh.uv=uv;mesh.triangles=triangles;gameObject.AddComponent<MeshFilter>().sharedMesh=mesh;gameObject.AddComponent<MeshRenderer>().sharedMaterial=material;
 }
 static Vector3 Curve(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float t){float u=1-t;return u*u*u*a+3*u*u*t*b+3*u*t*t*c+t*t*t*d;}
 static Vector3 Sample(Vector3 a,Vector3 b,Vector3 c,Vector3 d,float t,bool aroundEdge){
  if(!aroundEdge)return Curve(a,b,c,d,t);
  float q=t*3;int i=Mathf.Min(2,Mathf.FloorToInt(q));float u=q-i;
  Vector3 p0=i==0?a:i==1?a:b,p1=i==0?a:i==1?b:c,p2=i==0?b:i==1?c:d,p3=i==0?c:d;
  return .5f*((2*p1)+(-p0+p2)*u+(2*p0-5*p1+4*p2-p3)*u*u+(-p0+3*p1-3*p2+p3)*u*u*u);
 }
 public void Pose(Vector3 start,Vector3 bend,Vector3 reach,Transform head,bool underTable){
  Vector3 end=head.position-head.forward*.11f-head.up*.24f;
  Vector3 b=underTable?bend:Vector3.Lerp(start,end,.30f)-head.forward*.23f;
  Vector3 c=underTable?reach:Vector3.Lerp(start,end,.70f)-head.forward*.19f;
  float length=Vector3.Distance(start,b)+Vector3.Distance(b,c)+Vector3.Distance(c,end);
  for(int r=0;r<=Rings;r++){
   float t=r/(float)Rings;Vector3 p=Sample(start,b,c,end,t,underTable),tangent=(Sample(start,b,c,end,Mathf.Min(1,t+.005f),underTable)-Sample(start,b,c,end,Mathf.Max(0,t-.005f),underTable)).normalized;
   Vector3 axis=Vector3.Cross(tangent,head.right).normalized;if(axis.sqrMagnitude<.01f)axis=Vector3.Cross(tangent,Vector3.forward).normalized;Vector3 side=Vector3.Cross(axis,tangent).normalized;
   float radius=Mathf.Lerp(.13f,.105f,t)+.011f*Mathf.Sin(t*Mathf.PI);
   float folds=.004f*Mathf.Clamp01(1.8f/Mathf.Max(.1f,length))*Mathf.Sin(t*28*Mathf.PI)*Mathf.Sin(t*Mathf.PI);
   for(int s=0;s<=Sides;s++){float a=s*Mathf.PI*2/Sides;Vector3 normal=axis*Mathf.Cos(a)+side*Mathf.Sin(a);float tendon=.008f*Mathf.Pow(Mathf.Abs(Mathf.Sin(a)),10)*Mathf.Sin(t*Mathf.PI);vertices[r*(Sides+1)+s]=transform.InverseTransformPoint(p+normal*(radius+folds+tendon));}
  }
  mesh.vertices=vertices;mesh.RecalculateNormals();mesh.RecalculateTangents();mesh.RecalculateBounds();
 }
 void OnDestroy(){if(mesh)Destroy(mesh);}
}
