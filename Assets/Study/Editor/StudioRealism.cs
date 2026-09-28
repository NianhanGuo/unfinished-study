using UnityEngine;
using UnityEditor;
using UnityEngine.Rendering;
using System.Linq;
using System.Collections.Generic;
using System.IO;
public static class StudioRealism {
 const string gen="Assets/Study/Generated/",art="Assets/Study/ArtV4/";static int meshId;
 static Vector3 V(float x,float y,float z)=>new Vector3(x,y,z);
 static Material Mat(string name,Color color,float gloss=.2f,float metal=0){var path=gen+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}m.color=color;m.SetFloat("_Glossiness",gloss);m.SetFloat("_Metallic",metal);return m;}
 static Transform Group(string n,Transform p,Vector3 v){var t=new GameObject(n).transform;t.SetParent(p,false);t.localPosition=v;return t;}
 static GameObject Shape(string n,PrimitiveType type,Transform p,Vector3 pos,Vector3 scale,Material m){var o=GameObject.CreatePrimitive(type);o.name=n;o.transform.SetParent(p,false);o.transform.localPosition=pos;o.transform.localScale=scale;Object.DestroyImmediate(o.GetComponent<Collider>());o.GetComponent<Renderer>().sharedMaterial=m;return o;}
 static GameObject Box(string n,Transform p,Vector3 v,Vector3 s,Material m)=>Shape(n,PrimitiveType.Cube,p,v,s,m);
 static void Rod(string n,Transform p,Vector3 a,Vector3 b,float radius,Material m){var o=Shape(n,PrimitiveType.Cylinder,p,(a+b)/2,V(radius*2,(b-a).magnitude/2,radius*2),m);o.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);}
 static void Tube(string n,Transform p,List<Vector3> path,float radius,Material m){
  var vertices=new List<Vector3>();var tris=new List<int>();int sides=8;
  for(int i=0;i<path.Count;i++){
   Vector3 tangent=(path[Mathf.Min(i+1,path.Count-1)]-path[Mathf.Max(i-1,0)]).normalized;Vector3 a=Vector3.Cross(tangent,Vector3.up).normalized;if(a.sqrMagnitude<.01f)a=Vector3.right;Vector3 b=Vector3.Cross(tangent,a);
   for(int j=0;j<sides;j++){float r=j*2*Mathf.PI/sides;vertices.Add(path[i]+radius*(a*Mathf.Cos(r)+b*Mathf.Sin(r)));if(i>0){int q=(i-1)*sides+j,w=(i-1)*sides+(j+1)%sides;tris.AddRange(new[]{q,w,q+sides,w,w+sides,q+sides});}}
  }
  var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();Save(mesh);var g=new GameObject(n,typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(p,false);g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<Renderer>().sharedMaterial=m;
 }
 static void Save(Mesh mesh){AssetDatabase.CreateAsset(mesh,gen+"RealismMesh_"+(meshId++)+".asset");}
 static void Clear(Transform t){foreach(Transform c in t.Cast<Transform>().ToArray())Object.DestroyImmediate(c.gameObject);}
 static void Model(string name,Transform parent,Material material){
  // Use the authored board-space coordinates directly. The generic OBJ importer
  // reflects X, which swaps the handedness of these asymmetrical posed meshes.
  var vertices=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var indices=new List<int>();
  foreach(string line in File.ReadLines("Assets/Study/ArtV7/"+name+".obj")){
   string[] parts=line.Split(new[]{' '},System.StringSplitOptions.RemoveEmptyEntries);if(parts.Length<2)continue;
   if(parts[0]=="v"||parts[0]=="vn"){
    var v=V(float.Parse(parts[1],System.Globalization.CultureInfo.InvariantCulture),float.Parse(parts[2],System.Globalization.CultureInfo.InvariantCulture),float.Parse(parts[3],System.Globalization.CultureInfo.InvariantCulture));if(parts[0]=="v")vertices.Add(v);else normals.Add(v);
   }else if(parts[0]=="vt")uv.Add(new Vector2(float.Parse(parts[1],System.Globalization.CultureInfo.InvariantCulture),float.Parse(parts[2],System.Globalization.CultureInfo.InvariantCulture)));
   else if(parts[0]=="f")for(int i=1;i<4;i++)indices.Add(int.Parse(parts[i].Split('/')[0])-1);
  }
  var mesh=new Mesh();mesh.name=name+" / board space";mesh.indexFormat=IndexFormat.UInt32;mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.SetTriangles(indices,0);mesh.RecalculateTangents();Save(mesh);
  var o=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));o.transform.SetParent(parent,false);o.GetComponent<MeshFilter>().sharedMesh=mesh;o.GetComponent<Renderer>().sharedMaterial=material;
 }
 static void Physical(string name,float scale,float gloss,Color tint,float wear=.25f){var m=AssetDatabase.LoadAssetAtPath<Material>(gen+name+".mat");m.shader=Shader.Find("Study/PhysicalSurface");m.SetFloat("_Scale",scale);m.SetFloat("_Glossiness",gloss);m.SetFloat("_Wear",wear);m.SetFloat("_BumpScale",.35f);m.color=tint;}
 public static void Apply(StudyGame g){
  meshId=0;
  foreach(string name in new[]{"WritingHand","HoldingHand"}){var imp=(ModelImporter)AssetImporter.GetAtPath(art+name+".obj");imp.globalScale=1;imp.useFileScale=false;imp.importNormals=ModelImporterNormals.Calculate;imp.normalSmoothingAngle=80;imp.SaveAndReimport();}
  Physical("Floorboards",.78f,.39f,new Color(.78f,.76f,.69f),.34f);
  Physical("Dark oak",1.2f,.29f,new Color(.64f,.58f,.46f),.4f);
  Physical("Stained plaster",.8f,.08f,new Color(.5f,.52f,.46f),.45f);
  var linen=AssetDatabase.LoadAssetAtPath<Material>(gen+"Unbleached linen.mat");linen.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(gen+"Unbleached linen grain.png");Physical("Unbleached linen",5.2f,.055f,new Color(.64f,.62f,.54f),.2f);
  var bone=AssetDatabase.LoadAssetAtPath<Material>(gen+"Aged ivory.mat");bone.color=new Color(.72f,.70f,.61f);bone.SetFloat("_Glossiness",.23f);
  Hands(g);
  var architecture=GameObject.Find("STUDIO / architecture").transform;
  DetailRoom(architecture);
  // Small bevels catch light instead of infinitely sharp primitive edges.
  foreach(var mf in architecture.GetComponentsInChildren<MeshFilter>())if(mf.sharedMesh&&mf.sharedMesh.name=="Cube"&&mf.transform.childCount==0)Bevel(mf);
  foreach(var l in Object.FindObjectsByType<Light>(FindObjectsSortMode.None)){
   if(l.name=="Cold window spill"){l.intensity=1.5f;l.range=19;l.spotAngle=72;l.color=new Color(.38f,.48f,.57f);l.shadowBias=.025f;l.shadowNormalBias=.12f;}
   if(l.name=="Hands / soft bounce"){l.transform.localPosition=V(-.2f,.22f,.42f);l.intensity=1.65f;l.color=new Color(1,.91f,.8f);}
   if(l.name=="Warm workbench pool")l.intensity=1.7f;
  }
  g.headlamp.color=new Color(1,.93f,.79f);g.headlamp.shadowBias=.01f;g.headlamp.shadowNormalBias=.025f;g.headlamp.shadowResolution=LightShadowResolution.VeryHigh;g.headlamp.cookie=Cookie();
  RenderSettings.ambientMode=AmbientMode.Trilight;RenderSettings.ambientSkyColor=new Color(.105f,.13f,.16f);RenderSettings.ambientEquatorColor=new Color(.037f,.046f,.051f);RenderSettings.ambientGroundColor=new Color(.024f,.019f,.015f);
  RenderSettings.fogDensity=.023f;RenderSettings.fogColor=new Color(.017f,.024f,.029f);RenderSettings.reflectionIntensity=.38f;
  var probe=Group("Studio / local reflection",architecture,V(0,2,0)).gameObject.AddComponent<ReflectionProbe>();probe.mode=ReflectionProbeMode.Realtime;probe.refreshMode=ReflectionProbeRefreshMode.OnAwake;probe.timeSlicingMode=ReflectionProbeTimeSlicingMode.AllFacesAtOnce;probe.resolution=128;probe.size=V(15,6,19);probe.boxProjection=true;probe.cullingMask=~(1<<9);probe.clearFlags=ReflectionProbeClearFlags.SolidColor;probe.backgroundColor=new Color(.016f,.021f,.028f);
  g.eye.depthTextureMode=DepthTextureMode.Depth|DepthTextureMode.DepthNormals;g.eye.allowHDR=true;QualitySettings.shadowDistance=28;QualitySettings.pixelLightCount=8;
  AssetDatabase.SaveAssets();
 }
 static Texture2D Cookie(){
  var t=new Texture2D(256,256,TextureFormat.RGBA32,false);var c=new Color[256*256];for(int y=0;y<256;y++)for(int x=0;x<256;x++){
   float dx=(x-127.5f)/127.5f,dy=(y-127.5f)/127.5f,r=Mathf.Sqrt(dx*dx+dy*dy);float edge=1-Mathf.SmoothStep(.68f,1,r);float falloff=Mathf.Lerp(.42f,1,1-Mathf.SmoothStep(0,.62f,r));float value=edge*falloff*(.98f+.02f*Mathf.Sin(r*80));c[y*256+x]=new Color(value,value,value,value);
  }t.SetPixels(c);t.Apply();string path=art+"HeadlampCookie.png";File.WriteAllBytes(path,t.EncodeToPNG());AssetDatabase.ImportAsset(path);var ti=(TextureImporter)AssetImporter.GetAtPath(path);ti.textureType=TextureImporterType.Cookie;ti.wrapMode=TextureWrapMode.Clamp;ti.SaveAndReimport();Object.DestroyImmediate(t);return AssetDatabase.LoadAssetAtPath<Texture2D>(path);
 }
 public static void RefreshHands(){meshId=5000;Hands(Object.FindFirstObjectByType<StudyGame>());UnityEditor.SceneManagement.EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene());AssetDatabase.SaveAssets();}
 static void Hands(StudyGame g){
  Transform left=g.board.Cast<Transform>().FirstOrDefault(t=>t.name=="Left hand / grip"),right=g.artwork.pencilRig;
  if(left)Object.DestroyImmediate(left.gameObject);
  Clear(right);
  var graphite=Mat("Compressed charcoal",new Color(.043f,.039f,.031f),.18f);
  Rod("Charcoal / finger grip",right,V(.002f,.004f,-.002f),V(.078f,.104f,-.093f),.0035f,graphite);
  foreach(var tr in g.board.GetComponentsInChildren<Transform>())tr.gameObject.layer=9;
  foreach(var r in g.board.GetComponentsInChildren<Renderer>()){r.shadowCastingMode=ShadowCastingMode.On;r.receiveShadows=true;}
 }
 static void Bevel(MeshFilter mf){
  Vector3 size=mf.transform.localScale;float radius=Mathf.Min(.012f,Mathf.Min(size.x,Mathf.Min(size.y,size.z))*.16f);if(radius<.001f)return;
  var vertices=new List<Vector3>();var normals=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();int steps=6;
  Vector3[] directions={Vector3.right,Vector3.left,Vector3.up,Vector3.down,Vector3.forward,Vector3.back};
  foreach(var n in directions){Vector3 a=Vector3.Cross(n,Mathf.Abs(n.y)>.5f?Vector3.forward:Vector3.up),b=Vector3.Cross(n,a);int offset=vertices.Count;
   for(int y=0;y<=steps;y++)for(int x=0;x<=steps;x++){
    float u=x/(float)steps,v=y/(float)steps;Vector3 raw=Vector3.Scale(n*.5f+a*(u-.5f)+b*(v-.5f),size);Vector3 inner=V(Mathf.Clamp(raw.x,-size.x/2+radius,size.x/2-radius),Mathf.Clamp(raw.y,-size.y/2+radius,size.y/2-radius),Mathf.Clamp(raw.z,-size.z/2+radius,size.z/2-radius));Vector3 nn=(raw-inner).normalized;vertices.Add(inner+nn*radius);normals.Add(nn);uv.Add(new Vector2(u,v));
    if(x<steps&&y<steps){int q=offset+y*(steps+1)+x;tris.AddRange(new[]{q,q+1,q+steps+1,q+1,q+steps+2,q+steps+1});}
   }
  }
  var mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetNormals(normals);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);mesh.RecalculateTangents();Save(mesh);mf.sharedMesh=mesh;
  var collider=mf.GetComponent<BoxCollider>();if(collider)collider.size=Vector3.Scale(collider.size,size);mf.transform.localScale=Vector3.one;
 }
 static void DetailRoom(Transform p){
  var iron=Mat("Radiator enamel",new Color(.23f,.25f,.22f),.31f,.22f);var dark=Mat("Lamp black V4",new Color(.027f,.031f,.03f),.32f);var paper=Mat("Loose drawing paper V4",new Color(.63f,.61f,.53f),.05f);
  var oak=AssetDatabase.LoadAssetAtPath<Material>(gen+"Dark oak.mat");
  for(int x=-5;x<=5;x+=5){
   for(int i=0;i<14;i++)Shape("Cast radiator fin",PrimitiveType.Capsule,p,V(x-.78f+i*.12f,.55f,9.02f),V(.075f,.65f,.25f),iron);
   Rod("Radiator feed",p,V(x-.91f,.25f,9.04f),V(x+.91f,.25f,9.04f),.027f,iron);Rod("Radiator return",p,V(x-.91f,.86f,9.04f),V(x+.91f,.86f,9.04f),.027f,iron);
  }
  // Replace anonymous yellow floor rectangles with recognisable abandoned studies.
  int k=0;foreach(var t in p.GetComponentsInChildren<Transform>().Where(t=>t.name=="Discarded study").ToArray()){
   var m=Mat("Discarded charcoal "+k,new Color(.67f,.64f,.55f),.06f);m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Study/ArtV3/charcoal"+(k%3)+".png");t.GetComponent<Renderer>().sharedMaterial=m;k++;
  }
  foreach(var easel in p.GetComponentsInChildren<Transform>().Where(t=>t.name=="Working easel").ToArray()){
   foreach(var t in easel.GetComponentsInChildren<Transform>().Where(t=>t.name=="Charcoal").ToArray())Object.DestroyImmediate(t.gameObject);
   var face=easel.Find("Unfinished canvas");if(face){var m=Mat("Easel charcoal "+k,new Color(.61f,.58f,.50f));m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Study/ArtV3/charcoal"+(k++%3)+".png");face.GetComponent<Renderer>().sharedMaterial=m;}
  }
  // Clutter stays on furniture and at the perimeter, keeping the pursuit route clear.
  var bench=p.Find("Paint stained workbench");
  if(bench){
   for(int i=0;i<5;i++){var sheet=Box("Overdue drawing sheets",bench,V(.1f,1.116f+i*.001f,-.98f),V(.39f,.001f,.52f),paper);sheet.transform.localRotation=Quaternion.Euler(0,i*3-5,0);}
   Shape("Cold coffee cup",PrimitiveType.Cylinder,bench,V(.08f,1.19f,-.29f),V(.15f,.095f,.15f),paper);Shape("Cold coffee surface",PrimitiveType.Cylinder,bench,V(.08f,1.287f,-.29f),V(.127f,.001f,.127f),dark);
   for(int i=0;i<7;i++)Rod("Abandoned charcoal",bench,V(.17f-i*.025f,1.112f,.63f),V(.09f-i*.025f,1.112f,.84f),.004f,dark);
  }
  // Practical strips glow very faintly: overhead fixtures stay off until success.
  var glow=Mat("Exit sign glow",new Color(.13f,.20f,.14f));glow.EnableKeyword("_EMISSION");glow.SetColor("_EmissionColor",new Color(.025f,.065f,.033f));Box("Exit lamp",p,V(0,3.18f,-9.15f),V(.6f,.17f,.05f),glow);
 }
}
