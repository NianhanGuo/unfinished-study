import bpy, bmesh, math, json, struct
import numpy as np
from mathutils import Vector, Matrix, Quaternion
from pathlib import Path
PROJECT=Path(__file__).resolve().parents[1]
OUT=PROJECT/'Blender';ART=PROJECT/'Assets/Study/ArtV7'
OUT.mkdir(parents=True,exist_ok=True);ART.mkdir(parents=True,exist_ok=True)
bpy.ops.object.select_all(action='SELECT');bpy.ops.object.delete(use_global=False)
b=(PROJECT/'Assets/Study/ArtV5/GenericHand-source.glb').read_bytes();sz=struct.unpack_from('<I',b,12)[0];j=json.loads(b[20:20+sz]);blob=b[28+sz:]
def arr(i):
 a=j['accessors'][i];v=j['bufferViews'][a['bufferView']];n={'SCALAR':1,'VEC2':2,'VEC3':3,'VEC4':4,'MAT4':16}[a['type']];dt={5126:'<f4',5123:'<u2',5121:'u1',5125:'<u4'}[a['componentType']];return np.frombuffer(blob,dtype=dt,count=a['count']*n,offset=v.get('byteOffset',0)+a.get('byteOffset',0)).reshape(-1,n)
verts=arr(0).copy();faces=arr(5).reshape(-1,3).astype(int);weights=arr(4);joints=arr(3).astype(int);rest=np.array([n['translation'] for n in j['nodes'][:25]]);names=[n['name'] for n in j['nodes'][:25]]
parents=[None,0,1,2,3,0,5,6,7,8,0,10,11,12,13,0,15,16,17,18,0,20,21,22,23]
skin=bpy.data.materials.new('Matte skin / no nails');skin.diffuse_color=(.56,.37,.27,1);skin.use_nodes=True;bs=skin.node_tree.nodes.get('Principled BSDF');bs.inputs['Base Color'].default_value=(.56,.37,.27,1);bs.inputs['Roughness'].default_value=.49;bs.inputs['Subsurface Weight'].default_value=.065
noise=skin.node_tree.nodes.new('ShaderNodeTexNoise');noise.inputs['Scale'].default_value=950;noise.inputs['Detail'].default_value=2
bump=skin.node_tree.nodes.new('ShaderNodeBump');bump.inputs['Strength'].default_value=.11;bump.inputs['Distance'].default_value=.00012;skin.node_tree.links.new(noise.outputs['Fac'],bump.inputs['Height']);skin.node_tree.links.new(bump.outputs['Normal'],bs.inputs['Normal'])
finals=[]
def build(name,left,yaw,wrist):
 R=np.array(Matrix.Rotation(math.radians(yaw),3,'Y'));C=np.array([[0,0,-1 if left else 1],[1,0,0],[0,-1,0]],float);A=R@C;scale=.88;wr=np.array(wrist)
 V=(verts-rest[0])@A.T*scale+wr;P=(rest-rest[0])@A.T*scale+wr
 F=faces[:,::-1] if np.linalg.det(A)<0 else faces
 mesh=bpy.data.meshes.new(name+' neutral anatomy');mesh.from_pydata(V.tolist(),[],F.tolist());mesh.update();obj=bpy.data.objects.new(name+' / editable skin',mesh);bpy.context.collection.objects.link(obj);obj.data.materials.append(skin)
 arm=bpy.data.armatures.new(name+' anatomical joints');rig=bpy.data.objects.new(name+' / pose rig',arm);bpy.context.collection.objects.link(rig);bpy.context.view_layer.objects.active=rig;rig.select_set(True);bpy.ops.object.mode_set(mode='EDIT')
 for i,n in enumerate(names):
  bone=arm.edit_bones.new(n);bone.head=P[i]
  if i==0:bone.tail=P[0]+R@np.array([0,0,.028])
  elif i in [4,9,14,19,24]:bone.tail=P[i]+(P[i]-P[i-1])*.3
  else:bone.tail=P[i+1]
  bone.align_roll(Vector((0,1,0)))
 for i,p in enumerate(parents):
  if p is not None:arm.edit_bones[names[i]].parent=arm.edit_bones[names[p]]
 bpy.ops.object.mode_set(mode='OBJECT');rig.select_set(False)
 for i,n in enumerate(names):
  group=obj.vertex_groups.new(name=n)
  for k in range(4):
   for ix in np.where((joints[:,k]==i)&(weights[:,k]>.00001))[0]:group.add([int(ix)],float(weights[ix,k]),'REPLACE')
 # glTF duplicates vertices at UV seams. Weld BEFORE subdivision to avoid false gaps at wrists and fingertips.
 bm=bmesh.new();bm.from_mesh(mesh);bmesh.ops.remove_doubles(bm,verts=list(bm.verts),dist=.000002);bm.to_mesh(mesh);bm.free();mesh.update()
 mod=obj.modifiers.new('Anatomical pose','ARMATURE');mod.object=rig
 # Flexion is applied at anatomical joints without stretching phalanges.
 axis=Vector(R@np.array([1,0,0]))
 def curl(idx,degrees):
  pb=rig.pose.bones[names[idx]];local=pb.bone.matrix_local.to_3x3().inverted()@axis;pb.rotation_mode='QUATERNION';pb.rotation_quaternion=Quaternion(local,math.radians(degrees))
 if left:
  for first,angles in [(6,(58,83,28)),(11,(62,86,30)),(16,(65,85,34)),(21,(68,82,35))]:
   for idx,d in zip(range(first,first+3),angles):curl(idx,d)
 else:
  for first,angles in [(11,(53,92,34)),(16,(67,93,35)),(21,(73,89,34))]:
   for idx,d in zip(range(first,first+3),angles):curl(idx,d)
 def ik(last,tip,target,pole,count):
  target_obj=bpy.data.objects.new(name+' / '+tip+' target',None);bpy.context.collection.objects.link(target_obj);target_obj.location=target;target_obj.empty_display_size=.008
  c=rig.pose.bones[names[last]].constraints.new('IK');c.target=target_obj;c.chain_count=count;c.use_stretch=False;c.iterations=150
  # Planar initial flexion keeps a natural arch before the IK end point is imposed.
  if pole:
   po=bpy.data.objects.new(name+' / '+tip+' pole',None);bpy.context.collection.objects.link(po);po.location=pole;po.empty_display_size=.008;c.pole_target=po;c.pole_angle=0
  return target_obj
 if left:
  ik(3,'thumb',(.016,.014,-.038),None,3)
 else:
  curl(6,-15);curl(7,78);curl(8,25)
  ik(8,'index',(.025,.032,-.031),None,3)
  ik(3,'thumb',(.030,.022,-.035),None,3)
 bpy.context.view_layer.update()
 # Retain the editable weighted model and bake a dedicated clean game mesh.
 deps=bpy.context.evaluated_depsgraph_get();baked=bpy.data.meshes.new_from_object(obj.evaluated_get(deps));final=bpy.data.objects.new(name,baked);bpy.context.collection.objects.link(final);final.data.materials.clear();final.data.materials.append(skin)
 obj.hide_render=True;obj.hide_set(True);rig.hide_render=True;rig.hide_set(True)
 # Smooth the source topology once before voxel-welding a continuous oval wrist / forearm.
 bpy.context.view_layer.objects.active=final;final.select_set(True);sub=final.modifiers.new('Anatomical surface refinement','SUBSURF');sub.levels=1;bpy.ops.object.modifier_apply(modifier=sub.name)
 av=[];af=[];rings=10;sides=32
 for k in range(rings):
  t=k/(rings-1);center=wr+R@np.array([0,-.004,.016-.153*t]);rx=.024+.004*t;ry=.0175+.0045*t
  for q in range(sides):
   a=2*math.pi*q/sides;av.append(center+R@np.array([rx*math.cos(a),ry*math.sin(a),0]))
   if k>0:
    a0=(k-1)*sides+q;b0=(k-1)*sides+(q+1)%sides;af.append([a0,b0,b0+sides,a0+sides])
 af.append(list(reversed(range(sides))));af.append(list(range((rings-1)*sides,rings*sides)))
 am=bpy.data.meshes.new('Oval forearm');am.from_pydata([list(v) for v in av],[],af);am.update();ao=bpy.data.objects.new('Wrist transition',am);bpy.context.collection.objects.link(ao);ao.select_set(True);bpy.context.view_layer.objects.active=final;bpy.ops.object.join()
 # Correct normals before remeshing closed intersecting surfaces.
 bpy.ops.object.mode_set(mode='EDIT');bpy.ops.mesh.select_all(action='SELECT');bpy.ops.mesh.normals_make_consistent(inside=False);bpy.ops.object.mode_set(mode='OBJECT')
 rem=final.modifiers.new('Continuous skin at wrist','REMESH');rem.mode='VOXEL';rem.voxel_size=.00085;rem.use_smooth_shade=True;bpy.ops.object.modifier_apply(modifier=rem.name)
 sm=final.modifiers.new('Soft tissue smoothing','SMOOTH');sm.factor=.38;sm.iterations=3;bpy.ops.object.modifier_apply(modifier=sm.name)
 vg=final.vertex_groups.new(name='Wrist soft transition')
 for v in final.data.vertices:
  z=float(np.dot(np.array(v.co)-wr,R@np.array([0,0,1])));weight=math.exp(-((z+.006)/.025)**2)
  if weight>.01:vg.add([v.index],weight,'REPLACE')
 smooth=final.modifiers.new('Sculpted wrist blending','SMOOTH');smooth.factor=.7;smooth.iterations=22;smooth.vertex_group=vg.name;bpy.ops.object.modifier_apply(modifier=smooth.name)
 # Sculpt an uninterrupted oval wrist, blending into the original palm rather than retaining a cap seam.
 for v in final.data.vertices:
  local=R.T@(np.array(v.co)-wr);z=float(local[2]);u=max(0,min(1,(.036-z)/.026));u=u*u*(3-2*u);lo=max(0,min(1,(z+.10)/.025));lo=lo*lo*(3-2*lo);w=u*lo
  if w>0:
   rx=.023+max(0,-z)*.035;ry=.017+max(0,-z)*.029;angle=math.atan2((local[1]+.004)/ry,local[0]/rx)
   local[0]=(1-w)*local[0]+w*rx*math.cos(angle);local[1]=(1-w)*local[1]+w*(-.004+ry*math.sin(angle));v.co=Vector(wr+R@local)
 final.data.update()
 dec=final.modifiers.new('Game mesh','DECIMATE');dec.ratio=.22;bpy.ops.object.modifier_apply(modifier=dec.name)
 final.data.materials.clear();final.data.materials.append(skin)
 sub=final.modifiers.new('Smooth anatomical silhouette','SUBSURF');sub.levels=1;bpy.ops.object.modifier_apply(modifier=sub.name)
 for poly in final.data.polygons:poly.use_smooth=True;poly.material_index=0
 final['purpose']='Left support grip' if left else 'Right tripod charcoal grip';final['nails']='No nail geometry, nail materials or decorative tips';final['source']='MIT generic-hand anatomy, reposed and wrist-remodeled in Blender';final.select_set(False)
 finals.append(final)
 # OBJ uses exact board coordinates, avoiding Unity OBJ axis reflections.
 mesh=final.data;mesh.calc_loop_triangles()
 with (ART/(name+'.obj')).open('w') as f:
  f.write('o '+name+'\n')
  for v in mesh.vertices:f.write('v %.7f %.7f %.7f\n'%tuple(v.co))
  for v in mesh.vertices:f.write('vt %.7f %.7f\n'%(v.co.x*8,v.co.z*8))
  for v in mesh.vertices:f.write('vn %.7f %.7f %.7f\n'%tuple(v.normal))
  for tri in mesh.loop_triangles:f.write('f '+' '.join(f'{i+1}/{i+1}/{i+1}' for i in tri.vertices)+'\n')
 print('EXPORTED',name,len(mesh.vertices),len(mesh.loop_triangles),flush=True)
 return final
