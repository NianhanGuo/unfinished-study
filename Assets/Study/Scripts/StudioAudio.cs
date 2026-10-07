using UnityEngine;
// Deterministic original Foley-like synthesis, with no downloaded audio dependency.
public static class StudioAudio {
 public static AudioClip Rattle(){return Make("Castor friction",.65f,63,.7f,true);}
 public static AudioClip Cut(){return Make("Board slice and blade impact",.38f,120,.85f,false);}
 public static AudioClip Foot(){return Make("Ceiling contact",.17f,72,.5f,false);}
 public static AudioClip Neck(){return Make("Cloth under tension",.75f,49,.4f,true);}
 static AudioClip Make(string name,float duration,float hz,float noise,bool loop){
  int count=(int)(22050*duration);var data=new float[count];var random=new System.Random(1937);float low=0;
  for(int i=0;i<count;i++){float t=i/22050f;low=Mathf.Lerp(low,(float)random.NextDouble()*2-1,.15f);float env=loop?(.6f+.4f*Mathf.Sin(2*Mathf.PI*t/duration)):Mathf.Exp(-t*16)*Mathf.Min(t*250,1);data[i]=(low*noise+Mathf.Sin(t*hz*6.283f)*.15f)*env*.5f;}
  if(loop)for(int i=0;i<160;i++){float f=i/160f;data[i]*=f;data[count-1-i]*=f;}
  var clip=AudioClip.Create(name,count,1,22050,false);clip.SetData(data,0);return clip;
 }
}
