"""Reference-led professor asset. Blender 5.x background build, no user's scene edits.
Reference: References/Max-Planck-1933-reference.jpg; see References/README.md.
Coordinates are authored as X right, Y back, Z up; exported to Unity X,Y,Z=(x,z,-y).
"""
import bpy, math, json, random, os
import numpy as np
from pathlib import Path
from mathutils import Vector
P=Path(__file__).resolve().parents[1]; OUT=P/'Blender/Professor'; ART=P/'Assets/Study/ArtProfessor'
OUT.mkdir(parents=True,exist_ok=True);ART.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
random.seed(17); groups={'Head':[], 'Body':[]}; group='Head'; mats={}
def material(name,c,rough,metal=0):
 m=bpy.data.materials.new(name);m.diffuse_color=(*c,1);m.use_nodes=True
 bs=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');bs.inputs['Base Color'].default_value=(*c,1);bs.inputs['Roughness'].default_value=rough;bs.inputs['Metallic'].default_value=metal
 mats[name]=m;return m
skin=material('Skin',(.43,.295,.215),.56);lips=material('Lips',(.37,.235,.185),.67);crease=material('Crease',(.075,.033,.028),.76)
white=material('Sclera',(.59,.55,.46),.25);iris=material('Iris',(.08,.10,.09),.3);pupil=material('Pupil',(.012,.013,.012),.2)
frame=material('Bronze',(.20,.16,.095),.31,.82);hair=material('Hair',(.026,.030,.029),.87)
wool=material('Wool',(.048,.055,.059),.83);shirt=material('Cotton',(.61,.56,.44),.76);tie=material('Tie',(.105,.035,.026),.54);seam=material('Seam',(.019,.023,.024),.88)
def mesh(name,V,F,mat,uv=None):
 me=bpy.data.meshes.new(name);me.from_pydata(V,[],F);me.update();o=bpy.data.objects.new(name,me);bpy.context.collection.objects.link(o);o.data.materials.append(mat)
 if uv is not None:
  layer=me.uv_layers.new(name='UVMap')
  for poly in me.polygons:
   for li in poly.loop_indices:layer.data[li].uv=uv[me.loops[li].vertex_index]
 for p in me.polygons:p.use_smooth=True
 groups[group].append(o);return o
def tube(name,points,radii,mat,sides=10,closed=False):
 ps=[Vector(p) for p in points];V=[];F=[];uv=[]
 for j,p in enumerate(ps):
  tangent=(ps[(j+1)%len(ps)]-ps[j-1] if closed else ps[min(j+1,len(ps)-1)]-ps[max(0,j-1)]).normalized()
  ref=Vector((0,0,1)) if abs(tangent.z)<.9 else Vector((0,1,0));a=tangent.cross(ref).normalized();b=tangent.cross(a).normalized();r=radii[j] if isinstance(radii,list) else radii
  for k in range(sides):
   ang=k*2*math.pi/sides;V.append(tuple(p+r*(a*math.cos(ang)+b*math.sin(ang))));uv.append((k/sides,j/max(1,len(ps)-1)))
 for j in range(len(ps) if closed else len(ps)-1):
  for k in range(sides):F.append((j*sides+k,j*sides+(k+1)%sides,((j+1)%len(ps))*sides+(k+1)%sides,((j+1)%len(ps))*sides+k))
 if not closed:F.extend([tuple(reversed(range(sides))),tuple((len(ps)-1)*sides+k for k in range(sides))])
 return mesh(name,V,F,mat,uv)
def ellipsoid(name,loc,scale,mat,seg=40,rings=24):
 bpy.ops.mesh.primitive_uv_sphere_add(segments=seg,ring_count=rings,location=loc);o=bpy.context.object;o.name=name;o.scale=scale;o.data.materials.append(mat)
 for p in o.data.polygons:p.use_smooth=True
 groups[group].append(o);return o
def panel(name,pts,mat,thickness=.009,bevel=.005):
 o=mesh(name,pts,[tuple(range(len(pts)))],mat);s=o.modifiers.new('Tailored thickness','SOLIDIFY');s.thickness=thickness;b=o.modifiers.new('Rolled edge','BEVEL');b.width=bevel;b.segments=3;return o
