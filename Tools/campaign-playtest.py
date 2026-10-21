#!/usr/bin/env python3
"""Play all 72 certified solutions through ordinary native input, never writing a save."""
from ui_map import local_point
from pathlib import Path
import argparse,json,os,subprocess,time,datetime,hashlib
p=Path(__file__).resolve().parent.parent;cc=p.name=='CashflowChains';a=argparse.ArgumentParser();a.add_argument('--display',default=':1');a.add_argument('--resume');opt=a.parse_args();env={**os.environ,'DISPLAY':opt.display}
out=Path(opt.resume) if opt.resume else p/'Artifacts/CampaignReplay'/datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ');out.mkdir(parents=True,exist_ok=bool(opt.resume))
solutions=[json.loads(x) for x in (p/'Artifacts/Campaign/witnesses.jsonl').read_text().splitlines()][:72];actions=[];results=[];window=None;segments=[]
if opt.resume:
 prior=json.loads((out/'progress.json').read_text());results=prior['levels'];segments=prior.get('segments',[]);actions=prior.get('actions',[])
 for file,h in prior['source'].items():assert hashlib.sha256((p/file).read_bytes()).hexdigest()==h,file
 for file,h in prior['build'].items():assert hashlib.sha256((p/'Builds/Linux'/file).read_bytes()).hexdigest()==h,file
 for name in ['text-overflows.json','performance.json','actual-mix.wav']:
  f=out/name
  if f.exists():f.rename(out/(f.stem+'-segment-'+str(len(segments))+f.suffix))
