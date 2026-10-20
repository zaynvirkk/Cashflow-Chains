#!/usr/bin/env python3
"""Drive the actual Linux player with OS pointer/key input on an existing private X display."""
import argparse,json,os,subprocess,time,datetime,shutil,hashlib
from pathlib import Path
p=Path(__file__).resolve().parent.parent
a=argparse.ArgumentParser();a.add_argument('--display',default=':93');opt=a.parse_args()
game=p.name;cc=game=='CashflowChains';stamp=datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ')
out=p/'Artifacts'/'Acceptance'/stamp;out.mkdir(parents=True);env={**os.environ,'DISPLAY':opt.display};actions=[]
exe=p/'Builds/Linux'/(game+'.x86_64')
def xd(*args):return subprocess.check_output(['xdotool',*map(str,args)],env=env,text=True,stderr=subprocess.DEVNULL,timeout=5).strip()
def stop(process):
 if process.poll() is not None:return
 process.terminate()
 try:process.wait(timeout=10)
 except subprocess.TimeoutExpired:process.kill();process.wait(timeout=5)
def start():
 xd('getdisplaygeometry')
 started=time.time()
 process=subprocess.Popen([str(exe),'-screen-fullscreen','0','-screen-width','720','-screen-height','1280','-force-glcore','-save-dir',str(out/'Saves'),'-capture-dir',str(out),'-logFile',str(out/('player-'+str(len(actions))+'.log'))],env=env,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
 deadline=time.time()+45
 while time.time()<deadline:
  try:
   window=xd('search','--onlyvisible','--pid',process.pid).splitlines()[-1];xd('windowfocus',window)
   if (out/'state.json').exists() and (out/'state.json').stat().st_mtime>=started:time.sleep(.5);return process,window
  except subprocess.CalledProcessError:pass
  if process.poll() is not None:raise RuntimeError('Player exited early')
  time.sleep(.2)
 stop(process);raise RuntimeError('Player never became ready')
def state():return json.loads((out/'state.json').read_text())
def click(x,y):
 xd('mousemove','--window',window,x,y,'click','1');actions.append({'click':[x,y]});time.sleep(.22)
def drag(x,y,tx,ty):
 xd('mousemove','--window',window,x,y,'mousedown','1');time.sleep(.2)
 for k in range(1,9):xd('mousemove','--window',window,round(x+(tx-x)*k/8),round(y+(ty-y)*k/8));time.sleep(.04)
 xd('mouseup','1');actions.append({'drag':[x,y,tx,ty]});time.sleep(.3)
def capture(name):
 old=set(out.glob(game.lower().replace('chains','')+'-*.png'))
 xd('windowfocus',window);time.sleep(.15)
 pressed=time.time();xd('keydown','F12');time.sleep(.15);xd('keyup','F12')
 prefix='cashflow' if cc else 'blackbag'
 deadline=time.time()+5
 while time.time()<deadline:
  recent=sorted(out.glob(prefix+'-*.png'),key=lambda x:x.stat().st_mtime)
  if recent and recent[-1].stat().st_mtime>=pressed-.05:break
  time.sleep(.1)
 prefix='cashflow' if cc else 'blackbag';files=sorted(out.glob(prefix+'-*.png'),key=lambda x:x.stat().st_mtime)
 assert files and files[-1].stat().st_mtime>=pressed-.05,'No fresh real player capture produced'
 shutil.copyfile(files[-1],out/(name+'.png'));(out/(name+'.json')).write_text(json.dumps(state(),indent=2))
process,window=start()
try:
 capture('01-first-run')
 if cc:
  drag(435,577,285,577);assert state()['Board']['Nodes'][1]['X']==1
  click(576,1107);capture('02-preview')
  click(135,577);click(144,1107);click(576,1107);capture('03-rotated')
  assert state()['Board']['Nodes'][0]['Rotation']==1
  click(360,1107);assert state()['Board']['Nodes'][0]['Rotation']==0
  click(360,1208);time.sleep(5);capture('04-result')
  first=state();assert first['Wallet']==500 and first['Resolved'] and first['Cleared']==[0]
 else:
  # Free cash preview, deliberate Take, free move/rotation, two more selected objects, safe extraction.
  click(202,335);capture('02-preview');assert state()['Alert']==1 and not state()['Items'][0]['Secured']
  click(357,1130);assert state()['Alert']==2 and state()['Items'][0]['Secured']
  click(142,1130);assert state()['Alert']==2 and state()['Items'][0]['Rotation']==1
  click(520,929);click(307,335);click(357,1130);assert state()['Alert']==3
  # Vase is a true L footprint; shelf drag must finish without auto-committing.
  drag(193,1025,307,440);capture('03-vase-preview');assert state()['Alert']==3
  assert state()['PreviewItem']==2
  click(357,1130);assert state()['Alert']==4
  capture('04-packed');click(575,1130);capture('05-result')
  first=state();assert first['Ended'] and first['Result']['Haul']==6320 and first['Wallet']==6320
  assert len(first['Result']['ItemIds'])==3
 stop(process);time.sleep(.3)
 process,window=start();capture('06-cold-resume');resumed=state()
 if cc:
  assert resumed['Wallet']==first['Wallet'] and resumed['ResultId']==first['ResultId'] and resumed['Board']==first['Board']
  # Result surface twice cannot duplicate first clear.
  click(360,1208);capture('07-result-again');assert state()['Wallet']==500
 else:
  assert resumed['Wallet']==first['Wallet'] and resumed['Result']==first['Result'] and resumed['Items']==first['Items']
  assert resumed['Alert']==first['Alert']
 for width,height,name in [(720,1600,'08-tall-portrait'),(720,1024,'09-short-portrait')]:
  xd('windowsize',window,width,height);time.sleep(.8);capture(name)
 report={'game':game,'status':'PASS','scope':'desktop OS input / actual rendered player captures / cold resume','actions':actions,'sourceHashes':{},'executableSha256':hashlib.sha256(exe.read_bytes()).hexdigest(),'physicalPhone':'NOT_TESTED','humanEnjoyment':'NOT_TESTED'}
 for f in sorted((p/'Assets/_Game').rglob('*.cs')):report['sourceHashes'][str(f.relative_to(p))]=hashlib.sha256(f.read_bytes()).hexdigest()
 (out/'report.json').write_text(json.dumps(report,indent=2));(p/'Artifacts/latest-native-review.txt').write_text(str(out.relative_to(p))+'\n');print(out);print('PASS: real pointer input, extraction/result and cold-resume ledger')
finally:
 stop(process)