def bell(x,c,w):return math.exp(-((x-c)/w)**2)
# Head profile explicitly defines jaw angle, cheekbone breadth, temporal hollow and cranial vault.
profile=[(-.345,.018,.035),(-.32,.085,.115),(-.285,.135,.15),(-.23,.178,.16),(-.15,.192,.167),(-.065,.20,.18),(.02,.218,.186),(.10,.207,.20),(.17,.210,.211),(.25,.221,.218),(.32,.217,.218),(.385,.183,.18),(.425,.135,.13),(.447,.06,.057),(.455,.002,.002)]
def dims(z):
 for i in range(len(profile)-1):
  z0,w0,d0=profile[i];z1,w1,d1=profile[i+1]
  if z<=z1:
   t=max(0,(z-z0)/(z1-z0));h=z1-z0
   prev=profile[max(0,i-1)];nxt=profile[min(len(profile)-1,i+2)]
   def cubic(a,b,k):
    m0=(profile[i+1][k]-prev[k])/(profile[i+1][0]-prev[0]);m1=(nxt[k]-profile[i][k])/(nxt[0]-profile[i][0])
    return max(.001,(2*t**3-3*t*t+1)*a+(t**3-2*t*t+t)*h*m0+(-2*t**3+3*t*t)*b+(t**3-t*t)*h*m1)
   return cubic(w0,w1,1),cubic(d0,d1,2)
 return .002,.002
# Small-scale aging follows anatomy rather than uniform random displacement.
def sculpt(x,z):
 d=.020*bell(x,0,.15)*bell(z,.24,.2)
 d+=.068*bell(x,0,.033)*bell(z,-.005,.105) # nasal bridge
 d+=.068*bell(x,0,.040)*bell(z,-.105,.031) # bulb/columella
 d+=.046*(bell(x,-.043,.021)+bell(x,.043,.021))*bell(z,-.115,.021)
 d+=.028*bell(x,0,.105)*bell(z,-.19,.058) # muzzle
 d+=.046*bell(x,0,.087)*bell(z,-.28,.038) # chin pad
 d-=.013*bell(x,0,.066)*bell(z,-.247,.012) # underlip
 for side in [-1,1]:
  xx=x*side
  d-=.023*bell(xx,.097,.062)*bell(z,.052,.054) # orbit
  d+=.024*bell(xx,.098,.064)*bell(z,.119-.09*(xx-.09),.020) # brow arch
  d+=.022*bell(xx,.15,.042)*bell(z,-.025,.047) # zygoma
  d-=.024*bell(xx,.151,.040)*bell(z,-.125,.06) # buccal hollow
  d+=.011*bell(xx,.11,.058)*bell(z,-.012,.020) # eye bag
  fold=.052+max(0,-z-.115)*.48
  d-=.012*bell(xx,fold,.008)*bell(z,-.175,.065)
  for k in range(2):d-=.004*bell(xx,.102,.056)*bell(z,-.025-k*.013+.11*(xx-.10),.003)
 # Fine forehead furrows and glabella, shallow so light does the work.
 for k in range(3):d-=.0026*bell(z,.19+k*.037+.027*(x/.22)**2,.0027)*bell(x,0,.17)
 d-=.003*(bell(x,-.021,.003)+bell(x,.022,.003))*bell(z,.133,.028)
 return d
def front_y(x,z):
 w,dep=dims(z);s=math.sqrt(max(0,1-(x/w)**2));return .028-dep*s-s**4*sculpt(x,z)
V=[];F=[];UV=[];R=160;S=192
for r in range(R+1):
 z=-.345+.8*r/R;w,dep=dims(z)
 for s in range(S+1):
  a=s*math.pi*2/S;x=math.sin(a)*w;front=max(0,math.cos(a));y=.028-dep*math.cos(a)-front**4*sculpt(x,z)
  # restrained asymmetry in jaw / brow
  x+=.003*bell(z,-.16,.13);V.append((x,y,z));UV.append((s/S,r/R))
for r in range(R):
 for s in range(S):
  a=r*(S+1)+s;coords=[V[i] for i in [a,a+1,a+S+2,a+S+1]];cx=sum(v[0] for v in coords)/4;cy=sum(v[1] for v in coords)/4;cz=sum(v[2] for v in coords)/4
  eye=cy<-.08 and any(((cx-side*.097)/.061)**2+((cz-.05)/.033)**2<1 for side in [-1,1])
  if not eye:F.append((a,a+1,a+S+2,a+S+1))
