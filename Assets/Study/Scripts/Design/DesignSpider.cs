using System.Collections.Generic;
using UnityEngine;
public class DesignSpider:MonoBehaviour {
 public enum Mode { Patrol, Warning, Chase, Search, Probe, Recover }
 public Mode mode;public Transform head;public Vector3 lastSeen;public bool IsThreat=>mode!=Mode.Patrol&&mode!=Mode.Recover;
 public string Status=>mode==Mode.Warning?"HE SAW YOU — RUN":mode==Mode.Probe?"MOVE. HE'S LOOKING UNDER.":mode==Mode.Search?"BREAK SIGHT. KEEP MOVING.":"YOU'RE NEXT.";
 DesignPrototype g;Transform neck,neckB,neckC,tie;bool probingTable;readonly List<Transform> limbs=new List<Transform>();
 List<Vector3> path;int node,patrol;float timer,lost,repath,step,clock;Vector3 probeStart,probeTarget;AudioSource sound;AudioClip tick,alert;
 readonly Vector3[] patrolPoints={new Vector3(-7,4.7f,7),new Vector3(7,4.7f,9),new Vector3(7,4.7f,-9),new Vector3(-7,4.7f,-9)};
 public void Initialize(DesignPrototype game,Material skin,Material suit,Material shirt){
  g=game;var root=transform;
  DesignPrototype.Shape("Enormous tailored abdomen",root,PrimitiveType.Sphere,new Vector3(0,.05f,.45f),new Vector3(1.9f,.95f,2.1f),suit);
  DesignPrototype.Shape("Suit jacket",root,PrimitiveType.Cube,new Vector3(0,-.25f,-.25f),new Vector3(1.15f,.6f,1.35f),suit);
  DesignPrototype.Shape("White shirt",root,PrimitiveType.Cube,new Vector3(0,-.57f,-.5f),new Vector3(.43f,.06f,.85f),shirt);
  tie=DesignPrototype.Shape("Hanging tie",root,PrimitiveType.Cube,new Vector3(0,-.95f,-.4f),new Vector3(.13f,.75f,.07f),suit).transform;
  neck=DesignPrototype.Shape("Extending neck",null,PrimitiveType.Cylinder,Vector3.zero,Vector3.one,skin).transform;neck.SetParent(root,true);
  neckB=DesignPrototype.Shape("Neck / bend",root,PrimitiveType.Cylinder,Vector3.zero,Vector3.one,skin).transform;
  neckC=DesignPrototype.Shape("Neck / reach",root,PrimitiveType.Cylinder,Vector3.zero,Vector3.one,skin).transform;
  head=new GameObject("Bald professor head").transform;head.SetParent(root);head.position=root.position+Vector3.down;
  DesignPrototype.Shape("Bald scalp",head,PrimitiveType.Sphere,Vector3.zero,new Vector3(.65f,1.05f,.64f),skin);
  DesignPrototype.Shape("Face",head,PrimitiveType.Sphere,new Vector3(0,-.15f,.18f),new Vector3(.49f,.8f,.46f),skin);
  DesignPrototype.Shape("Nose",head,PrimitiveType.Sphere,new Vector3(0,-.02f,.47f),new Vector3(.13f,.22f,.2f),skin);
  for(int side=-1;side<=1;side+=2){
   DesignPrototype.Shape("Thin hair at temples",head,PrimitiveType.Sphere,new Vector3(side*.3f,.13f,-.02f),new Vector3(.12f,.35f,.48f),suit);
   DesignPrototype.Shape("Ear",head,PrimitiveType.Sphere,new Vector3(side*.38f,-.03f,0),new Vector3(.13f,.23f,.13f),skin);
   for(int rim=0;rim<16;rim++){
    float t=rim*Mathf.PI*2/16,u=(rim+1)*Mathf.PI*2/16;
    Vector3 a=new Vector3(side*.17f+Mathf.Cos(t)*.145f,.03f+Mathf.Sin(t)*.105f,.46f),b=new Vector3(side*.17f+Mathf.Cos(u)*.145f,.03f+Mathf.Sin(u)*.105f,.46f);
    var r=DesignPrototype.Shape("Round spectacle rim",head,PrimitiveType.Cylinder,(a+b)*.5f,new Vector3(.018f,(b-a).magnitude*.5f,.018f),suit);r.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);
   }
   DesignPrototype.Shape("Deep eye socket",head,PrimitiveType.Sphere,new Vector3(side*.17f,.025f,.416f),new Vector3(.24f,.15f,.025f),suit);
   DesignPrototype.Shape("Small reflected eye",head,PrimitiveType.Sphere,new Vector3(side*.17f,.02f,.44f),new Vector3(.042f,.032f,.02f),shirt);
  }
  DesignPrototype.Shape("Glasses bridge",head,PrimitiveType.Cube,new Vector3(0,.04f,.46f),new Vector3(.12f,.04f,.04f),suit);
  DesignPrototype.Shape("Disapproving mouth",head,PrimitiveType.Cube,new Vector3(0,-.36f,.40f),new Vector3(.17f,.025f,.035f),suit);
  for(int i=0;i<3;i++)DesignPrototype.Shape("Forehead crease",head,PrimitiveType.Cube,new Vector3(0,.24f+i*.05f,.29f-i*.01f),new Vector3(.23f-i*.025f,.008f,.012f),suit);
  for(int side=-1;side<=1;side+=2){var lapel=DesignPrototype.Shape("Jacket lapel",root,PrimitiveType.Cube,new Vector3(side*.32f,-.6f,-.45f),new Vector3(.23f,.06f,.85f),suit);lapel.transform.localRotation=Quaternion.Euler(0,side*22,0);}
  for(int i=0;i<16;i++)limbs.Add(DesignPrototype.Shape("Articulated suit limb "+i,root,PrimitiveType.Cylinder,Vector3.zero,Vector3.one,suit).transform);
  sound=gameObject.AddComponent<AudioSource>();sound.spatialBlend=1;sound.minDistance=2;sound.maxDistance=28;tick=DesignPrototype.Tone(75,.16f);alert=DesignPrototype.Tone(260,.65f);
  lastSeen=g.player.transform.position;Animate();
 }
 void Segment(Transform t,Vector3 a,Vector3 b,float width){t.position=(a+b)/2;t.rotation=Quaternion.FromToRotation(Vector3.up,b-a);t.localScale=new Vector3(width,(b-a).magnitude*.5f,width);}
 public bool SeesPlayer(){Vector3 target=g.eye.transform.position;return Vector3.Distance(transform.position,target)<12&&!Physics.Linecast(transform.position+Vector3.down*.45f,target,DesignPrototype.SolidMask);}
 public void Hear(Vector3 point){if(mode==Mode.Patrol||mode==Mode.Search){lastSeen=point;mode=Mode.Search;timer=5;repath=0;}}
 public void ForceChase(){lastSeen=g.player.transform.position;mode=Mode.Chase;lost=0;repath=0;}
 void Warn(){mode=Mode.Warning;timer=1.05f;lastSeen=g.player.transform.position;sound.PlayOneShot(alert,1);g.Notice("\"You're next.\"",1.6f);}
 void Move(Vector3 target,float speed){
  repath-=Time.deltaTime;if(repath<=0){path=DesignNavigation.Path(transform.position,target);node=0;repath=.55f;}
  if(path==null||node>=path.Count)return;
  Vector3 dest=path[node];if(Vector3.Distance(transform.position,dest)<.12f){node++;return;}
  transform.position=Vector3.MoveTowards(transform.position,dest,speed*Time.deltaTime);
  step+=Time.deltaTime*speed;if(step>1.3f){step=0;sound.PlayOneShot(tick,.7f);}
 }
 float Horizontal(Vector3 a,Vector3 b){a.y=0;b.y=0;return Vector3.Distance(a,b);}
 void StartProbe(){
  mode=Mode.Probe;timer=2.65f;probeTarget=g.eye.transform.position;int table=g.UnderTable();probingTable=table>=0;
  if(table>=0){Vector3 center=g.tables[table];float sign=g.player.transform.position.z>center.z?1:-1;probeStart=center+new Vector3(0,.65f,sign*1.65f);}
  else probeStart=probeTarget+Vector3.up*1.6f;
  g.Notice(table>=0?"His head is coming down. Get out the other side!":"He's reaching — move!",1.5f);
 }
 void Update(){
  if(!g||g.phase!=DesignPrototype.Phase.Playing)return;clock+=Time.deltaTime;bool sees=SeesPlayer();timer-=Time.deltaTime;
  switch(mode){
   case Mode.Patrol:
    Move(patrolPoints[patrol],2.3f);if(Horizontal(transform.position,patrolPoints[patrol])<.5f)patrol=(patrol+1)%patrolPoints.Length;
    if(sees&&g.elapsed>3)Warn();break;
   case Mode.Warning: if(sees)lastSeen=g.player.transform.position;if(timer<=0)ForceChase();break;
   case Mode.Chase:
    if(sees){lastSeen=g.player.transform.position;lost=0;}else lost+=Time.deltaTime;
    Move(lastSeen,5.1f);
    if(Horizontal(transform.position,g.player.transform.position)<2.1f&&(sees||g.UnderTable()>=0))StartProbe();
    else if(lost>2.2f){mode=Mode.Search;timer=4;repath=0;}break;
   case Mode.Search:
    Move(lastSeen,3.1f);if(sees)Warn();else if(timer<=0){mode=Mode.Recover;timer=5;g.Notice("He lost you. Get back to the assignment.",3);}break;
   case Mode.Probe:
    // Commit to the last position: a dodge succeeds instead of the neck tracking perfectly.
    if(timer<1.05f&&Vector3.Distance(head.position,g.eye.transform.position)<.78f&&!Physics.Linecast(head.position,g.eye.transform.position,DesignPrototype.SolidMask))g.Catch();
    if(timer<=0){mode=Mode.Recover;timer=3.2f;g.Notice("He missed. Move while he pulls back.",2);}break;
   case Mode.Recover: if(timer<=0){mode=Mode.Patrol;repath=0;}break;
  }
  Animate();
 }
 void Animate(){
  Vector3 rest=transform.position+new Vector3(0,-1,-.5f),target=rest;
  if(mode==Mode.Probe){float t=2.65f-timer;target=t<1?Vector3.Lerp(rest,probeStart,t):Vector3.Lerp(probeStart,probeTarget,Mathf.Clamp01((t-1)/1.1f));}
  head.position=Vector3.MoveTowards(head.position,target,Time.deltaTime*8+ .001f);Vector3 look=g.eye.transform.position-head.position;if(look.sqrMagnitude>.01f)head.rotation=Quaternion.LookRotation(look);
  bool bent=mode==Mode.Probe&&probingTable;
  neckB.gameObject.SetActive(bent);neckC.gameObject.SetActive(bent);
  if(bent){Vector3 above=probeStart+Vector3.up*1.4f;Segment(neck,transform.position+Vector3.down*.3f,above,.22f);Segment(neckB,above,probeStart,.21f);Segment(neckC,probeStart,head.position,.2f);}
  else Segment(neck,transform.position+Vector3.down*.3f,head.position,.22f);
  tie.localRotation=Quaternion.Euler(Mathf.Sin(clock*3)*12,0,Mathf.Sin(clock*2)*9);
  for(int i=0;i<8;i++){
   float side=i<4?-1:1;int k=i%4;float z=(k-1.5f)*.65f;float gait=Mathf.Sin(clock*(IsThreat?11:5)+k*2+side);
   Vector3 a=transform.position+new Vector3(side*.6f,0,z),b=transform.position+new Vector3(side*(1.6f+gait*.13f),.3f,z*1.7f),c=transform.position+new Vector3(side*2.2f,.72f,z*2+gait*.2f);
   Segment(limbs[i*2],a,b,.13f);Segment(limbs[i*2+1],b,c,.085f);
  }
 }
}
