#!/usr/bin/env python3
"""Fresh,ordinary-input persistent-city playtest. No save/clock/inventory injection."""
from pathlib import Path
from ui_map import local_point,archive_review,review_issues,wait_capture
import subprocess,os,time,json,datetime,hashlib,shutil
p=Path(__file__).resolve().parent.parent;env={**os.environ,'DISPLAY':os.environ.get('CASHFLOW_TEST_DISPLAY',':1')};out=p/'Artifacts/CityWorldReplay'/datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ');out.mkdir(parents=True);proc=None;window=None;actions=[]
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
 start(True);assert state()['EmpireSandbox'];assert city()['Name']=='Newhaven';assert money('CASH')==200;assert len(city()['Properties'])==1;cap('01-one-owned-city-start')
 tap('Owned plot 2 1',kind='pointer');cap('02-build-roles-and-costs');tap('BUILD',1);cap('03-cafe-investment-quote');tap('START CONSTRUCTION');assert money('CASH')==100 and money('WOOD')==8;cap('04-real-construction-cost-and-clock')
 until(lambda:any(b['Kind']=='cafe' and b['Level']==1 for b in city()['Properties']));cap('05-first-cafe-opens');until(lambda:city()['Day']>=2);assert city()['LastDayRevenue']>city()['LastDayBills'];assert money('CASH')>=137;assert len([r for r in empire()['Economy']['Receipts'] if r['CommandId']=='city-day-settle:1'])==1;cap('06-actual-day-one-cashflow')
 assert not any(e['Kind']=='button' and 'SKIP' in e['Name'] for e in layout()['Elements']);tap('Owned plot 0 1',kind='pointer');tap('BUILD',0);tap('START CONSTRUCTION');cap('07-new-homes-in-the-same-city');until(lambda:len([b for b in city()['Properties'] if b['Level']>0])==3,20)
 tap('CITY DESK');wait('city-desk');cap('08-actual-city-desk');tap('NAME MY CITY');xd('key','ctrl+a');time.sleep(.25);xd('type','--clearmodifiers','--delay','120','riverbank');tap('SAVE CITY NAME');wait('city-world');assert city()['Name']=='riverbank';cap('09-city-name-saved')
 tap('CITY LEDGER');wait('city-ledger');cap('10-population-demand-bills-forecast');tap('MY CITY');wait('city-world')
 # Move a real property. It remains at its operating address until the next day.
 tap('Owned plot 2 1',kind='pointer');tap('MOVE');tap('Owned plot 3 1',kind='pointer');cap('11-pending-move-on-owned-land');until(lambda:next(b for b in city()['Properties'] if b['Kind']=='cafe')['X']==3,40)
 tap('Owned plot 3 1',kind='pointer');cap('12-business-margin-and-management');tap('EMPLOYEES');cap('13-workers-wages-and-capacity');key('Escape');key('Escape');wait('city-world')
 tap('−');cap('14-zoom-out-retains-city');tap('FIT CITY');cap('15-fit-owned-land-and-expansion')
 for w,h,label in [(1280,720,'16-wide-permanent-city'),(720,992,'17-short-permanent-city'),(540,960,'18-portrait-permanent-city')]:xd('windowsize',window,w,h);time.sleep(.7);cap(label)
 tap('CITY DESK');tap('FINANCING & BUSINESS STRATEGY');wait('city-strategy');cap('19-connected-business-strategy');tap('COMPARE FINANCING');cap('20-actual-financing-terms');tap('REVIEW $100 TERMS');cap('20b-full-loan-cost-before-acceptance');tap('ACCEPT FIXED TERMS');wait('city-strategy');assert city()['Loans'][0]['Principal']==100 and city()['Loans'][0]['Remaining']==110;tap('COMPARE FINANCING');tap('REPAY $50');assert city()['Loans'][0]['Remaining']==60;cap('20c-actual-saved-loan-and-repayment');key('Escape');tap('SUPPLIERS & SPONSORS');cap('21-supplier-and-sponsor-obligations');key('Escape');key('Escape');wait('city-desk');tap('RETURN TO MY CITY');wait('city-world')
 # Read traffic repeatedly from the actual rendered component,not a UI-only fixture.
 samples=[]
 for _ in range(40):
  t=json.loads((out/'traffic-state.json').read_text());assert t['OverlapCount']==0;samples.append(t);time.sleep(.15)
 assert samples[-1]['Time']>samples[0]['Time'];assert any(a['X']!=b['X'] or a['Y']!=b['Y'] for a,b in zip(samples[0]['Cars'],samples[-1]['Cars']))
 (out/'traffic-samples.json').write_text(json.dumps(samples,indent=2));before=city();stop();start(True);assert city()['Name']=='riverbank';assert [(b['Id'],b['X'],b['Y']) for b in city()['Properties']]==[(b['Id'],b['X'],b['Y']) for b in before['Properties']];assert city()['Day']>=before['Day'];cap('22-cold-resume-same-city');time.sleep(27)
finally:stop()
issues=review_issues(out);assert not issues,issues
errors=[line for file in out.glob('*.log') for line in file.read_text().splitlines() if any(x in line for x in ['NullReferenceException','MissingReferenceException','IndexOutOfRangeException','ArgumentException','InvalidOperationException','error CS','Cannot add component'])];assert not errors,errors
for file,h in source.items():assert hashlib.sha256((p/file).read_bytes()).hexdigest()==h,'Source changed during replay: '+file
report={'passed':True,'scope':'Fresh genuine-input permanent city: founding allocation,real material/cash build,3-second construction,5-second day,second property,name,finite-demand ledger,pending move,staffing,zoom,three layouts,finance/agreement review,actual moving non-overlap traffic and cold resume. No edited saves/clocks/inventory or real purchase.','actions':actions,'source':source,'build':{str(f.relative_to(p/'Builds/EmpireSandbox')):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Builds/EmpireSandbox').rglob('*') if f.is_file()}};(out/'report.json').write_text(json.dumps(report,indent=2));(p/'Artifacts/latest-city-world.txt').write_text(str(out));print(out)
