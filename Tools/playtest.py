#!/usr/bin/env python3
"""Actual native player input replay. Private X display; no gameplay state injection."""
import argparse,json,subprocess,time,os,datetime,shutil,hashlib,wave,math,struct,signal
from ui_map import local_point
from pathlib import Path
p=Path(__file__).resolve().parent.parent
opt=argparse.ArgumentParser();opt.add_argument('--display',default=':93');args=opt.parse_args();env={**os.environ,'DISPLAY':args.display};cc=p.name=='CashflowChains'
out=p/'Artifacts/Playtest'/datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ');out.mkdir(parents=True)
actions=[];proc=None;window=None;movie=None

def xd(*v):return subprocess.check_output(['xdotool',*map(str,v)],env=env,text=True,stderr=subprocess.DEVNULL,timeout=10).strip()
def activate():
 try:xd('windowactivate','--sync',window)
 except (subprocess.CalledProcessError,subprocess.TimeoutExpired):xd('windowfocus',window)
 xd('windowraise',window)
def state():return json.loads((out/'campaign-state.json').read_text())
def wait_state(name=None):
 for i in range(100):
  try:
   s=state()
   if name is None or s['Screen']==name:return s
  except (FileNotFoundError,json.JSONDecodeError):pass
  time.sleep(.15)
 raise AssertionError('State not reached: '+str(name))
def click(x,y):activate();xd('mousemove','--window',window,*local_point(out,x,y),'mousedown','1');time.sleep(.08);xd('mouseup','1');actions.append({'click':[x,y]});time.sleep(.25)
def key(name):activate();xd('keydown',name);time.sleep(.08);xd('keyup',name);actions.append({'key':name});time.sleep(.25)
def drag(x,y,tx,ty):
 activate();ax,ay=local_point(out,x,y);bx,by=local_point(out,tx,ty)
 xd('mousemove','--window',window,ax,ay,'mousedown','1');time.sleep(.15)
 for i in range(1,9):xd('mousemove','--window',window,round(ax+(bx-ax)*i/8),round(ay+(by-ay)*i/8));time.sleep(.025)
 xd('mouseup','1');actions.append({'drag':[x,y,tx,ty]});time.sleep(.35)
def cap(name):
 before=time.time();key('F12');time.sleep(.5);prefix='cashflow' if cc else 'blackbag';files=sorted(out.glob(prefix+'-*.png'),key=lambda f:f.stat().st_mtime);assert files and files[-1].stat().st_mtime>=before,'Fresh capture missing';shutil.copyfile(files[-1],out/(name+'.png'));(out/(name+'.json')).write_text(json.dumps(state(),indent=2))
def desk():
 for _ in range(5):
  if state()['Screen']=='menu':return
  key('Escape')
 raise AssertionError('Could not return to desk')
