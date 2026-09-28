import numpy as np
from skimage.measure import marching_cubes
from pathlib import Path
out=Path(__file__).resolve().parents[1]/'Assets/Study/ArtV2'
def build(name,lo,hi,step,pieces,cuts):
 axes=[np.arange(lo[i],hi[i],step,dtype=np.float32) for i in range(3)];x,y,z=np.meshgrid(*axes,indexing='ij');field=np.ones(x.shape,dtype=np.float32)*10
 def ell(p,s):return (np.sqrt(((x-p[0])/s[0])**2+((y-p[1])/s[1])**2+((z-p[2])/s[2])**2)-1)*min(s)
 for p,s in pieces:
  d=ell(p,s);k=.008;h=np.maximum(k-np.abs(field-d),0)/k;field=np.minimum(field,d)-h*h*k*.25
 for p,s in cuts:field=np.maximum(field,-ell(p,s))
 v,f,n,_=marching_cubes(field,0,spacing=(step,step,step));v+=np.array(lo)
 with open(out/(name+'.obj'),'w') as w:
  w.write('o '+name+'\n')
  for a in v:w.write('v %.6f %.6f %.6f\n'%tuple(a))
  for a in n:w.write('vn %.6f %.6f %.6f\n'%tuple(-a))
  for a in f+1:w.write('f '+' '.join(f'{i}//{i}' for i in a)+'\n')
 print(name,len(v),len(f))
# coordinates: X horizontal, Y vertical, Z face direction
skull=[((0,.075,0),(.145,.13,.135)),((0,.070,-.045),(.13,.13,.125)),((0,-.025,.07),(.108,.10,.11)),((0,-.077,.082),(.084,.05,.078))]
for s in [-1,1]:skull += [((s*.104,-.015,.07),(.047,.035,.07)),((s*.069,.048,.111),(.073,.025,.043))]
cuts=[((s*.066,.012,.136),(.047,.041,.095)) for s in [-1,1]]+[((0,-.048,.157),(.025,.035,.06)),((0,-.116,.1),(.11,.033,.12))]
build('SkullSculpt',(-.21,-.2,-.22),(.21,.31,.24),.003,skull,cuts)
jaw=[((0,-.151,.103),(.067,.027,.027))]
for s in [-1,1]:jaw+=[((s*.066,-.145,.071),(.043,.026,.071)),((s*.106,-.091,-.009),(.022,.075,.03))]
build('JawSculpt',(-.17,-.2,-.07),(.17,.02,.16),.003,jaw,[])
portrait=[((0,.19,0),(.17,.235,.145)),((0,.048,.021),(.123,.13,.12)),((0,-.135,-.015),(.085,.16,.086)),((0,-.28,-.025),(.35,.13,.15)),((0,.13,.145),(.032,.089,.047)),((0,.089,.187),(.043,.035,.035)),((0,.025,.143),(.065,.012,.019)),((0,.004,.146),(.065,.013,.018))]
for s in [-1,1]:portrait +=[((s*.083,.2,.12),(.073,.025,.033)),((s*.095,.096,.101),(.064,.065,.045)),((s*.167,.137,-.008),(.026,.069,.042))]
build('PortraitSculpt',(-.4,-.44,-.22),(.4,.46,.26),.0045,portrait,[((s*.072,.157,.146),(.047,.022,.033)) for s in [-1,1]])
# Natural hand palm silhouette; fingers are separate articulated meshes in Unity.
palm=[((0,0,0),(.038,.018,.05)),((-.026,.004,.008),(.019,.016,.035)),((.014,-.008,-.03),(.024,.016,.03))]
build('PalmSculpt',(-.07,-.04,-.08),(.07,.04,.075),.002,palm,[])
