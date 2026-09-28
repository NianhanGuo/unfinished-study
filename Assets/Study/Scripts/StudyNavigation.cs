using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class StudyNavigation {
    const int W=29,H=37;const float Cell=.5f;static bool[] blocked;
    static Vector3 Pos(int n){return new Vector3((n%W-14)*Cell,0,(n/W-18)*Cell);}
    static int Node(Vector3 p){return Mathf.Clamp(Mathf.RoundToInt(p.z/Cell)+18,0,H-1)*W+Mathf.Clamp(Mathf.RoundToInt(p.x/Cell)+14,0,W-1);}
    public static void Build(){Physics.SyncTransforms();blocked=new bool[W*H];for(int n=0;n<blocked.Length;n++)blocked[n]=Physics.CheckBox(Pos(n)+Vector3.up,new Vector3(.4f,.85f,.4f),Quaternion.identity,1<<8);}
    public static List<Vector3> Path(Vector3 a,Vector3 b,Vector3? avoidCenter=null,float avoidRadius=0){
        if(blocked==null)Build();int start=Node(a),goal=Node(b);
        bool[] unavailable=blocked;
        if(avoidCenter.HasValue){
            unavailable=(bool[])blocked.Clone();Vector3 center=avoidCenter.Value;center.y=0;
            for(int n=0;n<unavailable.Length;n++)if(n!=start&&(Pos(n)-center).sqrMagnitude<avoidRadius*avoidRadius)unavailable[n]=true;
        }
        if(unavailable[goal]){float best=float.MaxValue;for(int n=0;n<blocked.Length;n++)if(!unavailable[n]){float d=(Pos(n)-b).sqrMagnitude;if(d<best){best=d;goal=n;}}}
        int[] parent=new int[W*H];float[] costs=new float[W*H];bool[] closed=new bool[W*H];for(int i=0;i<parent.Length;i++){parent[i]=-1;costs[i]=float.MaxValue;}costs[start]=0;
        List<int> open=new List<int>{start};int[] dx={1,-1,0,0,1,1,-1,-1},dz={0,0,1,-1,1,-1,1,-1};
        while(open.Count>0){int current=open[0];float best=float.MaxValue;foreach(int n in open){float f=costs[n]+Vector3.Distance(Pos(n),Pos(goal));if(f<best){best=f;current=n;}}
            open.Remove(current);if(current==goal){var route=new List<Vector3>();while(current!=start && current>=0){route.Add(Pos(current));current=parent[current];}route.Reverse();return route;}
            closed[current]=true;
            for(int k=0;k<8;k++){int x=current%W+dx[k],z=current/W+dz[k];if(x<0||x>=W||z<0||z>=H)continue;int n=z*W+x;
                if(unavailable[n]||closed[n])continue;
                if(k>=4&&(unavailable[current+dx[k]]||unavailable[current+dz[k]*W]))continue;
                float cost=costs[current]+(k<4?.5f:.7071f);if(cost>=costs[n])continue;costs[n]=cost;parent[n]=current;if(!open.Contains(n))open.Add(n);
            }
        }return new List<Vector3>();
    }
}
