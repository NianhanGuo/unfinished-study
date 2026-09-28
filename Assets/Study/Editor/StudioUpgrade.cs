using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using System.Linq;
using System.IO;
public static class StudioUpgrade {
 const string art="Assets/Study/ArtV2/", gen="Assets/Study/Generated/";
 static Material ivory,oak,steel,linen,skin,coat;
 static Vector3 V(float x,float y,float z)=>new Vector3(x,y,z);
 static Transform G(string n,Transform parent,Vector3 p){var t=new GameObject(n).transform;t.SetParent(parent,false);t.localPosition=p;return t;}
 static Material M(string n,Color c,float smooth=.1f){string path=gen+n+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Standard"));AssetDatabase.CreateAsset(m,path);}m.color=c;m.SetFloat("_Glossiness",smooth);return m;}
 static GameObject Shape(string n,PrimitiveType type,Transform p,Vector3 pos,Vector3 scale,Material m){var o=GameObject.CreatePrimitive(type);o.name=n;o.transform.SetParent(p,false);o.transform.localPosition=pos;o.transform.localScale=scale;Object.DestroyImmediate(o.GetComponent<Collider>());o.GetComponent<Renderer>().sharedMaterial=m;return o;}
 static GameObject Box(string n,Transform p,Vector3 v,Vector3 s,Material m)=>Shape(n,PrimitiveType.Cube,p,v,s,m);
 static GameObject Ball(string n,Transform p,Vector3 v,Vector3 s,Material m)=>Shape(n,PrimitiveType.Sphere,p,v,s,m);
 static void Rod(string n,Transform p,Vector3 a,Vector3 b,float r,Material m){var o=Shape(n,PrimitiveType.Capsule,p,(a+b)/2,V(r*2,(b-a).magnitude/2,r*2),m);o.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);}
 static Transform Model(string filename,Transform p,Vector3 pos,Material mat){var asset=AssetDatabase.LoadAssetAtPath<GameObject>(art+filename+".obj");if(!asset)throw new System.Exception("Missing sculpt "+filename);var o=Object.Instantiate(asset,p);o.name=filename;o.transform.localPosition=pos;o.transform.localRotation=Quaternion.identity;o.transform.localScale=Vector3.one;foreach(var r in o.GetComponentsInChildren<Renderer>())r.sharedMaterial=mat;return o.transform;}
 static void Clear(Transform p){foreach(Transform c in p.Cast<Transform>().ToArray())Object.DestroyImmediate(c.gameObject);}
 static void Texture(Material m,string label,Vector2 tiling,Color tint){m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(art+label+"_color.jpg");m.mainTextureScale=tiling;m.color=tint;m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(art+label+"_normal.jpg"));m.SetFloat("_BumpScale",.42f);m.EnableKeyword("_NORMALMAP");}
 static void Imports(){
  foreach(string name in new[]{"Floor","Oak","Plaster"}){var path=art+name+"_normal.jpg";var ti=AssetImporter.GetAtPath(path) as TextureImporter;if(ti&&ti.textureType!=TextureImporterType.NormalMap){ti.textureType=TextureImporterType.NormalMap;ti.SaveAndReimport();}}
  for(int i=0;i<3;i++){string path="Assets/Study/Resources/page"+i+".png";var ti=AssetImporter.GetAtPath(path) as TextureImporter;if(ti){ti.npotScale=TextureImporterNPOTScale.None;ti.isReadable=true;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.maxTextureSize=1024;ti.SaveAndReimport();}}
  for(int i=0;i<3;i++){var ti=AssetImporter.GetAtPath("Assets/Study/ArtV3/charcoal"+i+".png") as TextureImporter;if(ti){ti.isReadable=true;ti.npotScale=TextureImporterNPOTScale.None;ti.textureCompression=TextureImporterCompression.Uncompressed;ti.maxTextureSize=2048;ti.SaveAndReimport();}}
  foreach(string name in new[]{"SkullSculpt","JawSculpt","PortraitSculpt","PalmSculpt"}){var imp=AssetImporter.GetAtPath(art+name+".obj") as ModelImporter;if(imp){imp.globalScale=1;imp.useFileScale=false;imp.importNormals=ModelImporterNormals.Calculate;imp.normalSmoothingAngle=70;imp.SaveAndReimport();}}
 }
 public static void Apply(StudyGame game){
  Imports();ivory=AssetDatabase.LoadAssetAtPath<Material>(gen+"Aged ivory.mat");ivory.color=new Color(.72f,.66f,.5f);ivory.mainTexture=null;ivory.SetFloat("_Glossiness",.18f);
  oak=AssetDatabase.LoadAssetAtPath<Material>(gen+"Dark oak.mat");steel=AssetDatabase.LoadAssetAtPath<Material>(gen+"Oxidised steel.mat");linen=AssetDatabase.LoadAssetAtPath<Material>(gen+"Unbleached linen.mat");linen.color=new Color(.5f,.46f,.35f);linen.mainTexture=null;
  Texture(oak,"Oak",new Vector2(1,2),new Color(.68f,.59f,.46f));
  var floor=AssetDatabase.LoadAssetAtPath<Material>(gen+"Floorboards.mat");Texture(floor,"Floor",new Vector2(4,6),new Color(.62f,.62f,.55f));floor.SetFloat("_Glossiness",.3f);
  var wall=AssetDatabase.LoadAssetAtPath<Material>(gen+"Stained plaster.mat");Texture(wall,"Plaster",new Vector2(2.5f,1.4f),new Color(.42f,.44f,.35f));
  var a=GameObject.Find("STUDIO / architecture").transform;
  foreach(var t in a.GetComponentsInChildren<Transform>().ToArray())if(t.name=="Worn oak plank")Object.DestroyImmediate(t.gameObject);
  // Replace the most important silhouettes with sculpted, hollow anatomical forms.
  foreach(var enemy in game.skeletons){
   Clear(enemy.skull);Model("SkullSculpt",enemy.skull,Vector3.zero,ivory);Model("JawSculpt",enemy.skull,Vector3.zero,ivory);
   for(int i=0;i<12;i++)for(int row=0;row<2;row++){float theta=(i-5.5f)/11*2;Ball("Individual incisor",enemy.skull,V(Mathf.Sin(theta)*.071f,row==0?-.096f:-.118f,.074f+Mathf.Cos(theta)*.065f),V(.015f,.019f,.024f),ivory);}
   foreach(var tm in enemy.GetComponentsInChildren<TextMesh>())Object.DestroyImmediate(tm.gameObject);
   foreach(var tr in enemy.GetComponentsInChildren<Transform>())if(tr.name.Contains("joint"))tr.localScale*=.78f;
   enemy.skull.localRotation=Quaternion.Euler(5,0,-4);
   enemy.leftArm.localRotation=Quaternion.Euler(-5,0,-4);enemy.rightArm.localRotation=Quaternion.Euler(-9,0,4);
   var tag=Box("Museum inventory tag",enemy.transform,V(.2f,1.05f,.15f),V(.1f,.06f,.004f),linen);tag.transform.localRotation=Quaternion.Euler(0,0,13);
  }
  game.skeletons[0].transform.position=V(0,0,-.6f);game.skeletons[1].transform.position=V(3.1f,0,5.9f);
  game.body.transform.position=V(-.8f,0,-5.8f);game.subjects[0].title="Suspended figure";game.subjects[1].title="Reaching figure";game.subjects[2].title="Plaster, pears & linen";
  Transform bust=GameObject.Find("Plaster bust").transform;Clear(bust);
  Box("Carved plinth",bust,V(0,.07f,0),V(.5f,.14f,.42f),linen);var portrait=Model("PortraitSculpt",bust,V(0,.48f,0),linen);portrait.localRotation=Quaternion.Euler(0,180,-5);
  // Lower, painted wall panels contrast with stained plaster above.
  Material green=M("Old painted wainscot",new Color(.10f,.15f,.13f),.24f),black=M("Tar black",new Color(.026f,.034f,.031f)),copper=M("Aged copper",new Color(.35f,.23f,.12f),.4f);
  for(int side=-1;side<=1;side+=2){
   Box("Dark wainscot",a,V(side*7.32f,.62f,0),V(.1f,1.23f,18.9f),green);
   for(int z=-9;z<=9;z++)Box("Panel stile",a,V(side*7.23f,.63f,z),V(.1f,1.22f,.035f),oak);
  }
  // Large, framed studies provide visual history and recognizable drawing references.
  for(int i=0;i<5;i++){
   var f=G("Pinned anatomical study",a,V(-7.17f,2.55f,-6+i*3.2f));f.localRotation=Quaternion.Euler(0,90,(i-2)*2);
   Box("Study mount",f,Vector3.zero,V(.82f,1.1f,.035f),oak);
   var m=M("Wall study "+i,Color.white);m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Study/Resources/finished"+(i%3)+".png");
   var q=Shape("Charcoal on paper",PrimitiveType.Quad,f,V(0,0,-.025f),V(.77f,1.03f,1),m);q.transform.localRotation=Quaternion.identity;
   for(int s=-1;s<=1;s+=2)Ball("Drawing pin",f,V(s*.34f,.49f,-.037f),Vector3.one*.018f,copper);
  }
  // Shelves of portfolios, brushes, and plaster dust near the workbench.
  var shelf=G("Portfolio shelving",a,V(6.8f,0,1.3f));
  for(int y=0;y<3;y++)Box("Shelf board",shelf,V(0,.6f+y*.65f,0),V(1.0f,.06f,3.9f),oak);
  for(int z=-1;z<=1;z+=2)Box("Shelf upright",shelf,V(0,1.2f,z*1.9f),V(.09f,2.4f,.07f),steel);
  Random.InitState(52);
  for(int i=0;i<19;i++){var t=Box("Worn portfolio",shelf,V(-.2f,.95f+((i%3)==0?.65f:0),-1.65f+i*.18f),V(.48f,.65f,.055f),i%3==0?linen:oak);t.transform.localRotation=Quaternion.Euler(0,0,Random.Range(-9,9));}
  for(int i=0;i<12;i++){
   Vector3 p=V(-6.6f+Random.Range(-.3f,.3f),1.12f,-4.3f+Random.Range(0,2.3f));var tube=Box("Crumpled paint tube",a,p,V(.045f,.023f,.14f),i%2==0?linen:green);tube.transform.localRotation=Quaternion.Euler(0,Random.Range(0,180),0);Ball("Tube cap",tube.transform,V(0,0,.5f),V(.7f,.8f,.18f),black);
  }
  for(int i=0;i<16;i++){
   float angle=i*2.399f;var c=Box("Floor tape / model position",a,V(-.15f+Mathf.Cos(angle)*.85f,.008f,-.6f+Mathf.Sin(angle)*.65f),V(.035f,.003f,.2f),linen);c.transform.localRotation=Quaternion.Euler(0,angle*Mathf.Rad2Deg,0);
  }
  var deskLamp=G("Task lamp",a,V(-6.2f,1.11f,-2.1f));Rod("Lamp stem",deskLamp,Vector3.zero,V(0,.65f,0),.015f,steel);Rod("Bent lamp arm",deskLamp,V(0,.65f,0),V(.35f,.8f,0),.02f,steel);
  var shade=Shape("Lamp shade",PrimitiveType.Sphere,deskLamp,V(.35f,.76f,0),V(.3f,.1f,.24f),green);
  var task=G("Warm workbench pool",deskLamp,V(.35f,.7f,0)).gameObject.AddComponent<Light>();task.type=LightType.Point;task.color=new Color(1,.63f,.3f);task.intensity=1.0f;task.range=3.8f;task.shadows=LightShadows.Soft;
  foreach(var light in Object.FindObjectsByType<Light>(FindObjectsSortMode.None))if(light.name=="Cold window spill"){light.intensity=.65f;light.color=new Color(.27f,.48f,.57f);}
  // The viewmodel is lit separately so the paper and fingers stay readable.
  skin=M("Clay skin",new Color(.39f,.29f,.20f),.18f);coat=M("Charcoal wool sleeve",new Color(.06f,.09f,.085f),.04f);
  Clear(game.board);BuildBoard(game);
  game.headlamp.cullingMask=~(1<<9);game.headlamp.spotAngle=53;game.headlamp.innerSpotAngle=31;game.headlamp.intensity=3.2f;
  var fill=G("Hands / soft bounce",game.eye.transform,V(-.15f,.05f,.2f)).gameObject.AddComponent<Light>();fill.type=LightType.Point;fill.range=1.4f;fill.intensity=.8f;fill.color=new Color(1,.8f,.55f);fill.cullingMask=1<<9;
  game.pencilClip=AssetDatabase.LoadAssetAtPath<AudioClip>(art+"PencilEdited.wav");game.humClip=AssetDatabase.LoadAssetAtPath<AudioClip>(art+"RoomEdited.wav");foreach(var e in game.skeletons)e.wheelClip=AssetDatabase.LoadAssetAtPath<AudioClip>(art+"WheelsEdited.wav");
  RenderSettings.ambientLight=new Color(.072f,.087f,.083f);RenderSettings.fogColor=new Color(.025f,.043f,.041f);RenderSettings.fogDensity=.028f;
  game.eye.depthTextureMode=DepthTextureMode.Depth;QualitySettings.shadowResolution=ShadowResolution.VeryHigh;QualitySettings.shadowDistance=24;QualitySettings.antiAliasing=4;
  foreach(var e in game.skeletons)PrefabUtility.SaveAsPrefabAsset(e.gameObject,gen+"Skeleton_"+e.GetComponent<StudySubject>().index+".prefab");
  AssetDatabase.SaveAssets();
 }
 static void Finger(Transform p,string n,Vector3 a,Vector3 b,Vector3 c,float r){Rod(n,p,a,b,r,skin);Rod(n,p,b,c,r*.85f,skin);Ball("Knuckle",p,b,Vector3.one*r*1.9f,skin);}
 static void BuildBoard(StudyGame game){
  var board=game.board;Box("Battered hardboard",board,Vector3.zero,V(.48f,.016f,.625f),oak);
  var paperMat=new Material(Shader.Find("Study/Paper"));AssetDatabase.CreateAsset(paperMat,gen+"PaperV2.mat");
  var m=new Mesh();m.name="Paper explicit UV";m.vertices=new[]{V(-.213f,.01f,-.281f),V(.213f,.01f,-.281f),V(-.213f,.01f,.281f),V(.213f,.01f,.281f)};m.uv=new[]{new Vector2(0,0),new Vector2(1,0),new Vector2(0,1),new Vector2(1,1)};m.triangles=new[]{0,2,1,1,2,3};m.RecalculateNormals();AssetDatabase.CreateAsset(m,gen+"PaperV2.asset");
  var page=new GameObject("Paper / live charcoal",typeof(MeshFilter),typeof(MeshRenderer));page.transform.SetParent(board,false);page.GetComponent<MeshFilter>().sharedMesh=m;page.GetComponent<MeshRenderer>().sharedMaterial=paperMat;game.paper=page.GetComponent<Renderer>();
  Box("Spring clip",board,V(0,.028f,.284f),V(.115f,.025f,.035f),steel);Rod("Clip hinge",board,V(-.052f,.044f,.287f),V(.052f,.044f,.287f),.005f,steel);
  var left=G("Left hand / grip",board,V(-.242f,.006f,-.13f));Model("PalmSculpt",left,V(-.018f,-.008f,-.025f),skin);
  Rod("Left sleeve",left,V(-.03f,-.027f,-.2f),V(-.019f,-.021f,-.055f),.049f,coat);
  for(int i=0;i<4;i++){float z=.005f+i*.018f;Finger(left,"Holding finger",V(-.025f,0,z),V(.012f,.028f,z),V(.042f,.023f,z-.014f),.009f);}
  Finger(left,"Thumb",V(-.035f,.0f,-.044f),V(.008f,.042f,-.032f),V(.055f,.022f,-.018f),.012f);
  var rig=G("Writing hand / nib pivot",board,V(.08f,.045f,0));
  Rod("Charcoal stick",rig,V(0,.004f,0),V(.105f,.079f,-.12f),.0042f,M("Graphite",new Color(.032f,.029f,.024f),.08f));
  var palm=Model("PalmSculpt",rig,V(.09f,.056f,-.071f),skin);palm.localRotation=Quaternion.Euler(-25,18,-20);
  Rod("Right wrist",rig,V(.101f,.047f,-.094f),V(.146f,.026f,-.164f),.028f,skin);
  Rod("Right sleeve",rig,V(.13f,.023f,-.13f),V(.22f,.012f,-.32f),.05f,coat);
  Finger(rig,"Index finger",V(.071f,.074f,-.047f),V(.036f,.066f,-.015f),V(.027f,.027f,-.033f),.009f);
  Finger(rig,"Thumb",V(.073f,.033f,-.038f),V(.044f,.023f,-.046f),V(.026f,.033f,-.029f),.011f);
  for(int i=0;i<3;i++)Finger(rig,"Curled finger",V(.096f+i*.012f,.044f,-.04f-i*.012f),V(.064f+i*.012f,.014f,-.025f-i*.014f),V(.064f+i*.011f,.012f,-.063f-i*.012f),.0085f-i*.0006f);
  var draw=game.gameObject.AddComponent<StudyDrawing>();draw.paper=game.paper;draw.pencilRig=rig;draw.backgrounds=new Texture2D[3];draw.finished=new Texture2D[3];
  for(int i=0;i<3;i++){draw.backgrounds[i]=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Study/Resources/page"+i+".png");draw.finished[i]=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Study/Resources/finished"+i+".png");}
  draw.charcoalSources=new Texture2D[3];for(int i=0;i<3;i++)draw.charcoalSources[i]=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Study/ArtV3/charcoal"+i+".png");
  game.artwork=draw;
  foreach(var tr in board.GetComponentsInChildren<Transform>())tr.gameObject.layer=9;
  foreach(var r in board.GetComponentsInChildren<Renderer>()){r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;}
 }
}
