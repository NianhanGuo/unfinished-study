#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
public class StudyMenuQA:MonoBehaviour {
 readonly List<string> results=new List<string>();
 void Check(bool ok,string name){results.Add((ok?"PASS ":"FAIL ")+name);File.WriteAllLines("MenuQA-output/results.txt",results);}
 IEnumerator Shot(string name){yield return new WaitForEndOfFrame();var t=ScreenCapture.CaptureScreenshotAsTexture();File.WriteAllBytes("MenuQA-output/"+name+".png",t.EncodeToPNG());Destroy(t);}
 void Plate(Camera cam,string name){var effect=cam.GetComponent<StudyImageEffect>();bool grading=effect&&effect.enabled;if(effect)effect.enabled=false;var old=cam.targetTexture;var active=RenderTexture.active;var rt=new RenderTexture(1200,720,24);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;var image=new Texture2D(1200,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1200,720),0,0);image.Apply();File.WriteAllBytes("MenuQA-output/"+name+".png",image.EncodeToPNG());cam.targetTexture=old;RenderTexture.active=active;rt.Release();Destroy(rt);Destroy(image);if(effect)effect.enabled=grading;}
 IEnumerator Start(){
  DontDestroyOnLoad(gameObject);Directory.CreateDirectory("MenuQA-output");Application.runInBackground=true;
  yield return new WaitForSeconds(.6f);var g=FindFirstObjectByType<StudyGame>();g.automation=true;
  Check(g.frontEnd.page==StudyFrontEnd.Page.Cover&&!g.board.gameObject.activeSelf,"Cover starts with cinematic view and no player board");
  Vector3 camera=g.eye.transform.position,player=g.body.transform.position;yield return Shot("01-Cover");Plate(g.eye,"MenuDrawing");
  yield return new WaitForSeconds(4);Check(Vector3.Distance(camera,g.eye.transform.position)>.25f,"Cover camera visibly orbits the studio");Check(Vector3.Distance(player,g.body.transform.position)<.001f,"Cover camera does not move gameplay body");yield return Shot("02-Cover-orbit");
  g.frontEnd.ShowAssignments();yield return new WaitForSeconds(.3f);yield return Shot("03-Assignments");Check(g.frontEnd.page==StudyFrontEnd.Page.Assignments,"Separate assignment selection page opens");
  g.frontEnd.ShowCover();Check(g.frontEnd.page==StudyFrontEnd.Page.Cover,"Assignment page returns to cover");
  g.Enter();yield return null;Check(!g.frontEnd.Presenting&&g.intro.Visible&&g.phase==StudyGame.Phase.Reading,"Drawing opens its short physical note");
  Check(g.eye.transform.localPosition.y>1.5f&&g.eye.transform.localPosition.y<1.7f&&Mathf.Abs(g.eye.transform.localPosition.x)<.01f,"Drawing restores first-person camera pose");
  Check(g.headlamp.intensity>0&&!GameObject.Find("Cover / cinematic lighting"),"Cover light rig is disabled and player lamp restored");
  yield return Shot("04-Drawing-note");g.StartStudy();yield return null;Check(g.board.gameObject.activeSelf&&g.phase==StudyGame.Phase.Playing,"Drawing gameplay remains available");
  var design=DesignPrototype.Launch();design.automation=true;yield return new WaitForSeconds(.5f);yield return Shot("05-Design-brief");var briefRotation=design.eye.transform.rotation;design.eye.transform.rotation*=Quaternion.Euler(0,22,0);Plate(design.eye,"MenuDesign");design.eye.transform.rotation=briefRotation;
  Check(design.phase==DesignPrototype.Phase.Brief,"Design opens its concise preparation page");design.Begin();yield return null;Check(Vector3.Distance(design.eye.transform.localPosition,Vector3.up*1.6f)<.01f,"Design briefing restores player eye position on entry");
  design.ReturnToMenu();yield return new WaitForSeconds(.7f);g=FindFirstObjectByType<StudyGame>();Check(g&&g.frontEnd.page==StudyFrontEnd.Page.Cover,"Return from Design resets the cover correctly");
  results.Add("DONE");File.WriteAllLines("MenuQA-output/results.txt",results);UnityEditor.EditorApplication.isPlaying=false;
 }
}
#endif