left=build('HoldingHand',True,25,(-.075,.005,-.12));right=build('WritingHand',False,-35,(.105,.055,-.105))
# Neutral studio presentation for checking proportions before Unity integration.
left.location=(-.17,0,0);right.location=(.10,0,0)
# Contact props are inspection references only; neither is exported with the hand.
paper=bpy.data.materials.new('Inspection board');paper.diffuse_color=(.12,.16,.17,1);paper.use_nodes=True;paper.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.12,.16,.17,1)
bpy.ops.mesh.primitive_cube_add(size=1,location=(-.065,-.006,-.045));board=bpy.context.object;board.name='Grip inspection / board';board.dimensions=(.21,.012,.31);board.data.materials.append(paper);bevel=board.modifiers.new('Soft edge','BEVEL');bevel.width=.002;bevel.segments=3
charcoal=bpy.data.materials.new('Charcoal reference');charcoal.diffuse_color=(.018,.016,.014,1);charcoal.use_nodes=True;charcoal.node_tree.nodes.get('Principled BSDF').inputs['Base Color'].default_value=(.018,.016,.014,1)
a=Vector((.10,0,0));b=Vector((.18,.100,-.106));bpy.ops.mesh.primitive_cylinder_add(vertices=16,radius=.003,depth=(b-a).length,location=(a+b)*.5);pen=bpy.context.object;pen.name='Grip inspection / charcoal';pen.rotation_euler=(b-a).to_track_quat('Z','Y').to_euler();pen.data.materials.append(charcoal)
# Mirror presentation coordinates to account for Unity's left-handed view convention.
for o in list(bpy.context.scene.objects):o.matrix_world=Matrix.Diagonal(Vector((-1,1,1,1)))@o.matrix_world
world=bpy.context.scene.world or bpy.data.worlds.new('Studio');bpy.context.scene.world=world;world.use_nodes=True;world.node_tree.nodes['Background'].inputs[0].default_value=(.10,.12,.14,1);world.node_tree.nodes['Background'].inputs[1].default_value=.4
for loc,power,size in [((-.35,.6,.3),4,.5),((.4,.3,-.1),2,.4),((0,-.25,.2),1.5,.3)]:
 data=bpy.data.lights.new('Softbox','AREA');data.energy=power;data.shape='DISK';data.size=size;o=bpy.data.objects.new('Softbox',data);bpy.context.collection.objects.link(o);o.location=loc;o.rotation_euler=(Vector((0,0,-.06))-o.location).to_track_quat('-Z','Y').to_euler()
