"""Read charcoal artwork and derive drawing-tool paths; does not edit source art.
Requires Pillow, numpy, scipy and scikit-image. Run after sketch_paths.py.
"""
from pathlib import Path
from PIL import Image
import numpy as np,json,math,random
from scipy.ndimage import gaussian_filter
from skimage.measure import find_contours
root=Path(__file__).resolve().parents[1];allpages=[]
for n in range(3):
 rng=random.Random(340+n)
 image=Image.open(root/f'Assets/Study/ArtV3/charcoal{n}.png').convert('L').resize((584,778))
 lum=np.asarray(image,dtype=float);ink=np.clip((244-lum-7)/209,0,1);groups=[[],[],[],[]]
 def add(points,radius,stage):
  if len(points)<2:return
  points=np.asarray(points);length=np.r_[0,np.cumsum(np.linalg.norm(np.diff(points,axis=0),axis=1))]
  if length[-1]<9:return
  q=np.linspace(0,length[-1],max(2,int(length[-1]/2)+1));pts=np.c_[np.interp(q,length,points[:,0]),np.interp(q,length,points[:,1])]
  pressure=[round(.87+.11*math.sin(i*.21+rng.random()*.5),3) for i in range(len(pts))]
  groups[stage].append({'xy':np.round((pts+[92,130])/[768,1024],6).flatten().tolist(),'pressure':pressure,'width':radius,'ink':1,'seed':rng.randrange(99999)})
 smooth=gaussian_filter(ink,1.2)
 # Coherent contour marks first, then successively wider tonal hatching.
 for level,stage in [(.13,0),(.36,1),(.61,1)]:
  contours=find_contours(smooth,level);contours.sort(key=lambda p:len(p),reverse=True)
  for c in contours:
   if len(c)<(32 if stage==0 else 16):continue
   pts=c[:,::-1]
   for i in range(0,len(pts)-1,100):add(pts[i:i+102],3.4 if stage==0 else 4.2,stage)
 # Alternating diagonal strokes follow the actual shaded regions.
 for stage,slope,gap in [(2,-.72,12),(3,.64,14)]:
  lines=list(range(-420,1240,gap));rng.shuffle(lines)
  for intercept in lines:
   run=[]
   for x in range(0,584,2):
    y=round(intercept+slope*x)
    active=0<=y<778 and ink[y,x]>.018
    if active:run.append((x,y))
    elif run:
     for j in range(0,len(run)-1,35):add(run[j:j+37],9 if stage==2 else 11,stage)
     run=[]
   if run:
    for j in range(0,len(run)-1,35):add(run[j:j+37],9 if stage==2 else 11,stage)
 # Draw by connected areas in the outline stage, avoiding random teleports.
 for stage in [0,1]:groups[stage].sort(key=lambda s:(int(s['xy'][1]*7),s['xy'][0]))
 strokes=sum(groups,[]);lengths=[]
 for group in groups:
  total=0
  for s in group:
   xy=np.asarray(s['xy']).reshape(-1,2)*[768,1024];total+=max(18,np.linalg.norm(np.diff(xy,axis=0),axis=1).sum())+9
  lengths.append(total)
 allpages.append({'strokes':strokes,'stageEnds':list(np.cumsum(lengths)/sum(lengths))})
 print(n,len(strokes),'traced strokes',list(np.round(np.cumsum(lengths)/sum(lengths),2)))
(root/'Assets/Study/Resources/strokeBook.json').write_text(json.dumps({'pages':allpages},separators=(',',':')))
