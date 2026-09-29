using UnityEngine;

// Presentation-only camera and lights; RestoreForPlay returns every altered gameplay setting.
public class StudyFrontEnd:MonoBehaviour {
 public enum Page { Cover, Assignments }
 public Page page;public bool Presenting{get;private set;}
 StudyGame game;GameObject rig;Vector3 localPosition;Quaternion localRotation;float fov,lampIntensity,fogDensity;Color ambient;
 Texture2D fade,drawingImage,designImage;GUIStyle kicker,hero,section,cardTitle,body,action;float age,transition;
 public void Initialize(StudyGame g){
  game=g;localPosition=g.eye.transform.localPosition;localRotation=g.eye.transform.localRotation;fov=g.eye.fieldOfView;lampIntensity=g.headlamp.intensity;ambient=RenderSettings.ambientLight;fogDensity=RenderSettings.fogDensity;
  fade=new Texture2D(256,1,TextureFormat.RGBA32,false);for(int i=0;i<256;i++)fade.SetPixel(i,0,new Color(.012f,.019f,.024f,Mathf.Pow(1-i/255f,1.6f)*.91f));fade.Apply();fade.wrapMode=TextureWrapMode.Clamp;
  drawingImage=Resources.Load<Texture2D>("MenuDrawing");designImage=Resources.Load<Texture2D>("MenuDesign");
  rig=new GameObject("Cover / cinematic lighting");rig.transform.SetParent(transform);
  Spot("Warm studio key",new Vector3(-3,4,-3),new Vector3(0,1,-.5f),new Color(1,.81f,.61f),1.25f,75);
  Spot("Moonlit rim",new Vector3(4,4.8f,5),new Vector3(0,1,0),new Color(.45f,.62f,.77f),1.3f,90);
  Spot("Still-life pool",new Vector3(-2,4.5f,4),new Vector3(0,1.3f,3.2f),new Color(1,.84f,.65f),.85f,64);
  RenderSettings.ambientLight=new Color(.09f,.115f,.135f);RenderSettings.fogDensity=.015f;
  Presenting=true;g.board.gameObject.SetActive(false);g.headlamp.intensity=0;Tick(0);
 }
 void Spot(string name,Vector3 position,Vector3 target,Color color,float intensity,float angle){var o=new GameObject(name);o.transform.SetParent(rig.transform);o.transform.position=position;o.transform.LookAt(target);var l=o.AddComponent<Light>();l.type=LightType.Spot;l.range=18;l.spotAngle=angle;l.color=color;l.intensity=intensity;l.shadows=LightShadows.Soft;l.shadowResolution=UnityEngine.Rendering.LightShadowResolution.High;l.cullingMask=~(1<<9);}
 public void Tick(float dt){
  if(!Presenting)return;age+=dt;transition=Mathf.MoveTowards(transition,0,dt*2.4f);
  float orbit=-.38f+Mathf.Sin(age*.055f)*.48f;
  game.eye.transform.position=new Vector3(Mathf.Sin(orbit)*5.7f,2.18f+Mathf.Sin(age*.11f)*.12f,1.8f-Mathf.Cos(orbit)*7.5f);
  game.eye.transform.LookAt(new Vector3(.1f,1.5f,1.6f));game.eye.fieldOfView=page==Page.Cover?53:58;
  if(Input.GetKeyDown(KeyCode.Escape)&&page==Page.Assignments)ShowCover();
  if(page==Page.Cover){if(Input.GetKeyDown(KeyCode.Return))ShowAssignments();}
  else {if(Input.GetKeyDown(KeyCode.Alpha1)||Input.GetKeyDown(KeyCode.Return))game.Enter();if(Input.GetKeyDown(KeyCode.Alpha2))DesignPrototype.Launch();}
 }
 public void ShowAssignments(){page=Page.Assignments;transition=.3f;}
 public void ShowCover(){page=Page.Cover;transition=.3f;}
 public void RestoreForPlay(){if(!Presenting)return;Presenting=false;rig.SetActive(false);game.eye.transform.localPosition=localPosition;game.eye.transform.localRotation=localRotation;game.eye.fieldOfView=fov;game.headlamp.intensity=lampIntensity;RenderSettings.ambientLight=ambient;RenderSettings.fogDensity=fogDensity;game.board.gameObject.SetActive(true);}
 void Styles(){if(hero!=null)return;kicker=new GUIStyle(GUI.skin.label){fontSize=16};hero=new GUIStyle(kicker){font=game.serif,fontSize=112};section=new GUIStyle(hero){fontSize=58};cardTitle=new GUIStyle(hero){fontSize=44};body=new GUIStyle(kicker){fontSize=22};action=new GUIStyle(kicker){fontSize=19};foreach(var s in new[]{kicker,hero,section,cardTitle,body,action})s.normal.textColor=new Color(.91f,.88f,.79f);}
 void Fill(Rect r,Color color){GUI.color=color;GUI.DrawTexture(r,Texture2D.whiteTexture);GUI.color=Color.white;}
 void Text(float x,float y,float w,float h,string value,GUIStyle style){GUI.Label(new Rect(x,y,w,h),value,style);}
 bool Link(Rect r,string label){bool hover=r.Contains(Event.current.mousePosition);Fill(r,new Color(.12f,.17f,.18f,hover?.95f:.72f));Fill(new Rect(r.x,r.y,r.width,1),new Color(.65f,.58f,.43f,.8f));Text(r.x+22,r.y+18,r.width-35,40,label,action);return GUI.Button(r,GUIContent.none,GUIStyle.none);}
 void Assignment(Rect r,int number,string name,string subtitle,Texture2D image){
  bool hover=r.Contains(Event.current.mousePosition);Fill(new Rect(r.x-1,r.y-1,r.width+2,r.height+2),new Color(.75f,.66f,.46f,hover?1:.32f));Fill(r,new Color(.027f,.042f,.046f,.98f));
  if(image)GUI.DrawTexture(new Rect(r.x,r.y,r.width,310),image,ScaleMode.ScaleAndCrop);
  Fill(new Rect(r.x,r.y+276,r.width,34),new Color(.025f,.04f,.04f,.42f));
  Text(r.x+25,r.y+329,r.width-50,30,number==1?"01  /  DRAWING":"02  /  DESIGN",kicker);
  Text(r.x+23,r.y+369,r.width-46,65,name,cardTitle);Text(r.x+25,r.y+432,r.width-50,35,subtitle,body);
  Text(r.x+25,r.y+492,r.width-50,32,number==1?"ENTER THE STUDIO     [1]  →":"ENTER THE CRITIQUE     [2]  →",action);
  if(GUI.Button(r,GUIContent.none,GUIStyle.none)){if(number==1)game.Enter();else DesignPrototype.Launch();}
 }
 public void Draw(){
  Styles();GUI.DrawTexture(new Rect(0,0,1120,900),fade);Fill(new Rect(0,0,1440,28),new Color(.008f,.015f,.019f,.8f));Fill(new Rect(0,865,1440,35),new Color(.008f,.015f,.019f,.8f));
  if(page==Page.Cover){
   Text(88,75,850,32,"A F T E R   H O U R S    /    A N   A R T   S C H O O L   N I G H T",kicker);
   Text(82,239,850,270,"UNFINISHED\nSTUDY",hero);
   Fill(new Rect(92,534,63,2),new Color(.75f,.49f,.27f));Text(92,563,780,48,"Two assignments. One last night.",body);
   if(Link(new Rect(92,660,380,66),"CHOOSE AN ASSIGNMENT    →"))ShowAssignments();
   Text(94,741,420,30,"ENTER TO BEGIN",kicker);
   if(GUI.Button(new Rect(92,809,130,30),"LEAVE",kicker))Application.Quit();
   Text(1170,778,220,45,"02:00 AM",cardTitle);Text(1174,832,210,30,"BEFORE THE DEADLINE",kicker);
  }else{
   Fill(new Rect(0,28,1440,837),new Color(.011f,.023f,.027f,.63f));
   Text(94,66,700,30,"TONIGHT'S ASSIGNMENTS",kicker);Text(90,106,1100,85,"Where will you begin?",section);Text(96,188,1050,35,"Both studios are open. Choose either one.",body);
   Assignment(new Rect(94,254,606,550),1,"Unfinished Study","Look up. Remember. Draw.",drawingImage);
   Assignment(new Rect(740,254,606,550),2,"The Critique","Finish your model. Don't get caught.",designImage);
   if(GUI.Button(new Rect(94,828,250,30),"← BACK TO COVER   [ESC]",kicker))ShowCover();
  }
  if(transition>0)Fill(new Rect(0,0,1440,900),new Color(.01f,.02f,.025f,transition));
 }
 void OnDestroy(){if(fade)Destroy(fade);}
}