camdata=bpy.data.cameras.new('Hand inspection');cam=bpy.data.objects.new('Hand inspection',camdata);bpy.context.collection.objects.link(cam);cam.location=(0,.8,-.30);target=Vector((0,0,-.1));cam.rotation_euler=(target-cam.location).to_track_quat('-Z','Y').to_euler();camdata.type='ORTHO';camdata.ortho_scale=.64;bpy.context.scene.camera=cam
scene=bpy.context.scene;scene.render.engine='CYCLES';scene.cycles.device='CPU';scene.cycles.samples=32;scene.cycles.use_denoising=True;scene.render.resolution_x=1300;scene.render.resolution_y=850;scene.render.resolution_percentage=100;scene.view_settings.view_transform='AgX';scene.render.image_settings.file_format='PNG';scene.render.film_transparent=False
for o in bpy.context.selected_objects:o.select_set(False)
for o in finals:o.select_set(True)
for screen in bpy.data.screens:
 for area in screen.areas:
  if area.type=='VIEW_3D':
   area.spaces.active.clip_start=.001;area.spaces.active.shading.color_type='MATERIAL';area.spaces.active.region_3d.view_perspective='CAMERA'
bpy.context.view_layer.objects.active=right
bpy.ops.wm.save_as_mainfile(filepath=str(OUT/'StudyHands.blend'))
scene.render.filepath=str(OUT/'Hands-Blender.png');bpy.ops.render.render(write_still=True)
print('HAND_MODELS_DONE',flush=True)
