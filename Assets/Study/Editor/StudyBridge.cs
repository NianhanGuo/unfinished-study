using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using System.IO;
[InitializeOnLoad] public static class StudyBridge {
 static double next;
 static StudyBridge(){EditorApplication.update+=Poll;}
 static void Poll(){
  if(EditorApplication.timeSinceStartup<next || EditorApplication.isCompiling || EditorApplication.isUpdating)return;next=EditorApplication.timeSinceStartup+1;
  if(!File.Exists("StudyCommand.txt"))return;
  string cmd=File.ReadAllText("StudyCommand.txt").Trim();File.Delete("StudyCommand.txt");
  try{
   if(cmd=="stop")EditorApplication.isPlaying=false;
   if(cmd=="refresh"){AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);}
   if(cmd=="rebuild")StudioBuilder.Create();
   if(cmd=="hands")StudioRealism.RefreshHands();
   if(cmd=="build")StudioBuilder.BuildMac();
   if(cmd=="professorassets")ProfessorAssetBuilder.Build();
   if(cmd=="workshopassets")WorkshopAssetBuilder.Build();
   if(cmd=="capture")Capture();
   if(cmd=="anatomy")StudyAnatomyPreview.Capture();
   if(cmd=="menuqa"){SetGameSize();EditorSceneManager.OpenScene("Assets/Study/Studio.unity");File.WriteAllText("MenuQA.request","run");EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.isPlaying=true;}
   if(cmd=="designqa"){SetGameSize();EditorSceneManager.OpenScene("Assets/Study/Studio.unity");File.WriteAllText("DesignQA.request","run");EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.isPlaying=true;}
   if(cmd=="play"){EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.isPlaying=true;}
   if(cmd=="regression"){EditorSceneManager.OpenScene("Assets/Study/Studio.unity");File.WriteAllText("StudyRegression.request","run");EditorApplication.isPlaying=true;}
   if(cmd=="qa"){SetGameSize();EditorSceneManager.OpenScene("Assets/Study/Studio.unity");File.WriteAllText("StudyQA.request","run");EditorApplication.ExecuteMenuItem("Window/General/Game");EditorApplication.isPlaying=true;}
   File.WriteAllText("StudyCommand.result","OK "+cmd);
  }catch(System.Exception e){File.WriteAllText("StudyCommand.result",e.ToString());Debug.LogException(e);}
 }
 static void SetGameSize(){
  var assembly=typeof(Editor).Assembly;
  var sizesType=assembly.GetType("UnityEditor.GameViewSizes");
  var singleton=typeof(ScriptableSingleton<>).MakeGenericType(sizesType);
  var sizes=singleton.GetProperty("instance").GetValue(null);
  var group=sizesType.GetMethod("GetGroup").Invoke(sizes,new object[]{GameViewSizeGroupType.Standalone});
  var flags=System.Reflection.BindingFlags.Instance|System.Reflection.BindingFlags.Public|System.Reflection.BindingFlags.NonPublic;
  var sizeType=assembly.GetType("UnityEditor.GameViewSize");var kind=assembly.GetType("UnityEditor.GameViewSizeType");
  var ctor=sizeType.GetConstructor(flags,null,new[]{kind,typeof(int),typeof(int),typeof(string)},null);
  var size=ctor.Invoke(new object[]{System.Enum.ToObject(kind,1),1440,900,"Study QA 1440x900"});
  group.GetType().GetMethod("AddCustomSize").Invoke(group,new[]{size});
  int count=(int)group.GetType().GetMethod("GetTotalCount").Invoke(group,null);
  var view=EditorWindow.GetWindow(assembly.GetType("UnityEditor.GameView"));
  view.GetType().GetProperty("selectedSizeIndex",flags).SetValue(view,count-1);view.Focus();
 }
 static void Capture(){
  if(UnityEngine.SceneManagement.SceneManager.GetActiveScene().path!="Assets/Study/Studio.unity")EditorSceneManager.OpenScene("Assets/Study/Studio.unity");
  var game=Object.FindFirstObjectByType<StudyGame>();var cam=game.eye;var old=cam.targetTexture;
  var rt=new RenderTexture(1440,900,24);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
  var tex=new Texture2D(1440,900,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,1440,900),0,0);tex.Apply();File.WriteAllBytes("../Studio-preview.png",tex.EncodeToPNG());
  cam.targetTexture=old;RenderTexture.active=null;Object.DestroyImmediate(rt);Object.DestroyImmediate(tex);
 }
}