head=mesh('Head / cranial planes, cheeks, jaw and nose',V,F,skin,UV)
# Socket apertures have continuous annular lids wrapping around actual eyeballs.
for side in [-1,1]:
 cx=side*.097;cz=.05;yc=-.113
 ellipsoid('Eye globe', (cx,yc,cz),(.053,.050,.046),white)
 ellipsoid('Iris', (cx,yc-.049,cz),(.017,.003,.017),iris,32,16)
 ellipsoid('Pupil', (cx,yc-.052,cz),(.0065,.0015,.0065),pupil,24,12)
 lv=[];lf=[];lu=[]
 for j in range(7):
  t=j/6
  for k in range(65):
   a=k*math.pi*2/64;dx=math.cos(a)*(.045+.027*t);dz=math.sin(a)*(.013+.039*t);x=cx+dx;z=cz+dz
   surface=front_y(x,z);inner=yc-.050*math.sqrt(max(.001,1-(math.cos(a)*.045/.053)**2-(math.sin(a)*.013/.046)**2))-.002
   y=(1-t)*inner+t*surface-.003*math.sin(t*math.pi)
   coverage=1-(dx/.053)**2-(dz/.046)**2
   if coverage>0:y=min(y,yc-.050*math.sqrt(coverage)-.002)
   y=min(y,surface-.002)
   lv.append((x,y,z));w,_=dims(z);lu.append(((math.asin(max(-1,min(1,x/w)))/(2*math.pi))%1,(z+.345)/.8))
   if j<6 and k<64:q=j*65+k;lf.append((q,q+1,q+66,q+65))
 mesh('Eyelid / upper lower and tear duct',lv,[f[::-1] for f in lf],skin,lu)
 # eyebrow follows the arch, individual small locks rather than a single dark bar.
 for k in range(20):
  t=k/19;x=cx+(.057- .114*t);z=.118+.009*math.sin(t*math.pi);y=front_y(x,z)-.004
  tube('Grey eyebrow hair',[(x,y,z),(x+side*.004,y-.002,z+.005),(x+side*.009,y+.001,z+.004)],.0012,hair,5)
 # Ear shell: depressed concha, raised helix, recessed opening and lobe.
 ev=[];ef=[];eu=[]
 for r in range(15):
  t=r/14
  for k in range(49):
   a=k*2*math.pi/48;z=.013+math.cos(a)*.099*t;x=side*(.211+math.sin(a)*.044*t+.021*t);y=.006-.039*math.sin(t*math.pi*.82)
   ev.append((x,y,z));eu.append((k/48,t))
   if r<14 and k<48:q=r*49+k;ef.append((q,q+1,q+50,q+49))
 mesh('Ear / concha shell',ev,ef if side>0 else [f[::-1] for f in ef],skin,eu)
 points=[]
 for k in range(65):
  a=k*2*math.pi/64;points.append((side*(.232+math.sin(a)*.044),-.02,.013+math.cos(a)*.099))
 tube('Ear / rolled helix',points,.007,skin,10)
 ellipsoid('Ear canal shadow',(side*.233,-.031,-.007),(.012,.003,.021),crease,24,16)
 # Spectacles stand clear of the nose and lids, with real bridge, pads, hinges and arms.
 points=[]
 for k in range(80):
  a=k*2*math.pi/80;x=cx+math.cos(a)*.081;z=.038+math.sin(a)*.059;y=-.253+.055*(abs(x)/.22)**2
  points.append((x,y,z))
 tube('Spectacle / oval wire rim',points,.0031,frame,8,True)
 tube('Spectacle / temple arm',[(side*.177,-.211,.048),(side*.22,-.16,.055),(side*.24,-.035,.05),(side*.246,.022,.036),(side*.24,.035,.003)],.0032,frame,8)
 ellipsoid('Spectacle / nose pad',(side*.026,-.251,.018),(.007,.009,.012),frame,24,12)
 ellipsoid('Spectacle / hinge',(side*.176,-.213,.05),(.007,.006,.005),frame,16,10)
 # Nostrils recessed under alae.
 ellipsoid('Nostril',(side*.034,front_y(side*.034,-.133)-.001,-.133),(.008,.0015,.004),crease,24,12)
