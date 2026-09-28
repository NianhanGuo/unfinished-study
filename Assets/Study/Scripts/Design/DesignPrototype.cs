using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

// A self-contained runtime scene: the Drawing scene and its assets remain independent.
public class DesignPrototype : MonoBehaviour {
 public enum Phase { Brief, Playing, Paused, Caught, Complete }
 public Phase phase=Phase.Brief;
 public Camera eye; public CharacterController player; public DesignSpider teacher;
 public bool crouching,working,automation; public float stamina=4,elapsed;
 public int stage; public float[] progress=new float[3];
 public readonly List<Vector3> tables=new List<Vector3>();
 public readonly Vector3[] stations={new Vector3(-7,0,7),new Vector3(7,0,-1),new Vector3(-6,0,-8),new Vector3(0,0,11)};
 public static readonly string[] Tasks={"01  CUT THE BOARD","02  ASSEMBLE THE MODEL","03  PHOTOGRAPH YOUR WORK","04  SUBMIT AT THE CRITIQUE DESK"};
 public const int SolidMask=1<<8;
 float yaw,pitch,stepClock,noticeUntil;string notice="";
 readonly List<Transform> signs=new List<Transform>();
 Material wall,wood,metal,paper,accent,skin; Transform model;AudioSource sound;AudioClip foot,tool;
 GUIStyle title,text,small,button;Texture2D white;
 public static DesignPrototype Launch(){
  Scene old=SceneManager.GetActiveScene();var scene=SceneManager.CreateScene("Design — The Critique " + System.Guid.NewGuid().ToString("N").Substring(0,6));SceneManager.SetActiveScene(scene);
  foreach(var root in old.GetRootGameObjects())root.SetActive(false);
  var g=new GameObject("Design / prototype director").AddComponent<DesignPrototype>();
  SceneManager.UnloadSceneAsync(old);return g;
 }
 void Awake(){Build();}
 Material Mat(string name,Color color){var m=new Material(Shader.Find("Standard"));m.name=name;m.color=color;m.SetFloat("_Glossiness",.18f);return m;}
 public static GameObject Shape(string name,Transform parent,PrimitiveType type,Vector3 position,Vector3 scale,Material material,bool solid=false){
  var o=GameObject.CreatePrimitive(type);o.name=name;o.transform.SetParent(parent,false);o.transform.localPosition=position;o.transform.localScale=scale;o.GetComponent<Renderer>().sharedMaterial=material;
  if(solid)o.layer=8;else {o.GetComponent<Collider>().enabled=false;Destroy(o.GetComponent<Collider>());}return o;
 }
 GameObject Box(string n,Vector3 p,Vector3 s,Material m,bool solid=true){return Shape(n,transform,PrimitiveType.Cube,p,s,m,solid);}
 void Sign(string caption,Vector3 position,Quaternion rotation,float size=.13f){
  var o=new GameObject(caption);o.transform.SetParent(transform);o.transform.position=position;o.transform.rotation=rotation;
  var t=o.AddComponent<TextMesh>();t.text=caption;t.fontSize=64;t.characterSize=size;t.anchor=TextAnchor.MiddleCenter;t.color=new Color(.87f,.86f,.73f);if(caption.Contains(" / "))signs.Add(o.transform);
 }
 void LightAt(Vector3 p,Color color,float range,float intensity){var o=new GameObject("Practical light");o.transform.SetParent(transform);o.transform.position=p;var l=o.AddComponent<Light>();l.type=LightType.Point;l.color=color;l.range=range;l.intensity=intensity;}
 void Table(Vector3 p){
  tables.Add(p);Box("Crawl-under worktable",p+Vector3.up*1.35f,new Vector3(3.8f,.18f,2.4f),wood);
  foreach(float x in new[]{-1.65f,1.65f})foreach(float z in new[]{-.95f,.95f})Box("Table leg",p+new Vector3(x,.65f,z),new Vector3(.14f,1.3f,.14f),metal);
  Box("Cutting mat",p+new Vector3(0,1.45f,0),new Vector3(1.9f,.015f,1.25f),accent,false);
 }
 void Build(){
  white=Texture2D.whiteTexture;
  wall=Mat("Blue grey plaster",new Color(.19f,.24f,.25f));wood=Mat("Worn worktop",new Color(.31f,.23f,.15f));metal=Mat("Painted steel",new Color(.065f,.09f,.1f));paper=Mat("Model board",new Color(.81f,.77f,.63f));accent=Mat("Cutting mat",new Color(.08f,.27f,.24f));skin=Mat("Professor skin",new Color(.43f,.40f,.34f));
  RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.24f,.28f,.3f);RenderSettings.fog=true;RenderSettings.fogColor=new Color(.035f,.055f,.064f);RenderSettings.fogDensity=.017f;
  Box("Floor",new Vector3(0,-.12f,0),new Vector3(22,.24f,28),wall);
  Box("Ceiling",new Vector3(0,5.6f,0),new Vector3(22,.2f,28),wall);
  foreach(float x in new[]{-11f,11f})Box("Side wall",new Vector3(x,2.7f,0),new Vector3(.3f,5.6f,28),wall);
  foreach(float z in new[]{-14f,14f})Box("End wall",new Vector3(0,2.7f,z),new Vector3(22,5.6f,.3f),wall);
  for(int z=-12;z<=12;z+=6){Box("Fluorescent housing",new Vector3(0,5.35f,z),new Vector3(3,.1f,.25f),paper,false);LightAt(new Vector3(0,4.7f,z),new Color(.66f,.8f,.83f),10,1.65f);}
  // Tall pin-up walls interrupt both sight and ceiling movement. Openings remain at each end.
  foreach(var p in new[]{new Vector3(-3.5f,2.65f,3),new Vector3(3.5f,2.65f,-5)}){
   Box("Full-height pin-up divider",p,new Vector3(.3f,5.3f,6),wood);
   for(int i=0;i<3;i++)Box("Pinned design studies",p+new Vector3(-.17f,-.2f,-2+i*2),new Vector3(.025f,1.2f,.8f),paper,false);
  }
  foreach(var p in new[]{new Vector3(0,0,2),new Vector3(-6,0,-3),new Vector3(6,0,6)})Table(p);
  for(int i=0;i<4;i++){
   Vector3 p=stations[i];Box("Assignment station "+i,p+Vector3.up*.5f,new Vector3(2.8f,1f,1.1f),wood);
   Sign(new[]{"01 / CUT","02 / BUILD","03 / PHOTO","04 / CRITIQUE"}[i],p+new Vector3(0,2.6f,0),Quaternion.identity,.10f);
   LightAt(p+Vector3.up*3,new Color(.95f,.65f,.32f),5,1.3f);
   Box("Station mat",p+Vector3.up*1.03f,new Vector3(2.3f,.03f,.8f),accent,false);
  }
  Box("Cutter bed",stations[0]+new Vector3(0,1.1f,.05f),new Vector3(1.5f,.08f,.55f),paper,false);
  Box("Guillotine handle",stations[0]+new Vector3(.6f,1.4f,0),new Vector3(.06f,.65f,.06f),metal,false);
  model=new GameObject("Model / physical progress").transform;model.SetParent(transform);model.position=stations[1]+Vector3.up*1.08f;
  Shape("Model base",model,PrimitiveType.Cube,Vector3.zero,new Vector3(1.4f,.06f,.7f),paper);
  Box("Camera body",stations[2]+new Vector3(0,1.25f,0),new Vector3(.4f,.3f,.25f),metal,false);
  Shape("Camera lens",transform,PrimitiveType.Cylinder,stations[2]+new Vector3(0,1.25f,-.2f),new Vector3(.2f,.15f,.2f),metal).transform.rotation=Quaternion.Euler(90,0,0);
  Sign("THE CRITIQUE\nBRING YOUR FINISHED WORK",new Vector3(0,4.0f,13.75f),Quaternion.identity,.10f);
  var body=new GameObject("Design student");body.transform.SetParent(transform);body.transform.position=new Vector3(0,0,-11);
  player=body.AddComponent<CharacterController>();player.height=1.75f;player.center=new Vector3(0,.875f,0);player.radius=.28f;player.stepOffset=.15f;
  var cam=new GameObject("Eyes");cam.transform.SetParent(body.transform,false);cam.transform.localPosition=new Vector3(0,1.6f,0);eye=cam.AddComponent<Camera>();eye.nearClipPlane=.04f;eye.farClipPlane=60;eye.fieldOfView=72;eye.backgroundColor=RenderSettings.fogColor;eye.clearFlags=CameraClearFlags.SolidColor;cam.AddComponent<AudioListener>();
  var lamp=cam.AddComponent<Light>();lamp.type=LightType.Spot;lamp.range=14;lamp.spotAngle=75;lamp.intensity=2.2f;lamp.color=new Color(.85f,.88f,.76f);lamp.shadows=LightShadows.Soft;
  sound=body.AddComponent<AudioSource>();foot=Tone(100,.08f);tool=Tone(190,.15f);
  var enemy=new GameObject("Professor / ceiling spider");enemy.transform.SetParent(transform);enemy.transform.position=new Vector3(0,4.7f,9);teacher=enemy.AddComponent<DesignSpider>();teacher.Initialize(this,skin,metal,paper);
  Physics.SyncTransforms();DesignNavigation.Build();SetCursor(false);
 }
 public static AudioClip Tone(float hz,float length){int n=(int)(22050*length);var samples=new float[n];for(int i=0;i<n;i++){float t=i/22050f;samples[i]=Mathf.Sin(t*hz*6.283f)*Mathf.Exp(-t*24)*.3f;}var c=AudioClip.Create("Dry knock",n,1,22050,false);c.SetData(samples,0);return c;}
 public void Begin(){phase=Phase.Playing;SetCursor(true);Notice("Critique has started. Finish your model and bring it to the front.",5);}
 void SetCursor(bool locked){if(automation)return;Cursor.lockState=locked?CursorLockMode.Locked:CursorLockMode.None;Cursor.visible=!locked;}
 public void Notice(string s,float seconds=2.5f){notice=s;noticeUntil=Time.time+seconds;}
 public void Catch(){if(phase!=Phase.Playing)return;phase=Phase.Caught;SetCursor(false);working=false;}
 public void ReturnToMenu(){SetCursor(false);SceneManager.LoadScene("Studio");}
 public int UnderTable(){for(int i=0;i<tables.Count;i++){Vector3 d=player.transform.position-tables[i];if(Mathf.Abs(d.x)<1.9f&&Mathf.Abs(d.z)<1.2f&&crouching)return i;}return -1;}
 public bool CanWork(){if(stage>3)return false;Vector3 delta=stations[stage]-player.transform.position;delta.y=0;return delta.magnitude<2.7f&&!Physics.Linecast(eye.transform.position,stations[stage]+Vector3.up*1.2f,SolidMask);}
 public void Work(float dt){
  if(!CanWork()||phase!=Phase.Playing)return;
  if(stage==3){phase=Phase.Complete;working=false;SetCursor(false);return;}
  progress[stage]=Mathf.Min(1,progress[stage]+dt/new[]{5f,7f,3f}[stage]);
  if(stage==0)teacher.Hear(stations[0]);
  if(stage==1){int parts=Mathf.FloorToInt(progress[1]*5);while(model.childCount<parts+1){int i=model.childCount;Shape("Foamboard volume",model,PrimitiveType.Cube,new Vector3((i-3)*.22f,.12f+i*.045f,0),new Vector3(.17f,.18f+i*.09f,.5f),paper);}}
  if(progress[stage]>=1){stage++;Notice(stage==3?"Model documented. Bring it to the critique desk.":"Step complete. Progress saved — move to the next station.",4);}
 }
 public void SetCrouch(bool want){
  if(!want&&crouching&&Physics.CheckCapsule(player.transform.position+Vector3.up*.4f,player.transform.position+Vector3.up*1.5f,.27f,SolidMask))want=true;
  crouching=want;player.height=want?.85f:1.75f;player.center=Vector3.up*player.height*.5f;eye.transform.localPosition=Vector3.up*(want?.68f:1.6f);
 }
 void Update(){
  foreach(var sign in signs){Vector3 d=sign.position-eye.transform.position;d.y=0;if(d.sqrMagnitude>.01f)sign.rotation=Quaternion.LookRotation(d);}
  if(Input.GetKeyDown(KeyCode.Escape)){if(phase==Phase.Playing){phase=Phase.Paused;working=false;SetCursor(false);}else if(phase==Phase.Paused){phase=Phase.Playing;SetCursor(true);}}
  if(phase!=Phase.Playing)return;elapsed+=Time.deltaTime;
  if(automation)return;
  yaw+=Input.GetAxis("Mouse X")*1.7f;pitch=Mathf.Clamp(pitch-Input.GetAxis("Mouse Y")*1.6f,-85,80);player.transform.rotation=Quaternion.Euler(0,yaw,0);eye.transform.localRotation=Quaternion.Euler(pitch,0,0);
  SetCrouch(Input.GetKey(KeyCode.LeftControl)||Input.GetKey(KeyCode.C));
  Vector3 input=new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical")).normalized;
  bool sprint=Input.GetKey(KeyCode.LeftShift)&&input.sqrMagnitude>.1f&&!crouching&&stamina>.05f;
  stamina=Mathf.Clamp(stamina+Time.deltaTime*(sprint?-1:.65f),0,4);
  float speed=crouching?2.2f:sprint?6.4f:3.4f;player.Move((player.transform.TransformDirection(input)*speed+Vector3.down*5)*Time.deltaTime);
  if(input.sqrMagnitude>.1f){stepClock+=Time.deltaTime*speed;if(stepClock>.95f){stepClock=0;sound.PlayOneShot(foot,crouching?.15f:.6f);}}
  working=Input.GetKey(KeyCode.E)&&CanWork();if(working){Work(Time.deltaTime);if(!sound.isPlaying)sound.PlayOneShot(tool,.35f);}
 }
 void OnApplicationFocus(bool focus){if(!automation&&!focus&&phase==Phase.Playing){phase=Phase.Paused;working=false;SetCursor(false);}}
 void Styles(){if(title!=null)return;title=new GUIStyle(GUI.skin.label){fontSize=62,fontStyle=FontStyle.Bold};text=new GUIStyle(GUI.skin.label){fontSize=24,wordWrap=true};small=new GUIStyle(text){fontSize=18};button=new GUIStyle(GUI.skin.button){fontSize=22};foreach(var s in new[]{title,text,small})s.normal.textColor=new Color(.91f,.87f,.73f);}
 void Panel(Rect r,float alpha=.86f){GUI.color=new Color(.025f,.04f,.045f,alpha);GUI.DrawTexture(r,white);GUI.color=Color.white;}
 void Label(float x,float y,float w,float h,string s,GUIStyle style){GUI.Label(new Rect(x,y,w,h),s,style);}
 void OnGUI(){
  Styles();float scale=Mathf.Min(Screen.width/1440f,Screen.height/900f);GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1440*scale)/2,(Screen.height-900*scale)/2,0),Quaternion.identity,Vector3.one*scale);
  if(phase==Phase.Brief){Panel(new Rect(70,90,1050,730));Label(105,115,950,45,"DESIGN ASSIGNMENT / PLAYABLE PROTOTYPE",small);Label(100,165,990,100,"THE CRITIQUE",title);
   Label(105,280,960,140,"Critique has started. Your model isn't ready.\nYour professor is collecting students — finish your work before he catches you.",text);
   Label(105,420,930,190,"CUT → BUILD → PHOTOGRAPH → SUBMIT\n\nSprint between cover. Crouch beneath worktables, then escape from the opposite side when his head comes down. Tall dividers break his line of sight. The cutter makes noise.",text);
   Label(105,630,930,60,"WASD move · MOUSE look · SHIFT sprint · C / CTRL crouch · HOLD E work",small);
   if(GUI.Button(new Rect(105,710,400,60),"FINISH THE DESIGN ASSIGNMENT",button))Begin();if(GUI.Button(new Rect(535,710,260,60),"BACK TO ASSIGNMENTS",small))ReturnToMenu();return;
  }
  if(phase==Phase.Playing){
   Panel(new Rect(28,25,540,132),.72f);Label(48,39,510,35,Tasks[Mathf.Min(stage,3)],text);Label(48,82,500,30,stage<3?"Progress saved: "+Mathf.RoundToInt(progress[stage]*100)+"%":"Your work is ready. Hold E at the critique desk.",small);
   if(stage<3){GUI.color=new Color(.4f,.7f,.6f);GUI.DrawTexture(new Rect(49,127,480*progress[stage],5),white);GUI.color=Color.white;}
   Label(1090,32,320,35,"SPRINT  "+Mathf.CeilToInt(stamina*25)+"%",small);Label(1090,66,320,35,crouching?"CROUCHED":"SHIFT sprint / C crouch",small);
   Label(710,430,30,40,"·",text);
   string hint=CanWork()?"HOLD E — "+Tasks[stage]+"  /  release to flee":UnderTable()>=0?"Stay low. If his face appears, leave through the other side.":"Find the numbered workstation. Tall dividers break line of sight.";
   Panel(new Rect(220,800,1000,60),.78f);Label(242,816,965,40,hint,small);
   if(Time.time<noticeUntil){Panel(new Rect(320,675,820,80),.8f);Label(343,692,775,65,notice,text);}
   if(teacher.IsThreat){GUI.color=new Color(.7f,.15f,.09f,.75f);GUI.DrawTexture(new Rect(0,0,1440,7),white);GUI.color=Color.white;Label(650,45,410,40,teacher.Status,small);}
   Vector3 p=eye.WorldToViewportPoint(stations[Mathf.Min(stage,3)]+Vector3.up*2.7f);if(p.z>0&&p.x>.05f&&p.x<.95f&&p.y>.15f&&p.y<.8f)Label(p.x*1440-50,(1-p.y)*900,200,50,(stage+1).ToString("00")+"  /  "+Mathf.RoundToInt(Vector3.Distance(player.transform.position,stations[stage]))+"m",small);
   return;
  }
  Panel(new Rect(0,0,1440,900),.91f);Label(100,200,1200,110,phase==Phase.Paused?"HOLD THAT THOUGHT.":phase==Phase.Caught?"YOU'RE UP NEXT.":"ACCEPTED.",title);
  Label(105,335,1050,130,phase==Phase.Caught?"He brought you to critique before your model was ready.\nBreak sight behind dividers. Leave the table before his neck reaches you.":phase==Phase.Complete?"He studies the model. Straightens his tie.\n\"You may go.\"": "WASD move · SHIFT sprint · C / CTRL crouch · HOLD E work",text);
  if(phase==Phase.Paused&&GUI.Button(new Rect(105,520,350,65),"RESUME",button)){phase=Phase.Playing;SetCursor(true);}
  if(phase!=Phase.Paused&&GUI.Button(new Rect(105,520,350,65),"RETRY DESIGN",button))Launch();
  if(GUI.Button(new Rect(105,610,350,65),"CHOOSE ASSIGNMENT",button))ReturnToMenu();
 }
}
