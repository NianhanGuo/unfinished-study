using System.Collections.Generic;
using UnityEngine;
public static class DesignNavigation {
 const int W=21,H=27;static bool[] blocked;
 static Vector3 Position(int n)=>new Vector3(n%W-10,4.7f,n/W-13);
 static int Node(Vector3 p)=>Mathf.Clamp(Mathf.RoundToInt(p.z)+13,0,H-1)*W+Mathf.Clamp(Mathf.RoundToInt(p.x)+10,0,W-1);
 public static void Build(){Physics.SyncTransforms();blocked=new bool[W*H];for(int i=0;i<blocked.Length;i++)blocked[i]=Physics.CheckBox(Position(i),new Vector3(.65f,.35f,.65f),Quaternion.identity,DesignPrototype.SolidMask);}
 public static List<Vector3> Path(Vector3 a,Vector3 b){
  if(blocked==null)Build();int start=Node(a),goal=Node(b);if(blocked[goal]){float best=float.MaxValue;for(int i=0;i<blocked.Length;i++)if(!blocked[i]){float d=(Position(i)-b).sqrMagnitude;if(d<best){goal=i;best=d;}}}
  var queue=new Queue<int>();var parents=new int[blocked.Length];for(int i=0;i<parents.Length;i++)parents[i]=-2;parents[start]=-1;queue.Enqueue(start);
  while(queue.Count>0){int n=queue.Dequeue();if(n==goal){var route=new List<Vector3>();while(n!=start){route.Add(Position(n));n=parents[n];}route.Reverse();return route;}
   foreach(int delta in new[]{-1,1,-W,W}){int next=n+delta;if(next<0||next>=blocked.Length||blocked[next]||parents[next]!=-2)continue;if(Mathf.Abs(next%W-n%W)>1)continue;parents[next]=n;queue.Enqueue(next);}
  }return new List<Vector3>();
 }
}
