"""Authored charcoal studies with adult proportions and pressure-bearing stroke paths.
The same paths drive live ink and the pencil hand in Unity. No bitmap fade reveal.
"""
from pathlib import Path
from PIL import Image, ImageDraw, ImageFont, ImageFilter
import numpy as np
import random, math, json
out=Path(__file__).resolve().parents[1]/'Assets/Study/Resources'
W,H=768,1024
allpages=[]

def smooth(points):
 p=[np.array(v,dtype=float) for v in points];ext=[p[0]]+p+[p[-1]];result=[]
 for i in range(1,len(ext)-2):
  a,b,c,d=ext[i-1:i+3]
  for t in np.linspace(0,1,max(4,int(np.linalg.norm(c-b)/3)),endpoint=False):
   result.append(.5*((2*b)+(-a+c)*t+(2*a-5*b+4*c-d)*t*t+(-a+3*b-3*c+d)*t*t*t))
 result.append(p[-1]);return result

def resample(points,spacing=1.6):
 pts=np.asarray(points);length=np.r_[0,np.cumsum(np.linalg.norm(np.diff(pts,axis=0),axis=1))]
 if length[-1]<.1:return [pts[0],pts[-1]]
 q=np.linspace(0,length[-1],max(2,int(length[-1]/spacing)+1));return np.c_[np.interp(q,length,pts[:,0]),np.interp(q,length,pts[:,1])]

