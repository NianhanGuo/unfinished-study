from PIL import Image,ImageDraw,ImageFont
import random,math
from pathlib import Path
out=Path(__file__).resolve().parents[1]/'Resources'
random.seed(5)
for index in range(3):
 im=Image.new('RGBA',(768,1024),(255,255,255,255));d=ImageDraw.Draw(im)
 def line(points,stage=.3,width=2):
  for j in range(3):
   pts=[(x+random.uniform(-2,2),y+random.uniform(-2,2)) for x,y in points]
   g=random.randint(45,140)
   alpha=int(255*max(.02,min(.98,stage+random.uniform(-.04,.04))))
   d.line(pts,fill=(g,g,g,alpha),width=width)
 def ellipse(box,stage=.3):
  x0,y0,x1,y1=box;line([((x0+x1)/2+(x1-x0)/2*math.cos(t*math.pi/40),(y0+y1)/2+(y1-y0)/2*math.sin(t*math.pi/40)) for t in range(81)],stage)
 if index<2:
  shift=30 if index else 0
  ellipse((305,120,430,263),.17);ellipse((323,174,360,207),.59);ellipse((380,174,415,207),.59)
  line([(371,205),(360,226),(380,226),(371,205)],.65)
  line([(320,238),(338,267),(398,268),(421,237)],.37)
  for x in range(341,402,8):line([(x,240),(x,259)],.7,1)
  line([(369,266),(366,510)],.13)
  for y in range(278,509,13):line([(356,y),(377,y+2)],.49,1)
  for j in range(9):
   y=302+j*16;w=90-abs(j-4)*7
   for s in [-1,1]:
    line([(367,y),(367+s*w*.8,y-8),(367+s*w,y+11),(367+s*w*.65,y+30),(369,y+23)],.25+j*.047,2)
  line([(280,307),(369,285),(457,307)],.2)
  line([(328,493),(285,508),(301,563),(342,584),(367,560),(395,584),(436,552),(445,510),(402,491)],.43)
  for s in [-1,1]:
   x=367+s*84;ex=x+s*(43+shift);ey=445-shift
   line([(x,310),(ex,ey),(ex+s*15,585-shift)],.24)
   line([(x+s*10,311),(ex+s*10,ey),(ex+s*25,585-shift)],.56)
   for f in range(5):line([(ex+s*18+(f-2)*6,588-shift),(ex+s*20+(f-2)*11,635-shift)],.68,1)
   hx=367+s*45;kx=367+s*53
   line([(hx,570),(kx,704),(kx+s*12,869)],.3)
   line([(hx+s*12,568),(kx+s*12,704),(kx+s*20,869)],.64)
   ellipse((kx-7,694,kx+14,716),.57)
   line([(kx+s*12,869),(kx+s*41,890),(kx-s*2,893),(kx+s*6,870)],.56)
  line([(470,160),(470,890),(520,932),(460,934),(435,918)],.76)
  for j in range(130):
   x=random.randint(315,420);y=random.randint(300,475);line([(x,y),(x+random.randint(4,17),y-8)],.8,1)
 else:
  line([(95,664),(295,568),(659,657),(461,776),(95,664)],.12)
  line([(285,617),(281,687),(399,744),(590,683),(600,620)],.4)
  ellipse((227,238,379,421),.22)
  line([(267,414),(267,448),(193,477),(178,538),(387,550),(382,475),(337,447),(336,414)],.31)
  line([(279,298),(294,284),(314,292),(319,328),(332,353),(309,360)],.53)
  line([(281,377),(308,381),(330,376)],.6)
  line([(268,288),(250,300)],.54);line([(329,291),(350,302)],.54)
  line([(223,550),(215,584),(350,599),(363,551)],.35)
  for x,y in [(448,544),(535,593)]:
   line([(x,y-74),(x-22,y-62),(x-24,y-37),(x-46,y-10),(x-43,y+22),(x-19,y+39),(x+20,y+33),(x+35,y+8),(x+18,y-32),(x+16,y-64),(x,y-74)],.44)
   line([(x,y-71),(x+9,y-97)],.64)
  for j in range(15):line([(303+j*19,682-j*2),(295+j*18,753-j*2),(300+j*18,852-j*3)],.4+j*.025,1)
  for j in range(190):
   x=random.randint(212,380);y=random.randint(450,548);line([(x,y),(x+8,y-14)],.79,1)
 # faint construction and hatching appears first, finishing lines last.
 for j in range(50):
  x=random.randint(180,590);y=random.randint(840,910);line([(x,y),(x+20,y-4)],.9,1)
 try: font=ImageFont.truetype('/System/Library/Fonts/Supplemental/Georgia Italic.ttf',22)
 except:font=None
 d.text((65,55),['I.  Suspended figure','II.  Reaching figure','III.  Plaster, pears & linen'][index],font=font,fill=(65,65,65,15))
 im.save(out/f'sketch{index}.png')
