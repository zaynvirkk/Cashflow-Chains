#!/usr/bin/env python3
"""Fresh,ordinary-input persistent-city playtest. No save/clock/inventory injection."""
from pathlib import Path
from ui_map import local_point,archive_review,review_issues,wait_capture
import subprocess,os,time,json,datetime,hashlib,shutil
p=Path(__file__).resolve().parent.parent;env={**os.environ,'DISPLAY':os.environ.get('CASHFLOW_TEST_DISPLAY',':1')};out=p/'Artifacts/CityWorldReplay'/datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ');out.mkdir(parents=True);proc=None;window=None;actions=[]
source={str(f.relative_to(p)):hashlib.sha256(f.read_bytes()).hexdigest() for f in [*(p/'Assets/_Game').rglob('*.cs'),p/'Assets/_Game/Data/CC2/balance-v1.json',p/'Assets/_Game/Resources/CC2/balance-v1.json',*[f for folder in ['Art/V6','Fonts/V6'] for f in (p/'Assets/_Game/Resources'/folder).rglob('*') if f.is_file()],Path(__file__)]}
def xd(*a):return subprocess.check_output(['xdotool',*map(str,a)],env=env,text=True,stderr=subprocess.DEVNULL,timeout=10).strip()
def read_json(name):
 for _ in range(100):
  try:return json.loads((out/name).read_text())
  except (FileNotFoundError,json.JSONDecodeError):time.sleep(.02)
 raise AssertionError('Valid read-only runtime probe missing: '+name)
