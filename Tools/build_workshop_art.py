"""Authored workshop meshes and material maps. Run with Blender -b --python.
Hands derive from the existing MIT licensed GenericHand-source.glb; see Blender/GenericHand-LICENSE.md.
"""
import bpy,bmesh,math,json,struct
import numpy as np
from pathlib import Path
from mathutils import Vector
P=Path(__file__).resolve().parents[1];ART=P/'Assets/Study/ArtWorkshop';OUT=P/'Blender/Workshop'
ART.mkdir(parents=True,exist_ok=True);OUT.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
groups={};group='';mats={}
def mat(n,c,rough=.7,metal=0):
 m=bpy.data.materials.new(n);m.use_nodes=True;m.diffuse_color=(*c,1);bs=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');bs.inputs['Base Color'].default_value=(*c,1);bs.inputs['Roughness'].default_value=rough;bs.inputs['Metallic'].default_value=metal;mats[n]=m;return m
wood=mat('Wood',(.32,.235,.16),.65);steel=mat('Steel',(.43,.46,.47),.42,.85);paint=mat('Paint',(.07,.085,.088),.74);paper=mat('Paper',(.72,.70,.64),.89);rubber=mat('Mat',(.065,.20,.17),.86);skin=mat('StudentSkin',(.41,.285,.21),.66);cloth=mat('Sleeve',(.17,.20,.20),.85);ink=mat('Ink',(.045,.056,.057),.87)
def add(o,name,material):
 o.name=name;o.data.materials.append(material);groups.setdefault(group,[]).append(o);return o
def box(name,pos,size,m,bevel=.003):
 # Unity X,Y,Z -> Blender X,-Z,Y.
 bpy.ops.mesh.primitive_cube_add(size=1,location=(pos[0],-pos[2],pos[1]));o=bpy.context.object;o.scale=(size[0],size[2],size[1]);bpy.ops.object.transform_apply(location=False,rotation=False,scale=True);add(o,name,m)
 # Physical projection: consistent grain scale regardless of furniture size.
 layer=o.data.uv_layers.active
 for poly in o.data.polygons:
  axis=max(range(3),key=lambda i:abs(poly.normal[i]))
  for li in poly.loop_indices:
   v=o.data.vertices[o.data.loops[li].vertex_index].co
   uv=(v.x/2,-v.y/1.2) if axis==2 else (v.y/2,v.z/1.2) if axis==0 else (v.x/2,v.z/1.2)
   layer.data[li].uv=uv
 if bevel:
  b=o.modifiers.new('Manufactured edge radius','BEVEL');b.width=bevel;b.segments=3;b.affect='EDGES'
  n=o.modifiers.new('Stable broad surface normals','WEIGHTED_NORMAL');n.keep_sharp=True
 return o
def cyl(name,pos,r,depth,m,axis='Y'):
 bpy.ops.mesh.primitive_cylinder_add(vertices=16,radius=r,depth=depth,location=(pos[0],-pos[2],pos[1]));o=bpy.context.object
 if axis=='Z':o.rotation_euler.x=math.pi/2
 if axis=='X':o.rotation_euler.y=math.pi/2
 add(o,name,m);b=o.modifiers.new('Machined edge','BEVEL');b.width=.001;b.segments=2;return o
# Crawl-under table stays exactly within the existing tabletop and leg collision envelope.
group='Worktable'
box('Laminated worktop',(0,1.11,0),(3.8,.14,2.4),wood,.009)
for x in [-1.65,1.65]:
 for z in [-.95,.95]:
  box('Powder-coated square tube',(x,.52,z),(.14,1.04,.14),paint,.008)
  box('Rubber foot',(x,.025,z),(.15,.05,.15),paint,.009)
  for dz in [-.066,.066]:cyl('Recessed fastener',(x,.96,z+dz),.014,.007,steel,'Z')
for x in [-1.65,1.65]:box('Under-top rail',(x,1.00,0),(.07,.065,2.0),paint,.006)
for z in [-.95,.95]:box('Under-top rail',(0,1.00,z),(3.30,.065,.07),paint,.006)
# Closed workstation carcass visually accounts for its solid gameplay collider.
group='Cabinet'
box('Carcass',(0,.48,0),(2.77,.95,1.08),wood,.007);box('Countertop',(0,.97,0),(2.8,.06,1.1),wood,.006)
for x in [-.69,.69]:
 box('Inset cabinet door',(x,.47,-.545),(1.32,.79,.028),wood,.004)
 box('Recessed door pull',(x+(.48 if x<0 else -.48),.58,-.565),(.026,.18,.022),steel,.006)
