#!/usr/bin/env python3
"""Repeat native taps on earned empire saves; never mutate gameplay snapshots."""
from pathlib import Path
import datetime,hashlib,json,os,shutil,subprocess,time
p=Path(__file__).resolve().parent.parent
out=p/'Artifacts/CC2/NavigationReplay'/datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ')
out.mkdir(parents=True)
source=p/'Artifacts/CC2/OperationsReplay/20261022T185520Z/Saves'
shutil.copytree(source,out/'Saves')
env={**os.environ,'DISPLAY':os.environ.get('CASHFLOW_TEST_DISPLAY',':1')}
proc=subprocess.Popen([str(p/'Builds/EmpireSandbox/CashflowChains-Empire.x86_64'),'-screen-fullscreen','0','-screen-width','540','-screen-height','960','-cc2-empire-sandbox','yes','-save-dir',str(out/'Saves'),'-capture-dir',str(out),'-logFile',str(out/'player.log')],env=env,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
def xd(*a):return subprocess.check_output(['xdotool',*map(str,a)],env=env,text=True,stderr=subprocess.DEVNULL,timeout=10).strip()
def layout():return json.loads((out/'ui-layout.json').read_text())
actions=[]
def click(name,edge=False):
 time.sleep(.15)
 items=[e for e in layout()['Elements'] if e['Kind']=='button' and e['Name']==name]
 assert items,(name,layout()['Screen'])
 e=max(items,key=lambda e:e['Priority']);r=e['Pixels'];x,y=round(r['x']+(2 if edge else r['width']/2)),round(r['y']+r['height']/2)
 with (out/'driver.txt').open('a') as f:f.write(f"{layout()['Frame']} | {layout()['Screen']} | {name} | {x},{y}\n")
 xd('mousemove','--window',window,x,y,'mousedown','1');time.sleep(.18)
 if edge:
  during=next(a['Pixels'] for a in layout()['Elements'] if a['Name']==name and a['Priority']==e['Priority']);assert all(abs(during[k]-r[k])<.5 for k in ('x','y','width','height')),'Press animation moved the hitbox'
 xd('mousemove','--window',window,x,y,'mouseup','1');time.sleep(.4);actions.append({'button':name,'edge':edge})
def wait(screen):
 for _ in range(100):
  try:
   if layout()['Screen']==screen:return
  except (FileNotFoundError,json.JSONDecodeError):pass
  time.sleep(.1)
 raise AssertionError((screen,layout()['Screen']))
try:
 for _ in range(160):
  try:window=xd('search','--onlyvisible','--pid',proc.pid).splitlines()[-1];break
  except subprocess.CalledProcessError:time.sleep(.2)
 xd('windowfocus','--sync',window);xd('windowmove',window,50 if env['DISPLAY']!=':1' else 2100,45);wait('title');time.sleep(.5)
 click('CONTINUE BUILDING  →');wait('board');xd('key','Escape');wait('menu');click('MY EMPIRE');wait('empire')
 for i in range(25):
  click('DEVELOP',edge=i%5==0);assert any(e['Name']=='FIND SOURCES' and e['Priority']>100 for e in layout()['Elements']),('DEVELOP missed',i)
  click('×');wait('empire');click('DISPATCH');wait('empire-dispatch');click('CHOOSE A DELIVERY');wait('dispatch-catalog')
  click('MY ACTIVE DELIVERIES');wait('empire-dispatch');click('MY CITY');wait('empire')
 (out/'report.json').write_text(json.dumps({'passed':True,'cycles':25,'taps':len(actions),'edge_presses':sum(a['edge'] for a in actions),'method':'Native press/release, copied earned saves, no retries or direct callbacks. Edge presses also verify that the real hit rectangle stays fixed during animation.','actions':actions,'source':{str(f.relative_to(p)):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Assets/_Game').rglob('*.cs')}},indent=2))
finally:
 if proc.poll() is None:proc.terminate();proc.wait(timeout=15)
 print(out,flush=True)
