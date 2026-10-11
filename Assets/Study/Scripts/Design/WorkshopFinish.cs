using UnityEngine;
// Replaces visual shells only. Existing escape-route collision stays authoritative.
public static class WorkshopFinish {
 static void Spawn(string name,Transform root,Vector3 position){var prefab=Resources.Load<GameObject>("Workshop/"+name);if(!prefab)throw new System.InvalidOperationException("Missing workshop art: "+name);var o=Object.Instantiate(prefab,root,false);o.transform.localPosition=position;}
 public static void Apply(DesignPrototype g,Material wood,Material paper,Material rubber){
  Material timber=Resources.Load<Material>("Workshop/Wood"),stock=Resources.Load<Material>("Workshop/Paper"),mat=Resources.Load<Material>("Workshop/Mat");
  foreach(var r in g.GetComponentsInChildren<Renderer>()){
   string name=r.gameObject.name;
   if(name=="Crawl-under worktable"||name=="Table leg"||name.StartsWith("Assignment station ")||name=="Steel ruler"||name=="Cutter bed"||name=="Cutting stock"){r.enabled=false;continue;}
   if(name=="Cutting mat grid"||name=="Mat cross grid"||name=="Worn table edge"){r.enabled=false;continue;}
   if(name=="Full-height pin-up divider"){r.sharedMaterial=Resources.Load<Material>("Workshop/Cork");continue;}
   if(r.sharedMaterial==wood)r.sharedMaterial=timber;
   else if(r.sharedMaterial==paper)r.sharedMaterial=stock;
   else if(r.sharedMaterial==rubber)r.sharedMaterial=mat;
  }
  Spawn("Cutter",g.transform,g.stations[0]);
  foreach(var p in g.tables)Spawn("Worktable",g.transform,p);
  foreach(var p in g.stations){Spawn("Cabinet",g.transform,p);Spawn("Ruler",g.transform,p+new Vector3(.96f,1.065f,.13f));Spawn("PaperStack",g.transform,p+new Vector3(-.98f,1.063f,.07f));}
  var floor=g.transform.Find("Floor").GetComponent<Renderer>();var floorMaterial=new Material(Resources.Load<Material>("Workshop/Floor"));floorMaterial.mainTextureScale=new Vector2(4,14);floor.sharedMaterial=floorMaterial;
  // One owned runtime material, released with the scene.
  g.gameObject.AddComponent<WorkshopFinishLifetime>().owned=floorMaterial;
 }
}
public class WorkshopFinishLifetime:MonoBehaviour {public Material owned;void OnDestroy(){if(owned)Destroy(owned);}}