# Bridge rises over the actual nasal bridge.
tube('Spectacle / arched bridge',[(-.02,-.257,.043),(-.013,-.274,.059),(0,-.28,.065),(.013,-.274,.059),(.02,-.257,.043)],.0031,frame,10)
# Closed lips: shaped vermilion surfaces with cupid bow, not a floating black box.
for upper in [True,False]:
 vv=[];ff=[];uv=[]
 for j in range(8):
  v=j/7
  for i in range(65):
   u=i/64;x=(u-.5)*.155;arch=max(0,1-(x/.0775)**2)
   mid=-.208+.006*math.cos(x/.0775*math.pi)-.004*abs(x)/.0775
   dz=(.013 if upper else -.017)*arch*v
   z=mid+dz;y=front_y(x,z)-(.004*math.sin(v*math.pi)+.0015)*arch
   vv.append((x,y,z));uv.append((u,v))
   if j<7 and i<64:q=j*65+i;ff.append((q,q+1,q+66,q+65))
 mesh('Upper lip' if upper else 'Lower lip',vv,ff if upper else [f[::-1] for f in ff],lips,uv)
points=[]
for k in range(49):
 x=-.0775+k*.155/48;z=-.208+.006*math.cos(x/.0775*math.pi)-.004*abs(x)/.0775;points.append((x,front_y(x,z)-.005,z))
tube('Mouth / closed seam',points,.0009,crease,7)
# Receding silver hair at the temples/back only. Forehead remains bare.
for side in [-1,1]:
 for i in range(240):
  a=side*(1.03+random.random()*1.75);z=.10+random.random()*.245;w,d=dims(z);pts=[]
  for k in range(5):
   zz=z-.012*k;ww,dd=dims(zz);aa=a+side*.018*k;pts.append((math.sin(aa)*(ww+.003),.028-math.cos(aa)*(dd+.003),zz))
  tube('Temple hair',pts,.0006,hair,4)
# Cropped hair mass follows the scalp; a receding hairline exposes the crown.
for side in [-1,1]:
 vv=[];ff=[]
 for j in range(30):
  t=j/29;z=-.025+.365*t
  for i in range(65):
   u=i/64;edge=1.10+.34*t+.08*math.sin(t*7);a=side*(edge+u*(math.pi-edge));w,d=dims(z)
   vv.append((math.sin(a)*(w+.0017),.028-math.cos(a)*(d+.0017),z))
   if j<29 and i<64:q=j*65+i;ff.append((q,q+1,q+66,q+65))
 mesh('Cropped receding hair',vv,ff if side>0 else [f[::-1] for f in ff],hair)
# Shorten vertical silhouette, preserving all feature relationships.
for obj in groups['Head']:
 obj.location.z*=.88
 obj.scale.z*=.88

# Sparse stubble at jaw, no historical moustache: preserve a fictional character identity.
# Cloth body with a shoulder girdle, a tapered waist and separately rolled lapels.
group='Body'
ellipsoid('Jacket / hunched back',(0,.30,.04),(.79,.97,.39),wool,64,40)
ellipsoid('Jacket / shoulder left',(-.40,-.22,-.10),(.42,.50,.22),wool)
ellipsoid('Jacket / shoulder right',(.40,-.22,-.10),(.42,.50,.22),wool)
ellipsoid('Jacket / waist',(0,.91,.04),(.53,.52,.29),wool)
bpy.ops.object.select_all(action='DESELECT')
base=list(groups['Body'])
for ob in base:ob.select_set(True)
bpy.context.view_layer.objects.active=base[0];bpy.ops.object.join();jacket=bpy.context.object;jacket.name='Jacket / continuous shoulder back and waist';groups['Body']=[jacket]
rem=jacket.modifiers.new('Joined cloth volume','REMESH');rem.mode='VOXEL';rem.voxel_size=.025;rem.use_smooth_shade=True;bpy.ops.object.modifier_apply(modifier=rem.name)
sm=jacket.modifiers.new('Relaxed cloth silhouette','SMOOTH');sm.factor=.7;sm.iterations=4;bpy.ops.object.modifier_apply(modifier=sm.name)

