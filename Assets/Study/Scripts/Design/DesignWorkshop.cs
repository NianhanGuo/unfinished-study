using UnityEngine;
// Physical task feedback and short interruptible actions; no camera lock or quality score.
public class DesignWorkshop:MonoBehaviour {
 DesignPrototype g;Transform blade,guide,leftHand,rightHand,leftSleeve,rightSleeve,photoModel;Light flash;AudioSource audioSource;AudioClip cut,snap;
 Material paper,accent;bool needsRelease;float flashUntil;int displayedStage=-1;
 public bool NeedsRelease=>needsRelease;
 public string StepLabel=>g.stage==0?"CUT "+Mathf.Min(3,Mathf.FloorToInt(g.progress[0]*3)+1)+" / 3":g.stage==1?"PLACE "+Mathf.Min(3,Mathf.FloorToInt(g.progress[1]*3)+1)+" / 3":g.stage==2?"FRAME THE MODEL  ·  "+Mathf.RoundToInt(g.progress[2]*100)+"%":"READY";
 public string Prompt=>g.stage==0?"HOLD E — ALIGN & CUT  /  release to flee":g.stage==1?"AIM AT THE OUTLINED PIECE · HOLD E TO PLACE":g.stage==2?"CENTER THE MODEL · HOLD E TO EXPOSE":"HOLD E — SUBMIT YOUR WORK";
 public Vector3 Target(int stage){
  int id=Mathf.Clamp(stage,0,3);Vector3 p=g.stations[id]+Vector3.up*(stage==2?1.48f:1.2f);
  if(stage<2){int piece=Mathf.Min(2,Mathf.FloorToInt(g.progress[stage]*3+.0001f));p.x+=(piece-1)*.35f;}return p;
 }
 public void Initialize(DesignPrototype game,Material wood,Material metal,Material board,Material mat,Material skin){
  g=game;paper=board;accent=mat;var source=new GameObject("Workstation / audio");source.transform.SetParent(g.transform);audioSource=source.AddComponent<AudioSource>();audioSource.spatialBlend=1;audioSource.minDistance=1;audioSource.maxDistance=14;cut=StudioAudio.Cut();snap=DesignPrototype.Tone(540,.14f);
  blade=g.transform.Find("Guillotine handle");
  guide=new GameObject("Active work outline").transform;guide.SetParent(g.transform);
  var mark=new Material(Shader.Find("Standard"));mark.color=new Color(.7f,.37f,.2f);mark.EnableKeyword("_EMISSION");mark.SetColor("_EmissionColor",new Color(.18f,.055f,.015f));
  for(int i=0;i<4;i++){bool side=i<2;DesignPrototype.Shape("Registration mark",guide,PrimitiveType.Cube,new Vector3(side?(i==0?-.16f:.16f):0,0,side?0:(i==2?-.22f:.22f)),side?new Vector3(.014f,.012f,.44f):new Vector3(.32f,.012f,.014f),mark);}
  leftHand=Hand("Left working hand",skin,metal);rightHand=Hand("Right working hand",skin,metal);
  leftSleeve=DesignPrototype.Shape("Left sleeve",g.transform,PrimitiveType.Capsule,Vector3.zero,Vector3.one,metal).transform;rightSleeve=DesignPrototype.Shape("Right sleeve",g.transform,PrimitiveType.Capsule,Vector3.zero,Vector3.one,metal).transform;
  photoModel=new GameObject("Model on photo sweep").transform;photoModel.SetParent(g.transform);photoModel.position=g.stations[2]+Vector3.up*1.07f;
  DesignPrototype.Shape("Photo model plinth",photoModel,PrimitiveType.Cube,Vector3.zero,new Vector3(.85f,.05f,.55f),paper);
  for(int i=0;i<3;i++)DesignPrototype.Shape("Documented volume",photoModel,PrimitiveType.Cube,new Vector3((i-1)*.24f,.2f+i*.07f,0),new Vector3(.18f,.4f+i*.14f,.35f),paper);
  var f=new GameObject("Exposure flash");f.transform.SetParent(g.transform);f.transform.position=g.stations[2]+new Vector3(0,2,-.25f);flash=f.AddComponent<Light>();flash.range=9;flash.color=new Color(.9f,.93f,1);flash.intensity=0;
 }
 Transform Hand(string label,Material skin,Material cloth){
  var h=new GameObject(label).transform;h.SetParent(g.transform);
  DesignPrototype.Shape("Palm",h,PrimitiveType.Capsule,Vector3.zero,new Vector3(.095f,.075f,.045f),skin).transform.localRotation=Quaternion.Euler(90,0,0);
  for(int i=0;i<4;i++)DesignPrototype.Shape("Finger",h,PrimitiveType.Capsule,new Vector3((i-1.5f)*.024f,0,.095f),new Vector3(.019f,.055f,.018f),skin).transform.localRotation=Quaternion.Euler(90,0,0);
  DesignPrototype.Shape("Thumb",h,PrimitiveType.Capsule,new Vector3(-.061f,0,.025f),new Vector3(.025f,.045f,.024f),skin).transform.localRotation=Quaternion.Euler(90,-35,0);
  DesignPrototype.Shape("Sleeve cuff",h,PrimitiveType.Cube,new Vector3(0,0,-.13f),new Vector3(.12f,.065f,.13f),cloth);return h;
 }
 public void Release(){needsRelease=false;}
 public void CompleteAction(int stage,int piece){
  needsRelease=true;audioSource.transform.position=g.WorkTarget;
  if(stage==0){audioSource.PlayOneShot(cut,.9f);g.teacher.Hear(g.stations[0]);DesignPrototype.Shape("Cut board strip "+piece,g.transform,PrimitiveType.Cube,g.stations[0]+new Vector3((piece-2)*.3f,1.18f,.14f),new Vector3(.23f,.04f,.38f),paper);g.Notice("CUT "+piece+" / 3 — he may have heard that.",1.4f);}
  else if(stage==1){audioSource.PlayOneShot(snap,.3f);g.Notice("PIECE "+piece+" / 3 PLACED",1.2f);}
  else{audioSource.PlayOneShot(snap,.6f);flashUntil=Time.time+.16f;g.teacher.Hear(g.stations[2],12);}
 }
 public void Submit(){photoModel.position=g.stations[3]+Vector3.up*1.12f;}
 void LateUpdate(){
  bool active=g.phase==DesignPrototype.Phase.Playing;int stage=g.stage;
  guide.gameObject.SetActive(active&&stage<3);guide.position=Target(stage)+Vector3.up*.005f;
  if(displayedStage!=stage){displayedStage=stage;photoModel.gameObject.SetActive(stage>=2);}
  bool hands=active&&g.working;leftHand.gameObject.SetActive(hands);rightHand.gameObject.SetActive(hands);leftSleeve.gameObject.SetActive(hands);rightSleeve.gameObject.SetActive(hands);
  if(hands){Vector3 t=Target(stage);Vector3 forward=g.player.transform.forward;forward.y=0;
   leftHand.position=t-g.player.transform.right*.18f-forward*.14f;rightHand.position=t+g.player.transform.right*.16f-forward*.08f+Vector3.up*Mathf.Sin(Time.time*5)*.025f;
   leftHand.rotation=rightHand.rotation=Quaternion.LookRotation(forward);
   Sleeve(leftSleeve,g.eye.transform.TransformPoint(new Vector3(-.27f,-.36f,.12f)),leftHand.position-forward*.14f);
   Sleeve(rightSleeve,g.eye.transform.TransformPoint(new Vector3(.27f,-.36f,.12f)),rightHand.position-forward*.14f);
  }
  if(blade)blade.localRotation=Quaternion.Euler(g.working&&stage==0?Mathf.Lerp(0,60,Mathf.Repeat(g.progress[0]*3,1)):0,0,0);
  flash.intensity=Time.time<flashUntil?5:0;
 }
 void Sleeve(Transform t,Vector3 a,Vector3 b){t.position=(a+b)*.5f;t.rotation=Quaternion.FromToRotation(Vector3.up,b-a);t.localScale=new Vector3(.1f,Vector3.Distance(a,b)*.5f,.1f);}
 void OnGUI(){
  if(g.phase!=DesignPrototype.Phase.Playing||g.stage!=2||Vector3.Distance(g.player.transform.position,g.stations[2])>2)return;
  float s=Mathf.Min(Screen.width/1440f,Screen.height/900f);var old=GUI.matrix;GUI.matrix=Matrix4x4.TRS(new Vector3((Screen.width-1440*s)/2,(Screen.height-900*s)/2,0),Quaternion.identity,Vector3.one*s);
  GUI.color=g.CanWork()?new Color(.6f,.8f,.65f,.8f):new Color(.85f,.76f,.6f,.7f);
  foreach(var p in new[]{new Vector2(570,335),new Vector2(850,335),new Vector2(570,545),new Vector2(850,545)}){GUI.DrawTexture(new Rect(p.x,p.y,20,2),Texture2D.whiteTexture);GUI.DrawTexture(new Rect(p.x,p.y,2,20),Texture2D.whiteTexture);}GUI.color=Color.white;GUI.matrix=old;
 }
}
