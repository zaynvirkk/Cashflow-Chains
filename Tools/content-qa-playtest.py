#!/usr/bin/env python3
"""Separate development-player content fixtures; NEVER a payment or real progression receipt."""
from pathlib import Path
from ui_map import local_point,archive_review,review_issues,wait_capture
import subprocess,os,time,json,datetime,hashlib,shutil
p=Path(__file__).resolve().parent.parent;env={**os.environ,'DISPLAY':os.environ.get('CASHFLOW_TEST_DISPLAY',':1')};out=p/'Artifacts/ContentQA'/datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ');out.mkdir(parents=True);proc=None;window=None;actions=[]
def xd(*a):return subprocess.check_output(['xdotool',*map(str,a)],env=env,text=True,stderr=subprocess.DEVNULL,timeout=10).strip()
def state():return json.loads((out/'campaign-state.json').read_text())
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
def start(qa):
 global proc,window
 expected='title' if qa else 'board'
 player=p/('Builds/ContentQA/CashflowChains-QA.x86_64' if qa else 'Builds/Linux/CashflowChains.x86_64');before=time.time();proc=subprocess.Popen([str(player),'-screen-fullscreen','0','-screen-width','540','-screen-height','960','-qa-content','all','-save-dir',str(out/('FixtureSaves' if qa else 'ReleaseGuardSaves')),'-capture-dir',str(out),'-logFile',str(out/('qa-player.log' if qa else 'release-guard.log'))],env=env,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
 for _ in range(180):
  try:
   window=xd('search','--onlyvisible','--pid',proc.pid).splitlines()[-1]
   if (out/'ui-layout.json').exists() and (out/'ui-layout.json').stat().st_mtime>=before and layout()['Screen']==expected:break
  except subprocess.CalledProcessError:pass
  assert proc.poll() is None;time.sleep(.2)
 focus();xd('windowmove',window,50 if env['DISPLAY']!=':1' else 2100,45);time.sleep(1);wait(expected)
def stop():
 if proc and proc.poll() is None:proc.terminate();proc.wait(timeout=15);archive_review(out)
def click(x,y):focus();xd('mousemove','--window',window,x,y,'mousedown','1');time.sleep(.08);xd('mouseup','1');time.sleep(.18)
def named(name,index=0,prefix=False):
 time.sleep(.18);items=[e for e in layout()['Elements'] if e['Kind']=='button' and (e['Name'].startswith(name) if prefix else e['Name']==name)];assert items,(name,[e['Name'] for e in layout()['Elements'] if e['Kind']=='button']);priority=max(e['Priority'] for e in items);items=[e for e in items if e['Priority']==priority];r=items[index]['Pixels'];click(round(r['x']+r['width']/2),round(r['y']+r['height']/2));actions.append(['button',name,index])
def point(x,y):return local_point(out,x,y)
def key(k):focus();xd('keydown',k);time.sleep(.08);xd('keyup',k);time.sleep(.3)
def cap(label):
 if 'result' in label:time.sleep(.65)
 before=set(out.glob('cashflow-*.png'));key('F12');shutil.copyfile(wait_capture(out,before),out/(label+'.png'))
def desk():
 for _ in range(6):
  if state()['Screen']=='menu':return
  key('Escape')
 raise AssertionError('Menu unavailable')
def audit_controls():
 s=layout();buttons=[e for e in s['Elements'] if e['Kind']=='button']
 for e in buttons:
  r=e['Pixels'];assert r['x']>=-1 and r['y']>=-1 and r['x']+r['width']<=s['Width']+1 and r['y']+r['height']<=s['Height']+1,e
 for i,a in enumerate(buttons):
  for b in buttons[i+1:]:
   aa,bb=a['Pixels'],b['Pixels'];assert min(aa['x']+aa['width'],bb['x']+bb['width'])-max(aa['x'],bb['x'])<=1 or min(aa['y']+aa['height'],bb['y']+bb['height'])-max(aa['y'],bb['y'])<=1,(a,b)
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
 assert sorted(state()['Save']['Board']['Nodes'],key=lambda n:n['Id'])==sorted(target['Nodes'],key=lambda n:n['Id'])
try:
 start(False);assert not state()['Save']['QaFixture'];assert not any(state()['Save']['Profile']['Stars']) and not state()['Save']['Profile']['Entitlements'];cap('00-production-rejects-qa-command');stop()
 for name in ['actual-mix.wav','performance.json','text-overflows.json']:
  f=out/name
  if f.exists():f.rename(out/('release-guard-'+name))
 start(True);assert state()['Save']['QaFixture'];profile=state()['Save']['Profile'];assert sum(profile['Stars'])==312 and not profile['Purchases'] and not profile['StoreReceipts'];initial=profile['Credits'];named('CONTINUE BUILDING  →');wait('board');desk();named('DISTRICTS & PROGRESS');wait('hub');cap('01-qa-eight-complete-districts');named('FINANCE AFTER DARK',prefix=True);wait('map');assert len([e for e in layout()['Elements'] if e['Kind']=='button' and e['Name'].split('\n')[0].isdigit()])==20;cap('02-all-twenty-finance-cities');audit_controls()
 xd('windowsize',window,1280,720);time.sleep(.8);audit_controls();cap('03-wide-twenty-city-map');xd('windowsize',window,540,960);time.sleep(.8);named('104\n★★★');wait('board');assert state()['Save']['Profile']['Selected']==103;solve(103);cap('04-last-finance-network');key('space');time.sleep(1);cap('05-advanced-chain-running');wait('result');cap('06-last-finance-result');named('DISTRICT MAP');wait('map');cap('07-last-city-returns-to-finance');desk();named('DISTRICTS & PROGRESS');wait('hub');named('HARBOR STORIES',prefix=True);wait('map');named('73\n★★★');wait('board');solve(72);cap('08-limited-import-harbor');key('space');wait('result');assert state()['Save']['Profile']['Credits']==initial;desk();named('STORE');wait('store');named('›');named('›');named('DETAILS  →',0);cap('09-eight-neon-preview-sprites');named('USE THIS FINISH');wait('board');assert state()['Save']['Profile']['Finish']==1;cap('10-neon-city-in-play');desk();named('STORE');named('DETAILS  →',0);named('USE THIS FINISH');wait('board');assert state()['Save']['Profile']['Finish']==2;cap('11-founder-finish-and-badge');desk();named('BLUEPRINTS');wait('blueprints');named('›');cap('12-four-extra-blueprints');named('SAVE',0);assert state()['Save']['Blueprints'][8]['Challenge']==72;named('LOAD',0);wait('board');assert state()['Save']['Profile']['Selected']==72;desk();named('COLLECTIONS');wait('collections');cap('13-complete-collections');named('+200\nCOINS',0);assert state()['Save']['Profile']['Credits']==initial+200;named('CLAIMED',0);assert state()['Save']['Profile']['Credits']==initial+200;cap('14-collection-claimed-once');assert not state()['Save']['Profile']['Purchases'];time.sleep(27)
finally:stop()
issues=review_issues(out);assert not issues,issues
errors=[line for file in out.glob('*.log') for line in file.read_text().splitlines() if any(x in line for x in ['NullReferenceException','MissingReferenceException','IndexOutOfRangeException','ArgumentException','error CS'])];assert not errors,errors
report={'passed':True,'scope':'DEVELOPMENT CONTENT FIXTURE ONLY; not a purchase test or genuine earned progression. Regular release rejects QA command.','production_rejects_fixture_command':True,'fixture_purchases':0,'actions':actions,'source':{str(f.relative_to(p)):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Assets/_Game').rglob('*.cs')},'build':{str(f.relative_to(p/'Builds/ContentQA')):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Builds/ContentQA').rglob('*') if f.is_file()}};(out/'report.json').write_text(json.dumps(report,indent=2));(p/'Artifacts/latest-content-qa.txt').write_text(str(out));print(out)