# Front lies downward in the ceiling silhouette. It is visibly constructed rather than a cube.
panel('Shirt / placket',[(-.26,-.78,-.31),(.26,-.78,-.31),(.24,.48,-.38),(-.24,.48,-.38)],shirt,.018,.015)
for side in [-1,1]:
 panel('Jacket / rolled lapel',[(side*.25,-.79,-.37),(side*.62,-.48,-.36),(side*.24,.38,-.43),(side*.11,-.38,-.43)],wool,.018,.014)
 panel('Shirt / collar',[(side*.02,-.80,-.36),(side*.28,-.79,-.33),(side*.30,-.51,-.47),(side*.10,-.57,-.49)],shirt,.015,.008)
 tube('Jacket / shoulder seam',[(side*.28,-.77,-.24),(side*.51,-.62,-.35),(side*.73,-.31,-.28),(side*.76,.01,-.20)],.006,seam,8)
 for j in range(3):
  tube('Jacket / elbow fabric fold',[(side*.61,-.12+j*.11,-.26),(side*.76,-.06+j*.11,-.23),(side*.80,.00+j*.11,-.14)],.01,wool,8)
for j in range(3):ellipsoid('Jacket / horn button',(.08,.06+j*.21,-.427),(.038,.037,.012),seam,24,16)
panel('Jacket / welt pocket',[(-.55,.29,-.32),(-.30,.29,-.415),(-.30,.36,-.415),(-.55,.36,-.32)],wool,.012,.007)
tube('Jacket / back centre seam',[(0,-.41,.30),(0,.1,.431),(0,.6,.399),(0,1.15,.30)],.004,seam,8)
# Hanging tie remains separately animated by Unity.
# Texture sets. Skin has broad color zones plus restrained pores; wool has directional weave.
def save_img(name,array,noncolor=False):
 h,w=array.shape[:2];im=bpy.data.images.new(name,width=w,height=h,alpha=True);im.colorspace_settings.name='Non-Color' if noncolor else 'sRGB';im.pixels.foreach_set(array.astype(np.float32).ravel());im.filepath_raw=str(ART/(name+'.png'));im.file_format='PNG';im.save();return im
size=1024; yy,xx=np.mgrid[0:size,0:size].astype(float);u=xx/(size-1);v=yy/(size-1);rng=np.random.default_rng(103)
noise=rng.random((size,size));z=-.345+.8*v;a=u*math.pi*2;x=np.sin(a)*.215;front=np.maximum(0,np.cos(a))**8
mottle=(np.sin(xx*.041)*np.sin(yy*.029)+np.sin(xx*.087+yy*.051))*.010
col=np.ones((size,size,4));base=np.array([.52,.43,.36]);col[:,:,:3]=base+(noise[:,:,None]-.5)*.017+mottle[:,:,None]
cheek=np.exp(-((abs(x)-.15)/.045)**2-((z+.065)/.075)**2)*front
col[:,:,:3]+=cheek[:,:,None]*np.array([.045,-.009,-.008])
eyeage=np.exp(-((abs(x)-.10)/.07)**2-((z+.003)/.034)**2)*front
col[:,:,:3]-=eyeage[:,:,None]*np.array([.065,.056,.039])
spots=(np.sin(xx*.119+np.cos(yy*.052)) * np.sin(yy*.127+xx*.031)> .94).astype(float)*front*np.clip((z-.11)*5,0,1)
col[:,:,:3]-=spots[:,:,None]*np.array([.040,.029,.022]);skinmap=save_img('ProfessorSkin_BaseColor',np.clip(col,0,1))
bs=next(n for n in skin.node_tree.nodes if n.type=='BSDF_PRINCIPLED');tn=skin.node_tree.nodes.new('ShaderNodeTexImage');tn.image=skinmap;skin.node_tree.links.new(tn.outputs['Color'],bs.inputs['Base Color'])
# normal maps generated from subtle height, interpreted identically in Blender and Unity.
for name,height,strength,mat in [('ProfessorSkin_Normal',noise,.07,skin),('ProfessorWool_Normal',(np.sin(xx*math.pi*.5)+np.sin(yy*math.pi*.5+xx*.015))*.5,.16,wool)]:
 dy,dx=np.gradient(height);n=np.dstack((-dx*strength,-dy*strength,np.ones_like(dx)));n/=np.linalg.norm(n,axis=2)[:,:,None];pix=np.ones((size,size,4));pix[:,:,:3]=n*.5+.5;im=save_img(name,pix,True)
 bs=next(n for n in mat.node_tree.nodes if n.type=='BSDF_PRINCIPLED');t=mat.node_tree.nodes.new('ShaderNodeTexImage');t.image=im;nm=mat.node_tree.nodes.new('ShaderNodeNormalMap');mat.node_tree.links.new(t.outputs['Color'],nm.inputs['Color']);mat.node_tree.links.new(nm.outputs['Normal'],bs.inputs['Normal'])
