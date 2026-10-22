#!/usr/bin/env python3
"""Genuine native first-session -> construction -> production replay in the labelled CC2 development sandbox."""
from pathlib import Path
from ui_map import local_point
import subprocess,os,time,json,datetime,hashlib,shutil
p=Path(__file__).resolve().parent.parent;env={**os.environ,'DISPLAY':':1'};out=p/'Artifacts/CC2/EmpireReplay'/datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ');out.mkdir(parents=True);proc=None;window=None;actions=[]
source={str(f.relative_to(p)):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Assets/_Game').rglob('*.cs')}
def xd(*a):return subprocess.check_output(['xdotool',*map(str,a)],env=env,text=True,stderr=subprocess.DEVNULL,timeout=10).strip()
def state():return json.loads((out/'campaign-state.json').read_text())
def empire():return json.loads((out/'empire-state.json').read_text())
def money(id):return sum(b['Earned']+b['Purchased'] for b in empire()['Economy']['Balances'] if b['ItemId']==id)
def layout():return json.loads((out/'ui-layout.json').read_text())
def focus():
 if xd('getactivewindow')!=window:xd('windowactivate','--sync',window)
def wait(screen):
 for _ in range(180):
  try:
   if state()['Screen']==screen:return state()
  except (FileNotFoundError,json.JSONDecodeError):pass
  time.sleep(.1)
 raise AssertionError('Missing '+screen)
def start(sandbox):
 global proc,window
 player=p/('Builds/EmpireSandbox/CashflowChains-Empire.x86_64' if sandbox else 'Builds/Linux/CashflowChains.x86_64');before=time.time();proc=subprocess.Popen([str(player),'-screen-fullscreen','0','-screen-width','540','-screen-height','960','-cc2-empire-sandbox','yes','-save-dir',str(out/('Saves' if sandbox else 'ReleaseGuardSaves')),'-capture-dir',str(out),'-logFile',str(out/('empire-player.log' if sandbox else 'release-guard.log'))],env=env,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
 for _ in range(180):
  try:
   window=xd('search','--onlyvisible','--pid',proc.pid).splitlines()[-1]
   if (out/'ui-layout.json').exists() and (out/'ui-layout.json').stat().st_mtime>=before and layout()['Screen']=='title':break
  except subprocess.CalledProcessError:pass
  assert proc.poll() is None;time.sleep(.2)
 focus();xd('windowmove',window,2100,45);time.sleep(.8);wait('title')
def stop():
 if proc and proc.poll() is None:proc.terminate();proc.wait(timeout=15)
def click(x,y):focus();xd('mousemove','--window',window,x,y,'mousedown','1');time.sleep(.1);xd('mouseup','1');time.sleep(.25)
def point(x,y):return local_point(out,x,y)
def named(name,index=0,prefix=False):
 time.sleep(.15);items=[e for e in layout()['Elements'] if e['Kind']=='button' and (e['Name'].startswith(name) if prefix else e['Name']==name)];assert items,(name,[e['Name'] for e in layout()['Elements'] if e['Kind']=='button']);priority=max(e['Priority'] for e in items);items=[e for e in items if e['Priority']==priority];r=items[index]['Pixels'];click(round(r['x']+r['width']/2),round(r['y']+r['height']/2));actions.append(['button',name,index])
def key(k):focus();xd('keydown',k);time.sleep(.1);xd('keyup',k);time.sleep(.3)
def cap(label):
 time.sleep(.65);before=set(out.glob('cashflow-*.png'));key('F12');time.sleep(.3);files=set(out.glob('cashflow-*.png'))-before;assert files;shutil.copyfile(next(iter(files)),out/(label+'.png'))
def desk():
 for _ in range(6):
  if state()['Screen']=='menu':return
  key('Escape')
 raise AssertionError('Menu unavailable')
def my_empire():desk();named('MY EMPIRE');wait('empire')
def solve(level):
 target=json.loads((p/'Artifacts/Campaign/witnesses.jsonl').read_text().splitlines()[level]);w,h=target['Width'],target['Height'];pos=lambda x,y:(38+(x+.5)*644/w,310+(y+.5)*644/h)
 def choose(i):
  n=next(n for n in state()['Save']['Board']['Nodes'] if n['Id']==i);click(*point(*pos(n['X'],n['Y'])));assert state()['Selected']==i
 def move(i,x,y):choose(i);click(*point(*pos(x,y)));assert any(n['Id']==i and n['X']==x and n['Y']==y for n in state()['Save']['Board']['Nodes'])
 for wanted in target['Nodes']:
  current=next(n for n in state()['Save']['Board']['Nodes'] if n['Id']==wanted['Id'])
  if (current['X'],current['Y'])!=(wanted['X'],wanted['Y']):
   board=state()['Save']['Board'];blocker=next((n for n in board['Nodes'] if (n['X'],n['Y'])==(wanted['X'],wanted['Y'])),None)
   if blocker:
    empty=next((x,y) for y in range(h) for x in range(w) if not any(n['X']==x and n['Y']==y for n in board['Nodes']) and not(board['Blocked']&(1<<(y*w+x))));move(blocker['Id'],*empty)
   move(wanted['Id'],wanted['X'],wanted['Y'])
  choose(wanted['Id']);rotation=next(n['Rotation'] for n in state()['Save']['Board']['Nodes'] if n['Id']==wanted['Id'])
  for _ in range((wanted['Rotation']-rotation)%4):named('ROTATE')
try:
 start(False);assert not state()['EmpireSandbox'];cap('00-release-rejects-empire-command');stop()
 for name in ['actual-mix.wav','performance.json','text-overflows.json']:
  f=out/name
  if f.exists():f.rename(out/('release-guard-'+name))
 start(True);assert state()['EmpireSandbox'];named('BUILD YOUR FIRST CITY  →');wait('board')
 for level in range(3):
  solve(level);key('space');wait('result');assert state()['Save']['Profile']['Stars'][level]==3;cap('01-real-chain-result-'+str(level+1))
  if level<2:click(*point(360,990));wait('board')
 assert money('CASH')==280 and money('WOOD')==35 and money('CHEST:bronze')==1;my_empire();cap('02-three-chains-fund-the-empire')
 for w,h,label in [(1280,720,'03-wide-empire'),(720,992,'04-short-empire'),(540,960,'05-portrait-empire')]:xd('windowsize',window,w,h);time.sleep(.7);cap(label)
 named('BUILD HERE',0);cap('06-exact-build-cost');named('START CONSTRUCTION');wait('empire');assert money('CASH')==180 and money('WOOD')==25;assert len(empire()['Construction'])==1;project=empire()['Construction'][0];cap('07-real-project-under-construction');named('BUILDER',prefix=True);cap('08-real-timer');named('KEEP PLAYING · NO WAIT REQUIRED');wait('board');click(*point(360,990)) if state()['Screen']=='result' else None
 # The last tutorial result closes to its current board; playing it again must not grant resources.
 before=money('CASH');key('space');wait('result');assert money('CASH')==before;cap('09-replay-cannot-farm');my_empire();named('BUILDER',prefix=True)
 while time.time()<project['FinishAt']+1:time.sleep(.25)
 cap('10-project-ready');named('COMPLETE CONSTRUCTION');wait('empire');assert empire()['Businesses'][0]['Level']==1 and not empire()['Construction'];cap('11-business-completed')
 # Cold restart preserves the actual funded project/business and all receipts.
 saved=empire();stop();start(True);named('CONTINUE BUILDING  →');wait('board');my_empire();assert empire()['Economy']['Revision']==saved['Economy']['Revision'];assert empire()['Businesses'][0]['Level']==1;cap('12-cold-resume-business')
 # Real wall-clock accumulation; do not patch clocks, config, saves or inventory.
 ready_at=empire()['Businesses'][0]['ProducedAt']+450+2
 print('Waiting for genuine home production until UTC epoch',ready_at,flush=True)
 while time.time()<ready_at:time.sleep(.5)
 my_empire();before=money('CASH');cap('13-earned-production-ready');named('COLLECT OUTPUT',prefix=True);assert money('CASH')>=before+1;cap('14-real-production-collected');after=money('CASH');key('Escape');my_empire();assert money('CASH')==after;assert not any(x['Purchased'] for x in empire()['Economy']['Balances']);time.sleep(27)
finally:stop()
issues=json.loads((out/'text-overflows.json').read_text())['Issues'];assert not issues,issues
errors=[line for file in out.glob('*.log') for line in file.read_text().splitlines() if any(x in line for x in ['NullReferenceException','MissingReferenceException','IndexOutOfRangeException','ArgumentException','error CS'])];assert not errors,errors
for file,h in source.items():assert hashlib.sha256((p/file).read_bytes()).hexdigest()==h,'Source changed during replay: '+file
report={'passed':True,'scope':'Genuine native input in a labelled local-only development sandbox. Three actual puzzle wins,exact build cost,30-second timer,persistent business,cold resume,real450-second production and once-only claims. No fake purchase,clock/save edit or cloud claim.','production_rejects_sandbox':True,'actions':actions,'source':source,'build':{str(f.relative_to(p/'Builds/EmpireSandbox')):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Builds/EmpireSandbox').rglob('*') if f.is_file()}};(out/'report.json').write_text(json.dumps(report,indent=2));(p/'Artifacts/latest-cc2-empire.txt').write_text(str(out));print(out)
