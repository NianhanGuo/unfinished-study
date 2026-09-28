using UnityEngine;
// Separate sources preserve the existing room, pencil and wheel sound settings.
public class StudySoundscape : MonoBehaviour {
 StudyGame game;AudioSource musicA,musicB,heart;AudioClip beat;float clock,rate=62;bool crossfading;float cross;const float musicLevel=.19f;
 public float BPM=>rate;
 public static float RateForDistance(float metres)=>Mathf.Lerp(62,144,Mathf.SmoothStep(0,1,Mathf.InverseLerp(9,.85f,metres)));
 public void Initialize(StudyGame g){game=g;var clip=Resources.Load<AudioClip>("HorrorMusic");musicA=Source(clip);musicB=Source(clip);musicA.volume=0;musicA.Play();beat=Resources.Load<AudioClip>("Heartbeat");heart=Source(beat);clock=.5f;}
 AudioSource Source(AudioClip clip){var s=gameObject.AddComponent<AudioSource>();s.clip=clip;s.playOnAwake=false;s.loop=false;s.spatialBlend=0;s.volume=0;s.priority=90;return s;}
 void Update(){
  if(!game)return;float dt=Time.unscaledDeltaTime;bool active=game.phase==StudyGame.Phase.Playing;
  float nearest=100;foreach(var skeleton in game.skeletons){Vector3 d=skeleton.transform.position-game.body.transform.position;d.y=0;nearest=Mathf.Min(nearest,d.magnitude);}
  rate=Mathf.Lerp(rate,RateForDistance(nearest),1-Mathf.Exp(-dt*2.5f));
  heart.volume=Mathf.MoveTowards(heart.volume,active?Mathf.Lerp(.10f,.30f,Mathf.InverseLerp(62,144,rate)):0,dt*.8f);
  if(active){clock+=dt*rate/60;if(clock>=1){clock-=1;heart.PlayOneShot(beat);}}else{clock=.6f;if(heart.volume<=0)heart.Stop();}
  float target=game.phase==StudyGame.Phase.Lost?0:game.phase==StudyGame.Phase.Won?.045f:game.phase==StudyGame.Phase.Paused?.065f:musicLevel;
  // Crossfade the supplied track at the loop boundary, avoiding a hard cut or click.
  if(musicA.clip&&musicA.time>=musicA.clip.length-3&&!crossfading){musicB.time=0;musicB.Play();crossfading=true;cross=0;}
  if(crossfading){cross=Mathf.Clamp01(cross+dt/3);musicA.volume=target*(1-cross);musicB.volume=target*cross;if(cross>=1){musicA.Stop();var old=musicA;musicA=musicB;musicB=old;crossfading=false;}}
  else{musicA.volume=Mathf.MoveTowards(musicA.volume,target,dt*.14f);musicB.volume=0;}
 }
}