# Bake modifier geometry, join by material for a small draw-call count, keep named editable pieces in source.
export=[];deps=bpy.context.evaluated_depsgraph_get()
for group_name,objects in groups.items():
 bymat={}
 for ob in objects:
  me=bpy.data.meshes.new_from_object(ob.evaluated_get(deps));me.transform(ob.matrix_world);me.calc_loop_triangles();mat=ob.data.materials[0].name
  part=bymat.setdefault(mat,{'name':group_name+'_'+mat,'material':mat,'positions':[],'normals':[],'uv':[],'triangles':[]});layer=me.uv_layers.active
  # Split at UV seams and flat normals. Dedup keeps the export compact.
  cache=part.setdefault('_cache',{})
  for tri in me.loop_triangles:
   ids=[]
   for li in tri.loops:
    loop=me.loops[li];p=me.vertices[loop.vertex_index].co;n=me.vertices[loop.vertex_index].normal;uv=layer.data[li].uv if layer else (p.x,p.z)
    key=tuple(round(float(q),6) for q in (*p,*n,*uv))
    if key not in cache:
     cache[key]=len(part['positions'])//3;part['positions'] += [round(p.x,6),round(p.z,6),round(-p.y,6)];part['normals'] += [round(n.x,6),round(n.z,6),round(-n.y,6)];part['uv'] += [round(float(uv[0]),6),round(float(uv[1]),6)]
    ids.append(cache[key])
   # Axis conversion is a proper rotation (determinant +1), so preserve winding.
   part['triangles'] += ids
  bpy.data.meshes.remove(me)
 for part in bymat.values():del part['_cache']
 export.append({'name':group_name,'parts':list(bymat.values())})
(ART/'Professor.meshdata.json').write_text(json.dumps({'groups':export},separators=(',',':')))
print('EXPORT',[(g['name'],sum(len(p['triangles'])//3 for p in g['parts'])) for g in export],flush=True)
# Source presentation: portrait and body in separate collections. Body hidden for portrait.
for ob in groups['Body']:ob.hide_render=True;ob.hide_set(True)
world=bpy.data.worlds.new('Portrait studio');bpy.context.scene.world=world;world.use_nodes=True;bg=next(n for n in world.node_tree.nodes if n.type=='BACKGROUND');bg.inputs[0].default_value=(.065,.078,.09,1);bg.inputs[1].default_value=.35
for loc,power,size,color in [((-1.4,-1.4,1.5),85,1.1,(1,.83,.66)),((1,-1,.3),25,1,(.68,.79,1)),((.7,.5,.9),100,.7,(.62,.75,1))]:
 d=bpy.data.lights.new('Portrait softbox','AREA');d.energy=power;d.shape='DISK';d.size=size;d.color=color;o=bpy.data.objects.new('Portrait softbox',d);bpy.context.collection.objects.link(o);o.location=loc;o.rotation_euler=(Vector((0,0,.05))-o.location).to_track_quat('-Z','Y').to_euler()
d=bpy.data.cameras.new('Portrait camera');cam=bpy.data.objects.new('Portrait camera',d);bpy.context.collection.objects.link(cam);cam.location=(.80,-2.15,.30);cam.rotation_euler=(Vector((0,-.025,.055))-cam.location).to_track_quat('-Z','Y').to_euler();d.type='ORTHO';d.ortho_scale=1.08
sc=bpy.context.scene;sc.camera=cam;sc.render.engine='CYCLES';sc.cycles.samples=24;sc.cycles.use_denoising=True;sc.render.resolution_x=1000;sc.render.resolution_y=1000;sc.render.resolution_percentage=100;sc.view_settings.view_transform='AgX';sc.render.image_settings.file_format='PNG'
# Reference lives in the editable file as an image empty, hidden from render.
ref=bpy.data.objects.new('REFERENCE / Planck 1933 / structure only',None);bpy.context.collection.objects.link(ref);ref.empty_display_type='IMAGE';ref.data=bpy.data.images.load(str(P/'References/Max-Planck-1933-reference.jpg'));ref.empty_display_size=1;ref.location=(-1.1,.5,.05);ref.rotation_euler=(math.pi/2,0,0);ref.hide_render=True
bpy.ops.file.pack_all()
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Professor.blend'),compress=True)
sc.render.filepath=str(OUT/'Professor-portrait.png');bpy.ops.render.render(write_still=True)
print('PROFESSOR_DONE',flush=True)
