using UnityEngine;
[ExecuteAlways,RequireComponent(typeof(Camera))]
public class StudyImageEffect:MonoBehaviour {
 Material mat;Camera eye;
 void OnEnable(){eye=GetComponent<Camera>();eye.depthTextureMode|=DepthTextureMode.Depth|DepthTextureMode.DepthNormals;}
 void OnRenderImage(RenderTexture src,RenderTexture dst){
  if(!mat){var shader=Shader.Find("Study/Atmosphere");if(shader&&shader.isSupported)mat=new Material(shader);}
  if(mat){mat.SetFloat("_TimeValue",Time.time);mat.SetFloat("_Title",StudyGame.Instance&&StudyGame.Instance.phase==StudyGame.Phase.Title?1:0);mat.SetMatrix("_InvProjection",eye.projectionMatrix.inverse);Graphics.Blit(src,dst,mat);}else Graphics.Blit(src,dst);
 }
 void OnDisable(){if(mat)DestroyImmediate(mat);}
}
