using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class StudySubject : MonoBehaviour {
    public string title; public int index; public Transform focus;
    [NonSerialized] public float progress;
    Vector3[] observationPoints;
    public int ObservationPointCount => index==2 ? 4 : 1;
    public Vector3 ObservationPoint(int point){
        if(index!=2 || point==0)return focus.position;
        // The bust and both pears are valid targets, not just the empty space between them.
        if(observationPoints==null)observationPoints=new[]{new Vector3(-.48f,1.65f,0),new Vector3(.38f,1.34f,-.15f),new Vector3(.76f,1.34f,.04f)};
        return transform.TransformPoint(observationPoints[point-1]);
    }
}
