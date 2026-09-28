using UnityEngine;
using UnityEditor;
using System.IO;
public static class StudyAnatomyPreview {
 public static void Capture(){
  var source=Object.FindFirstObjectByType<StudyGame>().skeletons[0];
  var clone=Object.Instantiate(source.gameObject);clone.transform.position=new Vector3(30,0,0);clone.transform.rotation=Quaternion.identity;
  var sk=clone.GetComponent<StudySkeleton>();sk.torso.localRotation=Quaternion.identity;sk.skull.localRotation=Quaternion.identity;sk.leftArm.localRotation=Quaternion.identity;sk.rightArm.localRotation=Quaternion.identity;
  foreach(var tr in clone.GetComponentsInChildren<Transform>())tr.gameObject.layer=10;
  var cam=new GameObject("Anatomy proof camera").AddComponent<Camera>();cam.transform.position=new Vector3(30,1.14f,4);cam.transform.rotation=Quaternion.Euler(0,180,0);cam.orthographic=true;cam.orthographicSize=1.16f;cam.cullingMask=1<<10;cam.clearFlags=CameraClearFlags.SolidColor;cam.backgroundColor=new Color(.09f,.11f,.11f);
  var key=new GameObject("Anatomy proof key").AddComponent<Light>();key.type=LightType.Directional;key.transform.rotation=Quaternion.Euler(30,-155,0);key.intensity=1.2f;key.cullingMask=1<<10;
  var fill=new GameObject("Anatomy proof fill").AddComponent<Light>();fill.type=LightType.Directional;fill.transform.rotation=Quaternion.Euler(0,125,0);fill.intensity=.7f;fill.cullingMask=1<<10;
  bool fog=RenderSettings.fog;RenderSettings.fog=false;
  Directory.CreateDirectory("../PreviewsV3");var rt=new RenderTexture(900,1200,24);cam.targetTexture=rt;cam.Render();RenderTexture.active=rt;
  var tex=new Texture2D(900,1200,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,900,1200),0,0);tex.Apply();File.WriteAllBytes("../PreviewsV3/00-Anatomy.png",tex.EncodeToPNG());
  RenderTexture.active=null;RenderSettings.fog=fog;Object.DestroyImmediate(tex);Object.DestroyImmediate(rt);Object.DestroyImmediate(cam.gameObject);Object.DestroyImmediate(key.gameObject);Object.DestroyImmediate(fill.gameObject);Object.DestroyImmediate(clone);
 }
}