proc=subprocess.Popen([str(p/'Builds/Linux'/(p.name+'.x86_64')),'-screen-fullscreen','0','-screen-width','540','-screen-height','960','-force-glcore','-save-dir',str(out/'Saves'),'-capture-dir',str(out),'-logFile',str(out/'player.log')],env=env,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
def xd(*x):return subprocess.check_output(['xdotool',*map(str,x)],env=env,text=True,stderr=subprocess.DEVNULL).strip()
def activate():
 try:xd('windowactivate','--sync',window)
 except (subprocess.CalledProcessError,subprocess.TimeoutExpired):xd('windowfocus',window)
 xd('windowraise',window)
def state():
 for _ in range(30):
  try:return json.loads((out/'campaign-state.json').read_text())
  except (FileNotFoundError,json.JSONDecodeError):time.sleep(.02)
 raise FileNotFoundError('Review snapshot unavailable')
def until(predicate):
 for _ in range(150):
  data=state()
  if predicate(data):return data
  time.sleep(.02)
 raise AssertionError('Expected state transition was not observed')
def wait(screen=None):
 for _ in range(100):
  try:
   s=state()
   if screen is None or s['Screen']==screen:return s
  except (FileNotFoundError,json.JSONDecodeError):pass
  time.sleep(.1)
 raise AssertionError('Missing screen '+str(screen))
def click(x,y):
 activate();ax,ay=local_point(out,x,y)
 before=(out/'campaign-state.json').stat().st_mtime_ns;xd('mousemove','--window',window,ax,ay,'mousedown','1');time.sleep(.06);xd('mouseup','1');actions.append(['click',x,y])
 for _ in range(100):
  time.sleep(.015)
  if (out/'campaign-state.json').stat().st_mtime_ns>before:break
 time.sleep(.035)
def key(k):activate();xd('keydown',k);time.sleep(.08);xd('keyup',k);time.sleep(.16);actions.append(['key',k])
def drag(x,y,tx,ty):
 activate();ax,ay=local_point(out,x,y);bx,by=local_point(out,tx,ty)
 xd('mousemove','--window',window,ax,ay,'mousedown','1');time.sleep(.08)
 for i in range(1,7):xd('mousemove','--window',window,round(ax+(bx-ax)*i/6),round(ay+(by-ay)*i/6));time.sleep(.025)
 xd('mouseup','1');time.sleep(.12);actions.append(['drag',x,y,tx,ty])
def capture(label):
 before=time.time();key('F12');time.sleep(.5);files=sorted(out.glob(('cashflow' if cc else 'blackbag')+'-*.png'),key=lambda f:f.stat().st_mtime)
 assert files and files[-1].stat().st_mtime>=before,'Fresh capture missing'
 (out/(label+'.png')).write_bytes(files[-1].read_bytes())
try:
 for _ in range(150):
  try:window=xd('search','--onlyvisible','--pid',proc.pid).splitlines()[-1];xd('windowraise',window);xd('windowfocus',window);wait('title');break
  except subprocess.CalledProcessError:time.sleep(.2)
 assert window;activate();time.sleep(1);click(360,1060);wait('board')
 for level in range(len(results),len(solutions)):
  target=solutions[level]
  s=wait('board');assert s['Save']['Profile']['Selected']==level
  if cc:
   w=target['Width'];h=target['Height'];pos=lambda x,y:(38+(x+.5)*644/w,310+(y+.5)*644/h)
   def choose(node_id):
    current=next(v for v in state()['Save']['Board']['Nodes'] if v['Id']==node_id);click(*pos(current['X'],current['Y']));until(lambda d:d['Selected']==node_id);time.sleep(.12)
   def move(node_id,x,y):
    choose(node_id);click(*pos(x,y));until(lambda d:any(v['Id']==node_id and v['X']==x and v['Y']==y for v in d['Save']['Board']['Nodes']));time.sleep(.12)
   for n in target['Nodes']:
    b=state()['Save']['Board'];node=next(v for v in b['Nodes'] if v['Id']==n['Id'])
    if (node['X'],node['Y'])!=(n['X'],n['Y']):
     blocker=next((v for v in b['Nodes'] if (v['X'],v['Y'])==(n['X'],n['Y'])),None)
     if blocker:
      occupied={(v['X'],v['Y']) for v in b['Nodes']};empty=next((x,y) for y in range(h) for x in range(w) if (x,y) not in occupied and not (b['Blocked']&(1<<(y*w+x))))
      move(blocker['Id'],*empty)
     move(n['Id'],n['X'],n['Y'])
    choose(n['Id']);rotation=next(v['Rotation'] for v in state()['Save']['Board']['Nodes'] if v['Id']==n['Id'])
    for _ in range((n['Rotation']-rotation)%4):
     before=next(v['Rotation'] for v in state()['Save']['Board']['Nodes'] if v['Id']==n['Id']);click(222,1198);until(lambda d:next(v['Rotation'] for v in d['Save']['Board']['Nodes'] if v['Id']==n['Id'])==(before+1)%4);time.sleep(.12)
   board=state()['Save']['Board'];assert sorted(board['Nodes'],key=lambda n:n['Id'])==sorted(target['Nodes'],key=lambda n:n['Id']),(level,board,target)
   if level%12==11:capture('district-'+str(level//12+1)+'-network')
   click(580,1198);click(580,1198);wait('result');time.sleep(.8);profile=state()['Save']['Profile'];assert profile['Stars'][level]==3,(level,profile['Best'][level]);score=profile['Best'][level]
  else:
   w,h=target['Width'],target['Height'];cell=min(616/w,690/h);left=(720-cell*w)/2;top=260+(690-cell*h)/2;page=0
   for item in sorted(target['Items'],key=lambda i:i['Slot']):
    slot=item['Slot']
    existing=next((v for v in state()['Save']['Run']['Items'] if v['Slot']==slot),None)
    if existing is not None:
     assert all(existing[k]==item[k] for k in ['X','Y','Rotation']);continue
    while page<slot//2:click(638,1028);page+=1
    sx=155+(slot%2)*273;click(sx,1028)
    for _ in range(item['Rotation']):click(120,1205)
    drag(sx,1028,left+(item['X']+.5)*cell,top+(item['Y']+.5)*cell);click(322,1205)
    until(lambda d:any(i['Slot']==slot for i in d['Save']['Run']['Items']))
   if level%12==11:capture('venue-'+str(level//12+1)+'-packed')
   click(580,1205);click(360,955);wait('result');time.sleep(.8);profile=state()['Save']['Profile'];assert profile['Stars'][level]==3,(level,profile['Best'][level],target);score=profile['Best'][level];assert score==target['Par']
  if level%12==10:capture('complex-result-'+str(level//12+1))
  results.append({'level':level,'stars':3,'score':score});print(p.name,level+1,score,flush=True)
  (out/'progress.json').write_text(json.dumps({'levels':results,'segments':segments,'actions':actions,'source':{str(f.relative_to(p)):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Assets/_Game').rglob('*.cs')},'build':{str(f.relative_to(p/'Builds/Linux')):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Builds/Linux').rglob('*') if f.is_file()}}))
  if level%12==11:capture('result-'+str(level//12+1))
  if level<71:click(360,922 if cc else 1100);wait('board')
 capture('72-complete');assert sum(state()['Save']['Profile']['Stars'])==216
 proc.terminate();proc.wait(timeout=15)
 issues=[i for f in out.glob('text-overflows*.json') for i in json.loads(f.read_text())['Issues']];assert not issues,issues
 report={'passed':True,'levels':results,'actions':actions,'previous_segments':segments,'source':{str(f.relative_to(p)):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Assets/_Game').rglob('*.cs')},'build':{str(f.relative_to(p/'Builds/Linux')):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Builds/Linux').rglob('*') if f.is_file()}}
 (out/'report.json').write_text(json.dumps(report,indent=2));(p/'Artifacts/latest-campaign-replay.txt').write_text(str(out));print(out)
finally:
 if proc.poll() is None:proc.terminate();proc.wait(timeout=15)
