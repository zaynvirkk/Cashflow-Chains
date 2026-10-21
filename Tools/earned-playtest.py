#!/usr/bin/env python3
"""Actual native player input replay. Private X display; no gameplay state injection."""
import argparse,json,subprocess,time,os,datetime,shutil,hashlib,wave,math,struct
from pathlib import Path
p=Path(__file__).resolve().parent.parent
opt=argparse.ArgumentParser();opt.add_argument('--display',default=':1');opt.add_argument('--campaign',required=True);args=opt.parse_args();env={**os.environ,'DISPLAY':args.display};cc=p.name=='CashflowChains'
out=p/'Artifacts/EarnedPlaytest'/datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ');out.mkdir(parents=True)
actions=[];proc=None;window=None

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
def click(x,y):xd('mousemove','--window',window,round(x*.75),round(y*.75));time.sleep(.08);xd('mousedown','1');time.sleep(.08);xd('mouseup','1');actions.append({'click':[x,y]});time.sleep(.25)
def key(name):xd('keydown',name);time.sleep(.08);xd('keyup',name);actions.append({'key':name});time.sleep(.25)
def drag(x,y,tx,ty):
 xd('mousemove','--window',window,round(x*.75),round(y*.75),'mousedown','1');time.sleep(.15)
 for i in range(1,9):xd('mousemove','--window',window,round((x+(tx-x)*i/8)*.75),round((y+(ty-y)*i/8)*.75));time.sleep(.025)
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
   if (out/'campaign-state.json').exists() and (out/'campaign-state.json').stat().st_mtime>=before:return
  except subprocess.CalledProcessError:pass
  time.sleep(.2)
 raise RuntimeError('Player timeout')
def stop():
 if proc is not None and proc.poll() is None:
  proc.terminate()
  try:proc.wait(timeout=12)
  except subprocess.TimeoutExpired:proc.kill();proc.wait(timeout=5)

campaign=Path(args.campaign);proof=json.loads((campaign/'report.json').read_text());assert proof['passed'] and len(proof['levels'])==72
shutil.copytree(campaign/'Saves',out/'Saves')
input_saves={f.name:hashlib.sha256(f.read_bytes()).hexdigest() for f in (out/'Saves').iterdir() if f.is_file()}
try:
 start();activate();time.sleep(2);click(360,1060);time.sleep(1)
 if not cc and state()['Screen']=='result':click(360,1100)
 wait_state('board');profile=state()['Save']['Profile'];assert sum(profile['Stars'])==216;wallet=profile['Credits'];cap('01-earned-campaign')
 if cc:
  desk();click(360,914);wait_state('themes')
  for theme in range(6):click(195+theme%2*330,496+theme//2*285);assert state()['Save']['Profile']['Theme']==theme
  cap('02-all-palettes-earned');desk();click(360,710);wait_state('blueprints');click(450,1076);assert state()['Save']['Blueprints'][7]['Challenge']==71;cap('03-eighth-blueprint');click(595,1076);wait_state('board');desk()
  click(360,812);wait_state('contracts');click(625,1075);click(625,1075);cap('04-final-commissions');click(600,908);assert any(c['Template']==14 for c in state()['Save']['Contracts']);cap('05-fifteenth-route');click(360,1195);wait_state('achievements')
  for page in range(4):cap('06-achievements-'+str(page+1));click(620,1160)
 else:
  for bag in range(5):
   desk();click(360,812);wait_state('bags');click(600,320+bag*145);assert state()['Save']['Run']['Bag']==bag
  def tools():desk();click(360,812);wait_state('bags');click(360,1100);wait_state('tools')
  def tool(i):tools();click(600,317+i*143);wait_state('board')
  tool(4);assert state()['Save']['Run']['ToolMask']&16
  tool(1);assert state()['Save']['Run']['Opened']==1
  tool(2);assert state()['Save']['Run']['Alert']==1
  tool(2);assert state()['Save']['Run']['Alert']==0 and state()['Save']['Run']['ExtraToolUsed'];cap('02-courier-repeat')
  witness=json.loads((p/'Artifacts/Campaign/witnesses.jsonl').read_text().splitlines()[71]);cell=690/8;left=(720-cell*7)/2;top=260
  for item in witness['Items'][:2]:
   slot=item['Slot']
   if slot==2:click(638,1028)
   sx=155+slot%2*273;click(sx,1028);drag(sx,1028,left+(item['X']+.5)*cell,top+(item['Y']+.5)*cell);click(322,1205);assert any(v['Slot']==slot for v in state()['Save']['Run']['Items'])
  item=witness['Items'][0];click(left+(item['X']+.5)*cell,top+(item['Y']+.5)*cell);assert state()['Selected']==item['Slot'];tool(0);assert any(v['Slot']==item['Slot'] and v['Foamed'] for v in state()['Save']['Run']['Items']);cap('03-foam-protected')
  tools();click(600,746);cap('04-pivot-picker');click(360,410);wait_state('board');tool(5);assert state()['Save']['Run']['Seal'];cap('05-secure-seal');tools();cap('06-earned-tool-roll');desk()
  click(360,914);wait_state('hideout');cap('07-earned-hideout');click(530,1125);wait_state('themes')
  for theme in range(6):click(195+theme%2*330,496+theme//2*285);assert state()['Save']['Profile']['Theme']==theme
  cap('08-all-styles-earned')
 assert state()['Save']['Profile']['Credits']==wallet,'Earned equip/tool action charged the wallet';assert sum(state()['Save']['Profile']['Stars'])==216
 time.sleep(2);stop();issues=json.loads((out/'text-overflows.json').read_text())['Issues'];assert not issues,issues
 report={'passed':True,'earned_from':str(campaign),'input_save_sha256':input_saves,'method':'Copied actual 72-level earned saves byte for byte; no profile or economy edits. All actions use native pointer and keyboard input.','actions':actions,'source':{str(f.relative_to(p)):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Assets/_Game').rglob('*.cs')},'wallet_unchanged':wallet,'text_overflows':issues,'final_profile':state()['Save']['Profile']}
 (out/'report.json').write_text(json.dumps(report,indent=2));(p/'Artifacts/latest-earned-playtest.txt').write_text(str(out));print(out)
except Exception:
 try:cap('interrupted')
 except Exception:pass
 raise
finally:stop()