def start():
 global proc,window
 before=time.time();proc=subprocess.Popen([str(p/'Builds/Linux'/(p.name+'.x86_64')),'-screen-fullscreen','0','-screen-width','540','-screen-height','960','-force-glcore','-save-dir',str(out/'Saves'),'-capture-dir',str(out),'-logFile',str(out/('player-'+str(len(actions))+'.log'))],env=env,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
 for i in range(150):
  if proc.poll() is not None:raise RuntimeError('Player exited')
  try:
   window=xd('search','--onlyvisible','--pid',proc.pid).splitlines()[-1];xd('windowraise',window);xd('windowfocus',window)
   if (out/'ui-layout.json').exists() and (out/'ui-layout.json').stat().st_mtime>=before and json.loads((out/'ui-layout.json').read_text()).get('Screen')=='title':
    if args.display==':1':xd('windowmove',window,2100,45)
    activate();time.sleep(1);return
  except subprocess.CalledProcessError:pass
  time.sleep(.2)
 raise RuntimeError('Player timeout')
def begin_movie():
 global movie
 movie=subprocess.Popen(['ffmpeg','-hide_banner','-loglevel','error','-y','-f','x11grab','-window_id',window,'-framerate','30','-video_size','540x960','-i',args.display,'-t','25','-an','-c:v','libx264','-threads','2','-preset','ultrafast','-crf','21','-pix_fmt','yuv420p',str(out/'actual-first-session.mp4')],stdout=subprocess.DEVNULL,stderr=open(out/'movie-log.txt','w'))
def stop():
 if proc is not None and proc.poll() is None:
  proc.terminate()
  try:proc.wait(timeout=12)
  except subprocess.TimeoutExpired:proc.kill();proc.wait(timeout=5)
try:
 start();activate();time.sleep(1);begin_movie();# title probe written on Save/Board, title initially no Probe, fixed in production
 cap('01-title');click(360,1060);wait_state('board');cap('02-first-case')
 if cc:
  wallet=state()['Save']['Profile']['Credits'];click(630,1030);cap('25-building-tray');click(620,660);click(118,390);assert len(state()['Save']['Board']['Nodes'])==3;assert state()['Save']['Profile']['Credits']==wallet;key('z');assert len(state()['Save']['Board']['Nodes'])==2
  drag(602,712,441,712);s=state();assert s['Save']['Board']['Nodes'][1]['X']==2,s
  key('r');key('z');assert state()['Save']['Board']['Nodes'][1]['Rotation']==0
  click(375,1198);cap('03-forecast');click(360,1070)
  key('space');time.sleep(4.5);wait_state('result');cap('04-three-stars');assert state()['Save']['Profile']['Stars'][0]==3
  click(360,922);wait_state('board')
 else:
  # First shelf: cash roll. Exact domino preview -> Take. Then pocket watch.
  drag(155,1028,210,368);assert len(state()['Save']['Run']['Items'])==0
  click(322,1205);assert len(state()['Save']['Run']['Items'])==1
  click(428,1028);click(440,368);key('space');assert len(state()['Save']['Run']['Items'])==2
  cap('03-packed-case');risk=state()['Save']['Run']['Alert'];drag(220,368,210,520);assert state()['Save']['Run']['Alert']==risk
  key('e');cap('04-extraction-choice');click(360,955);wait_state('result');cap('05-result');assert state()['Save']['Profile']['Collection']
  click(535,1210);wait_state('map');cap('06-map');key('Escape')
 # exercise actual menus and settings
 key('Escape');time.sleep(.3);cap('07-desk')

 if cc:
  click(360,608);wait_state('library');cap('13-almanac');click(620,1140);cap('14-almanac-page2');click(620,1140);cap('26-almanac-page3');click(620,1140);cap('27-almanac-page4');desk()
  click(360,710);wait_state('blueprints');cap('15-blueprints');click(120,416);assert state()['Save']['Blueprints'][0]['Challenge']==1;click(270,416);wait_state('board');desk()
  click(360,812);wait_state('contracts');cap('16-contracts');wallet=state()['Save']['Profile']['Credits'];click(600,320);wait_state('board');assert state()['Save']['Profile']['Credits']==wallet;desk()
  click(360,404);wait_state('map');click(140,602);wait_state('board');drag(602,712,441,712);key('space');key('space');time.sleep(1);wait_state('result');desk()
  click(360,812);wait_state('contracts');click(600,320);assert len(state()['Save']['Contracts'])==1;route_started=time.monotonic();cap('17-contract-active');click(360,1195);wait_state('achievements');cap('18-achievements');desk()
  click(360,914);wait_state('themes');cap('19-palettes');desk()
 else:
  click(360,608);wait_state('gallery');cap('13-gallery');click(530,1120);cap('14-gallery-watch');desk()
  click(360,710);wait_state('catalogue');cap('15-catalogue');click(620,1140);cap('16-catalogue-page2');
  for page in range(3,11):click(620,1140);cap(str(24+page)+'-catalogue-page'+str(page))
  desk()
  click(360,812);wait_state('bags');cap('17-five-bags');click(360,1100);wait_state('tools');cap('18-tools');desk()
  click(360,914);wait_state('hideout');cap('19-hideout');desk()
 # daily opens a certified advanced puzzle through actual UI, never by save injection
 click(360,506);cap('20-daily-offer');click(360,1068);wait_state('board');cap('21-daily-board');desk()
 click(360,1118);cap('22-free-edition-about');before=state()['Save']['Profile']['Credits'];layout=json.loads((out/'ui-layout.json').read_text());assert not any(any(word in e['Name'].upper() for word in ['PURCHASE','RESTORE','UNAVAILABLE']) for e in layout['Elements'] if e['Priority']>1000);key('Escape');assert state()['Save']['Profile']['Credits']==before;desk()
 click(360,1016);wait_state('settings');cap('08-settings');click(360,320);click(360,658);click(360,770);s=state()['Save']['Profile'];assert not s['Music'] and s['ReducedMotion'] and s['HighContrast'];key('Escape')
 # portrait / short / landscape fit; allow one draw after resize
 for w,h,label in [(540,960,'laptop'),(720,1600,'tall'),(720,1024,'short'),(1080,720,'landscape')]:
  xd('windowsize',window,w,h);time.sleep(.7);cap('09-'+label)
 xd('windowsize',window,540,960);time.sleep(.5)
 saved=state()['Save']['Profile'];stop();shutil.copyfile(out/'actual-mix.wav',out/'actual-mix-first.wav');shutil.copyfile(out/'text-overflows.json',out/'text-overflows-first.json');shutil.copyfile(out/'performance.json',out/'performance-first.json');start();click(360,1060);time.sleep(.5);assert state()['Save']['Profile']['Stars']==saved['Stars'];assert state()['Save']['Profile']['Collection']==saved['Collection'];cap('10-cold-resume')
 if cc:
  while time.monotonic()-route_started<62:time.sleep(.25)
  desk();click(360,812);wait_state('contracts');click(600,320);assert state()['Save']['Contracts'][0]['Claimed'];assert state()['Save']['Profile']['Credits']==700;cap('24-contract-returned');desk();key('Escape')
 # explicit in-product reset only affects this isolated review save directory
 desk();click(360,1016);wait_state('settings');click(360,1125);click(360,938);cap('11-reset-confirmation');click(360,1070);wait_state('title');assert not any(state()['Save']['Profile']['Stars']);assert list((out/'Saves').glob('*.archived-*'));cap('12-fresh-after-archive');time.sleep(27);stop()
 issues=json.loads((out/'text-overflows-first.json').read_text())['Issues']+json.loads((out/'text-overflows.json').read_text())['Issues'];assert not issues,issues
 logs='\n'.join(f.read_text(errors='replace') for f in out.glob('player-*.log'));errors=[line for line in logs.splitlines() if any(k in line for k in ['NullReferenceException','ArgumentException','IndexOutOfRangeException','MissingReferenceException','error CS'])];assert not errors,errors
 mix=out/'actual-mix-first.wav';audio={}
 if mix.exists():
  with wave.open(str(mix)) as f:
   samples=struct.unpack('<'+'h'*(f.getnframes()*f.getnchannels()),f.readframes(f.getnframes()));audio={'samples':len(samples),'peak':max(abs(s) for s in samples)/32768,'rms':math.sqrt(sum(s*s for s in samples)/len(samples))/32768}
  assert audio['rms']>.001,'Silent actual output';assert audio['peak']<.999,'Clipped output'
 else:raise AssertionError('Actual Unity mix capture missing')
 report={'passed':True,'game':p.name,'actions':actions,'audio':audio,'source':{str(f.relative_to(p)):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Assets/_Game').rglob('*.cs')},'build':{str(f.relative_to(p/'Builds/Linux')):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Builds/Linux').rglob('*') if f.is_file()},'screenshots':[f.name for f in out.glob('[0-9][0-9]-*.png')]}
 (out/'report.json').write_text(json.dumps(report,indent=2));(p/'Artifacts/latest-playtest.txt').write_text(str(out));print(out)
finally:stop()
