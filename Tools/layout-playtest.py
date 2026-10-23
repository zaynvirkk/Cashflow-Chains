#!/usr/bin/env python3
"""Actual native screenshots and control geometry at portrait, wide and F11 fullscreen sizes."""
from pathlib import Path
from ui_map import local_point,wait_capture
import subprocess,os,time,json,datetime,hashlib,shutil
p=Path(__file__).resolve().parent.parent;env={**os.environ,'DISPLAY':os.environ.get('CASHFLOW_TEST_DISPLAY',':1')};out=p/'Artifacts/LayoutReview'/datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ');out.mkdir(parents=True);window=None;checks=[]
proc=subprocess.Popen([str(p/'Play.sh'),'-screen-fullscreen','0','-screen-width','1280','-screen-height','720','-save-dir',str(out/'Saves'),'-capture-dir',str(out),'-logFile',str(out/'player.log')],env=env,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
def xd(*a):return subprocess.check_output(['xdotool',*map(str,a)],env=env,text=True,stderr=subprocess.DEVNULL).strip()
def snapshot():return json.loads((out/'ui-layout.json').read_text())
def state():return json.loads((out/'campaign-state.json').read_text())
def focus():
 global window
 window=xd('search','--onlyvisible','--pid',proc.pid).splitlines()[-1];xd('windowactivate','--sync',window)
def click(x,y):focus();xd('mousemove','--window',window,*local_point(out,x,y),'mousedown','1');time.sleep(.08);xd('mouseup','1');time.sleep(.65)
def key(k):focus();xd('keydown',k);time.sleep(.09);xd('keyup',k);time.sleep(.8)
def audit(label):
 time.sleep(1);layout=snapshot();assert layout['Screen']=='board';w,h=layout['Width'],layout['Height'];buttons=[e for e in layout['Elements'] if e['Kind']=='button'];grid=next(e for e in layout['Elements'] if e['Kind']=='grid');g=grid['Pixels'];save=state()['Save'];cols=save['Board']['Width'] if p.name=='CashflowChains' else 4
 if p.name=='CashflowChains':rows=save['Board']['Height']
 else:cols,rows=4,5 # actual first certified case
 assert abs(g['width']/cols-g['height']/rows)<1,(label,'Non-square cells',g,cols,rows)
 for e in layout['Elements']:
  r=e['Pixels'];assert r['x']>=-1 and r['y']>=-1 and r['x']+r['width']<=w+1 and r['y']+r['height']<=h+1,(label,'Control outside screen',e)
 for i,a in enumerate(buttons):
  for b in buttons[i+1:]:
   aa,bb=a['Pixels'],b['Pixels'];overlap=min(aa['x']+aa['width'],bb['x']+bb['width'])-max(aa['x'],bb['x']);vertical=min(aa['y']+aa['height'],bb['y']+bb['height'])-max(aa['y'],bb['y']);assert overlap<=1 or vertical<=1,(label,'Buttons overlap',a,b)
 before=set(out.glob('cashflow-*.png'));key('F12');shutil.copyfile(wait_capture(out,before),out/(label+'.png'));checks.append({'view':label,'pixels':[w,h],'buttons':len(buttons),'square_cells':True,'controls_inside_screen':True,'buttons_do_not_overlap':True});print(label,w,h,flush=True)
try:
 for _ in range(150):
  if (out/'ui-layout.json').exists():break
  assert proc.poll() is None;time.sleep(.2)
 initial_wallet=state()['Save']['Profile']['Credits'];focus();xd('windowmove',window,50 if env['DISPLAY']!=':1' else 2100,60);time.sleep(1)
 if state()['Screen']=='title':click(360,1060)
 assert state()['Screen']=='board';audit('wide-board')
 xd('windowsize',window,540,960);audit('portrait-board');xd('windowsize',window,720,992);audit('short-board');xd('windowsize',window,1280,720);time.sleep(.8);focus();key('F11');focus();audit('fullscreen-board');assert snapshot()['Width']>=1280 and snapshot()['Height']>=720
 key('F11');focus();audit('window-restored');assert not any(state()['Save']['Profile']['Stars']) and state()['Save']['Profile']['Credits']==initial_wallet
finally:
 proc.terminate();proc.wait(timeout=15)
issues=json.loads((out/'text-overflows.json').read_text())['Issues'];assert not issues,issues
errors=[l for l in (out/'player.log').read_text().splitlines() if any(x in l for x in ['NullReferenceException','MissingReferenceException','error CS'])];assert not errors,errors
report={'passed':True,'method':'Actual native pointer controls, window resizing and F11, screenshots plus read-only presented-control geometry. No state edits.','checks':checks,'source':{str(f.relative_to(p)):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Assets/_Game').rglob('*.cs')}};(out/'report.json').write_text(json.dumps(report,indent=2));(p/'Artifacts/latest-layout-review.txt').write_text(str(out));print(out)
