using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StudySkeleton : MonoBehaviour {
    public Transform torso, skull, leftArm, rightArm;
    public AudioClip wheelClip; public bool enableWheelSound=false; StudyGame game;AudioSource wheels;
    List<Vector3> path;int next;float repath, motion; public bool frozen; float touchTime; Vector3 flankGoal; float flankUntil;
    public bool RearStalker => GetComponent<StudySubject>().index==1;
    public void Init(StudyGame g){game=g;if(RearStalker){
        Vector3 rear=RearDestination(2.6f);if((rear-game.body.transform.position).sqrMagnitude>2.25f)transform.position=rear;
    }enableWheelSound=true;if(!wheelClip)wheelClip=StudioAudio.Rattle();wheels=gameObject.AddComponent<AudioSource>();wheels.clip=wheelClip;wheels.loop=true;wheels.spatialBlend=1;wheels.minDistance=.6f;wheels.maxDistance=13;wheels.rolloffMode=AudioRolloffMode.Linear;wheels.volume=0;wheels.Play();}
    Vector3 RearDestination(float radius){
        Vector3 center=game.body.transform.position;center.y=0;
        Vector3 rear=-game.body.transform.forward;rear.y=0;rear.Normalize();
        for(int i=0;i<7;i++){
            float angle=(i==0?0:((i+1)/2)*20*(i%2==0?-1:1));
            Vector3 candidate=center+Quaternion.Euler(0,angle,0)*rear*radius;
            if(Mathf.Abs(candidate.x)>6.8f||Mathf.Abs(candidate.z)>8.8f)continue;
            if(!Physics.CheckBox(candidate+Vector3.up,new Vector3(.4f,.85f,.4f),Quaternion.identity,1<<8))return candidate;
        }
        return transform.position;
    }
    public Vector3 PursuitDestination(){
        Vector3 center=game.body.transform.position;center.y=0;
        if(!RearStalker)return center;
        Vector3 offset=transform.position-center;offset.y=0;
        Vector3 rear=-game.body.transform.forward;rear.y=0;rear.Normalize();
        float angle=Vector3.SignedAngle(offset,rear,Vector3.up);
        if(Mathf.Abs(angle)<35)return center;
        // Short arc waypoints keep flanking movement outside the player's personal space.
        float radius=Mathf.Max(2.4f,offset.magnitude);
        Vector3 arc=center+Quaternion.Euler(0,Mathf.Clamp(angle,-35,35),0)*offset.normalized*radius;
        if(Mathf.Abs(arc.x)<6.8f&&Mathf.Abs(arc.z)<8.8f&&!Physics.CheckBox(arc+Vector3.up,new Vector3(.4f,.85f,.4f),Quaternion.identity,1<<8))return arc;
        return RearDestination(2.4f);
    }
    public void Silence(){if(wheels)wheels.volume=0;}
    void Update(){
        if(!game || game.phase!=StudyGame.Phase.Playing){Silence();return;}
        if(GetComponent<StudySubject>().index==1&&!game.hasDrawn){Silence();return;}
        Vector3 point=torso.position+Vector3.up*.25f;
        Vector3 contact=game.body.transform.position-transform.position;contact.y=0;
        frozen=game.Illuminated(point)||game.Illuminated(skull.position);
        if(wheels)wheels.volume=frozen?0:Mathf.Lerp(.16f,.025f,Mathf.InverseLerp(1,9,contact.magnitude));
        if(frozen){touchTime=0;return;}
        // A brief visible reach allows a last-moment light save; no pre-light instant kill.
        touchTime=contact.magnitude<.83f?touchTime+Time.deltaTime:0;
        if(touchTime>.22f){game.Finish(false);return;}
        if(!game.hasDrawn&&game.elapsed<15){Silence();return;}
        motion+=Time.deltaTime;
        Vector3 destination=PursuitDestination();
        if(RearStalker&&(destination-game.body.transform.position).sqrMagnitude>1){
            if(Time.time>=flankUntil||(transform.position-flankGoal).sqrMagnitude<.15f){flankGoal=destination;flankUntil=Time.time+1.4f;}
            destination=flankGoal;
        }Vector3 d=game.body.transform.position-transform.position;d.y=0;
        float dist=d.magnitude;

        repath-=Time.deltaTime;
        if(repath<=0){bool flanking=RearStalker&&(destination-game.body.transform.position).sqrMagnitude>1;
            path=StudyNavigation.Path(transform.position,destination,flanking?(Vector3?)game.body.transform.position:null,Mathf.Min(1.7f,Mathf.Max(0,dist-.6f)));next=0;repath=.25f;}
        if(path!=null && next<path.Count){Vector3 step=path[next]-transform.position;step.y=0;
            if(step.magnitude<.15f)next++;
            else transform.position+=step.normalized*((RearStalker&&(destination-game.body.transform.position).sqrMagnitude>1?StudyTuning.FlankSpeed:.46f)+Mathf.Min(game.elapsed/500,.12f))*Time.deltaTime;
        }
        if(d.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(d),Time.deltaTime*1.9f);
        float reach=Mathf.InverseLerp(7,1,dist);
        torso.localRotation=Quaternion.Euler(8+reach*22+Mathf.Sin(motion*.8f)*2,0,Mathf.Sin(motion*.55f)*3);
        leftArm.localRotation=Quaternion.Euler(-reach*66-12,0,-8);
        rightArm.localRotation=Quaternion.Euler(-reach*78-6,0,9);
        skull.localRotation=Quaternion.Euler(-reach*18,Mathf.Sin(motion*.33f)*9,7+Mathf.Sin(motion*.5f)*5);
    }
}