def state():return read_json('campaign-state.json')
def empire():return read_json('empire-state.json')
def money(id):return sum(b['Earned']+b['Purchased'] for b in empire()['Economy']['Balances'] if b['ItemId']==id)
def layout():return read_json('ui-layout.json')
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
 player=Path(os.environ.get('CASHFLOW_CITY_PLAYER',str(p/('Builds/EmpireSandbox/CashflowChains-Empire.x86_64' if sandbox else 'Builds/Linux/CashflowChains.x86_64'))));before=time.time();proc=subprocess.Popen([str(player),'-screen-fullscreen','0','-screen-width','540','-screen-height','960','-cc2-empire-sandbox','yes','-save-dir',str(out/('Saves' if sandbox else 'ReleaseGuardSaves')),'-capture-dir',str(out),'-logFile',str(out/('empire-player.log' if sandbox else 'release-guard.log'))],env=env,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
 for _ in range(180):
  try:
   window=xd('search','--onlyvisible','--pid',proc.pid).splitlines()[-1]
   if (out/'ui-layout.json').exists() and (out/'ui-layout.json').stat().st_mtime>=before and layout()['Screen']=='city-world':break
  except subprocess.CalledProcessError:pass
  assert proc.poll() is None;time.sleep(.2)
 focus();xd('windowmove',window,50 if env['DISPLAY']!=':1' else 2100,45);time.sleep(.8);wait('city-world')
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

def city():return empire()['City']
def tap(name,index=0,kind=None):
 time.sleep(.2);items=[e for e in layout()['Elements'] if e['Name']==name and (kind is None or e['Kind']==kind)];assert items,(name,[e['Name'] for e in layout()['Elements']]);priority=max(e['Priority'] for e in items);items=[e for e in items if e['Priority']==priority];r=items[index]['Pixels'];click(round(r['x']+r['width']/2),round(r['y']+r['height']/2));actions.append(['tap',name,index])
def until(test,timeout=40):
 deadline=time.monotonic()+timeout
 while time.monotonic()<deadline:
  if test():return
  time.sleep(.15)
 raise AssertionError('Real-time city condition did not occur')
try:
 start(True);assert state()['EmpireSandbox']==(os.environ.get('CASHFLOW_EXPECT_SANDBOX','1')=='1');assert not city()['Properties'];assert money('CASH')==200;assert read_json('city-world-state.json')['Forecast']['Population']==0;cap('01-empty-city-no-free-home');xd('windowsize',window,1280,720);time.sleep(.8);cap('01b-wide-empty-city');xd('windowsize',window,540,960);time.sleep(.8)
 tap('BUILD & DEVELOP');cap('02-illustrated-building-tray');tap('BUILD',0);cap('03-first-home-real-cost');tap('START CONSTRUCTION');assert money('CASH')==120;cap('04-paid-home-construction');until(lambda:any(b['Kind']=='home' and b['Level']==1 for b in city()['Properties']));until(lambda:read_json('city-world-state.json')['Forecast']['Population']==4);cap('05-first-household-arrives')
 tap('Owned plot 2 1',kind='pointer');tap('BUILD',0);cap('06-cafe-cost-and-workforce');tap('START CONSTRUCTION');assert money('CASH')==20;xd('windowsize',window,1280,720);time.sleep(.8);until(lambda:city()['Day']>=2);assert money('CASH')>=42;assert len([r for r in empire()['Economy']['Receipts'] if r['CommandId']=='city-day-settle:1'])==1;cap('07-real-first-sale-and-payout');xd('windowsize',window,540,960);time.sleep(.8)
 tap('City census');wait('city-residents');cap('08-age-and-gender-census');tap('INSPECT A HOME');cap('09-individual-resident-profiles');key('Escape');key('Escape');wait('city-world')
 until(lambda:money('CASH')>=92,140);tap('Owned plot 0 1',kind='pointer');tap('BUILD',0);named('FIND ',prefix=True);named('BUY ',prefix=True);tap('START CONSTRUCTION');until(lambda:len([b for b in city()['Properties'] if b['Level']>0])==3,20);until(lambda:read_json('city-world-state.json')['Forecast']['Population']==6);cap('10-different-household-six-residents');tap('Owned plot 0 1',kind='pointer');tap('HOUSEHOLDS & RESIDENTS');cap('11-retired-household-and-budget');key('Escape');key('Escape');wait('city-world')
 tap('CITY DESK');wait('city-desk');cap('12-redesigned-city-desk');xd('windowsize',window,1280,720);time.sleep(.8);cap('12b-wide-city-desk');xd('windowsize',window,540,960);time.sleep(.8);tap('NAME MY CITY');xd('key','ctrl+a');time.sleep(.2);xd('type','--clearmodifiers','--delay','120','riverbank');tap('SAVE CITY NAME');wait('city-world');assert city()['Name']=='riverbank'
 tap('CITY DESK');tap('SETTINGS');tap('SAVE RECOVERY');cap('13-city-only-save-recovery');assert not any(e['Name']=='NEW LOCAL GAME' for e in layout()['Elements']);tap('KEEP MY CITY');tap('RETURN TO MY CITY');wait('city-world');tap('CITY LEDGER');wait('city-ledger');cap('14-real-costs-and-demographic-demand');tap('MY CITY');wait('city-world')
 for w,h,label in [(1280,720,'15-wide-city'),(720,992,'16-short-city'),(720,1600,'17-tall-city'),(540,960,'18-portrait-city')]:xd('windowsize',window,w,h);time.sleep(.8);cap(label)
 tap('CITY DESK');tap('FINANCING & BUSINESS STRATEGY');wait('city-strategy');tap('COMPARE FINANCING');tap('REVIEW $100 TERMS');cap('19-real-financing-terms');tap('ACCEPT FIXED TERMS');wait('city-strategy');assert city()['Loans'][0]['Remaining']==110;tap('COMPARE FINANCING');tap('REPAY $50');assert city()['Loans'][0]['Remaining']==60;cap('20-actual-repayment');key('Escape');tap('SUPPLIERS & SPONSORS');cap('21-agreement-conditions');key('Escape');key('Escape');wait('city-desk');tap('RETURN TO MY CITY');tap('LAND');tap('EXPAND EAST');cap('22-same-city-land-quote');key('Escape')
 samples=[]
 for _ in range(40):
  t=read_json('traffic-state.json');assert t['OverlapCount']==0;samples.append(t);time.sleep(.15)
 assert samples[-1]['Time']>samples[0]['Time'];assert any(a['X']!=b['X'] or a['Y']!=b['Y'] for a,b in zip(samples[0]['Cars'],samples[-1]['Cars']));(out/'traffic-samples.json').write_text(json.dumps(samples,indent=2))
 before=city();stop();start(True);assert city()['Name']=='riverbank';assert [(b['Id'],b['X'],b['Y']) for b in city()['Properties']]==[(b['Id'],b['X'],b['Y']) for b in before['Properties']];cap('23-cold-resume-same-city-and-people');time.sleep(27)

finally:stop()
issues=review_issues(out);assert not issues,issues
errors=[line for file in out.glob('*.log') for line in file.read_text().splitlines() if any(x in line for x in ['NullReferenceException','MissingReferenceException','IndexOutOfRangeException','ArgumentException','InvalidOperationException','error CS','Cannot add component'])];assert not errors,errors
for file,h in source.items():assert hashlib.sha256((p/file).read_bytes()).hexdigest()==h,'Source changed during replay: '+file
report={'passed':True,'scope':'Fresh ordinary-input city:empty start,paid house and cafe,actual clocks/payout,age/gender/preferences,second retired household/material purchase,name,ledger,four shapes,loan/repayment,land review,traffic and cold resume. No injected save/clock/inventory/purchase.','developmentSandbox':state()['EmpireSandbox'],'actions':actions,'source':source,'build':{str(f.relative_to(Path(os.environ.get('CASHFLOW_CITY_PLAYER',str(p/'Builds/EmpireSandbox/CashflowChains-Empire.x86_64'))).parent)):hashlib.sha256(f.read_bytes()).hexdigest() for f in Path(os.environ.get('CASHFLOW_CITY_PLAYER',str(p/'Builds/EmpireSandbox/CashflowChains-Empire.x86_64'))).parent.rglob('*') if f.is_file()}};(out/'report.json').write_text(json.dumps(report,indent=2));(p/'Artifacts/latest-city-world.txt').write_text(str(out));print(out)
