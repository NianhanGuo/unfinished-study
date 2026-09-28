from PIL import Image, ImageDraw, ImageFont
from pathlib import Path
import numpy as np, wave
out=Path(__file__).resolve().parents[1]/'Assets/Study/Resources'
rng=np.random.default_rng(51);w,h=1200,1400
noise=rng.normal(0,1.2,(h,w,1));paper=np.clip(np.array([218,210,186])+noise,0,255).astype('uint8');im=Image.fromarray(paper);d=ImageDraw.Draw(im)
def font(size):return ImageFont.truetype('/System/Library/Fonts/Supplemental/Bradley Hand Bold.ttf',size)
def text(x,y,s,size=43,fill=(45,42,37)):d.text((x,y),s,font=font(size),fill=fill,spacing=13)
text(92,170,'TO FINISH:',70)
for y,caption in [(370,'Suspended skeleton'),(515,'Reaching skeleton'),(660,'Plaster & pears')]:
 d.rectangle((95,y+18,131,y+54),outline=(71,61,51),width=3)
 text(163,y,caption,55)
text(92,1000,"They move when you're",58,(106,46,40))
text(92,1080,'not paying attention.',58,(106,46,40))
# Subtle old fold, including a narrow highlight beside its shadow.
d.line((598,24,605,1380),fill=(197,188,166),width=2);d.line((604,24,610,1380),fill=(226,219,196),width=2)
im.save(out/'HomeworkNote.png')
# A soft two-part heartbeat. Repetition rate changes in-game without pitch-shifting the sample.
sr=44100;t=np.arange(int(sr*.52))/sr;x=np.zeros_like(t)
for at,amp,freq,decay in [(0,.83,64,.074),(.18,.54,78,.058)]:
 q=np.maximum(t-at,0);env=(1-np.exp(-q/.005))*np.exp(-q/decay)*(t>=at)
 x+=amp*env*(np.sin(2*np.pi*(freq*q-17*q*q))+.15*np.sin(2*np.pi*freq*1.8*q))
x*=np.minimum(1,(.52-t)/.04);x=x/max(abs(x))*.68
with wave.open(str(out/'Heartbeat.wav'),'wb') as f:f.setnchannels(1);f.setsampwidth(2);f.setframerate(sr);f.writeframes((x*32767).astype('<i2').tobytes())
print('Note texture and heartbeat written')
