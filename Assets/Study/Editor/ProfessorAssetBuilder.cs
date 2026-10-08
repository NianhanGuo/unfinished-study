#if UNITY_EDITOR
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using System;
using System.IO;
// Deterministic import of the Blender-authored geometry. Gameplay never parses JSON.
public static class ProfessorAssetBuilder {
 [Serializable] class Document { public Group[] groups; }
 [Serializable] class Group { public string name;public Part[] parts; }
 [Serializable] class Part { public string name,material; public float[] positions,normals,uv;public int[] triangles; }
 const string Source="Assets/Study/ArtProfessor/",Dest="Assets/Study/Resources/Professor/";
 [MenuItem("Study/Import Professor Artwork")]
 public static void Build(){
  Directory.CreateDirectory(Dest);AssetDatabase.Refresh();
  foreach(string name in new[]{"ProfessorSkin_Normal","ProfessorWool_Normal"}){var imp=(TextureImporter)AssetImporter.GetAtPath(Source+name+".png");imp.textureType=TextureImporterType.NormalMap;imp.sRGBTexture=false;imp.maxTextureSize=1024;imp.SaveAndReimport();}
  var data=JsonUtility.FromJson<Document>(File.ReadAllText(Source+"Professor.meshdata.json"));
  foreach(var group in data.groups){
   var root=new GameObject("Professor / "+group.name);
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
  Material("Tie");AssetDatabase.SaveAssets();Debug.Log("PROFESSOR_ASSETS_IMPORTED");
 }
 static Material Material(string name){
  string path=Dest+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(path);if(!m){m=new Material(Shader.Find("Standard")){name=name};AssetDatabase.CreateAsset(m,path);}
  Color c=Color.white;float smooth=.25f,metal=0;
  switch(name){
   case "Skin":c=new Color(.8f,.8f,.8f);smooth=.22f;m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(Source+"ProfessorSkin_BaseColor.png");m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Source+"ProfessorSkin_Normal.png"));m.EnableKeyword("_NORMALMAP");m.SetFloat("_BumpScale",.5f);break;
   case "Lips":c=new Color(.40f,.24f,.20f);smooth=.3f;break;
   case "Crease":c=new Color(.075f,.033f,.028f);break;
   case "Sclera":c=new Color(.40f,.39f,.35f);smooth=.5f;break;
   case "Iris":c=new Color(.08f,.10f,.09f);smooth=.6f;break;
   case "Pupil":c=new Color(.012f,.013f,.012f);smooth=.65f;break;
   case "Bronze":c=new Color(.32f,.26f,.17f);smooth=.62f;metal=.82f;break;
   case "Hair":c=new Color(.17f,.17f,.15f);smooth=.15f;break;
   case "Wool":c=new Color(.12f,.135f,.145f);smooth=.16f;m.SetTexture("_BumpMap",AssetDatabase.LoadAssetAtPath<Texture2D>(Source+"ProfessorWool_Normal.png"));m.EnableKeyword("_NORMALMAP");m.SetFloat("_BumpScale",.35f);break;
   case "Cotton":c=new Color(.61f,.56f,.44f);smooth=.18f;break;
   case "Tie":c=new Color(.15f,.045f,.032f);smooth=.36f;break;
   case "Seam":c=new Color(.03f,.035f,.037f);smooth=.13f;break;
  }
  m.color=c;m.SetFloat("_Glossiness",smooth);m.SetFloat("_Metallic",metal);EditorUtility.SetDirty(m);return m;
 }
}
#endif
