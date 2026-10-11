using System.Collections.Generic;
using UnityEngine;
public class DesignSpider:MonoBehaviour {
 public enum Mode { Patrol, Warning, Chase, Search, Probe, Recover }
 public Mode mode;public Transform head;public Vector3 lastSeen;public bool IsThreat=>mode!=Mode.Patrol&&mode!=Mode.Recover;
 public string Status=>mode==Mode.Warning?"HE SAW YOU — RUN":mode==Mode.Probe?"MOVE. HE'S LOOKING UNDER.":mode==Mode.Search?"HE'S SEARCHING.":"YOU'RE NEXT.";
 DesignPrototype g;ProfessorNeck neck;Transform tie;ProfessorLimbs limbArt;bool probingTable;readonly List<Transform> limbs=new List<Transform>();
 List<Vector3> path;int node,patrol;float timer,lost,repath,step,clock,suspicion,retreatClock;bool warnedOnce,visible;
 Vector3 probeStart,probeTarget,travelDirection=Vector3.back,searchForward=Vector3.back,inspectionPoint;AudioSource sound;AudioClip tick,alert,stretch;
 readonly Vector3[] feet=new Vector3[8],footStart=new Vector3[8],footGoal=new Vector3[8];readonly float[] footPhase=new float[8];
 public Vector3 Facing=>transform.forward; public float WarningRemaining=>mode==Mode.Warning?timer:0;
 public bool HasVisualContact=>visible;
 readonly Vector3[] patrolPoints={new Vector3(-7,4.7f,7),new Vector3(7,4.7f,9),new Vector3(7,4.7f,-9),new Vector3(-7,4.7f,-9)};
 public void Initialize(DesignPrototype game,Material skin,Material suit,Material shirt){
  g=game;var root=transform;root.rotation=Quaternion.LookRotation(Vector3.back);
  var bodyAsset=Resources.Load<GameObject>("Professor/Body");
  if(!bodyAsset)throw new System.InvalidOperationException("Professor body asset missing. Import Professor Artwork first.");
  Instantiate(bodyAsset,root,false);
  Material wool=Resources.Load<Material>("Professor/Wool"),flesh=Resources.Load<Material>("Professor/SkinDetail");
  var neckObject=new GameObject("Professor / continuous skin neck");neckObject.transform.SetParent(root,false);neck=neckObject.AddComponent<ProfessorNeck>();neck.Initialize(flesh);
  tie=DesignPrototype.Shape("Hanging tie",root,PrimitiveType.Cube,new Vector3(0,-.78f,.6f),new Vector3(.10f,.49f,.045f),Resources.Load<Material>("Professor/Tie")??wool).transform;
  head=new GameObject("Bald professor head").transform;head.SetParent(root);head.position=root.position+Vector3.down;DesignFace.Build(head,flesh);
  var art=gameObject.AddComponent<ProfessorLimbs>();limbArt=art;
  for(int i=0;i<16;i++)limbs.Add(art.Segment(root,i,wool));
  sound=gameObject.AddComponent<AudioSource>();sound.spatialBlend=1;sound.minDistance=2;sound.maxDistance=28;tick=StudioAudio.Foot();alert=DesignPrototype.Tone(260,.65f);stretch=StudioAudio.Neck();
  lastSeen=transform.position+Vector3.back*4;for(int i=0;i<8;i++){feet[i]=FootHome(i);footGoal[i]=feet[i];footPhase[i]=1;}Animate();
 }
 void Segment(Transform t,Vector3 a,Vector3 b,float width){t.position=(a+b)/2;t.rotation=Quaternion.FromToRotation(Vector3.up,b-a);t.localScale=new Vector3(width,(b-a).magnitude*.5f,width);}
 public bool SeesPlayer(){
  Vector3 target=g.eye.transform.position,delta=target-transform.position,flat=delta;flat.y=0;
  bool close=flat.magnitude<1.5f;
  return delta.magnitude<StudyTuning.SightRange&&(close||Vector3.Angle(Facing,flat)<StudyTuning.SightHalfAngle)&&!Physics.Linecast(transform.position+Vector3.down*.45f,target,DesignPrototype.SolidMask);
 }
 public void Hear(Vector3 point,float radius=StudyTuning.HearingRange){
  if(Horizontal(transform.position,point)>radius)return;
  if(mode==Mode.Patrol||mode==Mode.Search){lastSeen=point;mode=Mode.Search;timer=4.5f;repath=0;travelDirection=Flat(point-transform.position);searchForward=travelDirection;}
 }
 // Explicit QA/debug entry. Production warning transitions preserve the last visible position.
 public void ForceChase(){lastSeen=g.player.transform.position;EnterChase();}
 void EnterChase(){mode=Mode.Chase;lost=0;repath=0;}
 void Warn(){mode=Mode.Warning;timer=warnedOnce?StudyTuning.Warning:StudyTuning.FirstWarning;warnedOnce=true;lastSeen=g.player.transform.position;travelDirection=Flat(lastSeen-transform.position);sound.PlayOneShot(alert,.75f);g.Notice("He noticed you. Find a screen.",timer);}
 Vector3 Flat(Vector3 v){v.y=0;return v.sqrMagnitude>.001f?v.normalized:Facing;}
 void Move(Vector3 target,float speed){
  repath-=Time.deltaTime;if(repath<=0){path=DesignNavigation.Path(transform.position,target);node=0;repath=.4f;}
  if(path==null||node>=path.Count)return;
  Vector3 dest=path[node];if(Vector3.Distance(transform.position,dest)<.12f){node++;return;}
  travelDirection=Flat(dest-transform.position);transform.position=Vector3.MoveTowards(transform.position,dest,speed*Time.deltaTime);
  step+=Time.deltaTime*speed;if(step>1.7f){step=0;sound.PlayOneShot(tick,.65f);}
 }
 float Horizontal(Vector3 a,Vector3 b){a.y=0;b.y=0;return Vector3.Distance(a,b);}
 void StartProbe(){
  mode=Mode.Probe;timer=2.65f;probeTarget=visible?g.eye.transform.position:lastSeen+Vector3.up*.68f;int table=-1;for(int i=0;i<g.tables.Count;i++)if(Horizontal(probeTarget,g.tables[i])<1.25f){table=i;break;}probingTable=table>=0;
  if(table>=0){Vector3 center=g.tables[table];float sign=probeTarget.z>center.z?1:-1;probeStart=center+new Vector3(0,.65f,sign*1.65f);}
  else probeStart=probeTarget+Vector3.up*1.6f;
  sound.PlayOneShot(stretch,.85f);g.Notice(table>=0?"His head is coming down. Get out the other side!":"He's reaching — move!",1.5f);
 }
 void Update(){
  if(!g)return;
  if(g.phase==DesignPrototype.Phase.Complete){clock+=Time.deltaTime;travelDirection=Flat(g.stations[3]-transform.position);Animate();return;}
  if(g.phase!=DesignPrototype.Phase.Playing)return;
  clock+=Time.deltaTime;visible=SeesPlayer();timer-=Time.deltaTime;
  switch(mode){
   case Mode.Patrol:
    Move(patrolPoints[patrol],2.3f);if(Horizontal(transform.position,patrolPoints[patrol])<.5f)patrol=(patrol+1)%patrolPoints.Length;
    suspicion=visible?suspicion+Time.deltaTime:0;if(suspicion>.25f&&g.elapsed>4)Warn();break;
   case Mode.Warning:
    if(visible){lastSeen=g.player.transform.position;travelDirection=Flat(lastSeen-transform.position);}
    if(timer<=0){if(visible)EnterChase();else{mode=Mode.Search;timer=4;repath=0;searchForward=Facing;}}break;
   case Mode.Chase:
    if(visible){lastSeen=g.player.transform.position;lost=0;}else lost+=Time.deltaTime;
    Move(lastSeen,StudyTuning.ChaseSpeed);
    // Cover checks use known position: hiding elsewhere is never exposed by UnderTable().
    if(Horizontal(transform.position,lastSeen)<2.1f&&(visible||(lost<1.4f&&g.tables.Exists(t=>Horizontal(t,lastSeen)<1.25f))))StartProbe();
    else if(lost>2.2f){mode=Mode.Search;timer=4;repath=0;searchForward=Facing;}break;
   case Mode.Search:
    if(Horizontal(transform.position,lastSeen)>1)Move(lastSeen,3.1f);
    else travelDirection=Quaternion.Euler(0,Mathf.Sin(clock*1.8f)*55,0)*searchForward;
    if(visible)Warn();else if(timer<=0){mode=Mode.Recover;timer=6;retreatClock=0;inspectionPoint=patrolPoints[patrol];patrol=(patrol+1)%patrolPoints.Length;g.Notice("His footsteps are moving away.",2);}break;
   case Mode.Probe:
    if(timer<.85f&&timer>.12f&&Vector3.Distance(head.position,g.eye.transform.position)<StudyTuning.CaptureRadius&&!Physics.Linecast(head.position,g.eye.transform.position,DesignPrototype.SolidMask))g.Catch();
    if(timer<=0){mode=Mode.Recover;timer=4.8f;retreatClock=0;inspectionPoint=transform.position+Flat(probeTarget-transform.position)*3;repath=0;g.Notice("He missed. Get back to your work.",2);}break;
   case Mode.Recover:
    retreatClock+=Time.deltaTime;
    if(retreatClock>1.15f)Move(inspectionPoint,2.3f);
    // Close, visible approach is still dangerous; breathing room comes from looking elsewhere.
    if(retreatClock>1.5f&&visible&&Horizontal(transform.position,g.player.transform.position)<3)Warn();
    else if(timer<=0){mode=Mode.Patrol;suspicion=0;repath=0;}break;
  }
  Animate();
 }
 Vector3 FootHome(int i){float side=i<4?-1:1;float z=(i%4-1.5f)*1.15f;return transform.position+transform.rotation*new Vector3(side*2.1f,.78f,z);}
 public void RefreshArtPose(){head.position=transform.position+Vector3.down+Facing*.48f;head.rotation=Quaternion.LookRotation(Facing);for(int i=0;i<8;i++){feet[i]=FootHome(i);footPhase[i]=1;}Animate();}
 void Animate(){
  transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(travelDirection),Time.deltaTime*(mode==Mode.Warning?5:3));
  Vector3 rest=transform.position+Vector3.down+Facing*.48f,target=rest;
  if(mode==Mode.Probe){float t=2.65f-timer;target=t<1?Vector3.Lerp(rest,probeStart,t):Vector3.Lerp(probeStart,probeTarget,Mathf.Clamp01((t-1)/1.1f));}
  head.position=Vector3.MoveTowards(head.position,target,Time.deltaTime*8+ .001f);Vector3 gaze=g.phase==DesignPrototype.Phase.Complete?g.stations[3]+Vector3.up*1.4f:mode==Mode.Probe?probeTarget:(visible&&(mode==Mode.Warning||mode==Mode.Chase)?g.eye.transform.position:transform.position+Facing*5+Vector3.down*2.1f);
  Vector3 look=gaze-head.position;if(look.sqrMagnitude>.01f)head.rotation=Quaternion.Slerp(head.rotation,Quaternion.LookRotation(look),Time.deltaTime*7);
  bool bent=mode==Mode.Probe&&probingTable;
  neck.Pose(transform.position+Facing*.10f+Vector3.down*.30f,probeStart+Vector3.up*1.7f,probeStart,head,bent);
  tie.localRotation=Quaternion.Euler(Mathf.Sin(clock*3)*12,0,Mathf.Sin(clock*2)*9);
  for(int i=0;i<8;i++){
   float side=i<4?-1:1;int k=i%4;Vector3 home=FootHome(i);
   bool beat=((int)(clock*(mode==Mode.Chase?5:2.7f))%2)==((i+k/2)%2);
   if(footPhase[i]>=1&&Vector3.Distance(feet[i],home)>.48f&&beat){footStart[i]=feet[i];footGoal[i]=home+Facing*.22f;footPhase[i]=0;}
   if(footPhase[i]<1){footPhase[i]=Mathf.Min(1,footPhase[i]+Time.deltaTime*(mode==Mode.Chase?9:5));feet[i]=Vector3.Lerp(footStart[i],footGoal[i],Mathf.SmoothStep(0,1,footPhase[i]))+Vector3.down*Mathf.Sin(footPhase[i]*Mathf.PI)*.16f;}
   if(Vector3.Distance(feet[i],home)>3){feet[i]=home;footPhase[i]=1;}
   Vector3 a=transform.position+transform.rotation*new Vector3(side*.6f,0,(k-1.5f)*.65f);
   Vector3 b=Vector3.Lerp(a,feet[i],.55f)+Vector3.down*.23f;
   Segment(limbs[i*2],a,b,.18f);Segment(limbs[i*2+1],b,feet[i],.13f);limbArt.PoseJoint(i,b);
  }
 }
}
