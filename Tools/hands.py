"""Continuous posed viewmodel hand meshes, in board-local metres."""
import numpy as np
from skimage.measure import marching_cubes
from pathlib import Path
out=Path(__file__).resolve().parents[1]/'Assets/Study/ArtV4';out.mkdir(exist_ok=True)
def hand(name,lo,hi,ellipsoids,chains):
 step=.00135
 x,y,z=np.meshgrid(*[np.arange(lo[i],hi[i],step,dtype=np.float32) for i in range(3)],indexing='ij')
 field=np.full(x.shape,10,np.float32)
 def unite(d,k=.0024):
  nonlocal field
  h=np.maximum(k-np.abs(field-d),0)/k;field=np.minimum(field,d)-h*h*k*.25
 for p,s in ellipsoids:unite((np.sqrt(((x-p[0])/s[0])**2+((y-p[1])/s[1])**2+((z-p[2])/s[2])**2)-1)*min(s),.004)
 for pts,radii in chains:
  for i in range(len(pts)-1):
   a=np.array(pts[i]);b=np.array(pts[i+1]);v=b-a;t=np.clip(((x-a[0])*v[0]+(y-a[1])*v[1]+(z-a[2])*v[2])/np.dot(v,v),0,1)
   d=np.sqrt((x-a[0]-t*v[0])**2+(y-a[1]-t*v[1])**2+(z-a[2]-t*v[2])**2)-(radii[i]*(1-t)+radii[i+1]*t);unite(d)
 v,f,n,_=marching_cubes(field,0,spacing=(step,step,step));v+=np.array(lo)
 with (out/(name+'.obj')).open('w') as w:
  w.write('o '+name+'\n')
  for a in v:w.write('v %.6f %.6f %.6f\n'%tuple(a))
  for a in v:w.write('vt %.6f %.6f\n'%(a[0]*8,a[2]*8))
  for a in n:w.write('vn %.6f %.6f %.6f\n'%tuple(-a))
  for a in f+1:w.write('f '+' '.join(f'{i}/{i}/{i}' for i in a)+'\n')
 print(name,len(v),'vertices')
# Right hand: tripod charcoal grip, smaller palm; continuous webbing and wrist.
palm=[((.087,.039,-.091),(.033,.0165,.046)),((.067,.028,-.068),(.022,.017,.031)),((.102,.037,-.118),(.024,.016,.026))]
chains=[
 ([(.102,.034,-.119),(.123,.027,-.158),(.151,.016,-.218)],[.024,.023,.028]),
 ([(.068,.048,-.059),(.047,.058,-.033),(.029,.048,-.030),(.025,.031,-.029)],[.009,.0083,.0074,.0067]),
 ([(.070,.025,-.077),(.048,.017,-.052),(.034,.023,-.034),(.029,.029,-.029)],[.012,.0094,.0086,.0078]),
 ([(.083,.043,-.05),(.062,.022,-.031),(.042,.014,-.044),(.047,.019,-.064)],[.009,.0082,.0072,.0062]),
 ([(.1,.042,-.059),(.078,.014,-.039),(.063,.007,-.054),(.07,.015,-.075)],[.0085,.0077,.0065,.0059]),
 ([(.114,.038,-.073),(.097,.01,-.053),(.084,.006,-.07),(.089,.013,-.086)],[.0073,.0065,.0058,.0052])]
hand('WritingHand',(.005,-.03,-.255),(.2,.079,.005),palm,chains)
# Left: thumb on upper sheet edge, fingers curl around board thickness.
palm=[((-.03,-.015,-.023),(.028,.018,.053)),((-.027,-.012,-.065),(.024,.017,.03))]
chains=[([(-.028,-.013,-.06),(-.034,-.025,-.13),(-.042,-.035,-.2)],[.023,.026,.03])]
for i in range(4):
 zz=.022-i*.022
 chains.append(([(-.033,-.008,zz),(-.022,-.027,zz+.004),(.007,-.029,zz+.003),(.028,-.019,zz-.005)],[.0088-i*.00045,.0082-i*.0004,.0075-i*.0004,.006-i*.0003]))
chains.append(([(-.031,-.002,-.019),(-.014,.022,.015),(.009,.028,.038),(.035,.013,.052)],[.013,.011,.0098,.008]))
hand('HoldingHand',(-.087,-.08,-.24),(.06,.055,.073),palm,chains)
