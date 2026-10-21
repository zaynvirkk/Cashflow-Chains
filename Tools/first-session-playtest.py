#!/usr/bin/env python3
"""Fresh-player lessons through real native input; snapshots only read production state."""
from pathlib import Path
from ui_map import local_point
import subprocess,os,time,json,datetime,hashlib,shutil
p=Path(__file__).resolve().parent.parent;env={**os.environ,'DISPLAY':':1'}
out=p/'Artifacts/FirstSession'/datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ');out.mkdir(parents=True)
proc=subprocess.Popen([str(p/'Play.sh'),'-screen-fullscreen','0','-screen-width','540','-screen-height','960','-save-dir',str(out/'Saves'),'-capture-dir',str(out),'-logFile',str(out/'player.log')],env=env,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
checks=[];actions=[];movie=None

def xd(*a):return subprocess.check_output(['xdotool',*map(str,a)],env=env,text=True,stderr=subprocess.DEVNULL,timeout=10).strip()
def state():return json.loads((out/'campaign-state.json').read_text())
def wait(screen):
 for _ in range(150):
  try:
   if state()['Screen']==screen:return state()
  except (FileNotFoundError,json.JSONDecodeError):pass
  time.sleep(.1)
 raise AssertionError('Missing '+screen)
def focus():
 if xd('getactivewindow')!=window:xd('windowactivate','--sync',window)
def click(x,y):focus();xd('mousemove','--window',window,*local_point(out,x,y),'mousedown','1');time.sleep(.08);xd('mouseup','1');time.sleep(.2);actions.append(['click',x,y])
def key(k):focus();xd('keydown',k);time.sleep(.08);xd('keyup',k);time.sleep(.16)
def cap(label):
 before=set(out.glob('cashflow-*.png'));key('F12');time.sleep(.3);files=set(out.glob('cashflow-*.png'))-before;assert files;shutil.copyfile(next(iter(files)),out/(label+'.png'))
def drag(x,y,tx,ty,label=None):
 focus();a=local_point(out,x,y);b=local_point(out,tx,ty);xd('mousemove','--window',window,*a,'mousedown','1');time.sleep(.12)
 for i in range(1,11):xd('mousemove','--window',window,round(a[0]+(b[0]-a[0])*i/10),round(a[1]+(b[1]-a[1])*i/10));time.sleep(.028)
 if label:cap(label)
 xd('mousemove','--window',window,*b);time.sleep(.08);xd('mouseup','1');time.sleep(.55);actions.append(['drag',x,y,tx,ty])
def nodes():return state()['Save']['Board']['Nodes']
def node(i):return next(n for n in nodes() if n['Id']==i)
def pos(x,y):return 38+(x+.5)*161,310+(y+.5)*161
try:
 for _ in range(180):
  try:
   window=xd('search','--onlyvisible','--pid',proc.pid).splitlines()[-1];wait('title');break
  except subprocess.CalledProcessError:time.sleep(.2)
 focus();xd('windowmove',window,2100,45);time.sleep(1)
 movie=subprocess.Popen(['ffmpeg','-hide_banner','-loglevel','error','-y','-f','x11grab','-window_id',window,'-framerate','30','-video_size','540x960','-i',':1','-t','65','-an','-c:v','libx264','-threads','2','-preset','ultrafast','-crf','23','-pix_fmt','yuv420p',str(out/'first-three-cities.mp4')],stdout=subprocess.DEVNULL,stderr=open(out/'video.log','w'))
 cap('01-title');started=time.monotonic();click(360,1060);wait('board');initial=state()['Save']['Profile']['Credits'];assert initial==350;cap('02-one-clear-instruction')
 drag(*pos(3,2),*pos(2,2),'03-lift-and-snap');assert node(2)['X']==2;assert state()['Save']['Profile']['Credits']==initial;cap('04-connection-made')
 # Real tray drag spends build budget, never the persistent wallet. Undo restores the board.
 before=nodes();drag(500,1030,*pos(0,0),'05-tray-preview');assert len(nodes())==3 and nodes()[-1]['Kind']=='farm';assert state()['Save']['Profile']['Credits']==initial;click(84,1198);assert nodes()==before
 board=state()['Save']['Board'];click(580,1198);time.sleep(.45);cap('06-city-paying-out')
 click(630,80);click(222,1198);click(120,1030);key('r');assert state()['Screen']=='board' and state()['Save']['Board']==board,'Controls changed a resolving city';checks.append('Menu, rotation, keyboard and tray blocked during payout')
 wait('result');first=time.monotonic()-started;assert state()['Save']['Profile']['Stars'][0]==3 and state()['Save']['Profile']['Credits']==650;cap('07-first-city-result');checks.append('One connection earns three stars; actual reward is 300 city coins')
 click(360,990);wait('board');assert state()['Save']['Profile']['Selected']==1;cap('08-delivery-lesson');click(*pos(2,3));click(222,1198);assert node(3)['Rotation']==3;cap('09-delivery-connected');click(580,1198);wait('result');assert state()['Save']['Profile']['Stars'][1]==3;cap('10-second-city-result')
 click(360,990);wait('board');assert state()['Save']['Profile']['Selected']==2;drag(*pos(0,2),*pos(1,2));click(*pos(2,3));click(222,1198);click(580,1198);wait('result');s=state()['Save'];assert s['Profile']['Stars'][:3]==[3,3,3] and s['Profile']['Credits']==1250;cap('11-three-lessons-complete');checks.append('Move, rotate and combined lessons all produce three stars through visible controls')
finally:
 if proc.poll() is None:proc.terminate();proc.wait(timeout=15)
 if movie and movie.poll() is None:movie.terminate();movie.wait(timeout=10)
issues=json.loads((out/'text-overflows.json').read_text())['Issues'];assert not issues,issues
errors=[l for l in (out/'player.log').read_text().splitlines() if any(x in l for x in ['NullReferenceException','MissingReferenceException','error CS'])];assert not errors,errors
report={'passed':True,'first_win_script_seconds':round(first,2),'checks':checks,'actions':actions,'method':'Real mouse and keyboard input against a fresh isolated save; no save injection. Script timing is not human usability evidence.','source':{str(f.relative_to(p)):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Assets/_Game').rglob('*.cs')},'build':{str(f.relative_to(p/'Builds/Linux')):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Builds/Linux').rglob('*') if f.is_file()}};(out/'report.json').write_text(json.dumps(report,indent=2));(p/'Artifacts/latest-first-session.txt').write_text(str(out));print(out)
