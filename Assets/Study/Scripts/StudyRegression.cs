#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
public class StudyRegression:MonoBehaviour {
 StudyGame g;readonly List<string> results=new List<string>();
 void Check(bool ok,string name){results.Add((ok?"PASS ":"FAIL ")+name);File.WriteAllLines("StudyRegression.results",results);}
 void Place(Vector3 p){g.body.enabled=false;g.body.transform.position=p;g.body.enabled=true;Physics.SyncTransforms();}
 IEnumerator Start(){
  g=GetComponent<StudyGame>();g.automation=true;Application.runInBackground=true;
  foreach(var s in g.skeletons)s.enabled=false;
  yield return null;g.StartStudy();
  var rear=g.skeletons[1];var offset=rear.transform.position-g.body.transform.position;
  Check(Vector3.Dot(offset,g.body.transform.forward)<-1.5f,"Second skeleton starts behind player");
  Check(!Physics.CheckBox(rear.transform.position+Vector3.up,new Vector3(.4f,.85f,.4f),Quaternion.identity,1<<8),"Rear spawn clears walls and furniture");
  Vector3 table=g.subjects[2].transform.position;
  foreach(var dir in new[]{Vector3.back,Vector3.left,Vector3.right,Vector3.forward}){
   Place(table+dir*3);g.memory=0;
   for(int j=1;j<4;j++){
    g.AimAt(g.subjects[2].ObservationPoint(j));yield return new WaitForSeconds(1.55f);
    Check(g.GetObserved()==2&&g.selected==2&&g.memory>4.9f,"Memorize still life from "+dir+" target "+j);
   }
  }
  g.automatedDraw=true;float before=g.subjects[2].progress;yield return new WaitForSeconds(5.7f);
  Check(g.memory==0&&g.subjects[2].progress>before,"Third drawing advances and exhausts memory");
  before=g.subjects[2].progress;yield return new WaitForSeconds(.3f);Check(g.subjects[2].progress==before,"No drawing without memory");
  g.automatedDraw=false;g.AimAt(g.subjects[2].ObservationPoint(1));yield return new WaitForSeconds(1.9f);
  Check(g.memory>4.9f,"Third drawing refills after releasing draw");
  g.automatedDraw=true;yield return new WaitForSeconds(.8f);Check(g.subjects[2].progress>before,"Third drawing resumes after refill");g.automatedDraw=false;yield return new WaitForSeconds(.5f);
  var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.layer=8;wall.transform.position=(g.eye.transform.position+g.subjects[2].focus.position)/2;wall.transform.localScale=new Vector3(4,4,.4f);Physics.SyncTransforms();
  Check(g.GetObserved()==-1,"Opaque wall blocks observation");Destroy(wall);yield return null;
  Place(new Vector3(0,0,-3.5f));g.AimAt(g.eye.transform.position+Vector3.forward*5);yield return null;
  rear.transform.position=new Vector3(0,0,-6.5f);rear.enabled=true;g.hasDrawn=true;Vector3 initial=rear.transform.position;
  yield return new WaitForSeconds(1);Check(rear.transform.position.z>initial.z+.2f,"Rear skeleton closes in from behind");
  g.AimAt(rear.torso.position+Vector3.up*.25f);yield return new WaitForSeconds(.3f);initial=rear.transform.position;
  yield return new WaitForSeconds(.5f);Check(rear.frozen&&Vector3.Distance(initial,rear.transform.position)<.01f,"Turning the light onto rear skeleton freezes it");
  rear.enabled=false;rear.transform.position=new Vector3(0,0,-.5f);g.AimAt(g.eye.transform.position+Vector3.forward*5);yield return null;
  Vector3 target=rear.PursuitDestination();Check(Mathf.Abs(target.x)>.5f&&(target-g.body.transform.position).magnitude>2,"Stalker chooses lateral flank instead of frontal charge");
  g.AimAt(g.eye.transform.position+Vector3.right*5);yield return null;Check(Vector3.Distance(target,rear.PursuitDestination())>.5f,"Flank destination responds to player turning");
  rear.enabled=true;g.AimAt(g.eye.transform.position+Vector3.back*5);yield return new WaitForSeconds(1);Check(!rear.frozen,"Stalker moves when outside flashlight");rear.enabled=false;
  var route=StudyNavigation.Path(new Vector3(0,0,-.5f),new Vector3(0,0,-6.5f),new Vector3(0,0,-3.5f),1.7f);
  bool avoids=route.Count>0;foreach(var point in route)if(Vector3.Distance(point,new Vector3(0,0,-3.5f))<1.7f||Physics.CheckBox(point+Vector3.up,new Vector3(.34f,.8f,.34f),Quaternion.identity,1<<8))avoids=false;
  Check(avoids,"Flank route avoids player center and furniture");
  results.Add("DONE");File.WriteAllLines("StudyRegression.results",results);UnityEditor.EditorApplication.isPlaying=false;
 }
}
#endif
