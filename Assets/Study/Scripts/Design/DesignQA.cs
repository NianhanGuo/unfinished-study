#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class DesignQA:MonoBehaviour {
 readonly List<string> results=new List<string>();DesignPrototype g;
 void Check(bool ok,string description){results.Add((ok?"PASS ":"FAIL ")+description);File.WriteAllLines("DesignQA-output/results.txt",results);}
 void Place(Vector3 p){g.player.enabled=false;g.player.transform.position=p;g.player.enabled=true;Physics.SyncTransforms();}
 IEnumerator Shot(string name){yield return new WaitForEndOfFrame();var tex=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes("DesignQA-output/"+name+".png",tex.EncodeToPNG());Destroy(tex);}
 IEnumerator Start(){
  DontDestroyOnLoad(gameObject);Directory.CreateDirectory("DesignQA-output");Application.runInBackground=true;
  yield return new WaitForSeconds(.8f);yield return Shot("01-Assignment-menu");
  g=DesignPrototype.Launch();g.automation=true;yield return null;
  Check(g.phase==DesignPrototype.Phase.Brief,"Design accessible directly without completing Drawing");yield return Shot("02-Design-brief");
  g.Begin();g.teacher.enabled=false;yield return null;
  Check(g.tables.Count==3&&g.teacher.head!=null,"Classroom cover and professor spider created");
  Place(g.tables[0]);g.SetCrouch(true);g.SetCrouch(false);Check(g.crouching,"Cannot stand through solid worktable");
  g.player.Move(Vector3.back*3);Check(g.player.transform.position.z<g.tables[0].z-1.3f,"Player can crawl out through table opening");g.SetCrouch(false);Check(!g.crouching,"Player stands after leaving table");
  Place(new Vector3(-7,0,4.8f));g.eye.transform.LookAt(g.stations[0]+Vector3.up*1.1f);yield return null;
  Check(g.CanWork(),"Cutting workstation is reachable");g.Work(2);float saved=g.progress[0];Place(new Vector3(0,0,-11));g.Work(3);Check(g.progress[0]==saved,"Leaving station preserves partial work and prevents remote work");
  for(int i=0;i<3;i++){
   Place(g.stations[i]+Vector3.back*2);g.eye.transform.LookAt(g.stations[i]+Vector3.up*1.2f);Check(g.CanWork(),"Workstation "+i+" has clear interaction sightline");g.Work(8);Check(g.stage==i+1,"Assignment step "+i+" completes");
  }
  Place(g.stations[3]+Vector3.back*2);g.Work(.1f);Check(g.phase==DesignPrototype.Phase.Complete,"Submission wins after all three production steps");yield return Shot("03-Accepted");
  g.phase=DesignPrototype.Phase.Playing;g.stage=0;g.progress=new float[3];g.elapsed=5;Place(new Vector3(0,0,-5));g.eye.transform.LookAt(new Vector3(0,4,0));
  g.teacher.transform.position=new Vector3(0,4.7f,0);g.teacher.mode=DesignSpider.Mode.Patrol;Check(g.teacher.SeesPlayer(),"Professor detects student in open space");
  Place(new Vector3(-5,0,3));g.teacher.transform.position=new Vector3(-1,4.7f,3);Check(!g.teacher.SeesPlayer(),"Tall partition blocks teacher line of sight");
  var route=DesignNavigation.Path(new Vector3(-5,4.7f,3),new Vector3(-1,4.7f,3));bool clear=route.Count>6;foreach(var p in route)if(Physics.CheckBox(p,new Vector3(.6f,.3f,.6f),Quaternion.identity,DesignPrototype.SolidMask))clear=false;Check(clear,"Ceiling chase routes around tall partitions");
  Place(new Vector3(0,0,-5));g.teacher.transform.position=new Vector3(0,4.7f,0);g.teacher.mode=DesignSpider.Mode.Patrol;g.teacher.enabled=true;yield return null;Check(g.teacher.mode==DesignSpider.Mode.Warning,"Detection gives warning before chase");
  yield return new WaitForSeconds(1.2f);Check(g.teacher.mode==DesignSpider.Mode.Chase,"Warning transitions into chase");Vector3 before=g.teacher.transform.position;yield return new WaitForSeconds(.25f);Check(Vector3.Distance(before,g.teacher.transform.position)>.4f,"Chase moves substantially faster than Drawing skeletons");
  g.phase=DesignPrototype.Phase.Paused;before=g.teacher.transform.position;yield return new WaitForSeconds(.3f);Check(Vector3.Distance(before,g.teacher.transform.position)<.01f,"Pause freezes pursuit");g.phase=DesignPrototype.Phase.Playing;
  Place(g.tables[0]);g.SetCrouch(true);g.teacher.transform.position=g.tables[0]+Vector3.up*4.7f;g.teacher.ForceChase();yield return null;Check(g.teacher.mode==DesignSpider.Mode.Probe,"Teacher probes beneath cover instead of instant table capture");
  g.eye.transform.LookAt(g.tables[0]+new Vector3(0,1,-1.5f));yield return new WaitForSeconds(.9f);yield return Shot("04-Under-table-probe");Place(g.tables[0]+Vector3.back*4);g.SetCrouch(false);yield return new WaitForSeconds(2);
  Check(g.phase==DesignPrototype.Phase.Playing&&g.teacher.mode==DesignSpider.Mode.Recover,"Escaping a committed neck attack grants recovery window");
  Place(new Vector3(0,0,-5));g.teacher.transform.position=new Vector3(0,4.7f,-5);g.teacher.ForceChase();yield return new WaitForSeconds(2.4f);Check(g.phase==DesignPrototype.Phase.Caught,"Remaining in attack reach triggers capture");yield return Shot("05-Caught");
  var previous=g;g=DesignPrototype.Launch();g.automation=true;yield return null;Check(g!=previous&&g.stage==0&&g.phase==DesignPrototype.Phase.Brief,"Retry creates a fresh Design run");
  g.Begin();g.teacher.enabled=false;Place(new Vector3(0,0,-8));g.eye.transform.LookAt(new Vector3(0,3,4));g.teacher.transform.position=new Vector3(0,4.7f,-3);yield return Shot("06-Classroom");
  g.ReturnToMenu();yield return new WaitForSeconds(1);var drawing=FindFirstObjectByType<StudyGame>();Check(drawing&&drawing.phase==StudyGame.Phase.Title,"Returning restores Drawing and independent assignment menu");
  if(drawing){drawing.automation=true;drawing.StartStudy();yield return null;Check(drawing.phase==StudyGame.Phase.Playing,"Original Drawing game can still start");}
  results.Add("DONE");File.WriteAllLines("DesignQA-output/results.txt",results);UnityEditor.EditorApplication.isPlaying=false;
 }
}
#endif
