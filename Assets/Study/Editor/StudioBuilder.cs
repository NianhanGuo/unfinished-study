using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEditor.Build.Reporting;
using System.IO;
using System.Collections.Generic;

public static class StudioBuilder {
    static string root="Assets/Study/Generated"; static Material bone, dark, metal, wood, wall, canvas, floor, paper, brass;
    static int meshId; static Transform architecture; static List<Light> lamps=new List<Light>();
    static Vector3 V(float x,float y,float z){return new Vector3(x,y,z);}
    static Material Mat(string name,Color col,float smooth=.2f,float metallic=0){
        Material m=new Material(Shader.Find("Standard"));m.name=name;m.color=col;m.SetFloat("_Glossiness",smooth);m.SetFloat("_Metallic",metallic);AssetDatabase.CreateAsset(m,root+"/"+name+".mat");return m;
    }
    static Transform Group(string name,Transform parent,Vector3 pos){var g=new GameObject(name).transform;g.SetParent(parent,false);g.localPosition=pos;return g;}
    static GameObject Shape(string name,PrimitiveType type,Transform parent,Vector3 pos,Vector3 scale,Material mat,bool solid=false){
        GameObject g=GameObject.CreatePrimitive(type);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=pos;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=mat;
        if(!solid)Object.DestroyImmediate(g.GetComponent<Collider>());else g.layer=8;return g;
    }
    static GameObject Box(string n,Transform p,Vector3 v,Vector3 s,Material m,bool solid=false){return Shape(n,PrimitiveType.Cube,p,v,s,m,solid);}
    static GameObject Ball(string n,Transform p,Vector3 v,Vector3 s,Material m){return Shape(n,PrimitiveType.Sphere,p,v,s,m);}
    static GameObject Rod(string n,Transform p,Vector3 a,Vector3 b,float r,Material m){
        GameObject g=Shape(n,PrimitiveType.Cylinder,p,(a+b)/2,V(r*2,(b-a).magnitude/2,r*2),m);g.transform.localRotation=Quaternion.FromToRotation(Vector3.up,b-a);return g;
    }
    static GameObject Tube(string n,Transform p,List<Vector3> path,float r,Material m){
        int sides=7;List<Vector3> verts=new List<Vector3>();List<int> tris=new List<int>();
        for(int i=0;i<path.Count;i++){
            Vector3 tangent=path[Mathf.Min(i+1,path.Count-1)]-path[Mathf.Max(0,i-1)];Vector3 normal=Vector3.Cross(tangent.normalized,Vector3.up).normalized;if(normal.sqrMagnitude<.01f)normal=Vector3.right;Vector3 binormal=Vector3.Cross(tangent.normalized,normal);
            for(int j=0;j<sides;j++)verts.Add(path[i]+r*(normal*Mathf.Cos(j*Mathf.PI*2/sides)+binormal*Mathf.Sin(j*Mathf.PI*2/sides)));
            if(i>0)for(int j=0;j<sides;j++){int a=(i-1)*sides+j,b=(i-1)*sides+(j+1)%sides,c=i*sides+j,d=i*sides+(j+1)%sides;tris.AddRange(new[]{a,b,c,b,d,c});}
        }
        Mesh mesh=new Mesh();mesh.name=n;mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,root+"/Mesh_"+(meshId++)+".asset");
        GameObject g=new GameObject(n,typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(p,false);g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<Renderer>().sharedMaterial=m;return g;
    }
    static void Bone(Transform p,string n,Vector3 a,Vector3 b,float radius){
        Rod(n,p,a,b,radius,bone);Ball(n+" joint A",p,a,Vector3.one*radius*2.7f,bone);Ball(n+" joint B",p,b,Vector3.one*radius*2.7f,bone);
    }
    static Transform Head(Transform parent,Vector3 pos,bool plaster=false){
        Transform h=Group(plaster?"Plaster head":"Skull pivot",parent,pos);Material m=plaster?canvas:bone;
        Ball("Cranium",h,V(0,.1f,0),V(.32f,.37f,.30f),m);
        Ball("Face",h,V(0,-.015f,.06f),V(.255f,.24f,.24f),m);
        if(plaster){
            Ball("Nose",h,V(0,.005f,.195f),V(.07f,.13f,.08f),m);
            for(int side=-1;side<=1;side+=2){Ball("Brow",h,V(side*.076f,.072f,.147f),V(.115f,.035f,.05f),m);Ball("Eye",h,V(side*.076f,.035f,.157f),V(.065f,.028f,.018f),dark);}
            Rod("Neck",h,V(0,-.3f,0),V(0,-.12f,0),.09f,m);
        }else{
            for(int side=-1;side<=1;side+=2){Ball("Eye socket",h,V(side*.074f,.055f,.148f),V(.105f,.097f,.023f),dark);Ball("Cheekbone",h,V(side*.114f,-.01f,.128f),V(.066f,.052f,.061f),m);}
            Ball("Nasal cavity",h,V(0,-.017f,.171f),V(.048f,.067f,.018f),dark);
            Box("Mouth gap",h,V(0,-.099f,.142f),V(.155f,.038f,.04f),dark);
            for(int i=0;i<8;i++)Box("Tooth",h,V((i-3.5f)*.020f,-.095f,.167f),V(.016f,.028f,.021f),m);
            Tube("Mandible",h,new List<Vector3>{V(-.13f,-.04f,0),V(-.105f,-.15f,.08f),V(-.055f,-.165f,.15f),V(.055f,-.165f,.15f),V(.105f,-.15f,.08f),V(.13f,-.04f,0)},.026f,m);
        }return h;
    }
    // Adult study proportions: crown 2.04 m, soles .20 m, skull .25 m.
    // Landmarks are authored in metres; the stand is not part of body height.
    static void LongBone(Transform p,string n,Vector3 a,Vector3 b,float radius){
        Vector3 axis=(b-a).normalized,side=Vector3.Cross(axis,Vector3.forward).normalized;
        if(side.sqrMagnitude<.01f)side=Vector3.right;
        Vector3 front=Vector3.Cross(axis,side);var verts=new List<Vector3>();var tris=new List<int>();
        float[] profile={1.38f,1.6f,1.12f,.8f,.76f,.86f,1.22f,1.5f,1.32f};int sides=12;
        for(int i=0;i<profile.Length;i++)for(int j=0;j<sides;j++){
            float t=i/(float)(profile.Length-1),angle=j*Mathf.PI*2/sides;
            verts.Add(Vector3.Lerp(a,b,t)+front*Mathf.Sin(t*Mathf.PI)*radius*.45f+(side*Mathf.Cos(angle)+front*Mathf.Sin(angle)*.84f)*radius*profile[i]);
            if(i>0){int v=(i-1)*sides+j,w=(i-1)*sides+(j+1)%sides;tris.AddRange(new[]{v,w,v+sides,w,w+sides,v+sides});}
        }
        for(int j=1;j<sides-1;j++){tris.AddRange(new[]{0,j,j+1});int q=(profile.Length-1)*sides;tris.AddRange(new[]{q,q+j+1,q+j});}
        Mesh mesh=new Mesh();mesh.SetVertices(verts);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,root+"/Mesh_"+(meshId++)+".asset");
        var o=new GameObject(n,typeof(MeshFilter),typeof(MeshRenderer));o.transform.SetParent(p,false);o.GetComponent<MeshFilter>().sharedMesh=mesh;o.GetComponent<MeshRenderer>().sharedMaterial=bone;
    }
    static List<Vector3> SmoothLoop(Vector3[] controls,int subdivisions=6){
        var points=new List<Vector3>();for(int i=0;i<controls.Length;i++)for(int j=0;j<subdivisions;j++){
            Vector3 a=controls[(i+controls.Length-1)%controls.Length],b=controls[i],c=controls[(i+1)%controls.Length],d=controls[(i+2)%controls.Length];float t=j/(float)subdivisions;
            points.Add(.5f*(2*b+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t));
        }return points;
    }
    static void IliacBlade(Transform p,int side){
        Vector3[] edge={V(.032f,.065f,-.035f),V(.064f,.126f,-.068f),V(.118f,.128f,-.075f),V(.164f,.084f,-.054f),V(.173f,.039f,-.012f),V(.134f,-.015f,.019f),V(.097f,-.056f,.03f),V(.06f,-.032f,.018f)};
        var loop=SmoothLoop(edge);int count=loop.Count,rings=8;var vertices=new List<Vector3>();var triangles=new List<int>();
        for(int face=0;face<2;face++)for(int r=0;r<rings;r++)for(int i=0;i<count;i++){
            float t=.025f+r/(float)(rings-1)*.975f;Vector3 v=Vector3.Lerp(V(.092f,.042f,-.05f),loop[i],t);v.z-=Mathf.Sin(t*Mathf.PI)*.028f;v.z+=(face==0?.003f:-.003f);v.x*=side;vertices.Add(v);
        }
        for(int face=0;face<2;face++)for(int r=0;r<rings-1;r++)for(int i=0;i<count;i++){
            int a=face*rings*count+r*count+i,b=face*rings*count+r*count+(i+1)%count,c=a+count,d=b+count;
            if((face==0)==(side>0))triangles.AddRange(new[]{a,c,b,b,c,d});else triangles.AddRange(new[]{a,b,c,b,d,c});
        }
        Mesh mesh=new Mesh();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,root+"/Mesh_"+(meshId++)+".asset");
        var g=new GameObject("Iliac blade",typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(p,false);g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<MeshRenderer>().sharedMaterial=bone;
        var rim=new List<Vector3>();for(int i=0;i<26;i++){Vector3 v=loop[i];v.x*=side;rim.Add(v);}Tube("Iliac crest",p,rim,.0065f,bone);
        var opening=SmoothLoop(new[]{V(side*.12f,-.035f,.025f),V(side*.125f,-.086f,.06f),V(side*.075f,-.127f,.09f),V(side*.024f,-.103f,.092f),V(side*.042f,-.05f,.077f)});opening.Add(opening[0]);Tube("Obturator ring",p,opening,.0095f,bone);
    }
    static Transform Arm(Transform torso,int side){
        Transform arm=Group(side<0?"Left shoulder":"Right shoulder",torso,V(side*.205f,.565f,0));
        LongBone(arm,"Humerus",V(0,0,0),V(side*.023f,-.32f,.006f),.0165f);
        Ball("Humeral head",arm,V(0,-.005f,0),V(.047f,.05f,.047f),bone);
        LongBone(arm,"Radius",V(side*.023f-.011f,-.323f,.009f),V(side*.037f-.012f,-.603f,.022f),.008f);
        LongBone(arm,"Ulna",V(side*.023f+.011f,-.323f,.004f),V(side*.037f+.012f,-.603f,.02f),.007f);
        for(int row=0;row<2;row++)for(int col=0;col<4;col++)Ball("Carpal",arm,V(side*.037f+(col-1.5f)*.009f,-.611f-row*.012f,.024f),V(.01f,.012f,.013f),bone);
        for(int f=0;f<4;f++){
            float x=side*.037f+(f-1.5f)*.014f;float length=.087f-Mathf.Abs(f-1)*.009f;
            Vector3 a=V(x,-.627f,.024f),b=V(x+(f-1.5f)*.003f,-.678f,.027f),c=b+V((f-1.5f)*.003f,-length*.46f,.007f),d=c+V(0,-length*.32f,.009f),e=d+V(0,-length*.22f,.008f);
            LongBone(arm,"Metacarpal",a,b,.0047f);LongBone(arm,"Proximal phalanx",b,c,.0042f);LongBone(arm,"Middle phalanx",c,d,.0035f);LongBone(arm,"Distal phalanx",d,e,.0028f);
        }
        LongBone(arm,"Thumb metacarpal",V(side*.018f,-.624f,.026f),V(-side*.006f,-.657f,.035f),.005f);
        LongBone(arm,"Thumb proximal",V(-side*.006f,-.657f,.035f),V(-side*.019f,-.68f,.045f),.0043f);
        LongBone(arm,"Thumb distal",V(-side*.019f,-.68f,.045f),V(-side*.021f,-.699f,.055f),.0035f);
        return arm;
    }
    static StudySkeleton Skeleton(string n,Vector3 pos,int index,AudioClip wheel){
        Transform stand=Group(n,null,pos);
        Shape("Cast iron base",PrimitiveType.Cylinder,stand,V(0,.13f,0),V(.16f,.055f,.16f),metal);
        for(int i=0;i<5;i++){
            float a=i*Mathf.PI*2/5;Vector3 b=V(Mathf.Cos(a)*.34f,.12f,Mathf.Sin(a)*.34f);Rod("Base spoke",stand,V(0,.17f,0),b,.021f,metal);
            GameObject w=Shape("Caster",PrimitiveType.Cylinder,stand,b-V(0,.045f,0),V(.09f,.03f,.09f),dark);w.transform.localRotation=Quaternion.Euler(0,0,90);
        }
        Rod("Steel suspension upright",stand,V(0,.16f,-.15f),V(0,2.13f,-.15f),.014f,metal);
        Tube("Suspension hook",stand,new List<Vector3>{V(0,2.13f,-.15f),V(0,2.19f,-.1f),V(0,2.18f,.015f),V(0,2.027f,.015f)},.009f,metal);
        Transform torso=Group("Spine hinge",stand,V(0,1.09f,0));
        for(int i=0;i<24;i++){
            float t=i/23f,y=.015f+t*.69f,z=-.036f-.027f*Mathf.Sin(t*Mathf.PI*2);float width=Mathf.Lerp(.051f,.024f,t);
            Ball("Vertebra",torso,V(0,y,z),V(width,.025f,width*.77f),bone);
            LongBone(torso,"Spinous process",V(0,y,z-.009f),V(0,y-.01f,z-.044f),.0045f);
        }
        Ball("Sacrum",torso,V(0,-.031f,-.041f),V(.079f,.131f,.05f),bone);
        for(int r=0;r<12;r++)for(int side=-1;side<=1;side+=2){
            float width=.07f+Mathf.Sin((r+1)/13f*Mathf.PI)*.10f,y=.537f-r*.024f;var pts=new List<Vector3>();
            float sweep=r<10?Mathf.PI:Mathf.PI*(r==10?.76f:.61f);
            for(int j=0;j<=24;j++){float a=j/24f*sweep;float x=side*(Mathf.Sin(a)*width+.012f);float front=(1-Mathf.Cos(a))*.078f;pts.Add(V(x,y-Mathf.Sin(a)*.042f-(1-Mathf.Cos(a))*.018f,-.055f+front));}
            Tube("Rib "+(r+1),torso,pts,r<8?.0062f:.0055f,bone);
        }
        LongBone(torso,"Sternum",V(0,.296f,.103f),V(0,.512f,.104f),.012f);
        Ball("Manubrium",torso,V(0,.515f,.10f),V(.045f,.056f,.014f),bone);
        for(int side=-1;side<=1;side+=2){
            Tube("Clavicle",torso,new List<Vector3>{V(0,.543f,.089f),V(side*.055f,.562f,.083f),V(side*.123f,.552f,.05f),V(side*.19f,.568f,.003f)},.009f,bone);
            var scapula=Ball("Scapular blade",torso,V(side*.108f,.453f,-.078f),V(.114f,.158f,.018f),bone);scapula.transform.localRotation=Quaternion.Euler(0,side*20,-side*17);
            IliacBlade(torso,side);
            LongBone(stand,"Femoral neck",V(side*.106f,1.068f,.016f),V(side*.149f,1.033f,0),.016f);
            Ball("Femoral head",stand,V(side*.106f,1.068f,.016f),V(.039f,.039f,.039f),bone);
            LongBone(stand,"Femur",V(side*.149f,1.033f,0),V(side*.104f,.647f,.012f),.017f);
            Ball("Patella",stand,V(side*.104f,.645f,.044f),V(.033f,.04f,.019f),bone);
            LongBone(stand,"Tibia",V(side*.10f,.62f,.018f),V(side*.112f,.233f,.024f),.013f);
            LongBone(stand,"Fibula",V(side*.139f,.611f,.01f),V(side*.144f,.237f,.022f),.006f);
            Ball("Calcaneus",stand,V(side*.117f,.215f,-.012f),V(.043f,.042f,.079f),bone);
            Ball("Talus",stand,V(side*.117f,.23f,.028f),V(.042f,.035f,.054f),bone);
            for(int f=0;f<5;f++){
                float x=side*.118f+(f-2)*.012f;Vector3 a=V(x,.224f,.043f),b=V(x+(f-2)*.005f,.204f,.136f-Mathf.Abs(f-1)*.005f),c=b+V(0,-.002f,.026f);
                LongBone(stand,"Metatarsal",a,b,.005f);LongBone(stand,"Toe phalanx",b,c,.004f);
            }
        }
        var skull=Head(torso,V(0,.80f,0));skull.localScale=Vector3.one*.62f;
        var left=Arm(torso,-1);var right=Arm(torso,1);
        var enemy=stand.gameObject.AddComponent<StudySkeleton>();enemy.torso=torso;enemy.skull=skull;enemy.leftArm=left;enemy.rightArm=right;enemy.wheelClip=wheel;
        var subject=stand.gameObject.AddComponent<StudySubject>();subject.index=index;subject.title=index==0?"The suspended figure":"The reaching figure";subject.focus=Group("Observe here",torso,V(0,.43f,.05f));
        PrefabUtility.SaveAsPrefabAsset(stand.gameObject,root+"/Skeleton_"+index+".prefab");return enemy;
    }
    static void TextSign(Transform parent,string text,Vector3 pos,float scale){
        var t=Group(text,parent,pos);var tm=t.gameObject.AddComponent<TextMesh>();tm.text=text;tm.fontSize=64;tm.characterSize=scale;tm.anchor=TextAnchor.MiddleCenter;tm.color=new Color(.53f,.5f,.39f);t.localRotation=Quaternion.Euler(0,180,0);
    }
    static void Easel(Vector3 pos,float angle,bool draped=false){
        Transform e=Group(draped?"Covered canvas":"Working easel",architecture,pos);e.localRotation=Quaternion.Euler(0,angle,0);
        Rod("Left stile",e,V(-.43f,.1f,0),V(-.17f,2.8f,.13f),.044f,wood);Rod("Right stile",e,V(.43f,.1f,0),V(.17f,2.8f,.13f),.044f,wood);
        Rod("Back leg",e,V(0,.08f,.75f),V(0,2.4f,.12f),.04f,wood);Box("Ledge",e,V(0,.98f,-.02f),V(1.14f,.08f,.21f),wood);
        Box("Canvas frame",e,V(0,1.74f,.02f),V(1.09f,1.37f,.075f),wood);
        Box("Unfinished canvas",e,V(0,1.74f,-.03f),V(1.01f,1.29f,.02f),canvas,true);
        if(draped)Cloth(e,V(0,2.44f,-.09f),1.22f,1.65f);
        else {
            // Charcoal anatomical gesture marks on the abandoned canvases.
            for(int i=0;i<13;i++){
                float y=1.28f+i*.072f;Rod("Charcoal",e,V(-.15f+Mathf.Sin(i)*.035f,y,-.048f),V(.1f+Mathf.Sin(i*.6f)*.09f,y+.035f,-.048f),.003f,dark);
            }
        }
    }
    static void Cloth(Transform p,Vector3 pos,float width,float length){
        var verts=new List<Vector3>();var uv=new List<Vector2>();var tris=new List<int>();int cols=56,rows=40;
        for(int y=0;y<=rows;y++)for(int x=0;x<=cols;x++){
            float u=x/(float)cols,v=y/(float)rows;verts.Add(pos+V((u-.5f)*width,-v*length+v*v*.06f*Mathf.Sin(u*9+v*1.7f),(.026f*Mathf.Sin(u*23+Mathf.Sin(u*8)+v*1.4f)+.018f*Mathf.Sin(u*47-v*2))*(.4f+v)-v*v*.085f));uv.Add(new Vector2(u,v));
            if(x<cols&&y<rows){int a=y*(cols+1)+x,b=a+cols+1;tris.AddRange(new[]{a,a+1,b,a+1,b+1,b});}
        }
        Mesh mesh=new Mesh();mesh.SetVertices(verts);mesh.SetUVs(0,uv);mesh.SetTriangles(tris,0);mesh.RecalculateNormals();AssetDatabase.CreateAsset(mesh,root+"/Cloth_"+(meshId++)+".asset");
        var g=new GameObject("Heavy draped linen",typeof(MeshFilter),typeof(MeshRenderer));g.transform.SetParent(p,false);g.GetComponent<MeshFilter>().sharedMesh=mesh;g.GetComponent<MeshRenderer>().sharedMaterial=canvas;
    }
    static Light Lamp(string n,Transform p,Vector3 pos,Color c,float intensity,float range,LightType type){var l=Group(n,p,pos).gameObject.AddComponent<Light>();l.type=type;l.color=c;l.intensity=intensity;l.range=range;return l;}
    static Transform MakeDeathHand(Transform eye){
        var hand=Group("Reaching hand / final contact",eye,V(.65f,-.42f,.6f));
        Bone(hand,"Radius",V(.25f,-.3f,0),V(.07f,0,0),.028f);
        Bone(hand,"Ulna",V(.29f,-.3f,0),V(.11f,0,0),.021f);
        for(int f=0;f<5;f++){
            float x=(f-2)*.035f;
            Bone(hand,"Metacarpal",V(.08f,0,0),V(x,.11f,.015f),.012f);
            Bone(hand,"Proximal phalanx",V(x,.11f,.015f),V(x-.035f,.19f,.005f),.011f);
            Bone(hand,"Distal phalanx",V(x-.035f,.19f,.005f),V(x-.06f,.2f,-.035f),.009f);
        }
        hand.localScale=Vector3.one*.7f;hand.localRotation=Quaternion.Euler(0,0,-24);
        hand.gameObject.SetActive(false);return hand;
    }
    static AudioClip Audio(string name,int kind){
        int rate=22050,len=rate*4;float[] data=new float[len];System.Random rng=new System.Random(14+kind);float low=0;
        for(int i=0;i<len;i++){float t=i/(float)rate,noise=(float)rng.NextDouble()*2-1;low=low*.95f+noise*.05f;
            if(kind==0)data[i]=(.3f*low+.018f*Mathf.Sin(t*490))*(.35f+.65f*Mathf.Pow(Mathf.Sin(t*12),2));
            if(kind==1)data[i]=.16f*Mathf.Sin(t*2*Mathf.PI*55)+.11f*Mathf.Sin(t*2*Mathf.PI*82)+.13f*low;
            if(kind==2)data[i]=.25f*low+.08f*Mathf.Sin(t*2*Mathf.PI*(420+20*Mathf.Sin(t*9)))*Mathf.Pow(Mathf.Max(0,Mathf.Sin(t*5)),10);
        }
        string path=root+"/"+name+".wav";using(var stream=new BinaryWriter(File.Open(path,FileMode.Create))){stream.Write(System.Text.Encoding.ASCII.GetBytes("RIFF"));stream.Write(36+len*2);stream.Write(System.Text.Encoding.ASCII.GetBytes("WAVEfmt "));stream.Write(16);stream.Write((short)1);stream.Write((short)1);stream.Write(rate);stream.Write(rate*2);stream.Write((short)2);stream.Write((short)16);stream.Write(System.Text.Encoding.ASCII.GetBytes("data"));stream.Write(len*2);foreach(float f in data)stream.Write((short)(Mathf.Clamp(f,-1,1)*32767));}
        AssetDatabase.ImportAsset(path);return AssetDatabase.LoadAssetAtPath<AudioClip>(path);
    }
    [MenuItem("Study/Build Studio")]
    public static void Create(){
        Directory.CreateDirectory(root);meshId=0;lamps.Clear();
        EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
        bone=Mat("Aged ivory",new Color(.64f,.60f,.45f),.27f);
        dark=Mat("Charcoal",new Color(.028f,.034f,.031f),.1f);
        metal=Mat("Oxidised steel",new Color(.18f,.21f,.19f),.46f,.7f);
        wood=Mat("Dark oak",new Color(.18f,.125f,.075f),.22f);
        wall=Mat("Stained plaster",new Color(.31f,.34f,.29f),.1f);
        canvas=Mat("Unbleached linen",new Color(.59f,.57f,.46f),.05f);
        floor=Mat("Floorboards",new Color(.16f,.18f,.16f),.32f);
        brass=Mat("Tarnished brass",new Color(.39f,.30f,.12f),.5f,.6f);
        // Procedural surface grain, packed locally without external assets.
        foreach(var m in new[]{wall,wood,canvas,floor,bone}){
            var tex=new Texture2D(128,128);var pixels=new Color[128*128];
            for(int y=0;y<128;y++)for(int x=0;x<128;x++){float noise=Mathf.PerlinNoise(x*(m==wood?.04f:.17f),y*(m==wood?.9f:.17f));float val=.65f+noise*.45f;pixels[y*128+x]=new Color(val,val,val);}
            tex.SetPixels(pixels);tex.Apply();string p=root+"/"+m.name+" grain.png";File.WriteAllBytes(p,tex.EncodeToPNG());AssetDatabase.ImportAsset(p);m.mainTexture=AssetDatabase.LoadAssetAtPath<Texture2D>(p);m.mainTextureScale=new Vector2(3,3);
        }
        architecture=Group("STUDIO / architecture",null,Vector3.zero);
        var ground=Box("Floor collision",architecture,V(0,-.12f,0),V(15,.2f,19),floor,true);ground.layer=0;
        for(int x=0;x<30;x++)for(int z=0;z<6;z++)Box("Worn oak plank",architecture,V(-7.25f+x*.5f,-.015f,-7.9f+z*3.15f+(x%2)*.05f),V(.485f,.025f,3.12f),floor);
        Box("North wall",architecture,V(0,3.1f,9.5f),V(15,.2f,6.2f),wall).transform.localRotation=Quaternion.Euler(90,0,0);
        Box("Back wall",architecture,V(0,3.1f,-9.5f),V(15,6.2f,.25f),wall,true);
        Box("West wall",architecture,V(-7.5f,3.1f,0),V(.25f,6.2f,19),wall,true);
        Box("East wall",architecture,V(7.5f,3.1f,0),V(.25f,6.2f,19),wall,true);
        Box("North collision",architecture,V(0,3.1f,9.6f),V(15,6.2f,.1f),wall,true);
        Box("Ceiling",architecture,V(0,6.25f,0),V(15,.2f,19),dark);
        for(int z=-8;z<=8;z+=4){Box("Ceiling beam",architecture,V(0,5.9f,z),V(15,.38f,.24f),wood);Rod("Overhead pipe",architecture,V(-7,5.63f,z+.2f),V(7,5.63f,z+.2f),.035f,metal);}
        for(int s=-1;s<=1;s+=2){Box("Wall lower rail",architecture,V(s*7.32f,1.25f,0),V(.1f,.07f,19),wood);Box("Skirting",architecture,V(s*7.3f,.18f,0),V(.14f,.3f,19),wood);}
        Material glass=Mat("Moonlit glass",new Color(.095f,.19f,.21f),.65f);glass.EnableKeyword("_EMISSION");glass.SetColor("_EmissionColor",new Color(.055f,.12f,.14f));
        for(int x=-5;x<=5;x+=5){
            Box("Tall window surround",architecture,V(x,3.9f,9.33f),V(2.45f,3.5f,.19f),wood);
            Box("Night glass",architecture,V(x,3.9f,9.21f),V(2.23f,3.28f,.035f),glass);
            for(int j=-1;j<=1;j++)Box("Window mullion",architecture,V(x+j*.72f,3.9f,9.16f),V(.055f,3.32f,.065f),metal);
            for(int j=-1;j<=1;j++)Box("Window transom",architecture,V(x,3.9f+j*1.06f,9.15f),V(2.3f,.055f,.07f),metal);
            Box("Window sill",architecture,V(x,2.18f,9.1f),V(2.65f,.12f,.45f),wood);
            var moon=Lamp("Cold window spill",architecture,V(x,3.6f,8.6f),new Color(.35f,.57f,.65f),1.15f,13,LightType.Spot);moon.spotAngle=88;moon.transform.rotation=Quaternion.Euler(32,180,0);moon.shadows=LightShadows.Soft;
        }
        Box("Door",architecture,V(0,1.5f,-9.31f),V(1.7f,3,.09f),wood);Box("Door inset",architecture,V(0,1.5f,-9.23f),V(1.4f,2.6f,.06f),dark);Ball("Door knob",architecture,V(.56f,1.33f,-9.1f),Vector3.one*.1f,brass);
        for(int z=-5;z<=5;z+=5){
            Rod("Lamp cord",architecture,V(0,6,z),V(0,4.4f,z),.015f,dark);
            Shape("Enamel lampshade",PrimitiveType.Cylinder,architecture,V(0,4.3f,z),V(.8f,.065f,.8f),metal);
            lamps.Add(Lamp("Dormant ceiling light",architecture,V(0,4.13f,z),new Color(1,.87f,.62f),0,13,LightType.Point));
        }
        Easel(V(-3.2f,0,-2.4f),-25);Easel(V(3.6f,0,-.3f),24,true);Easel(V(-3.7f,0,3.8f),-10,true);Easel(V(4.8f,0,4.5f),-28);Easel(V(-5.9f,0,7.3f),5);
        // Central still life island, with open circulation on all four sides.
        Transform table=Group("Still life platform",architecture,V(0,0,3.2f));
        Box("Heavy table top",table,V(0,1.03f,0),V(2.6f,.12f,1.35f),wood,true);
        for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Box("Table leg",table,V(x*1.1f,.5f,z*.5f),V(.12f,1,.12f),wood,true);
        Box("Linen on table",table,V(.4f,1.1f,0),V(1.55f,.025f,1.33f),canvas);Cloth(table,V(.4f,1.1f,-.69f),1.55f,.9f);
        Transform bust=Group("Plaster bust",table,V(-.48f,1.11f,0));
        Shape("Bust plinth",PrimitiveType.Cylinder,bust,V(0,.06f,0),V(.42f,.06f,.42f),canvas);
        Ball("Shoulders",bust,V(0,.26f,0),V(.6f,.32f,.32f),canvas);Head(bust,V(0,.58f,0),true);
        Material pear=Mat("Muted pear skin",new Color(.36f,.35f,.15f),.2f);
        for(int i=0;i<2;i++){Vector3 p=V(.38f+i*.38f,1.24f,-.15f+i*.19f);Ball("Pear",table,p,V(.22f,.29f,.22f),pear);Ball("Pear neck",table,p+V(.015f,.1f,0),V(.13f,.18f,.13f),pear);Rod("Pear stalk",table,p+V(.025f,.15f,0),p+V(.04f,.23f,0),.01f,wood);}
        var still=table.gameObject.AddComponent<StudySubject>();still.index=2;still.title="Plaster, pears & linen";still.focus=Group("Observe still life",table,V(0,1.5f,0));
        // Side workbench and sink.
        Transform bench=Group("Paint stained workbench",architecture,V(-6.6f,0,-3));Box("Bench top",bench,V(0,1.05f,0),V(1.3f,.1f,3.6f),wood,true);
        for(int z=-1;z<=1;z+=2)Box("Cabinet",bench,V(0,.5f,z*1.2f),V(1.15f,1,.8f),wall,true);
        Shape("Brush pot",PrimitiveType.Cylinder,bench,V(0,1.25f,0),V(.22f,.15f,.22f),metal);
        for(int i=0;i<7;i++)Rod("Brush",bench,V(i*.019f-.06f,1.2f,0),V(i*.024f-.05f,1.7f,i*.013f),.009f,wood);
        Transform sink=Group("Ceramic sink",architecture,V(6.8f,0,-6.6f));Box("Sink cabinet",sink,V(0,.55f,0),V(1.1f,1.1f,1.8f),wall,true);Box("Basin",sink,V(0,1.14f,0),V(1.2f,.1f,1.85f),canvas);Box("Basin shadow",sink,V(0,1.2f,0),V(.75f,.02f,1.25f),dark);Tube("Tap",sink,new List<Vector3>{V(.35f,1.2f,0),V(.35f,1.55f,0),V(.15f,1.58f,0),V(.1f,1.44f,0)},.028f,metal);
        Random.InitState(17);
        for(int i=0;i<28;i++){
            var p=Box("Discarded study",architecture,V(Random.Range(-6.7f,6.7f),.008f,Random.Range(-8.6f,8.6f)),V(.29f,.005f,.42f),canvas);p.transform.localRotation=Quaternion.Euler(0,Random.Range(0,180),0);
        }
        for(int i=0;i<6;i++){
            Transform stool=Group("Drawing stool",architecture,V(i%2==0?-4.6f:5.8f,0,-6+i*1.9f));
            Shape("Seat",PrimitiveType.Cylinder,stool,V(0,.65f,0),V(.52f,.045f,.52f),wood);
            for(int j=0;j<3;j++){float a=j*Mathf.PI*2/3;Rod("Stool leg",stool,V(Mathf.Cos(a)*.22f,.02f,Mathf.Sin(a)*.22f),V(Mathf.Cos(a)*.14f,.65f,Mathf.Sin(a)*.14f),.022f,metal);}
        }
        var pencil=Audio("Charcoal on paper",0);var hum=Audio("Room tone",1);var wheels=Audio("Caster wheels",2);
        var one=Skeleton("01 / suspended figure",V(-1.9f,0,1),0,wheels);var two=Skeleton("02 / reaching figure",V(3.3f,0,6.1f),1,wheels);one.transform.rotation=Quaternion.Euler(0,180,0);two.transform.rotation=Quaternion.Euler(0,190,0);
        GameObject player=new GameObject("Student");player.transform.position=V(0,0,-7.4f);var cc=player.AddComponent<CharacterController>();cc.height=1.7f;cc.radius=.27f;cc.center=V(0,.85f,0);cc.stepOffset=.2f;
        var eye=Group("Eyes / headlamp",player.transform,V(0,1.58f,0)).gameObject.AddComponent<Camera>();eye.tag="MainCamera";eye.fieldOfView=68;eye.nearClipPlane=.04f;eye.farClipPlane=45;eye.clearFlags=CameraClearFlags.SolidColor;eye.backgroundColor=new Color(.015f,.025f,.026f);eye.allowHDR=true;eye.gameObject.AddComponent<AudioListener>();
        Light lamp=Lamp("Headlamp",eye.transform,V(0,-.045f,.09f),new Color(1,.9f,.68f),4.6f,14,LightType.Spot);lamp.spotAngle=48;lamp.innerSpotAngle=30;lamp.shadows=LightShadows.Soft;lamp.shadowStrength=.9f;lamp.shadowBias=.02f;lamp.shadowNormalBias=.1f;
        var board=Group("Handheld sketchboard",eye.transform,V(.13f,-.58f,.5f));board.localRotation=Quaternion.Euler(-35,0,-8);
        Box("Clipboard",board,Vector3.zero,V(.48f,.027f,.62f),wood);
        paper=new Material(Shader.Find("Study/Paper"));AssetDatabase.CreateAsset(paper,root+"/Sketch paper.mat");
        var page=Box("Drawing paper",board,V(0,.016f,0),V(.424f,.002f,.556f),paper);
        Box("Brass clip",board,V(0,.03f,.27f),V(.12f,.023f,.04f),metal);
        // Sleeves and fingers anchor the board to the first-person body.
        Material sleeve=Mat("Student coat",new Color(.075f,.09f,.085f),.07f);Material skin=Mat("Hands",new Color(.46f,.37f,.27f),.15f);
        Box("Left sleeve",board,V(-.27f,-.045f,-.2f),V(.11f,.11f,.31f),sleeve);Ball("Left hand",board,V(-.225f,-.005f,-.08f),V(.10f,.055f,.13f),skin);
        for(int i=0;i<4;i++)Box("Fingers",board,V(-.197f+i*.016f,.035f,-.075f),V(.014f,.025f,.08f),skin);
        Rod("Charcoal pencil",board,V(.12f,.065f,-.17f),V(.23f,.09f,.06f),.007f,dark);
        var game=new GameObject("Unfinished Study / director").AddComponent<StudyGame>();game.eye=eye;game.body=cc;game.headlamp=lamp;game.board=board;game.paper=page.GetComponent<Renderer>();game.subjects=new[]{one.GetComponent<StudySubject>(),two.GetComponent<StudySubject>(),still};game.skeletons=new[]{one,two};game.ceilingLights=lamps.ToArray();game.pencilClip=pencil;game.humClip=hum;
        game.sketches=new Texture2D[3];for(int i=0;i<3;i++)game.sketches[i]=AssetDatabase.LoadAssetAtPath<Texture2D>("Assets/Study/Resources/sketch"+i+".png");
        game.deathHand=MakeDeathHand(eye.transform);
        game.serif=AssetDatabase.LoadAssetAtPath<Font>("Assets/Study/Resources/Georgia.ttf");
        eye.gameObject.AddComponent<StudyImageEffect>();
        RenderSettings.ambientMode=UnityEngine.Rendering.AmbientMode.Flat;RenderSettings.ambientLight=new Color(.105f,.13f,.125f);RenderSettings.fog=true;RenderSettings.fogMode=FogMode.ExponentialSquared;RenderSettings.fogColor=new Color(.022f,.042f,.043f);RenderSettings.fogDensity=.034f;RenderSettings.skybox=null;
        QualitySettings.shadows=ShadowQuality.All;QualitySettings.shadowResolution=ShadowResolution.High;QualitySettings.shadowDistance=25;QualitySettings.pixelLightCount=8;QualitySettings.antiAliasing=4;
        PlayerSettings.companyName="Night Studio";PlayerSettings.productName="Unfinished Study";PlayerSettings.defaultScreenWidth=1440;PlayerSettings.defaultScreenHeight=900;PlayerSettings.fullScreenMode=FullScreenMode.Windowed;PlayerSettings.runInBackground=false;PlayerSettings.colorSpace=ColorSpace.Linear;
        PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);
        StudioUpgrade.Apply(game);
        StudioRealism.Apply(game);
        EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(),"Assets/Study/Studio.unity");EditorBuildSettings.scenes=new[]{new EditorBuildSettingsScene("Assets/Study/Studio.unity",true)};
        AssetDatabase.SaveAssets();Debug.Log("STUDIO_CREATED");
    }
    public static void BuildMac(){
        string dest=Path.GetFullPath("../Mac-v7.2/Unfinished Study.app");
        var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{"Assets/Study/Studio.unity"},locationPathName=dest,target=BuildTarget.StandaloneOSX,options=BuildOptions.None});
        Debug.Log("STUDY_BUILD: "+report.summary.result);if(report.summary.result!=BuildResult.Succeeded)throw new System.Exception("Build failed");
    }
}
