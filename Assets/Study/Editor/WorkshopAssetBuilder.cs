#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.IO;
// Deterministic import of the Blender-authored geometry. Gameplay never parses JSON.
public static class WorkshopAssetBuilder {
 [Serializable] class Document { public Group[] groups; }
 [Serializable] class Group { public string name;public Part[] parts; }
 [Serializable] class Part { public string name,material; public float[] positions,normals,uv;public int[] triangles; }
 const string Source="Assets/Study/ArtWorkshop/",Dest="Assets/Study/Resources/Workshop/";
 [MenuItem("Study/Import Workshop Artwork")]
 public static void Build(){
  Directory.CreateDirectory(Dest);AssetDatabase.Refresh();
  foreach(string name in new[]{"Wood","Mat","Paper","Floor","Cork"}){
   var imp=(TextureImporter)AssetImporter.GetAtPath(Source+name+"_Normal.png");imp.textureType=TextureImporterType.NormalMap;imp.sRGBTexture=false;imp.maxTextureSize=1024;imp.SaveAndReimport();
   var mask=(TextureImporter)AssetImporter.GetAtPath(Source+name+"_Surface.png");mask.sRGBTexture=false;mask.alphaSource=TextureImporterAlphaSource.FromInput;mask.alphaIsTransparency=false;mask.SaveAndReimport();
  }
  var data=JsonUtility.FromJson<Document>(File.ReadAllText(Source+"Workshop.meshdata.json"));
  foreach(var group in data.groups){
   var root=new GameObject("Workshop / "+group.name);
   foreach(var part in group.parts){
    var mesh=new Mesh{name=part.name,indexFormat=IndexFormat.UInt32};int count=part.positions.Length/3;
    var v=new Vector3[count];var n=new Vector3[count];var uv=new Vector2[count];
    for(int i=0;i<count;i++){v[i]=new Vector3(part.positions[i*3],part.positions[i*3+1],part.positions[i*3+2]);n[i]=new Vector3(part.normals[i*3],part.normals[i*3+1],part.normals[i*3+2]);uv[i]=new Vector2(part.uv[i*2],part.uv[i*2+1]);}
    mesh.vertices=v;mesh.normals=n;mesh.uv=uv;mesh.triangles=part.triangles;mesh.RecalculateBounds();mesh.RecalculateTangents();
    string path=Dest+part.name+".asset";var old=AssetDatabase.LoadAssetAtPath<Mesh>(path);if(old){EditorUtility.CopySerialized(mesh,old);UnityEngine.Object.DestroyImmediate(mesh);mesh=old;}else AssetDatabase.CreateAsset(mesh,path);
    var child=new GameObject(part.name);child.transform.SetParent(root.transform,false);child.AddComponent<MeshFilter>().sharedMesh=mesh;child.AddComponent<MeshRenderer>().sharedMaterial=Material(part.material);
   }
   PrefabUtility.SaveAsPrefabAsset(root,Dest+group.name+".prefab");UnityEngine.Object.DestroyImmediate(root);
  }
  Material("Mat");Material("Floor");Material("Cork");AssetDatabase.SaveAssets();Debug.Log("WORKSHOP_ASSETS_IMPORTED");
 }
 static Material Material(string name){
  string path=Dest+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Standard")){name=name};AssetDatabase.CreateAsset(m,path);}
  Color c=Color.white;float smooth=.2f,metal=0;
  switch(name){
   case "Steel":c=new Color(.43f,.46f,.47f);smooth=.52f;metal=.85f;break;
   case "Paint":c=new Color(.07f,.085f,.088f);smooth=.23f;break;
   case "StudentSkin":c=new Color(.43f,.31f,.25f);smooth=.22f;break;
   case "Sleeve":c=new Color(.17f,.20f,.20f);smooth=.12f;break;
   case "Ink":c=new Color(.045f,.056f,.057f);smooth=.1f;break;
  }
  var color=AssetDatabase.LoadAssetAtPath<Texture2D>(Source+name+"_BaseColor.png");
  if(color){m.mainTexture=color;m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Source+name+"_Normal.png"));m.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Source+name+"_Surface.png"));m.EnableKeyword("_NORMALMAP");m.EnableKeyword("_METALLICGLOSSMAP");m.SetFloat("_BumpScale",.7f);m.SetFloat("_GlossMapScale",1);}
  m.mainTextureScale=name=="Cork"?new Vector2(6,5):Vector2.one;
  m.color=c;m.SetFloat("_Glossiness",smooth);m.SetFloat("_Metallic",metal);EditorUtility.SetDirty(m);return m;
 }
}
#endif
