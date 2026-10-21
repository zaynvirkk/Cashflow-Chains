from pathlib import Path
import shutil,numpy as np,wave
p=Path(__file__).resolve().parents[1];out=p/'Assets/_Game/Resources/Audio/V4';out.mkdir(exist_ok=True);sr=32000;rng=np.random.default_rng(456 if p.name=='CashflowChains' else 789)
for f in (p/'Assets/_Game/Resources/Audio/V3').glob('*.wav'):shutil.copyfile(f,out/f.name)
for name,dur,amp in [('press',.065,.32),('tap',.085,.42),('select',.10,.36),('lift',.16,.36),('snap',.21,.62),('place',.28,.72),('take',.34,.72),('rotate',.18,.48)]:
 t=np.arange(int(sr*dur))/sr;noise=rng.normal(0,1,len(t));soft=np.convolve(noise,np.ones(13)/13,mode='same');attack=np.minimum(1,t*1300)
 f0=135 if p.name=='CashflowChains' else 105
 wood=np.sin(2*np.pi*(f0*t-38*t*t))*np.exp(-t*55)+.3*np.sin(2*np.pi*255*t)*np.exp(-t*100)
 cloth=soft*np.exp(-t*34)*.8
 metal=sum(np.sin(2*np.pi*f*t)*np.exp(-t*decay)*a for f,decay,a in [(1450,40,.09),(2307,55,.065),(3901,80,.04)])
 a=(wood*.65+cloth+metal)*attack
 if name=='lift':a=soft*(1-np.exp(-t*130))*np.exp(-t*24)+.08*np.sin(2*np.pi*(420*t+1000*t*t))*np.exp(-t*24)
 if name in ['snap','take','place']:
  at=.045 if name=='take' else .022;u=np.maximum(0,t-at);a+=(np.sin(2*np.pi*185*u)*np.exp(-u*38)*.28+np.sin(2*np.pi*1237*u)*np.exp(-u*30)*.10)*(t>=at)*np.minimum(1,u*1500)
 a*=amp/max(np.max(np.abs(a)),1e-9);fade=min(75,len(a)//10);a[-fade:]*=np.linspace(1,0,fade)
 stereo=np.stack((a,a*.94+np.roll(a,9)*.06),axis=1)
 with wave.open(str(out/(name+'.wav')),'wb') as f:f.setnchannels(2);f.setsampwidth(2);f.setframerate(sr);f.writeframes((stereo*32767).astype('<i2').tobytes())
print(p.name,'tactile audio',len(list(out.glob('*.wav'))))
