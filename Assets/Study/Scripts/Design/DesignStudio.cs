using UnityEngine;
// Contemporary studio dressing; all detail is non-solid so authored escape routes stay clear.
public static class DesignStudio {
 static Transform root;static Material ink,tape;
 static GameObject Box(string n,Vector3 p,Vector3 s,Material m){return DesignPrototype.Shape(n,root,PrimitiveType.Cube,p,s,m);}
 static Material Mat(string name,Color color,float smooth=.1f){var m=new Material(Shader.Find("Standard"));m.name=name;m.color=color;m.SetFloat("_Glossiness",smooth);return m;}
 static void Grain(Material m,int seed,float strength,int axis){
  var t=new Texture2D(128,128,TextureFormat.RGB24,false);var rng=new System.Random(seed);
  for(int y=0;y<128;y++)for(int x=0;x<128;x++){float n=(float)rng.NextDouble();float v=.8f+(n-.5f)*strength+(axis>0?Mathf.Sin(x*.55f+Mathf.Sin(y*.04f))*.035f:0);t.SetPixel(x,y,new Color(v,v,v));}
  t.wrapMode=TextureWrapMode.Repeat;t.Apply();m.mainTexture=t;m.mainTextureScale=new Vector2(3,3);
 }
 public static void Dress(DesignPrototype g,Material wall,Material wood,Material metal,Material paper,Material mat,Material skin){
  root=new GameObject("Studio / material details").transform;root.SetParent(g.transform);ink=Mat("Charcoal marks",new Color(.13f,.15f,.14f));tape=Mat("Masking tape",new Color(.56f,.48f,.31f));
  Grain(wood,27,.15f,1);Grain(wall,91,.11f,0);Grain(paper,8,.08f,0);skin.SetFloat("_Glossiness",.26f);metal.SetFloat("_Glossiness",.12f);
  var floor=Mat("Paint-spattered oak",new Color(.23f,.18f,.13f));Grain(floor,39,.12f,1);floor.mainTextureScale=new Vector2(11,14);g.transform.Find("Floor").GetComponent<Renderer>().sharedMaterial=floor;
  // Board joints are authored in the floor material, with staggered ends.
  for(int z=-12;z<=12;z+=6){Box("Ceiling crossbeam",new Vector3(0,5.3f,z+1.3f),new Vector3(21.8f,.32f,.22f),metal);}
  for(int x=-9;x<=9;x+=18)Box("Ceiling service pipe",new Vector3(x,5.22f,0),new Vector3(.08f,.09f,27),metal);
  foreach(var p in g.stations){
   for(int i=0;i<9;i++)Box("Cutting mat grid",p+new Vector3(-1+i*.25f,1.053f,0),new Vector3(.006f,.005f,.72f),tape);
   for(int j=-2;j<=2;j++)Box("Mat cross grid",p+new Vector3(0,1.054f,j*.15f),new Vector3(2.15f,.005f,.006f),tape);
   Box("Taped task card",p+new Vector3(-.9f,1.073f,-.15f),new Vector3(.3f,.008f,.25f),paper);
   Box("Task card tape",p+new Vector3(-.9f,1.079f,-.25f),new Vector3(.12f,.008f,.05f),tape);
   for(int j=0;j<3;j++)Box("Pencil note",p+new Vector3(-.9f,1.079f,-.2f+j*.05f),new Vector3(.19f-j*.03f,.003f,.006f),ink);
   Box("Steel ruler",p+new Vector3(.96f,1.07f,.13f),new Vector3(.065f,.015f,.62f),metal);
  }
  var rng=new System.Random(40);
  foreach(var p in g.tables){
   for(int i=0;i<5;i++){float x=(float)rng.NextDouble()*2.3f-1.15f,z=(float)rng.NextDouble()*1.5f-.75f;var scrap=Box("Foamboard offcut",p+new Vector3(x,1.22f,z),new Vector3(.12f+i*.03f,.012f,.23f),paper);scrap.transform.rotation=Quaternion.Euler(0,i*31,0);}
   Box("Folded task sheet",p+new Vector3(1,1.22f,.3f),new Vector3(.4f,.01f,.55f),paper);
   // End markings identify traversable cover without filling the opening with props.
   foreach(float z in new[]{-1.15f,1.15f})Box("Worn table edge",p+new Vector3(0,1.19f,z),new Vector3(3.3f,.012f,.022f),tape);
  }
  foreach(var p in new[]{new Vector3(-3.5f,2.65f,3),new Vector3(3.5f,2.65f,-5)})for(int side=-1;side<=1;side+=2)for(int i=0;i<3;i++){
   var center=p+new Vector3(side*.165f,-.2f,-2+i*2);
   Box("Critique drawing paper",center,new Vector3(.01f,1.2f,.8f),paper);
   for(int j=0;j<4;j++)Box("Study line",center+new Vector3(side*.013f,-.3f+j*.2f,0),new Vector3(.008f,.009f,.52f-j*.045f),ink);
   Box("Tutor's red annotation",center+new Vector3(side*.022f,.43f,.18f),new Vector3(.006f,.025f,.23f),Mat("Red pencil",new Color(.38f,.12f,.07f)));
  }
  // Window rhythm and warm workstation pools keep edges readable against the cool ceiling.
  for(int z=-9;z<=9;z+=6){Box("Window recess",new Vector3(10.8f,2.7f,z),new Vector3(.07f,2.5f,2.8f),ink);for(int j=-1;j<=1;j++)Box("Window mullion",new Vector3(10.72f,2.7f,z+j*1.3f),new Vector3(.08f,2.5f,.07f),metal);}
  var light=new GameObject("Window / cool edge light");light.transform.SetParent(root);light.transform.position=new Vector3(9,3,0);var l=light.AddComponent<Light>();l.type=LightType.Point;l.range=17;l.intensity=1.1f;l.color=new Color(.35f,.48f,.62f);
 }
}
