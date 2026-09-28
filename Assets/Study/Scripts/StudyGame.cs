using System;
using UnityEngine;
using UnityEngine.SceneManagement;
public class StudyGame : MonoBehaviour {
 public Camera eye;public CharacterController body;public Light headlamp;
 public Transform board,deathHand;public Renderer paper;public StudySubject[] subjects;public StudySkeleton[] skeletons;public Light[] ceilingLights;public Font serif;
 public AudioClip pencilClip,humClip;public Texture2D[] sketches;public Texture2D paperTexture;public StudyDrawing artwork;
 public enum Phase{Title,Reading,Entering,Playing,Paused,Lost,Won}
 public Phase phase=Phase.Title;
 public float memory,observation,elapsed;public int selected=-1,tutorial;public bool drawing,hasDrawn;
 [NonSerialized]public bool automation,automatedDraw;
 public StudyIntro intro;public StudySoundscape soundscape;
 public bool Marking{get;private set;} public float Lower=>lower;
 public static StudyGame Instance;
 float observationLostAt;float yaw,pitch,lower,endTime,footTime,startTime,flashTime,lastProgress,black;bool lightsOn,ruined;int lookedAt=-1;
 AudioSource pencil,room;GUIStyle small,label,heading,large,serifSmall,story,titleStyle;Texture2D pixel,titleShade;readonly Color ivory=new Color(.84f,.81f,.71f),ochre=new Color(.68f,.34f,.24f);
 void Awake(){Instance=this;}
 void Start(){
  pixel=Texture2D.whiteTexture;titleShade=new Texture2D(256,1,TextureFormat.RGBA32,false);for(int i=0;i<256;i++)titleShade.SetPixel(i,0,new Color(.006f,.008f,.009f,.96f*Mathf.Pow(1-i/255f,.72f)));titleShade.wrapMode=TextureWrapMode.Clamp;titleShade.Apply();yaw=body.transform.eulerAngles.y;artwork.Initialize();
  pencil=gameObject.AddComponent<AudioSource>();pencil.clip=pencilClip;pencil.loop=true;pencil.volume=0;pencil.Play();
  room=gameObject.AddComponent<AudioSource>();room.clip=humClip;room.loop=true;room.volume=.08f;room.Play();
  foreach(var s in skeletons)s.Init(this);StudyNavigation.Build();intro=gameObject.AddComponent<StudyIntro>();intro.Initialize(this);soundscape=gameObject.AddComponent<StudySoundscape>();soundscape.Initialize(this);Cursor.lockState=CursorLockMode.None;Cursor.visible=true;
  #if UNITY_EDITOR
  if(System.IO.File.Exists("StudyRegression.request")){System.IO.File.Delete("StudyRegression.request");gameObject.AddComponent<StudyRegression>();}
  if(System.IO.File.Exists("StudyQA.request")){System.IO.File.Delete("StudyQA.request");gameObject.AddComponent<StudyQA>();}
  #endif
  
 }
 public void AimAt(Vector3 p){var d=p-eye.transform.position;yaw=Quaternion.LookRotation(d).eulerAngles.y;pitch=-Mathf.Asin(d.normalized.y)*Mathf.Rad2Deg;}
 public void StartStudy(){if(intro)intro.Complete();phase=Phase.Playing;Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;}
 void Enter(){phase=Phase.Reading;intro.Present();Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
 void FoldNote(){phase=Phase.Entering;startTime=Time.time;Cursor.lockState=CursorLockMode.Locked;Cursor.visible=false;}
 void Pause(){phase=Phase.Paused;drawing=false;Marking=false;pencil.volume=0;Cursor.lockState=CursorLockMode.None;Cursor.visible=true;}
 void OnApplicationFocus(bool f){if(!automation&&!f&&(phase==Phase.Playing||phase==Phase.Entering))Pause();}
 public bool Illuminated(Vector3 p){Vector3 d=p-headlamp.transform.position;return d.magnitude<headlamp.range&&Vector3.Angle(headlamp.transform.forward,d)<headlamp.spotAngle*.44f&&!Physics.Linecast(headlamp.transform.position,p,1<<8);}
 public int GetObserved(){
  int result=-1;float best=12;
  for(int i=0;i<subjects.Length;i++){
   if(subjects[i].progress>=1)continue;
   for(int j=0;j<subjects[i].ObservationPointCount;j++){
    Vector3 point=subjects[i].ObservationPoint(j),d=point-eye.transform.position;
    float angle=Vector3.Angle(eye.transform.forward,d);
    if(angle<best&&d.magnitude<headlamp.range&&Illuminated(point)){result=i;best=angle;}
   }
  }
  return result;
 }
 public void Finish(bool success){
  if(phase!=Phase.Playing)return;phase=success?Phase.Won:Phase.Lost;endTime=Time.time;drawing=false;Marking=false;pencil.volume=0;ruined=false;lightsOn=false;
  foreach(var s in skeletons)s.Silence();Cursor.lockState=CursorLockMode.None;Cursor.visible=true;room.volume=success?.035f:0;
 }
 void Update(){
  if(Input.GetKeyDown(KeyCode.Escape)){if(phase==Phase.Playing)Pause();else if(phase==Phase.Paused)StartStudy();}
  if(phase==Phase.Title){
   eye.fieldOfView=51;eye.transform.localRotation=Quaternion.Euler(4+Mathf.Sin(Time.time*.19f)*.3f,-18+Mathf.Sin(Time.time*.11f)*.5f,0);headlamp.intensity=1.2f*(1-.12f*Mathf.Pow(Mathf.Max(0,Mathf.Sin(Time.time*.71f)),28));
   intro.Complete();board.localPosition=new Vector3(.27f,-.52f,.64f);board.localRotation=Quaternion.Euler(-43,0,-12);
   if(Input.GetKeyDown(KeyCode.Return))Enter();return;
  }
  if(phase==Phase.Reading){if(Input.GetKeyDown(KeyCode.Return))FoldNote();return;}
  if(phase==Phase.Entering){float seconds=Time.time-startTime;float t=Mathf.Clamp01(seconds/StudyIntro.Duration);float move=Mathf.SmoothStep(0,1,Mathf.Clamp01((t-.64f)/.36f));eye.fieldOfView=Mathf.Lerp(51,66,move);eye.transform.localRotation=Quaternion.Euler(Mathf.Lerp(4,pitch,move),Mathf.Lerp(-18,0,move),0);headlamp.intensity=Mathf.Lerp(1.2f,3.2f,move);intro.Animate(seconds);if(t>=1||Input.GetKeyDown(KeyCode.Return))StartStudy();return;}

  if(phase==Phase.Won||phase==Phase.Lost){Ending();return;}
  if(phase!=Phase.Playing)return;
  eye.fieldOfView=66;elapsed+=Time.deltaTime;
  if(!automation){yaw+=Input.GetAxis("Mouse X")*1.65f;if(!drawing)pitch=Mathf.Clamp(pitch-Input.GetAxis("Mouse Y")*1.5f,-60,60);}
  bool wants=automation?automatedDraw:(Input.GetKey(KeyCode.Space)||Input.GetMouseButton(0));drawing=wants&&selected>=0&&subjects[selected].progress<1;
  lower=Mathf.MoveTowards(lower,drawing?1:0,Time.deltaTime*2.7f);body.transform.rotation=Quaternion.Euler(0,yaw,0);
  eye.transform.localRotation=Quaternion.Euler(Mathf.Lerp(pitch,49,Mathf.SmoothStep(0,1,lower)),0,Mathf.Sin(elapsed)*.12f);
  board.localPosition=Vector3.Lerp(new Vector3(.12f,-.53f,.53f),new Vector3(-.04f,.005f,.66f),lower)+new Vector3(Mathf.Sin(elapsed*1.9f)*.002f,Mathf.Sin(elapsed*1.3f)*.0025f,0);
  board.localRotation=Quaternion.Euler(Mathf.Lerp(-36,-70,lower),0,Mathf.Lerp(-9,-3,lower));
  Vector3 input=automation?Vector3.zero:new Vector3(Input.GetAxisRaw("Horizontal"),0,Input.GetAxisRaw("Vertical")).normalized;float speed=drawing?.62f:2.3f;
  body.Move((body.transform.TransformDirection(input)*speed+Vector3.down*4)*Time.deltaTime);
  if(input.sqrMagnitude>.1f){footTime+=Time.deltaTime*speed;eye.transform.localPosition=new Vector3(0,1.58f+Mathf.Sin(footTime*5)*.008f,0);}
  if(!drawing&&lower<.12f){int seen=GetObserved();
   if(seen>=0&&subjects[seen].progress<1){
    if(selected!=seen){selected=seen;memory=0;observation=0;}
    if(lookedAt!=seen)observation=0;lookedAt=seen;observationLostAt=Time.time;observation+=Time.deltaTime;
    if(observation>=1.2f){if(memory<4.9f)flashTime=Time.time;memory=5;tutorial=Mathf.Max(1,tutorial);}
   }else if(Time.time-observationLostAt>.18f){lookedAt=-1;observation=0;}
   if(hasDrawn)tutorial=Mathf.Max(3,tutorial);
  }
  Marking=drawing&&lower>.82f&&memory>0;
  if(Marking){hasDrawn=true;tutorial=Mathf.Max(2,tutorial);memory=Mathf.Max(0,memory-Time.deltaTime);subjects[selected].progress=Mathf.Min(1,subjects[selected].progress+Time.deltaTime/34);}
  if(selected>=0)artwork.RenderProgress(selected,subjects[selected].progress,Marking);
  pencil.volume=Mathf.MoveTowards(pencil.volume,Marking&&artwork.TipDown?.19f:0,Time.deltaTime*2.5f);pencil.pitch=.88f+Mathf.Sin(elapsed*2.3f)*.07f;
  bool done=true;foreach(var s in subjects)if(s.progress<1)done=false;if(done)Finish(true);
  headlamp.intensity=3.2f+Mathf.Sin(elapsed*13)*.01f;
 }
 void Ending(){
  float t=Time.time-endTime;
  if(phase==Phase.Lost){
   if(deathHand){deathHand.gameObject.SetActive(true);deathHand.localPosition=Vector3.Lerp(new Vector3(.6f,-.34f,.56f),new Vector3(.11f,-.17f,.38f),Mathf.SmoothStep(0,1,t/.65f));}
   if(!ruined&&t>.45f){artwork.Ruin(Mathf.Max(0,selected));ruined=true;}
   board.localRotation=Quaternion.Euler(-67,0,-3-Mathf.Min(t*8,9));black=Mathf.Clamp01((t-.7f)/.55f);
  }else{
   lower=Mathf.MoveTowards(lower,0,Time.deltaTime*.8f);board.localPosition=Vector3.Lerp(new Vector3(.12f,-.58f,.53f),new Vector3(-.04f,.005f,.66f),lower);eye.transform.localRotation=Quaternion.Euler(Mathf.Lerp(pitch,49,lower),0,0);
   if(t>.75f&&!lightsOn){lightsOn=true;RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;foreach(var l in ceilingLights)l.intensity=1.35f;RenderSettings.ambientLight=new Color(.18f,.2f,.18f);RenderSettings.fogDensity=.018f;headlamp.intensity=.3f;}
  }
  if(t>2&&Input.GetKeyDown(KeyCode.R))SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
 }
 void Styles(){if(small!=null)return;small=new GUIStyle(GUI.skin.label){fontSize=17};small.normal.textColor=ivory;label=new GUIStyle(small){fontSize=23};heading=new GUIStyle(small){fontSize=30,font=serif};large=new GUIStyle(heading){fontSize=83};serifSmall=new GUIStyle(heading){fontSize=23,fontStyle=FontStyle.Italic};story=new GUIStyle(label){fontSize=23,wordWrap=true};titleStyle=new GUIStyle(large){fontSize=87};}
 void Fill(Rect r,Color c){GUI.color=c;GUI.DrawTexture(r,pixel);GUI.color=Color.white;}
 void Text(float x,float y,float w,float h,string s,GUIStyle st){GUI.Label(new Rect(x,y,w,h),s,st);}
 void Line(Vector2 a,Vector2 b,float width,Color color){Matrix4x4 old=GUI.matrix;GUIUtility.RotateAroundPivot(Mathf.Atan2(b.y-a.y,b.x-a.x)*Mathf.Rad2Deg,a);Fill(new Rect(a.x,a.y,Vector2.Distance(a,b),width),color);GUI.matrix=old;}
 bool Button(float x,float y,float w,string s){Rect r=new Rect(x,y,w,52);bool hover=r.Contains(Event.current.mousePosition);Fill(r,new Color(.12f,.08f,.065f,hover?.8f:.42f));Fill(new Rect(x,y,3,52),ochre);Fill(new Rect(x,y+51,w,1),new Color(.62f,.45f,.32f,hover?.8f:.28f));Text(x+18,y+12,w-25,36,s,label);return GUI.Button(r,"",GUIStyle.none);}
 void Key(float x,float y,string key,string action){Fill(new Rect(x,y,80,35),new Color(.84f,.81f,.71f,.1f));Text(x+8,y+5,75,30,key,small);Text(x+96,y+4,400,36,action,small);}
 void Card(float x,float y,int index,bool complete,float scale=1){var tex=complete?artwork.finished[index]:artwork.Pages[index];Fill(new Rect(x+8,y+12,245*scale,330*scale),new Color(0,0,0,.55f));GUI.DrawTexture(new Rect(x,y,245*scale,330*scale),tex,ScaleMode.StretchToFill);}
 void OnGUI(){
  if(!artwork||artwork.Pages==null)return;Styles();float uiScale=Mathf.Min(Screen.width/1440f,Screen.height/900f);GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1440*uiScale)/2,(Screen.height-900*uiScale)/2,0),Quaternion.identity,new Vector3(uiScale,uiScale,1));
  if(phase==Phase.Title){
   GUI.DrawTexture(new Rect(0,0,1232,900),titleShade);
   Fill(new Rect(0,0,1440,24),Color.black);Fill(new Rect(0,872,1440,28),Color.black);
   Text(88,59,760,30,"A R T   S C H O O L   /   D R A W I N G",small);
   GUI.color=ochre;Text(1150,60,270,35,"02:00 AM",heading);GUI.color=Color.white;
   Text(1062,101,365,26,"THE NIGHT BEFORE IT'S DUE",small);
   GUI.color=new Color(.23f,.12f,.09f,.8f);Text(86,177,810,222,"UNFINISHED\nSTUDY",titleStyle);GUI.color=Color.white;
   Text(82,174,810,222,"UNFINISHED\nSTUDY",titleStyle);
   Line(new Vector2(89,416),new Vector2(592,416),1,new Color(.63f,.37f,.24f,.55f));
   Text(90,450,680,155,"You're an art school student.\nYou put off your drawing homework.\nNow it's 2:00 AM, the night before it's due.\nThree drawings. An empty studio.",story);
   GUI.color=new Color(.72f,.57f,.45f);Text(91,624,730,45,"Keep them in the light.",serifSmall);GUI.color=Color.white;
   Text(90,669,950,26,"LOOK UP TO REMEMBER · HOLD SPACE TO DRAW · KEEP THEM IN THE LIGHT",small);
   if(Button(90,702,545,"FINISH THE HOMEWORK   [ENTER]"))Enter();
   if(GUI.Button(new Rect(91,774,260,30),"LEAVE THE STUDIO",small))Application.Quit();
   Text(90,826,880,28,"WASD  move     /     MOUSE  look     /     HOLD SPACE  draw",small);
   // Sparse charcoal registration marks, rather than a clean software frame.
   for(int i=0;i<9;i++){float x=1114+i*11;Line(new Vector2(x,789),new Vector2(x-3,814+(i%3)*3),i%3==0?2:1,new Color(.63f,.52f,.4f,.25f));}
   Text(1096,740,350,30,"03 STUDIES / 00 COMPLETE",small);
  }else if(phase==Phase.Reading){
   if(Button(453,796,535,"PUT THE NOTE AWAY    [ENTER]"))FoldNote();
  }else if(phase==Phase.Entering){
   Text(95,839,700,30,"ENTER TO SKIP",small);
  }else if(phase==Phase.Playing){
   float uiAlpha=elapsed<60||drawing?1:.55f;
   Fill(new Rect(36,32,310,152),new Color(.009f,.012f,.013f,.60f*uiAlpha));Text(55,45,310,30,"DUE THIS MORNING",small);
   for(int i=0;i<3;i++){
    float y=82+i*31;bool active=selected==i;Color c=active?ochre:ivory;
    Text(55,y,255,28,(i+1).ToString("00")+"   "+new[]{"SUSPENDED FIGURE","REACHING FIGURE","PLASTER & PEARS"}[i],small);
    Fill(new Rect(55,y+24,270,2),new Color(.6f,.6f,.5f,.17f));Fill(new Rect(55,y+24,270*subjects[i].progress,2),c);
   }
   if(!drawing&&lower<.3f){
    int seen=GetObserved();Fill(new Rect(718,448,4,4),ivory);
    if(seen>=0){Text(758,421,510,35,subjects[seen].title,heading);
     Text(758,463,570,40,subjects[seen].progress>=1?"COMPLETE — IT STILL FOLLOWS":memory>0&&observation>=1.2f?"FORM REMEMBERED · HOLD SPACE":"HOLD YOUR LIGHT HERE",small);
    }
    for(int i=0;i<3;i++){
     if(subjects[i].progress>=1)continue;if(tutorial<2&&i!=0)continue;
     Vector3 p=eye.WorldToViewportPoint(subjects[i].focus.position+Vector3.up*.6f);
     if(p.z<=0||p.x<.04f||p.x>.96f||p.y<.15f||p.y>.9f)continue;
     if(Physics.Linecast(eye.transform.position,subjects[i].focus.position,1<<8))continue;
     float x=p.x*1440,y=(1-p.y)*900;Fill(new Rect(x-17,y-17,34,30),new Color(.01f,.02f,.02f,.82f));Text(x-10,y-13,60,30,(i+1).ToString("00"),small);
     if(tutorial==0)Text(x+27,y-14,420,35,"LOOK HERE · TEACHING SKELETON",small);
    }
   }
   if(drawing&&selected>=0){
    Fill(new Rect(1030,545,370,220),new Color(.01f,.012f,.012f,.86f));Text(1051,563,330,35,"STUDY "+(selected+1).ToString("00"),heading);
    Text(1051,609,330,38,artwork.Stage(subjects[selected].progress)+"   "+Mathf.FloorToInt(subjects[selected].progress*100)+"%",label);
    Fill(new Rect(1051,655,325,7),new Color(.6f,.6f,.5f,.2f));Fill(new Rect(1051,655,325*subjects[selected].progress,7),ochre);
    Text(1051,681,330,30,"MEMORY   "+memory.ToString("0.0")+" s",small);
    for(int j=0;j<5;j++)Fill(new Rect(1051+j*66,719,56,5),j<memory?ochre:new Color(.5f,.5f,.45f,.2f));
   }
   string hint=tutorial==0?"01   AIM YOUR LIGHT AT THE SKELETON AND KEEP LOOKING":drawing?(memory>0?"HOLD SPACE TO KEEP DRAWING · RELEASE TO LOOK UP":"MEMORY EMPTY — RELEASE SPACE AND OBSERVE AGAIN"):selected>=0&&subjects[selected].progress>=1?"STUDY COMPLETE · FIND THE NEXT NUMBERED SUBJECT":memory>0?"FORM REMEMBERED — HOLD SPACE TO DRAW":"KEEP LOOKING AT A SUBJECT TO REMEMBER ITS FORM";
   Fill(new Rect(250,811,940,52),new Color(.009f,.01f,.011f,.82f));Text(272,826,940,36,hint,small);
   if(tutorial==2&&!drawing)Text(440,750,900,40,"Listen. The wheels move when you lower your eyes.",serifSmall);
  }else if(phase==Phase.Paused){
   Fill(new Rect(0,0,1440,900),new Color(.01f,.02f,.02f,.85f));Text(90,170,1000,120,"Hold your breath.",large);
   Key(94,350,"WASD","Move through the studio");Key(94,398,"MOUSE","Aim your headlamp");Key(94,446,"SPACE","Hold to draw · Release to look up");
   if(Button(94,558,345,"RETURN TO THE ROOM [ESC]"))StartStudy();if(Button(94,631,345,"START AGAIN"))SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
  }else if(phase==Phase.Lost){
   float t=Time.time-endTime;Fill(new Rect(0,0,1440,900),new Color(.006f,.01f,.01f,black));
   if(t>1.8f){Text(90,95,800,35,"THE WHEELS HAVE STOPPED.",small);Text(83,245,900,210,"An unfinished\nstudy.",large);Text(92,490,700,70,"You left something out of the light.",serifSmall);
    Matrix4x4 m=GUI.matrix;GUIUtility.RotateAroundPivot(-8,new Vector2(1080,460));Card(900,170,Mathf.Max(0,selected),false,1.35f);GUI.matrix=m;
    if(Button(94,620,330,"TRY AGAIN     [R]"))SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);if(Button(94,689,330,"LEAVE THE STUDIO"))Application.Quit();}
  }else if(phase==Phase.Won){
   float t=Time.time-endTime;if(t>3.0f){float fade=Mathf.Clamp01(t-3);Fill(new Rect(0,0,1440,900),new Color(.014f,.024f,.024f,.9f*fade));
    Text(92,65,1200,110,"Nothing left to draw.",large);Text(99,170,1000,45,"In the light, they were only bones.",serifSmall);
    for(int i=0;i<3;i++)Card(210+i*365,260+(1-fade)*120,i,true,1.08f);
    Text(210,640,1040,50,"03 / 03 STUDIES COMPLETE                                      "+TimeSpan.FromSeconds(elapsed).ToString(@"mm\:ss"),small);
    if(Button(375,750,310,"RETURN TO THE STUDIO"))SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);if(Button(730,750,290,"LEAVE"))Application.Quit();}
  }
 }

}