box('Recessed toe kick',(0,.05,-.535),(2.66,.09,.04),paint,.003)
group='Cutter'
box('Rounded cutter bed',(0,1.075,.05),(1.5,.05,.55),paint,.004)
box('Five millimetre model stock',(0,1.104,.05),(1.4,.005,.50),paper,.0006)
for z in [-.195,.295]:box('Steel alignment edge',(.72,1.105,z),(.025,.006,.05),steel,.001)
group='Ruler';box('Brushed steel ruler',(0,0,0),(.07,.004,.62),steel,.001)
for i in range(61):
 z=-.3+i*.01;length=.033 if i%10==0 else .024 if i%5==0 else .013
 box('Etched millimetre divisions',(-.03+length*.5,.0025,z),(length,.0005,.001),ink,0)
cyl('Ruler hanging rivet',(0,.003,.29),.009,.002,ink)
group='PaperStack'
for i in range(3):
 o=box('Offset paper leaf',(i*.006,i*.0014,0),(.27,.001,.35),paper,.0004);o.rotation_euler.z=i*.04
# Anatomical continuous skin, mild finger flexion, from the project's licensed neutral hand mesh.
b=(P/'Assets/Study/ArtV5/GenericHand-source.glb').read_bytes();sz=struct.unpack_from('<I',b,12)[0];j=json.loads(b[20:20+sz]);blob=b[28+sz:]
def arr(i):
 a=j['accessors'][i];v=j['bufferViews'][a['bufferView']];n={'SCALAR':1,'VEC2':2,'VEC3':3,'VEC4':4}[a['type']];dt={5126:'<f4',5123:'<u2',5121:'u1',5125:'<u4'}[a['componentType']];return np.frombuffer(blob,dtype=dt,count=a['count']*n,offset=v.get('byteOffset',0)+a.get('byteOffset',0)).reshape(-1,n)
verts=arr(0).copy();faces=arr(5).reshape(-1,3).astype(int);weights=arr(4);joints=arr(3).astype(int);rest=np.array([n['translation'] for n in j['nodes'][:25]])
parents=[None,0,1,2,3,0,5,6,7,8,0,10,11,12,13,0,15,16,17,18,0,20,21,22,23]
for left in [True,False]:
 group='LeftHand' if left else 'RightHand';C=np.array([[0,0,-1 if left else 1],[1,0,0],[0,-1,0]],float);V=(verts-rest[0])@C.T*.94;Q=(rest-rest[0])@C.T*.94
 # Descending toward the table by a few degrees, never stretching finger bones.
 transforms=[];H=np.column_stack((V,np.ones(len(V))));posed=np.zeros_like(H)
 for i in range(25):
  angle=0
  if i in [6,11,16,21]:angle=math.radians(7 if left else 12)
  if i in [7,12,17,22]:angle=math.radians(9 if left else 16)
  if i in [8,13,18,23]:angle=math.radians(4)
  R=np.eye(4);R[1:3,1:3]=[[math.cos(angle),-math.sin(angle)],[math.sin(angle),math.cos(angle)]]
  T=np.eye(4);T[:3,3]=Q[i];I=np.eye(4);I[:3,3]=-Q[i];M=T@R@I
  if parents[i] is not None:M=transforms[parents[i]]@M
  transforms.append(M)
 for k in range(4):
  for idx,M in enumerate(transforms):
   mask=joints[:,k]==idx;posed[mask]+=(H[mask]@M.T)*weights[mask,k,None]
 V=posed[:,:3];F=faces[:,::-1] if np.linalg.det(C)<0 else faces
 # Proper rotation to Blender coordinates, same mapping used on export.
 BV=np.column_stack((V[:,0],-V[:,2],V[:,1]));me=bpy.data.meshes.new(group+' anatomy');me.from_pydata(BV.tolist(),[],F.tolist());me.update();o=bpy.data.objects.new(group,me);bpy.context.collection.objects.link(o);add(o,group,skin)
 bm=bmesh.new();bm.from_mesh(me);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.000002);bmesh.ops.recalc_face_normals(bm,faces=list(bm.faces));bm.to_mesh(me);bm.free()
 for poly in me.polygons:poly.use_smooth=True
 bpy.context.view_layer.objects.active=o;o.select_set(True);sub=o.modifiers.new('Skin continuity','SUBSURF');sub.levels=1;bpy.ops.object.modifier_apply(modifier=sub.name)
 print('HAND_BOUNDS',group,[(round(float(min(v[i] for v in V)),3),round(float(max(v[i] for v in V)),3)) for i in range(3)],flush=True)
 # Open knitted cuff wraps the wrist, unlike a solid cube across the hand.
 vv=[];ff=[];uv=[]
 for r in range(9):
  t=r/8;z=-.052+t*.056;rx=.032+.002*math.sin(t*8*math.pi);ry=.022
  for k in range(25):
   a=k*2*math.pi/24;vv.append((math.cos(a)*rx,-z,math.sin(a)*ry));uv.append((k/24,t))
   if r<8 and k<24:q=r*25+k;ff.append((q,q+1,q+26,q+25))
 cm=bpy.data.meshes.new('Ribbed cuff');cm.from_pydata(vv,[],ff);cm.update();co=bpy.data.objects.new('Ribbed cuff',cm);bpy.context.collection.objects.link(co);add(co,'Ribbed cuff',cloth)
 for poly in cm.polygons:poly.use_smooth=True
