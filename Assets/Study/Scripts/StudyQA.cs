#if UNITY_EDITOR
using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
public class StudyQA:MonoBehaviour {
 StudyGame g;List<string> results=new List<string>();
 void Check(bool ok,string name){string r=(ok?"PASS ":"FAIL ")+name;results.Add(r);Debug.Log(r);File.WriteAllLines("DrawingQA-output/results.txt",results);}
 IEnumerator Start(){
  Directory.CreateDirectory("DrawingQA-output");File.WriteAllText("DrawingQA-output/results.txt","RUNNING\n");
  g=GetComponent<StudyGame>();g.automation=true;Application.runInBackground=true;
  yield return new WaitForSeconds(1);yield return Capture("01-Title");
  Check(!g.intro.Visible&&!g.board.gameObject.activeSelf,"Cinematic cover shows the studio without the task note or player board");
  Check(Resources.Load<AudioClip>("HorrorMusic")!=null&&Resources.Load<AudioClip>("Heartbeat")!=null,"Supplied background music and heartbeat clips load");
  Check(StudySoundscape.RateForDistance(1)>StudySoundscape.RateForDistance(4)&&StudySoundscape.RateForDistance(4)>StudySoundscape.RateForDistance(9),"Heartbeat tempo increases as the nearest skeleton approaches");
  float headMin=float.MaxValue,headMax=float.MinValue,sole=float.MaxValue;
  foreach(var mf in g.skeletons[0].GetComponentsInChildren<MeshFilter>()){
   var rr=mf.GetComponent<Renderer>();if(!rr||rr.sharedMaterial.name!="Aged ivory")continue;
   bool isHead=mf.transform.IsChildOf(g.skeletons[0].skull);
   foreach(var v in mf.sharedMesh.vertices){float y=g.skeletons[0].transform.InverseTransformPoint(mf.transform.TransformPoint(v)).y;sole=Mathf.Min(sole,y);if(isHead){headMin=Mathf.Min(headMin,y);headMax=Mathf.Max(headMax,y);}}
  }
  float headRatio=(headMax-sole)/(headMax-headMin);Check(headRatio>7&&headRatio<8,"Measured adult body-to-skull height ratio: "+headRatio.ToString("0.00")+" heads (expected 7 to 8)");
  bool handsRemoved=true;foreach(var r in g.board.GetComponentsInChildren<Renderer>(true)){string n=r.name.ToLowerInvariant();if(n.Contains("hand")||n.Contains("sleeve")||n.Contains("cuff"))handsRemoved=false;}
  Check(handsRemoved,"Player viewmodel contains no hands, sleeves or cuffs");
  Check(g.artwork.pencilRig.childCount==1&&g.artwork.pencilRig.GetChild(0).name=="Charcoal / finger grip","Floating charcoal remains on the animated drawing rig");
  bool spatial=true;foreach(var skeleton in g.skeletons){var source=skeleton.GetComponent<AudioSource>();if(!skeleton.enableWheelSound||!source||source.spatialBlend<.99f||!source.loop)spatial=false;}
  Check(spatial,"Skeleton motion uses spatial looping wheel audio");
  var originalPositions=new Vector3[g.skeletons.Length];for(int i=0;i<g.skeletons.Length;i++){originalPositions[i]=g.skeletons[i].transform.position;g.skeletons[i].transform.position=g.body.transform.position+Vector3.right*(20+i);}
  yield return new WaitForSeconds(.8f);float farRate=g.soundscape.BPM;g.skeletons[0].transform.position=g.body.transform.position+Vector3.right*1.5f;yield return new WaitForSeconds(.8f);
  Check(g.soundscape.BPM>farRate+35,"Actual nearest-skeleton movement raises the runtime heartbeat tempo");for(int i=0;i<g.skeletons.Length;i++)g.skeletons[i].transform.position=originalPositions[i];
  AudioSource music=null;foreach(var source in g.GetComponents<AudioSource>())if(source.clip&&source.clip.name=="HorrorMusic"&&source.isPlaying)music=source;
  music.time=music.clip.length-.25f;yield return new WaitForSeconds(3.3f);int loops=0;foreach(var source in g.GetComponents<AudioSource>())if(source.clip&&source.clip.name=="HorrorMusic"&&source.isPlaying&&source.volume>.1f)loops++;
  Check(loops==1,"Supplied music crosses the loop boundary and continues on one active source");
  g.SendMessage("Enter");yield return new WaitForSeconds(.7f);yield return Capture("01b-Task-note");
  Check(g.phase==StudyGame.Phase.Reading&&g.intro.Visible&&!g.board.gameObject.activeSelf,"Start opens a separate readable task note");
  yield return new WaitForSeconds(1);Check(g.phase==StudyGame.Phase.Reading,"The note waits for the player instead of folding automatically");
  g.SendMessage("FoldNote");yield return new WaitForSeconds(1.3f);yield return Capture("01c-Folding");yield return new WaitForSeconds(2.45f);yield return Capture("01d-Board-out");yield return new WaitForSeconds(.65f);
  Check(g.phase==StudyGame.Phase.Playing&&!g.intro.Visible&&g.board.gameObject.activeSelf,"Fold and pocket sequence reveals the board and starts play");
  g.StartStudy();g.AimAt(g.subjects[0].focus.position);yield return new WaitForSeconds(1.8f);
  Check(g.selected==0&&g.memory>4.5f,"Aiming at the marked subject completes observation and stores memory");
  Vector3 p=g.skeletons[0].transform.position;yield return new WaitForSeconds(.5f);
  Check(Vector3.Distance(p,g.skeletons[0].transform.position)<.01f,"Headlamp freezes the skeleton");yield return Capture("02-Observed");
  var obstacle=GameObject.CreatePrimitive(PrimitiveType.Cube);obstacle.layer=8;obstacle.transform.position=(g.headlamp.transform.position+g.skeletons[0].torso.position+Vector3.up*.25f)/2;obstacle.transform.localScale=new Vector3(2,3,.2f);Physics.SyncTransforms();Check(!g.Illuminated(g.skeletons[0].torso.position+Vector3.up*.25f),"Opaque objects block the freezing light");Destroy(obstacle);yield return null;
  g.automatedDraw=true;yield return new WaitForSeconds(1.3f);Vector3 hand=g.artwork.pencilRig.localPosition;yield return new WaitForSeconds(.3f);
  Check(Vector3.Distance(hand,g.artwork.pencilRig.localPosition)>.005f,"Drawing animates the floating charcoal along actual stroke coordinates");
  Check(g.subjects[0].progress>.01f,"Holding draw advances progress");
  Check(Vector3.Distance(p,g.skeletons[0].transform.position)>.05f,"Lowering the headlamp releases the skeleton");
  Color32[] empty=g.artwork.backgrounds[0].GetPixels32(),live=g.artwork.Pages[0].GetPixels32();int changed=0;for(int i=0;i<empty.Length;i++)if(live[i].r<empty[i].r-8)changed++;
  Check(changed>100,"Visible charcoal pixels appear during the first drawing interval");
  foreach(var s in g.skeletons)s.enabled=false;
  g.subjects[0].progress=.53f;yield return null;yield return Capture("03-Drawing");
  foreach(float progress in new[]{.18f,.82f}){g.subjects[0].progress=progress;g.memory=5;yield return new WaitForSeconds(.25f);yield return Capture("03-Hand-pose-"+Mathf.RoundToInt(progress*100));}
  // Keep the exhaustion fixture incomplete even when assignment durations change.
  g.subjects[0].progress=.5f;g.memory=StudyTuning.MemorySeconds;
  yield return new WaitForSeconds(StudyTuning.MemorySeconds+.1f);float done=g.subjects[0].progress;yield return new WaitForSeconds(.5f);
  Check(g.memory==0&&done<1&&Mathf.Abs(done-g.subjects[0].progress)<.0001f,"Memory exhaustion stops drawing and requests another observation");yield return Capture("04-Memory-empty");
  g.automatedDraw=false;g.AimAt(g.subjects[0].focus.position);yield return new WaitForSeconds(2);
  Check(g.memory>4.5f,"Looking up restores drawing memory");
  var path=StudyNavigation.Path(new Vector3(0,0,1.5f),new Vector3(0,0,5));bool clear=path.Count>2;foreach(var v in path)if(Physics.CheckBox(v+Vector3.up,new Vector3(.34f,.8f,.34f),Quaternion.identity,1<<8))clear=false;
  Check(clear,"Pursuit routes around the still-life table");
  g.subjects[0].progress=1;g.AimAt(g.eye.transform.position+Vector3.back*5);g.skeletons[0].enabled=true;p=g.skeletons[0].transform.position;yield return new WaitForSeconds(1);
  Check(Vector3.Distance(p,g.skeletons[0].transform.position)>.05f,"A completed subject continues pursuing");g.skeletons[0].enabled=false;
  g.AimAt(g.subjects[0].focus.position);foreach(var s in g.subjects)s.progress=1;yield return new WaitForSeconds(1.2f);
  Check(g.phase==StudyGame.Phase.Won&&g.ceilingLights[0].intensity>1,"Final stroke triggers the delayed ceiling-light reveal");yield return new WaitForSeconds(3);yield return Capture("05-Victory-gallery");
  foreach(var l in g.ceilingLights)l.intensity=0;RenderSettings.ambientLight=new Color(.072f,.087f,.083f);g.phase=StudyGame.Phase.Playing;g.subjects[2].progress=0;g.skeletons[0].transform.position=g.body.transform.position+Vector3.back*.5f;g.skeletons[0].enabled=true;
  yield return new WaitForSeconds(.4f);Check(g.phase==StudyGame.Phase.Lost,"Contact triggers the hand-grab failure sequence");yield return new WaitForSeconds(.25f);yield return Capture("06-Contact");yield return new WaitForSeconds(2);yield return Capture("07-Failure-study");
  Check(g.pencilClip&&g.humClip&&g.skeletons[0].wheelClip,"Edited recording assets are assigned to all sound roles");
  results.Add("DONE");File.WriteAllLines("DrawingQA-output/results.txt",results);UnityEditor.EditorApplication.isPlaying=false;
 }
 IEnumerator Capture(string name){
  Directory.CreateDirectory("DrawingQA-output");yield return new WaitForEndOfFrame();
  var shot=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes("DrawingQA-output/"+name+".png",shot.EncodeToPNG());Destroy(shot);
 }
}
#endif
