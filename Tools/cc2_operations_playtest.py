#!/usr/bin/env python3
"""Genuine native first-session -> construction -> production replay in the labelled CC2 development sandbox."""
from pathlib import Path
from ui_map import local_point,archive_review,review_issues,wait_capture
import subprocess,os,time,json,datetime,hashlib,shutil
p=Path(__file__).resolve().parent.parent;env={**os.environ,'DISPLAY':os.environ.get('CASHFLOW_TEST_DISPLAY',':1')};out=p/'Artifacts/CC2/OperationsReplay'/datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ');out.mkdir(parents=True);proc=None;window=None;actions=[]
earned=Path((p/'Artifacts/latest-cc2-empire.txt').read_text().strip());assert json.loads((earned/'report.json').read_text())['passed'];shutil.copytree(earned/'Saves',out/'Saves')
source={str(f.relative_to(p)):hashlib.sha256(f.read_bytes()).hexdigest() for f in [*(p/'Assets/_Game').rglob('*.cs'),p/'Assets/_Game/Data/CC2/balance-v1.json',p/'Assets/_Game/Resources/CC2/balance-v1.json']}
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
 focus();xd('windowmove',window,50 if env['DISPLAY']!=':1' else 2100,45);time.sleep(.8);wait('title')
def stop():
 if proc and proc.poll() is None:proc.terminate();proc.wait(timeout=15);archive_review(out)
def click(x,y):focus();xd('mousemove','--window',window,x,y,'mousedown','1');time.sleep(.18);xd('mousemove','--window',window,x,y,'mouseup','1');time.sleep(.4)
def point(x,y):return local_point(out,x,y)
def named(name,index=0,prefix=False):
 time.sleep(.15);items=[e for e in layout()['Elements'] if e['Kind']=='button' and (e['Name'].startswith(name) if prefix else e['Name']==name)];assert items,(name,[e['Name'] for e in layout()['Elements'] if e['Kind']=='button']);priority=max(e['Priority'] for e in items);items=[e for e in items if e['Priority']==priority];r=items[index]['Pixels'];click(round(r['x']+r['width']/2),round(r['y']+r['height']/2));actions.append(['button',name,index])
def key(k):focus();xd('keydown',k);time.sleep(.1);xd('keyup',k);time.sleep(.3)
def cap(label):
 time.sleep(.65);before=set(out.glob('cashflow-*.png'));key('F12');shutil.copyfile(wait_capture(out,before),out/(label+'.png'))
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
 start(True);assert state()['EmpireSandbox'];named('CONTINUE BUILDING  →');wait('board');my_empire();named('BOARD');wait('board');before=money('CASH');key('space');wait('result');assert money('CASH')==before;cap('01-existing-plan-does-not-regrant')
 for level in range(3,6):
  click(*point(360,990));wait('board');assert state()['Save']['Profile']['Selected']==level;solve(level);key('space');wait('result');assert state()['Save']['Profile']['Stars'][level]==3
 cap('02-six-genuine-city-wins');my_empire();named('RESEARCH');wait('empire-research');cap('03-research-needs-real-dispatches');named('GO TO MY DELIVERY ROUTES');wait('empire-dispatch')
 for number in range(3):
  named('CHOOSE A DELIVERY');wait('dispatch-catalog');named('DETAILS',0);cap('04-guaranteed-route-offer-'+str(number+1));named('START DELIVERY');wait('empire-dispatch');assert len([d for d in empire()['Dispatches'] if not d['Claimed']])==1;job=empire()['Dispatches'][-1];named('VIEW RETURN');cap('05-real-dispatch-timer-'+str(number+1));print('Waiting for genuine dispatch',number+1,'until',job['FinishAt'],flush=True)
  while time.time()<job['FinishAt']+1:time.sleep(.3)
  named('COLLECT THIS RETURN');wait('empire-dispatch');assert len([d for d in empire()['Dispatches'] if d['Claimed']])==number+1;cap('06-real-return-'+str(number+1))
 assert money('RECRUIT_TICKET')==1 and money('RESEARCH_DOCS')==3 and money('CONSTRUCTION_SPEEDUP_MIN')==3;named('RESEARCH');wait('empire-research');cap('07-thirty-node-research-entry');named('DETAILS',0);cap('08-actual-research-cost');named('START RESEARCH');wait('empire-research');research=empire()['ResearchQueue'][0];named('VIEW');cap('09-real-research-timer');print('Waiting for genuine research until',research['FinishAt'],flush=True)
 while time.time()<research['FinishAt']+1:time.sleep(.3)
 named('COMPLETE RESEARCH');wait('empire-research');assert 'operations-1' in empire()['Research'];cap('10-production-research-active');named('LOGISTICS');cap('11-logistics-tree');named('INNOVATION');cap('12-innovation-tree');named('›');cap('13-advanced-research-nodes');named('MY CITY');wait('empire');cap('13b-return-to-developed-city');named('DEVELOP',0);named('START UPGRADE');wait('empire');named('BUILDER',prefix=True);cap('14-earned-speedups-ready')
 for use in range(3):named('USE 1 BUILDER MINUTE')
 assert money('CONSTRUCTION_SPEEDUP_MIN')==0;named('COMPLETE CONSTRUCTION');wait('empire');assert empire()['Businesses'][0]['Level']==2;cap('15-real-upgraded-business')
 named('DISPATCH');wait('empire-dispatch')
 for w,h,label in [(1280,720,'16-wide-dispatch'),(720,992,'17-short-dispatch'),(540,960,'18-portrait-dispatch')]:xd('windowsize',window,w,h);time.sleep(.7);cap(label)
 named('RESEARCH');wait('empire-research');before=empire();stop();start(True);named('CONTINUE BUILDING  →');wait('board');my_empire();named('RESEARCH');wait('empire-research');assert empire()['Research']==before['Research'] and len([d for d in empire()['Dispatches'] if d['Claimed']])==3;cap('19-cold-resume-research-and-returns');time.sleep(27)
finally:stop()
issues=review_issues(out);assert not issues,issues
errors=[line for file in out.glob('*.log') for line in file.read_text().splitlines() if any(x in line for x in ['NullReferenceException','MissingReferenceException','IndexOutOfRangeException','ArgumentException','error CS'])];assert not errors,errors
for file,h in source.items():assert hashlib.sha256((p/file).read_bytes()).hexdigest()==h,'Source changed during replay: '+file
report={'passed':True,'scope':'Genuine native input from byte-for-byte copied earned P03 saves. Three more real city wins,three real60-second dispatches,one real60-second research,earned speedup consumption,level2 business and cold resume. No edited clocks/saves,cloud or payment simulation.','actions':actions,'source':source,'build':{str(f.relative_to(p/'Builds/EmpireSandbox')):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Builds/EmpireSandbox').rglob('*') if f.is_file()}};(out/'report.json').write_text(json.dumps(report,indent=2));(p/'Artifacts/latest-cc2-operations.txt').write_text(str(out));print(out)