# Tileable authored PBR maps.
N=1024;y,x=np.mgrid[0:N,0:N].astype(float);u=x/N;v=y/N;rng=np.random.default_rng(37);noise=rng.random((N,N))
def image(name,pixels,data=False):
 im=bpy.data.images.new(name,width=N,height=N,alpha=True);im.colorspace_settings.name='Non-Color' if data else 'sRGB';im.pixels.foreach_set(np.clip(pixels,0,1).astype(np.float32).ravel());im.filepath_raw=str(ART/(name+'.png'));im.file_format='PNG';im.save();return im
def maps(name,base,height,smooth,metal=0):
 color=np.ones((N,N,4));color[:,:,:3]=base;im=image(name+'_BaseColor',color)
 dy,dx=np.gradient(height);nn=np.dstack((-dx,-dy,np.ones_like(dx)));nn/=np.linalg.norm(nn,axis=2)[:,:,None];normal=np.ones_like(color);normal[:,:,:3]=nn*.5+.5;nm=image(name+'_Normal',normal,True)
 mask=np.zeros_like(color);mask[:,:,:3]=metal;mask[:,:,3]=smooth;image(name+'_Surface',mask,True)
 if name in mats:
  m=mats[name];bs=next(n for n in m.node_tree.nodes if n.type=='BSDF_PRINCIPLED');tex=m.node_tree.nodes.new('ShaderNodeTexImage');tex.image=im;m.node_tree.links.new(tex.outputs['Color'],bs.inputs['Base Color']);t=m.node_tree.nodes.new('ShaderNodeTexImage');t.image=nm;n=m.node_tree.nodes.new('ShaderNodeNormalMap');m.node_tree.links.new(t.outputs['Color'],n.inputs['Color']);m.node_tree.links.new(n.outputs['Normal'],bs.inputs['Normal'])
wave=np.sin(v*220+np.sin(u*12)*1.6+np.sin(u*31)*.3);fine=np.sin(v*970+np.sin(u*16)*3)
variation=wave*.013+fine*.006+(noise-.5)*.009
maps('Wood',np.array([.33,.245,.17])+variation[:,:,None],wave*.035+fine*.025,.25+.08*np.sin(v*220)**2)
# Neutral cork pinboard: granular, without stretched furniture grain.
coarse=np.repeat(np.repeat(rng.random((64,64)),16,axis=0),16,axis=1)
for _ in range(8):coarse=(coarse+np.roll(coarse,1,0)+np.roll(coarse,-1,0)+np.roll(coarse,1,1)+np.roll(coarse,-1,1))/5
cork=(coarse-.5)*.07+(noise-.5)*.025
maps('Cork',np.array([.245,.218,.176])+cork[:,:,None],cork*.7,.09)
# Fine printed grid, worn knife marks, matte rubber. No raised yellow bars.
minor=(np.minimum(np.mod(x,32),32-np.mod(x,32))<.65)|(np.minimum(np.mod(y,32),32-np.mod(y,32))<.65)
major=(np.minimum(np.mod(x,160),160-np.mod(x,160))<1.2)|(np.minimum(np.mod(y,160),160-np.mod(y,160))<1.2)
base=np.broadcast_to(np.array([.065,.19,.16]),(N,N,3)).copy();base+=(noise[:,:,None]-.5)*.008;base[minor]=[.24,.36,.30];base[major]=[.37,.45,.36]
cut=np.zeros((N,N))
for i in range(35):
 slope=rng.uniform(-1,1);b=rng.uniform(0,N);lo=rng.uniform(0,N*.7);hi=lo+rng.uniform(60,280);line=(abs(y-slope*x-b)<.7)&(x>lo)&(x<hi);cut[line]=1