for subject in range(3):
 rng=random.Random(811+subject*193);groups=[[],[],[],[]]
 def mark(points,w=.95,ink=.65,stage=2,curve=True,echo=True):
  if len(points)<2:return
  pts=resample(smooth(points) if curve else points);n=len(pts);phase=rng.random()*6.28
  # Uneven finger pressure and slight searching motion, coherent along each mark.
  pres=[];path=[]
  for i,(x,y) in enumerate(pts):
   t=i/max(1,n-1);off=math.sin(t*9+phase)*.48+math.sin(t*31+phase*2)*.16
   path.extend([round((x+off)/W,6),round((y+math.sin(t*13+phase)*.38)/H,6)])
   taper=min(1,.28+6*t,.26+6*(1-t));pres.append(round(taper*(.78+.18*math.sin(t*10+phase)+.1*math.sin(t*27+phase)),4))
  groups[stage].append({'xy':path,'pressure':pres,'width':w,'ink':ink,'seed':rng.randrange(99999)})
  if echo and stage==2 and len(pts)>15 and rng.random()<.75:
   lo=rng.uniform(0,.3);hi=rng.uniform(.58,1);seg=pts[int(n*lo):max(int(n*lo)+2,int(n*hi))];dx=rng.uniform(-2.2,2.2);dy=rng.uniform(-1.7,1.7)
   mark([(x+dx,y+dy) for x,y in seg],w*.58,ink*.36,stage,False,False)
 def arc(cx,cy,rx,ry,start=0,end=6.28,**kw):
  mark([(cx+rx*math.cos(t),cy+ry*math.sin(t)) for t in np.linspace(start,end,19)],**kw)
 def hatch(cx,cy,rx,ry,n,angle=-.8,stage=3,ink=.4):
  for _ in range(n):
   a=rng.random()*6.28;r=math.sqrt(rng.random());x=cx+math.cos(a)*rx*r;y=cy+math.sin(a)*ry*r
   length=rng.uniform(7,23);an=angle+rng.uniform(-.17,.17)
   mark([(x,y),(x+length*.5*math.cos(an),y+length*.5*math.sin(an)+.5),(x+length*math.cos(an),y+length*math.sin(an))],rng.uniform(.42,.83),ink*rng.uniform(.55,1.2),stage,True,False)
 def tone(points,width=7,ink=.08):mark(points,width,ink,1,True,False)
 def bone(a,b,width,side=1):
  a=np.array(a,dtype=float);b=np.array(b,dtype=float);v=b-a;u=np.array([-v[1],v[0]])/np.linalg.norm(v)
  # Flared ends and a gently bowed shaft replace uniform rods and round joints.
  for sign in [-1,1]:
   points=[]
   for t,f in [(0,.9),(.055,1.2),(.17,.64),(.52,.44),(.82,.62),(.95,1.1),(1,.9)]:
    points.append(a+v*t+u*(sign*width*f+math.sin(t*math.pi)*width*.18))
   mark(points,.92 if sign==side else .58,.75 if sign==side else .38)
  mark([a-u*width,a-v*.015,a+u*width],.7,.55)
  mark([b-u*width,b+v*.008,b+u*width],.8,.58)
  for j in range(2):tone([a+v*.12+u*(width*.3+j),a+v*.55+u*(width*.28+j),b-v*.08+u*(width*.45+j)],2.1,.1)
 if subject<2:
  # Anatomical landmarks: 7.5 skull-heights, narrow ribcage, long lower limbs.
  tilt=-.035 if subject==0 else .035
  mark([(360,158),(371,280),(373,405),(387,512),(383,696),(372,900)],.65,.17,0)
  mark([(289,291),(370,278),(454,294)],.65,.2,0)
  mark([(313,462),(380,480),(454,468)],.6,.16,0)
  arc(367,198,34,49,w=.65,ink=.15,stage=0,echo=False)
  arc(375,354,73,90,start=3.4,end=8.7,w=.7,ink=.14,stage=0,echo=False)
  mark([(340,511),(334,697),(343,888)],.6,.16,0);mark([(421,514),(420,697),(424,889)],.6,.16,0)
  # Skull in a slight three-quarter view: discontinuous light edges, dark cavities.
  mark([(335,200),(334,181),(344,159),(364,151),(386,157),(400,175),(402,193),(396,210)],1.0,.7)
  mark([(336,202),(339,216),(345,220),(345,234),(355,247),(378,248),(392,233),(395,215)],.9,.67)
  mark([(337,215),(350,219),(352,229)],.7,.6)
  mark([(394,211),(383,217),(381,229)],1.0,.8)
  mark([(346,231),(357,240),(377,241),(387,231)],.8,.62)
  mark([(340,195),(347,189),(359,190),(363,199),(357,209),(346,208),(340,195)],1.1,.8)
  mark([(373,196),(381,190),(393,193),(394,202),(386,211),(377,208),(373,196)],1.0,.85)
  tone([(345,199),(352,200),(357,199)],4.2,.45);tone([(378,201),(385,199),(390,199)],4,.5)
  hatch(350,200,6,5,12,ink=.7);hatch(385,201,6,6,14,ink=.7)
  mark([(368,202),(363,218),(372,222),(375,213)],.8,.8)
  hatch(369,216,3,4,7,ink=.75)
  mark([(351,226),(368,230),(383,226)],.9,.7)
  for i in range(7):mark([(354+i*4,227),(354+i*4,233+rng.uniform(-1,1))],.48,.55,3,False,False)
  tone([(397,174),(395,188),(398,198)],5,.15);hatch(390,173,5,10,18,ink=.32)
  mark([(349,167),(359,162),(372,163)],.48,.25)
  # Cervical spine and shoulder girdle.
  for i in range(7):
   y=251+i*4.7;mark([(366,y),(373,y+1),(372,y+4),(365,y+3)],.6,.5,echo=False)
  mark([(372,284),(343,278),(316,284),(291,291)],.9,.68)
  mark([(375,284),(403,279),(430,282),(453,291)],.8,.62)
  mark([(321,299),(312,315),(319,348)],.63,.4);mark([(430,297),(437,319),(428,350)],.7,.46)
  # Egg-shaped thorax; ribs curve down around volume and taper toward floating ribs.
  for r in range(12):
   y=292+r*10.2;span=31+math.sin((r+1)/13*math.pi)*46
   for side in [-1,1]:
    sx=span*(1 if side==-1 else .89);end=373+side*(7 if r<8 else 13+(r-8)*8)
    pts=[(373+side*5,y),(373+side*sx*.61,y-3),(373+side*sx,y+7),(373+side*sx*.88,y+20),(end,y+22)]
    mark(pts,.7,.60 if side==1 else .44)
    if r<10:mark([(373+side*sx*.88,y+22),(373+side*sx*.48,y+30),(end,y+25)],.43,.28,echo=False)
  mark([(371,292),(368,310),(373,349),(375,374)],.9,.72)
  mark([(380,294),(381,319),(382,350),(377,376)],.6,.42)
  for i in range(17):
   y=300+i*9.2;x=370+math.sin(i*.25)*4
   mark([(x-5,y),(x+5,y+1),(x+6,y+6),(x-4,y+7)],.52,.38,echo=False)
  tone([(421,319),(433,354),(420,400)],9,.06);hatch(410,358,25,47,88,ink=.3)
  # Pelvic plates flare into thin crests with visible obturator openings.
  for side in [-1,1]:
   x=382;mark([(x+side*8,472),(x+side*37,457),(x+side*63,464),(x+side*65,483),(x+side*49,501),(x+side*39,518),(x+side*22,532),(x+side*4,516)],.85,.65)
   mark([(x+side*10,477),(x+side*34,470),(x+side*56,471)],.6,.36)
   mark([(x+side*38,504),(x+side*19,496),(x+side*10,508),(x+side*22,521),(x+side*38,514),(x+side*38,504)],.85,.67)
   hatch(x+side*47,483,12,16,24,angle=-1.1,ink=.32)
  mark([(374,463),(389,465),(396,491),(384,510),(373,484),(374,463)],.75,.5)
  # Femora angle inward from the hips; tibia and fibula remain distinct.
  for side in [-1,1]:
   hip=(382+side*48,528);knee=(382+side*35,703);ankle=(382+side*41,881)
   bone(hip,knee,7.5,side)
   mark([(knee[0]-7,697),(knee[0],692),(knee[0]+7,700),(knee[0]+4,712),(knee[0]-4,713),(knee[0]-7,697)],.8,.62)
   bone((knee[0]-side*2,718),(ankle[0]-side*2,880),5,side)
   mark([(knee[0]+side*12,719),(knee[0]+side*14,768),(ankle[0]+side*12,881)],.6,.58)
   mark([(ankle[0]-10,883),(ankle[0]-14,898),(ankle[0]-side*22,912),(ankle[0]+side*27,914),(ankle[0]+13,892),(ankle[0]+9,883)],.85,.65)
   for f in range(5):mark([(ankle[0]+(f-2)*4,890),(ankle[0]+(f-2)*7+side*6,908),(ankle[0]+(f-2)*7+side*7,913)],.5,.53)
   hatch(knee[0]+side*7,696,5,9,13,ink=.3)
  # Arms / fingers have separate joints, no circular 'stick person' symbols.
  for side in [-1,1]:
   shoulder=(373+side*88,296);elbow=(373+side*108,424)
   wrist=(373+side*124,540 if subject==0 else 506)
   bone(shoulder,elbow,6,side)
   bone((elbow[0]-6,elbow[1]+5),(wrist[0]-5,wrist[1]),2.8,side)
   bone((elbow[0]+6,elbow[1]+5),(wrist[0]+5,wrist[1]),2.5,side)
   for f in range(4):
    a=(wrist[0]+(f-1.5)*5,wrist[1]+6);b=(a[0]+(f-1.5)*2,a[1]+22);c=(b[0]+(f-1.5)*2,b[1]+15);d=(c[0]+(f-1.5),c[1]+10-abs(f-1)*2)
    mark([a,b,c,d],.65,.57)
    mark([(b[0]+2,b[1]-1),(b[0]+2,c[1]-2)],.42,.3,echo=False)
    mark([(c[0]-2,c[1]),(c[0]+2,c[1]+2)],.48,.44,echo=False)
   mark([(wrist[0]-side*8,wrist[1]+9),(wrist[0]-side*21,wrist[1]+24),(wrist[0]-side*26,wrist[1]+38)],.74,.63)
   hatch(shoulder[0]+side*4,shoulder[1]+8,6,8,10,ink=.32)
  # Light stand, cast shadow and a few working measurements are subordinate.
  mark([(465,151),(465,907),(437,932),(510,932),(465,907)],.55,.24,1,False,False)
  hatch(392,927,112,5,50,angle=-.18,ink=.18)
  for y in [152,250,293,470,705,913]:mark([(237,y),(249+rng.uniform(-2,3),y-1)],.4,.14,0,False,False)
 else:
  # Traditional still life: asymmetrical plaster planes and curved pear contours.
  mark([(130,704),(293,642),(641,710),(489,799),(130,704)],.65,.2,0)
  arc(310,290,59,87,w=.6,ink=.16,stage=0,echo=False)
  mark([(311,198),(316,367),(305,557)],.55,.16,0)
  mark([(253,283),(252,246),(270,212),(302,197),(334,203),(358,224),(371,259),(368,294)],.9,.66)
  mark([(254,285),(260,323),(277,347),(302,365),(328,364),(351,344),(363,318),(369,288)],.9,.59)
  mark([(259,267),(277,257),(294,262)],.9,.7);mark([(325,262),(346,260),(358,270)],.95,.72)
  mark([(265,279),(279,276),(292,282)],.65,.54);mark([(328,281),(341,277),(354,283)],.7,.63)
  mark([(308,266),(303,304),(300,319),(312,325),(326,319)],.8,.7)
  mark([(298,322),(300,326),(305,326)],.8,.68)
  mark([(286,341),(301,336),(312,338),(326,337),(337,342)],.7,.62)
  mark([(294,346),(311,350),(326,345)],.6,.42)
  mark([(281,354),(280,397),(233,428),(212,484),(224,523),(377,542),(407,513),(396,465),(345,410),(344,356)],.9,.66)
  mark([(279,408),(307,452),(343,414)],.65,.39)
  mark([(225,527),(222,570),(381,587),(396,543)],.85,.53)
  mark([(223,571),(310,580),(381,587),(393,556)],.55,.32)
  tone([(354,236),(357,276),(348,302),(345,326)],11,.12)
  tone([(268,302),(277,332),(291,347)],8,.095)
  tone([(344,417),(370,459),(370,511)],14,.08)
  hatch(351,278,10,43,100,ink=.38);hatch(275,324,10,19,42,ink=.3);hatch(354,470,21,31,85,ink=.25)
  for cx,cy in [(470,643),(570,673)]:
   mark([(cx+1,cy-100),(cx-18,cy-92),(cx-23,cy-57),(cx-42,cy-25),(cx-45,cy+2),(cx-33,cy+28),(cx-2,cy+35),(cx+28,cy+26),(cx+39,cy+4),(cx+34,cy-23),(cx+19,cy-56),(cx+17,cy-91),(cx+1,cy-100)],.85,.61)
   mark([(cx+2,cy-98),(cx+7,cy-113),(cx+15,cy-119)],1,.69)
   tone([(cx+20,cy-34),(cx+31,cy),(cx+16,cy+24)],12,.09)
   hatch(cx+18,cy+2,14,23,65,angle=-1.1,ink=.32)
   hatch(cx+15,cy+40,29,4,18,angle=-.12,ink=.23)
  for i in range(8):
   x=290+i*38
   mark([(x,743),(x-12,789),(x+11,857-i*3),(x-3,906-i*6)],.75,.44)
   mark([(x+7,746),(x-4,792),(x+17,855-i*3)],.48,.25)
   tone([(x-8,787),(x+4,831),(x+7,866-i*3)],6,.07)
  mark([(279,741),(264,861),(285,900),(331,918),(397,899),(452,908),(574,861),(592,766)],.8,.52)
  hatch(366,835,72,46,85,angle=-1.35,ink=.18)
  hatch(285,609,78,9,30,angle=-.2,ink=.2)
 strokes=sum(groups,[])
 # Paper tooth: restrained warm grey, with imperfect deckle-free edges.
 nr=np.random.default_rng(44+subject);noise=nr.normal(0,1.65,(H,W));yy,xx=np.mgrid[:H,:W];edge=2.8*((abs(xx-W/2)/(W/2))**6+(abs(yy-H/2)/(H/2))**6)
 base=np.stack([235+noise-edge,230+noise-edge,213+noise-edge],axis=2).clip(0,255).astype(np.uint8)
 im=Image.fromarray(base);d=ImageDraw.Draw(im);font=str(out/'Georgia.ttf');f=ImageFont.truetype(font,21);sm=ImageFont.truetype(font,12)
 d.text((52,47),['01 / Suspended figure','02 / Reaching figure','03 / Plaster, pears & linen'][subject],fill=(89,85,74),font=f)
 d.line((52,85,717,85),fill=(185,178,155),width=1)
 d.text((52,972),'NIGHT STUDIO    /    OBSERVATIONAL STUDIES',fill=(137,129,111),font=sm)
 im.save(out/f'page{subject}.png')
 # Gallery uses the same pressure-bearing paths, rendered with paper tooth.
 ink=np.zeros((H,W),dtype=np.float32)
 for st in strokes:
  pts=np.asarray(st['xy']).reshape(-1,2)*[W,H];pressure=st['pressure'];mask=Image.new('L',(W,H));md=ImageDraw.Draw(mask)
  for i in range(1,len(pts)):
   p=(pressure[i]+pressure[i-1])*.5;md.line([tuple(pts[i-1]),tuple(pts[i])],fill=int(255*min(.92,st['ink']*1.65*p)),width=max(1,round(st['width']*2*p)))
  layer=np.asarray(mask,dtype=float)/255;ink=1-(1-ink)*(1-layer)
 tooth=nr.uniform(.58,1.0,(H,W));ink=(ink*tooth).clip(0,.94)
 image=np.asarray(im,dtype=float);pigment=np.array([37,35,31]);image=image*(1-ink[:,:,None])+pigment*ink[:,:,None]
 Image.fromarray(image.clip(0,255).astype(np.uint8)).save(out/f'finished{subject}.png')
 lengths=[sum(max(18,sum(math.hypot((st['xy'][j]-st['xy'][j-2])*W,(st['xy'][j+1]-st['xy'][j-1])*H) for j in range(2,len(st['xy']),2)))+9 for st in group) for group in groups]
 ends=list(np.cumsum(lengths)/sum(lengths))
 allpages.append({'strokes':strokes,'stageEnds':ends});print(subject,len(strokes),'strokes')
(out/'strokeBook.json').write_text(json.dumps({'pages':allpages},separators=(',',':')))
