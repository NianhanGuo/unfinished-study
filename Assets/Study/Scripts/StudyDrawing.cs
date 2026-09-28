using System;
using UnityEngine;

[Serializable] public class CharcoalStroke { public float[] xy,pressure;public float width,ink;public int seed; }
[Serializable] public class CharcoalPage { public CharcoalStroke[] strokes;public float[] stageEnds; }
[Serializable] public class CharcoalBook { public CharcoalPage[] pages; }
public class StudyDrawing : MonoBehaviour {
 public Renderer paper;public Transform pencilRig; public Texture2D[] backgrounds, finished, charcoalSources;
 public Texture2D[] Pages{get;private set;}
 public Vector2 PenUV{get;private set;}public bool TipDown{get;private set;}
 public int StrokeCount(int p){return book.pages[p].strokes.Length;}
 CharcoalBook book;Color32[][] pixels,basePixels,targetPixels;float[][] coverage;float[] rendered;float[][] costs;float[] totals;int width=768,height=1024,current=-1;Material material;
 public void Initialize(){
  width=backgrounds[0].width;height=backgrounds[0].height;
  book=JsonUtility.FromJson<CharcoalBook>(Resources.Load<TextAsset>("strokeBook").text);
  Pages=new Texture2D[3];pixels=new Color32[3][];rendered=new float[3];costs=new float[3][];totals=new float[3];
  for(int p=0;p<3;p++){costs[p]=new float[StrokeCount(p)+1];for(int i=0;i<StrokeCount(p);i++){var st=book.pages[p].strokes[i];float length=0;for(int j=2;j<st.xy.Length;j+=2)length+=Vector2.Distance(new Vector2(st.xy[j-2]*width,st.xy[j-1]*height),new Vector2(st.xy[j]*width,st.xy[j+1]*height));costs[p][i+1]=costs[p][i]+Mathf.Max(18,length)+9;}totals[p]=costs[p][StrokeCount(p)];}
  for(int i=0;i<3;i++){pixels[i]=backgrounds[i].GetPixels32();Pages[i]=new Texture2D(width,height,TextureFormat.RGBA32,false);Pages[i].SetPixels32(pixels[i]);Pages[i].Apply();}
  PrepareCharcoal();material=paper.material;Select(0);
 }
 void PrepareCharcoal(){
  if(charcoalSources==null||charcoalSources.Length<3)return;
  basePixels=new Color32[3][];targetPixels=new Color32[3][];coverage=new float[3][];
  for(int p=0;p<3;p++){
   basePixels[p]=(Color32[])pixels[p].Clone();targetPixels[p]=(Color32[])pixels[p].Clone();coverage[p]=new float[width*height];
   for(int y=0;y<height;y++)for(int x=0;x<width;x++){
    float u=(x-92f)/584f,v=(y-116f)/778f;if(u<0||u>1||v<0||v>1)continue;
    Color c=charcoalSources[p].GetPixelBilinear(u,v);float gray=(c.r*.299f+c.g*.587f+c.b*.114f)*255;
    float amount=Mathf.Clamp01((244-gray-7)/209);int i=y*width+x;Color b=basePixels[p][i];
    targetPixels[p][i]=Color.Lerp(b,new Color(.10f,.10f,.09f),amount);
   }
   finished[p]=new Texture2D(width,height,TextureFormat.RGBA32,false);finished[p].SetPixels32(targetPixels[p]);finished[p].Apply();
  }
 }
 public void Select(int p){if(current==p)return;current=p;material.mainTexture=Pages[p];}
 public string Stage(float progress){var ends=book.pages[Mathf.Max(0,current)].stageEnds;if(ends==null||ends.Length<3)return "CHARCOAL STUDY";return progress<ends[0]?"CONTOUR":progress<ends[1]?"DARK ACCENTS":progress<ends[2]?"HATCHING":"SOFT SHADOW";}
 public void RenderProgress(int p,float progress,bool marking){
  Select(p);float target=Mathf.Clamp01(progress)*totals[p],prev=rendered[p];
  int index=0;while(index<StrokeCount(p)-1&&costs[p][index+1]<=target)index++;
  if(target>prev){
   int first=0;while(first<StrokeCount(p)-1&&costs[p][first+1]<=prev)first++;
   for(int i=first;i<=index;i++){
    float duration=costs[p][i+1]-costs[p][i];DrawSection(p,book.pages[p].strokes[i],Mathf.Clamp01((prev-costs[p][i])/duration),Mathf.Clamp01((target-costs[p][i])/duration));
   }
   rendered[p]=target;if(progress>=1&&targetPixels!=null)pixels[p]=(Color32[])targetPixels[p].Clone();Pages[p].SetPixels32(pixels[p]);Pages[p].Apply(false);
  }
  float fraction=Mathf.Clamp01((target-costs[p][index])/(costs[p][index+1]-costs[p][index]));
  PenUV=Point(book.pages[p].strokes[index],Mathf.Clamp01(fraction/.88f));TipDown=marking&&fraction<.88f;
  if(pencilRig){
   Vector3 pos=new Vector3((PenUV.x-.5f)*.426f,.019f,(.5f-PenUV.y)*.562f);
   float lift=marking?(fraction>.88f?Mathf.Sin((fraction-.88f)/.12f*Mathf.PI)*.014f:0):.035f;
   pos.y+=lift;pencilRig.localPosition=Vector3.Lerp(pencilRig.localPosition,pos,Time.deltaTime*32);
   pencilRig.localRotation=Quaternion.Euler(0,Mathf.Sin(Time.time*1.7f)*3,Mathf.Sin(Time.time*2)*1.2f);
  }
 }
 Vector2 Point(CharcoalStroke st,float t){int count=st.xy.Length/2;float n=Mathf.Clamp01(t)*(count-1);int i=Mathf.Min((int)n,count-2);return Vector2.Lerp(new Vector2(st.xy[i*2],st.xy[i*2+1]),new Vector2(st.xy[i*2+2],st.xy[i*2+3]),n-i);}
 float Pressure(CharcoalStroke st,float t){
  if(st.pressure==null||st.pressure.Length<2)return 1;
  float q=t*(st.pressure.Length-1);int i=Mathf.Min((int)q,st.pressure.Length-2);return Mathf.Lerp(st.pressure[i],st.pressure[i+1],q-i);
 }
 void DrawSection(int p,CharcoalStroke st,float from,float to){
  from=Mathf.Clamp01(from/.88f);to=Mathf.Clamp01(to/.88f);if(to<=from)return;
  // A fixed sample lattice makes pigment coverage independent of frame rate.
  int samples=Mathf.Max(2,(st.xy.Length/2-1)*3);
  int start=from<=0?0:Mathf.FloorToInt(from*samples)+1,end=Mathf.FloorToInt(to*samples);
  for(int j=start;j<=end;j++){
   float t=j/(float)samples;float pressure=Pressure(st,t);Vector2 point=Point(st,t);
   Stamp(p,new Vector2(point.x*width,(1-point.y)*height),Mathf.Max(.42f,st.width*pressure),st.ink*(.48f+.52f*pressure),st.seed);
  }
 }
 void Stamp(int p,Vector2 point,float radius,float opacity,int seed){
  int x0=Mathf.RoundToInt(point.x),y0=Mathf.RoundToInt(point.y),r=Mathf.CeilToInt(radius);
  for(int dy=-r;dy<=r;dy++)for(int dx=-r;dx<=r;dx++){
   int x=x0+dx,y=y0+dy;if(x<0||y<0||x>=width||y>=height)continue;
   float dist=Vector2.Distance(new Vector2(x,y),point);if(dist>radius+.28f)continue;
   uint hash=unchecked((uint)(x*374761393+y*668265263));hash=(hash^(hash>>13))*1274126177u;float tooth=(hash&1023)/1023f;
   float grain=tooth<.16f?.065f:.32f+.68f*tooth;
   float drag=.72f+.28f*Mathf.Sin(x*.17f+y*.09f+seed);
   float strength=Mathf.Clamp01(opacity*.76f*Mathf.Clamp01(1-dist/(radius+.4f))*grain*drag);
   int n=y*width+x;
   if(targetPixels!=null){
    float deposit=Mathf.Clamp01(opacity*1.8f*(1-dist/(radius+.4f)));
    coverage[p][n]=Mathf.Clamp01(coverage[p][n]+deposit);pixels[p][n]=Color32.Lerp(basePixels[p][n],targetPixels[p][n],coverage[p][n]);
   }else{Color32 c=pixels[p][n];c.r=(byte)Mathf.Lerp(c.r,32,strength);c.g=(byte)Mathf.Lerp(c.g,30,strength);c.b=(byte)Mathf.Lerp(c.b,27,strength);pixels[p][n]=c;}
  }
 }
 void Segment(int p,Vector2 a,Vector2 b,float radius,float opacity){
  a=new Vector2(a.x*width,(1-a.y)*height);b=new Vector2(b.x*width,(1-b.y)*height);int count=Mathf.Max(1,Mathf.CeilToInt(Vector2.Distance(a,b)*1.3f));
  for(int i=0;i<=count;i++){
   Vector2 point=Vector2.Lerp(a,b,i/(float)count);int x0=Mathf.RoundToInt(point.x),y0=Mathf.RoundToInt(point.y);int r=Mathf.CeilToInt(radius);
   for(int dy=-r;dy<=r;dy++)for(int dx=-r;dx<=r;dx++){
    int x=x0+dx,y=y0+dy;if(x<0||y<0||x>=width||y>=height)continue;
    float dist=Mathf.Sqrt(dx*dx+dy*dy);if(dist>radius)continue;
    float grain=.48f+.52f*Mathf.Abs(Mathf.Sin(x*12.92f+y*78.23f));float strength=opacity*.3f*(1-dist/(radius+.5f))*grain;
    int n=y*width+x;Color32 c=pixels[p][n];c.r=(byte)Mathf.Lerp(c.r,29,strength);c.g=(byte)Mathf.Lerp(c.g,27,strength);c.b=(byte)Mathf.Lerp(c.b,24,strength);pixels[p][n]=c;
   }
  }
 }
 public void Ruin(int p){Segment(p,new Vector2(.28f,.79f),new Vector2(.82f,.2f),5,.9f);Segment(p,new Vector2(.3f,.82f),new Vector2(.84f,.21f),2,.8f);Pages[p].SetPixels32(pixels[p]);Pages[p].Apply();}
}
