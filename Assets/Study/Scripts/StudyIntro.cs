using UnityEngine;
// A physical, two-sided sheet. Each half has its own hinge so folding also moves the text.
public class StudyIntro : MonoBehaviour {
 StudyGame game;Transform root,hinge;Material front,back;
 public const float Duration=4.2f;
 public bool Visible=>root&&root.gameObject.activeSelf;
 public float FoldAngle=>hinge?hinge.localEulerAngles.z:0;
 public void Initialize(StudyGame g){
  game=g;root=new GameObject("Homework note / held sheet").transform;root.SetParent(g.eye.transform,false);
  front=new Material(Shader.Find("Standard"));front.mainTexture=Resources.Load<Texture2D>("HomeworkNote");front.color=Color.white;front.SetFloat("_Glossiness",.035f);front.EnableKeyword("_EMISSION");front.SetColor("_EmissionColor",new Color(.14f,.135f,.12f));front.SetTexture("_EmissionMap",front.mainTexture);
  back=new Material(Shader.Find("Standard"));back.color=new Color(.74f,.70f,.60f);back.SetFloat("_Glossiness",.035f);
  Sheet(root,-.23f,0,0,.5f);hinge=new GameObject("Fold / center crease").transform;hinge.SetParent(root,false);hinge.localPosition=Vector3.up*.0015f;Sheet(hinge,0,.23f,.5f,1);
  foreach(var t in root.GetComponentsInChildren<Transform>())t.gameObject.layer=9;
  foreach(var r in root.GetComponentsInChildren<Renderer>()){r.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;r.receiveShadows=false;}
  Complete();
 }
 void Sheet(Transform parent,float x0,float x1,float u0,float u1){
  var mesh=new Mesh();mesh.name="Foldable sheet";float z0=-.26f,z1=.26f;
  mesh.vertices=new[]{new Vector3(x0,0,z0),new Vector3(x1,0,z0),new Vector3(x0,0,z1),new Vector3(x1,0,z1),new Vector3(x0,-.0007f,z0),new Vector3(x1,-.0007f,z0),new Vector3(x0,-.0007f,z1),new Vector3(x1,-.0007f,z1)};
  mesh.uv=new[]{new Vector2(u0,0),new Vector2(u1,0),new Vector2(u0,1),new Vector2(u1,1),Vector2.zero,Vector2.right,Vector2.up,Vector2.one};
  mesh.subMeshCount=2;mesh.SetTriangles(new[]{0,2,1,1,2,3},0);mesh.SetTriangles(new[]{4,5,6,5,7,6},1);mesh.RecalculateNormals();mesh.RecalculateBounds();
  var o=new GameObject("Paper",typeof(MeshFilter),typeof(MeshRenderer));o.transform.SetParent(parent,false);o.GetComponent<MeshFilter>().sharedMesh=mesh;o.GetComponent<Renderer>().sharedMaterials=new[]{front,back};
 }
 public void Present(){root.gameObject.SetActive(true);game.board.gameObject.SetActive(false);root.localPosition=new Vector3(0,.035f,.76f);root.localRotation=Quaternion.Euler(-83,0,1.2f);}
 public void Animate(float seconds){
  float fold=Mathf.SmoothStep(0,1,Mathf.Clamp01((seconds-.3f)/1.35f));hinge.localRotation=Quaternion.Euler(0,0,178*fold);
  float pocket=Mathf.SmoothStep(0,1,Mathf.Clamp01((seconds-1.8f)/.9f));root.localPosition=new Vector3(0,.035f,.76f)+new Vector3(-.23f,-.85f,-.15f)*pocket;root.localRotation=Quaternion.Euler(-83+25*pocket,0,1.2f-24*pocket);
  root.gameObject.SetActive(seconds<2.75f);
  if(seconds>=2.75f){game.board.gameObject.SetActive(true);float lift=Mathf.SmoothStep(0,1,Mathf.Clamp01((seconds-2.75f)/.7f));float settle=Mathf.SmoothStep(0,1,Mathf.Clamp01((seconds-3.45f)/.75f));Vector3 raised=Vector3.Lerp(new Vector3(.12f,-1.1f,.4f),new Vector3(.06f,-.24f,.64f),lift);game.board.localPosition=Vector3.Lerp(raised,new Vector3(.12f,-.53f,.53f),settle);game.board.localRotation=Quaternion.Euler(Mathf.Lerp(Mathf.Lerp(-12,-55,lift),-36,settle),0,-9);}
 }
 public void Complete(){root.gameObject.SetActive(false);game.board.gameObject.SetActive(true);}
}
