import numpy as np,struct,json
from scipy.spatial.transform import Rotation
from pathlib import Path
project=Path(__file__).resolve().parents[1];source=project/'Assets/Study/ArtV5/GenericHand-source.glb';b=source.read_bytes();sz=struct.unpack_from('<I',b,12)[0];j=json.loads(b[20:20+sz]);blob=b[28+sz:]
def arr(i):
 a=j['accessors'][i];bv=j['bufferViews'][a['bufferView']];dt={5126:'<f4',5123:'<u2',5121:'u1',5125:'<u4'}[a['componentType']];n={'SCALAR':1,'VEC2':2,'VEC3':3,'VEC4':4,'MAT4':16}[a['type']];return np.frombuffer(blob,dtype=dt,count=a['count']*n,offset=bv.get('byteOffset',0)+a.get('byteOffset',0)).reshape(-1,n)
v=arr(0);faces=arr(5).reshape(-1,3).astype(int);weights=arr(4);joints=arr(3).astype(int);rest=np.array([n['translation'] for n in j['nodes'][:25]])
def align(a,b):
 a=a/np.linalg.norm(a);b=b/np.linalg.norm(b);cross=np.cross(a,b);c=np.dot(a,b)
 if c<-.999:return Rotation.from_rotvec(np.array([0,np.pi,0])).as_matrix()
 K=np.array([[0,-cross[2],cross[1]],[cross[2],0,-cross[0]],[-cross[1],cross[0],0]])
 return np.eye(3)+K+K@K/(1+c)
def make(name,A,wrist,scale=.82):
 base=(rest-rest[0])@A.T*scale+wrist;verts=(v-rest[0])@A.T*scale+wrist;target=base.copy()
 if name=='WritingHand':
  target[7:10]=[(.028,.060,-.038),(.021,.042,-.028),(.024,.031,-.027)]
  target[2:5]=[(.048,.026,-.071),(.029,.024,-.041),(.024,.030,-.027)]
  for start in [11,16,21]:
   m=base[start];factor=1 if start<21 else .83
   target[start+1]=m+np.array([-.014,-.028,.022])*factor
   target[start+2]=target[start+1]+np.array([.005,-.009,-.019])*factor
   target[start+3]=target[start+2]+np.array([.010,.007,-.010])*factor
 else:
  for start in [6,11,16,21]:
   m=base[start].copy();m[1]=min(m[1],-.021);target[start]=m;factor=1 if start<21 else .8
   target[start+1]=m+np.array([.030,-.018,.010])*factor
   target[start+2]=target[start+1]+np.array([.017,.010,.006])*factor
   target[start+3]=target[start+2]+np.array([.008,.010,.002])*factor
  target[2:5]=[(-.028,.007,-.040),(-.007,.017,-.022),(.007,.010,-.012)]
 rotations=[]
 for idx in range(25):
  if idx in [0,4,9,14,19,24]:rotations.append(np.eye(3) if idx==0 else rotations[-1]);continue
  # Metacarpals retain the palm; deformation starts at the first articulated digit.
  if idx in [5,10,15,20]:rotations.append(np.eye(3));continue
  rotations.append(align(base[idx+1]-base[idx],target[idx+1]-target[idx]))
 posed=np.zeros_like(verts)
 for k in range(4):
  for idx in range(25):
   mask=joints[:,k]==idx;posed[mask]+=((verts[mask]-base[idx])@rotations[idx].T+target[idx])*weights[mask,k:k+1]
 f=faces[:,::-1] if np.linalg.det(A)<0 else faces.copy()
 # Extend the open wrist to a tapered forearm. Preserve the model's natural wrist section.
 # Boundary edges are the cuff opening; UV seams remain paired by position for smoothing.
 # Clip off the capped end of the source wrist before adding the forearm.
 axis=A@np.array([0,1,0]);cut=.009;newv=[];newf=[]
 for tri in f:
  poly=[posed[i] for i in tri];clipped=[]
  for a,b in zip(poly,poly[1:]+poly[:1]):
   da=np.dot(a-wrist,axis)-cut;db=np.dot(b-wrist,axis)-cut
   if da<=0:clipped.append(a)
   if (da<=0)!=(db<=0):clipped.append(a+(b-a)*da/(da-db))
  if len(clipped)>=3:
   off=len(newv);newv.extend(clipped)
   for k in range(1,len(clipped)-1):newf.append([off,off+k,off+k+1])
 posed=np.array(newv);f=np.array(newf)
 rounded=np.round(posed,6);unique,inv=np.unique(rounded,axis=0,return_inverse=True);f=inv[f];posed=unique
 edges={}
 for tri in f:
  for a,b in zip(tri,np.roll(tri,-1)):
   key=tuple(sorted((a,b)));edges[key]=edges.get(key,0)+1
 boundary=[e for e,c in edges.items() if c==1];graph={}
 for a,b in boundary:graph.setdefault(a,[]).append(b);graph.setdefault(b,[]).append(a)
 # Normals averaged across the welded surface for smooth knuckles, fingers and wrist.
 normals=np.zeros_like(posed);fn=np.cross(posed[f[:,1]]-posed[f[:,0]],posed[f[:,2]]-posed[f[:,0]])
 for c in range(3):np.add.at(normals,f[:,c],fn)
 normals/=np.maximum(np.linalg.norm(normals,axis=1)[:,None],1e-12)
 out=(project/'Assets/Study/ArtV4')/(name+'.obj')
 with out.open('w') as fp:
  fp.write('o '+name+'\n')
  for a in posed:fp.write('v %.6f %.6f %.6f\n'%tuple(a))
  for a in posed:fp.write('vt %.6f %.6f\n'%(a[0]*8,a[2]*8))
  for a in normals:fp.write('vn %.6f %.6f %.6f\n'%tuple(a))
  for tri in f+1:fp.write('f '+' '.join(f'{i}/{i}/{i}' for i in tri)+'\n')
 print(name,'vertices',len(posed),'bounds',posed.min(0),posed.max(0),'boundary',len(boundary))
 print('target thumb',target[1:5],'index',target[6:10])
rot=Rotation.from_euler('y',-35,degrees=True).as_matrix();make('WritingHand',rot@np.array([[0,0,1],[1,0,0],[0,-1,0]]),np.array([.105,.058,-.105]))
rot=Rotation.from_euler('y',35,degrees=True).as_matrix();make('HoldingHand',rot@np.array([[0,0,-1],[1,0,0],[0,-1,0]]),np.array([-.080,-.003,-.080]))