base[cut>0]*=.65;maps('Mat',base,(noise-.5)*.012-cut*.08,.10-cut*.04)
# Thin paper fibres and low intensity mottling.
maps('Paper',np.array([.72,.70,.64])+(noise[:,:,None]-.5)*.014,(noise-.5)*.025,.08)
# Actual floorboards have staggered end joints and per-board variation.
row=np.floor(v*7).astype(int);offset=(row%3)*.333;along=(u*3+offset)%1
joint=(np.mod(v*7,1)<.008)|(along<.0035)
floor=np.array([.235,.182,.132])+variation[:,:,None]*.8+np.sin(row*13.1)[:,:,None]*.011;floor[joint]*=.45
maps('Floor',floor,wave*.025-joint*.08,.20+.04*np.sin(row*3))
# Export evaluated meshes with corner normals (bevels must not smear broad surfaces).
export=[];deps=bpy.context.evaluated_depsgraph_get()
for name,objects in groups.items():
 parts={}
 for ob in objects:
  me=bpy.data.meshes.new_from_object(ob.evaluated_get(deps));me.transform(ob.matrix_world);me.calc_loop_triangles();mn=ob.data.materials[0].name;part=parts.setdefault(mn,{'name':name+'_'+mn,'material':mn,'positions':[],'normals':[],'uv':[],'triangles':[],'cache':{}});layer=me.uv_layers.active
  for tri in me.loop_triangles:
   for li in tri.loops:
    p=me.vertices[me.loops[li].vertex_index].co;n=me.corner_normals[li].vector;uv=layer.data[li].uv if layer else (p.x*4,p.y*4);key=tuple(round(float(q),6) for q in (*p,*n,*uv))
    if key not in part['cache']:
     part['cache'][key]=len(part['positions'])//3;part['positions'] += [round(p.x,6),round(p.z,6),round(-p.y,6)];part['normals'] += [round(n.x,6),round(n.z,6),round(-n.y,6)];part['uv'] += [round(float(uv[0]),6),round(float(uv[1]),6)]
    part['triangles'].append(part['cache'][key])
  bpy.data.meshes.remove(me)
 for part in parts.values():del part['cache']
 export.append({'name':name,'parts':list(parts.values())})
(ART/'Workshop.meshdata.json').write_text(json.dumps({'groups':export},separators=(',',':')))
# Organized editable source: separate collections, hide duplicate study groups for the table render.
for name,objects in groups.items():
 col=bpy.data.collections.new(name);bpy.context.scene.collection.children.link(col)
 for ob in objects:
  for old in list(ob.users_collection):old.objects.unlink(ob)
  col.objects.link(ob)
  if name not in ['Worktable']:ob.hide_render=True
world=bpy.data.worlds.new('Workshop studio');bpy.context.scene.world=world;world.use_nodes=True;next(n for n in world.node_tree.nodes if n.type=='BACKGROUND').inputs[1].default_value=.25
for loc,power,size in [((-3,-4,5),450,4),((3,1,3),320,3)]:
 d=bpy.data.lights.new('Softbox','AREA');d.energy=power;d.size=size;o=bpy.data.objects.new('Softbox',d);bpy.context.collection.objects.link(o);o.location=loc;o.rotation_euler=(Vector((0,0,.6))-o.location).to_track_quat('-Z','Y').to_euler()
d=bpy.data.cameras.new('Workbench');o=bpy.data.objects.new('Workbench',d);bpy.context.collection.objects.link(o);o.location=(4,-5,3.4);o.rotation_euler=(Vector((0,0,.6))-o.location).to_track_quat('-Z','Y').to_euler();d.type='ORTHO';d.ortho_scale=5.3
sc=bpy.context.scene;sc.camera=o;sc.render.engine='CYCLES';sc.cycles.samples=24;sc.cycles.use_denoising=True;sc.render.resolution_x=1200;sc.render.resolution_y=850;sc.render.resolution_percentage=100;sc.view_settings.view_transform='AgX';sc.render.image_settings.file_format='PNG'
bpy.ops.file.pack_all();bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'Workshop.blend'),compress=True);sc.render.filepath=str(OUT/'Worktable.png');bpy.ops.render.render(write_still=True)
print('WORKSHOP_DONE',[(g['name'],sum(len(p['triangles'])//3 for p in g['parts'])) for g in export],flush=True)
