from pathlib import Path
import numpy as np,wave,json,hashlib
root=Path(__file__).resolve().parents[1];cc=root.name=='CashflowChains';out=root/'Assets/_Game/Resources/Audio/V3';out.mkdir(parents=True,exist_ok=True);sr=32000;rng=np.random.default_rng(612 if cc else 913)
def write(name,a):
 a=np.nan_to_num(a);peak=np.max(np.abs(a));a*=min(1,.72/max(peak,1e-9));fade=min(100,len(a)//10);a[:fade]*=np.linspace(0,1,fade)[:,None];a[-fade:]*=np.linspace(1,0,fade)[:,None]
 with wave.open(str(out/(name+'.wav')),'wb') as f:f.setnchannels(2);f.setsampwidth(2);f.setframerate(sr);f.writeframes((a*32767).astype('<i2').tobytes())
def note(a,at,midi,dur,amp,voice='key',pan=0):
 start=int(at*sr);n=min(int(dur*sr),len(a)-start)
 if n<=0:return
 t=np.arange(n)/sr;freq=440*2**((midi-69)/12);end=np.minimum(1,(dur-t)*18)
 if voice=='pad':env=(1-np.exp(-t*3))*np.exp(-t*.7)*end;s=sum(np.sin(2*np.pi*freq*k*t+(.002 if k==2 else 0)*np.sin(t*3))*(1/k**1.7) for k in range(1,5))
 elif voice=='bass':env=(1-np.exp(-t*95))*np.exp(-t*4)*end;s=np.sin(2*np.pi*freq*t)+.17*np.sin(2*np.pi*freq*2*t)
 else:env=(1-np.exp(-t*130))*np.exp(-t*(4.2 if cc else 2.7))*end;s=np.sin(2*np.pi*freq*t)+.32*np.sin(2*np.pi*freq*2.003*t)*np.exp(-t*4)+.10*np.sin(2*np.pi*freq*3.99*t)*np.exp(-t*10)
 sound=s*env*amp;a[start:start+n,0]+=sound*np.sqrt((1-pan)/2);a[start:start+n,1]+=sound*np.sqrt((1+pan)/2)
for track in range(6 if cc else 4):
 beat=60/106 if cc else 60/96;duration=beat*32;a=np.zeros((int(sr*duration),2));chords=([[48,55,59,62,64],[45,52,55,59,62],[41,48,52,57,60],[43,50,53,57,62]] if cc else [[45,52,57,60,64],[41,48,53,57,60],[43,50,55,58,62],[40,47,52,55,59]])
 for bar in range(8):
  chord=chords[(bar+track)%4];at=bar*beat*4
  for i,n in enumerate(chord):note(a,at,n+(track%2)*12,beat*3.8,.012,'pad',(i-2)*.18)
  if cc:
   for q in [0,1.5,2.75]:note(a,at+beat*q,chord[0]-12,beat*.8,.13,'bass',-.08)
   for q in range(8):
    start=int((at+q*beat/2)*sr);n=min(int(.045*sr),len(a)-start);t=np.arange(n)/sr;brush=np.diff(rng.normal(0,.007,n+1))*np.exp(-t*100);a[start:start+n]+=brush[:,None]
  if not cc:
   note(a,at,chord[0]-12,beat*.7,.17,'bass',-.1)
   for q in [1,2,3]:
    for n in chord[1:4]:note(a,at+beat*q,n,beat*.4,.042,'key',.2)
  for q in range(4):note(a,at+beat*(q+.16),chord[(q+bar)%5]+24,beat*1.4,.085 if cc else .065,'key',np.sin(q+bar)*.32)
 write('music'+str(track),a)
for k,name in enumerate(['tap','select','place','rotate','deny','take','cash','beat','star','win','loss','open','escape','undo','page','hint','save','bonus','alarm','drop','unlock','result','collect','tool']):
 duration=.12 if name in ['tap','select','beat'] else 1.25 if name in ['win','escape','result','unlock'] else .65 if name in ['alarm','loss'] else .28;a=np.zeros((int(sr*duration),2))
 if name in ['win','escape','result','unlock','collect']:
  for i,n in enumerate([72,76,79,83,84] if cc else [69,72,76,81,88]):note(a,i*.11,n,.65,.22,'key',(i-2)*.15)
 elif name in ['alarm','deny','loss']:
  for i in range(3 if name=='alarm' else 1):note(a,i*.15,45-i*2,.3,.25,'bass')
 else:
  note(a,0,(72 if cc else 69)+[0,2,4,7,9][k%5],duration,.18,'key')
  if name in ['place','take','rotate','drop','open']:
   t=np.arange(len(a))/sr;noise=rng.normal(0,.09,len(a));rustle=np.convolve(noise,np.ones(11)/11,mode='same')*np.exp(-t*50);thud=np.sin(2*np.pi*(105*t+28*t*t))*np.exp(-t*35)*.22;a+=((rustle+thud)*np.minimum(1,t*600))[:,None]
 write(name,a)
print(root.name,'authored',len(list(out.glob('*.wav'))),'original stereo music/cue assets')
