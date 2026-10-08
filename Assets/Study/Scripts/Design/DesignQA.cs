#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class DesignQA:MonoBehaviour {
 readonly List<string> results=new List<string>();DesignPrototype g;
 void Check(bool ok,string description){results.Add((ok?"PASS ":"FAIL ")+description);File.WriteAllLines("DesignQA-output/results.txt",results);}
 void Place(Vector3 p){g.player.enabled=false;g.player.transform.position=p;g.player.enabled=true;Physics.SyncTransforms();}
 void AtStation(int i){Place(g.stations[i]+Vector3.back*1.2f);g.moving=false;g.SetCrouch(false);Aim();}
 void Aim(){g.eye.transform.LookAt(g.WorkTarget);}
 void Teacher(Vector3 p,Vector3 forward){g.teacher.enabled=false;g.teacher.transform.position=p;g.teacher.transform.rotation=Quaternion.LookRotation(forward);g.teacher.mode=DesignSpider.Mode.Patrol;}
 IEnumerator Shot(string name){yield return new WaitForEndOfFrame();var tex=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes("DesignQA-output/"+name+".png",tex.EncodeToPNG());Destroy(tex);}
 IEnumerator Start(){
  DontDestroyOnLoad(gameObject);Directory.CreateDirectory("DesignQA-output");File.WriteAllText("DesignQA-output/results.txt","RUNNING\n");Application.runInBackground=true;
  yield return new WaitForSeconds(.6f);yield return Shot("01-Assignment-menu");
  g=DesignPrototype.Launch();g.automation=true;yield return null;
  Check(g.phase==DesignPrototype.Phase.Brief,"Design accessible without Drawing completion");yield return Shot("02-Design-brief");
  g.Begin();g.teacher.enabled=false;yield return null;
  Check(g.tables.Count==3&&g.teacher.head&&g.workshop,"Cover, professor and physical workshop created");
  Check(Resources.Load<GameObject>("Professor/Head")&&Resources.Load<GameObject>("Professor/Body")&&g.teacher.head.GetComponentsInChildren<MeshRenderer>().Length>=6,"Blender-authored professor assets load in the runtime scene");
  var skinMaterial=Resources.Load<Material>("Professor/Skin");Check(skinMaterial&&skinMaterial.mainTexture&&skinMaterial.GetTexture("_BumpMap"),"Professor skin uses imported color and normal maps");
  Place(g.tables[0]);g.SetCrouch(true);g.SetCrouch(false);Check(g.crouching,"Cannot stand through lowered table");
  g.player.Move(Vector3.back*3);Check(g.player.transform.position.z<g.tables[0].z-1.3f,"Crouched player exits table opening");g.SetCrouch(false);Check(!g.crouching,"Can stand after exit");
  AtStation(0);Check(g.CanWork(),"Close aligned cutting station is usable");
  g.eye.transform.rotation=Quaternion.LookRotation(Vector3.back);Check(!g.CanWork(),"Back-facing interaction rejected");
  g.Work(1);Check(g.progress[0]==0,"Back-facing work cannot advance progress");
  Aim();g.moving=true;g.Work(1);Check(g.progress[0]==0&&!g.CanWork(),"Movement interrupts work immediately");g.moving=false;
  Place(g.stations[0]+Vector3.back*2);Aim();Check(!g.CanWork(),"Old remote 2m interaction rejected");
  AtStation(0);g.Work(1.5f);float saved=g.progress[0];Check(saved>0,"Partial cutting advances");
  Place(new Vector3(0,0,-11));g.Work(3);Check(g.progress[0]==saved,"Leaving retains progress but blocks remote work");
  AtStation(0);g.phase=DesignPrototype.Phase.Paused;g.Work(3);Check(g.progress[0]==saved,"Pause blocks production");g.phase=DesignPrototype.Phase.Playing;
  g.Work(10);Check(g.stage==0&&Mathf.Abs(g.progress[0]-1f/3)<.001f&&g.workshop.NeedsRelease,"One cut completes one physical action");
  g.Work(20);Check(Mathf.Abs(g.progress[0]-1f/3)<.001f,"Holding through a cut cannot skip remaining actions");
  Check(g.transform.position==Vector3.zero,"Work audio does not move the scene root");
  for(int i=0;i<3;i++){
   AtStation(i);int guard=0;while(g.stage==i&&guard++<4){g.ReleaseWork();Aim();g.Work(StudyTuning.DesignSeconds[i]);}
   Check(g.stage==i+1,"Ordered workstation "+i+" completes after its distinct actions");
  }
  AtStation(3);g.ReleaseWork();g.Work(.1f);Check(g.phase==DesignPrototype.Phase.Complete,"Finished work submits and ends pursuit");yield return Shot("03-Accepted");
  g.phase=DesignPrototype.Phase.Playing;g.stage=0;g.progress=new float[3];g.ReleaseWork();g.elapsed=6;
  g.stamina=4;g.TickStamina(4,true);Check(g.stamina==0,"Four seconds depletes sprint");
  Check(!g.TickStamina(.1f,true)&&g.stamina==0,"Exhaustion does not flicker sprint on next frame");g.TickStamina(.3f,false);Check(g.stamina==0,"Stamina waits before regeneration");g.TickStamina(1.2f,false);Check(g.stamina>.8f&&g.TickStamina(.1f,true),"Recovered stamina permits sprint again");
  Place(new Vector3(0,0,-5));Teacher(new Vector3(0,4.7f,0),Vector3.back);Check(g.teacher.SeesPlayer(),"Front-facing player visible");
  g.teacher.transform.rotation=Quaternion.LookRotation(Vector3.forward);Check(!g.teacher.SeesPlayer(),"Player behind teacher is not automatically detected");
  Place(new Vector3(0,0,-12));g.teacher.transform.rotation=Quaternion.LookRotation(Vector3.back);Check(!g.teacher.SeesPlayer(),"Sight range limited");
  Place(new Vector3(-5,0,3));Teacher(new Vector3(-1,4.7f,3),Vector3.left);Check(!g.teacher.SeesPlayer(),"Tall screen blocks line of sight");
  g.teacher.Hear(new Vector3(100,0,100));Check(g.teacher.mode==DesignSpider.Mode.Patrol,"Distant sound does not reveal a target");
  g.teacher.Hear(new Vector3(-5,0,3));Check(g.teacher.mode==DesignSpider.Mode.Search&&g.teacher.lastSeen==new Vector3(-5,0,3),"Nearby sound investigates source rather than live player");
  var route=DesignNavigation.Path(new Vector3(-5,4.7f,3),new Vector3(-1,4.7f,3));bool clear=route.Count>6;foreach(var p in route)if(Physics.CheckBox(p,new Vector3(.6f,.3f,.6f),Quaternion.identity,DesignPrototype.SolidMask))clear=false;Check(clear,"Ceiling route clears full-height screens");
  Place(new Vector3(0,0,-8));Teacher(new Vector3(0,4.7f,0),Vector3.back);g.teacher.enabled=true;yield return new WaitForSeconds(.4f);
  Check(g.teacher.mode==DesignSpider.Mode.Warning&&g.teacher.WarningRemaining>1.2f,"First detection supplies extended warning");
  yield return new WaitForSeconds(1.85f);Check(g.teacher.mode==DesignSpider.Mode.Chase,"Visible target enters chase");Vector3 before=g.teacher.transform.position;yield return new WaitForSeconds(.2f);Check(Vector3.Distance(before,g.teacher.transform.position)>.4f,"Chase moves at the intended fast pace");
  g.phase=DesignPrototype.Phase.Paused;before=g.teacher.transform.position;yield return new WaitForSeconds(.3f);Check(Vector3.Distance(before,g.teacher.transform.position)<.01f,"Pause freezes pursuit");g.phase=DesignPrototype.Phase.Playing;
  Place(new Vector3(-1,0,0));Teacher(new Vector3(-1,4.7f,3),Vector3.back);g.teacher.enabled=true;yield return new WaitForSeconds(.4f);
  Check(g.teacher.mode==DesignSpider.Mode.Warning,"Second encounter warns again");Vector3 known=g.teacher.lastSeen;Place(new Vector3(-6,0,3));yield return new WaitForSeconds(1.3f);
  Check(g.teacher.mode==DesignSpider.Mode.Search&&Vector3.Distance(g.teacher.lastSeen,known)<.1f,"Breaking sight during warning preserves last known position");
  Place(g.tables[0]);g.SetCrouch(true);Teacher(g.tables[0]+Vector3.up*4.7f,Vector3.back);g.teacher.ForceChase();g.teacher.enabled=true;yield return null;
  Check(g.teacher.mode==DesignSpider.Mode.Probe,"Table gives a committed probe instead of immediate capture");
  g.eye.transform.LookAt(g.tables[0]+new Vector3(0,.7f,-1.5f));yield return new WaitForSeconds(1);yield return Shot("04-Under-table-probe");
  var neckMesh=g.teacher.GetComponentInChildren<ProfessorNeck>().GetComponent<MeshFilter>().sharedMesh;
  Check(neckMesh.bounds.size.sqrMagnitude>1&&neckMesh.bounds.size.sqrMagnitude<150,"Continuous neck has valid bounds during table probe");
  Place(g.tables[0]+Vector3.back*4);g.SetCrouch(false);yield return new WaitForSeconds(1.9f);Check(g.phase==DesignPrototype.Phase.Playing&&g.teacher.mode==DesignSpider.Mode.Recover,"Leaving opposite side evades committed attack");
  Place(new Vector3(0,0,-5));Teacher(new Vector3(0,4.7f,-5),Vector3.back);g.teacher.ForceChase();g.teacher.enabled=true;yield return new WaitForSeconds(2.5f);Check(g.phase==DesignPrototype.Phase.Caught,"Remaining in visible reach is captured");yield return Shot("05-Caught");
  g=DesignPrototype.Launch();g.automation=true;yield return null;Check(g.stage==0&&g.phase==DesignPrototype.Phase.Brief,"Retry resets work and returns to briefing");
  g.Begin();g.teacher.enabled=false;Place(new Vector3(0,0,-8));g.eye.transform.LookAt(new Vector3(0,2.8f,4));g.teacher.transform.position=new Vector3(0,4.7f,-1);g.teacher.head.LookAt(g.eye.transform);yield return Shot("06-Classroom");
  AtStation(0);g.working=true;yield return Shot("07-Physical-work");g.working=false;
  g.ReturnToMenu();yield return new WaitForSeconds(.8f);var drawing=FindFirstObjectByType<StudyGame>();Check(drawing&&drawing.phase==StudyGame.Phase.Title,"Return restores assignment menu");
  if(drawing){drawing.automation=true;drawing.StartStudy();yield return null;Check(drawing.phase==StudyGame.Phase.Playing,"Drawing still starts after Design");}
  results.Add("DONE");File.WriteAllLines("DesignQA-output/results.txt",results);UnityEditor.EditorApplication.isPlaying=false;
 }
}
#endif
