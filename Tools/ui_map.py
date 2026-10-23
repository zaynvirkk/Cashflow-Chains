"""Map authored UI coordinates to actual presented controls, including wide layouts."""
import json,time

def wait_capture(out,before,timeout=5):
 """Wait for one requested asynchronous PNG to finish; never repeat input."""
 deadline=time.monotonic()+timeout
 while time.monotonic()<deadline:
  for file in set(out.glob('cashflow-*.png'))-before:
   if file.read_bytes().endswith(b'IEND\xaeB`\x82'):return file
  time.sleep(.05)
 raise AssertionError('The requested native screenshot did not complete')

def archive_review(out):
 """Keep every player's audit before a cold restart overwrites the latest files."""
 import shutil
 index=len(list(out.glob('session-*-text-overflows.json')))
 for name in ('text-overflows.json','performance.json','actual-mix.wav'):
  source=out/name
  if source.exists():shutil.copyfile(source,out/f'session-{index}-{name}')

def review_issues(out):
 files=list(out.glob('session-*-text-overflows.json'))
 latest=out/'text-overflows.json'
 if latest.exists():files.append(latest)
 if not files:raise FileNotFoundError('No runtime readability audit was produced')
 return [issue for file in files for issue in json.loads(file.read_text())['Issues']]

def map_point(layout,x,y,grid=False):
 candidates=[]
 for e in layout.get('Elements',[]):
  r=e['Source'];w,h=r['width'],r['height']
  if w<=0 or h<=0 or not(r['x']<=x<=r['x']+w and r['y']<=y<=r['y']+h):continue
  if grid and e['Kind']!='grid':continue
  candidates.append((e['Priority'],-w*h,e))
 if candidates:
  e=max(candidates,key=lambda a:a[:2])[2];r=e['Source'];a=e['Pixels'];return (round(a['x']+(x-r['x'])/r['width']*a['width']),round(a['y']+(y-r['y'])/r['height']*a['height']))
 return round(x*layout.get('Width',540)/720),round(y*layout.get('Height',960)/1280)

def local_point(out,x,y,grid=False):
 for _ in range(25):
  try:
   layout=json.loads((out/'ui-layout.json').read_text());state=json.loads((out/'campaign-state.json').read_text())
   if layout.get('Screen')!=state.get('Screen') or (out/'ui-layout.json').stat().st_mtime_ns<(out/'campaign-state.json').stat().st_mtime_ns:time.sleep(.04);continue
   return map_point(layout,x,y,grid)
  except (FileNotFoundError,json.JSONDecodeError):time.sleep(.04)
 raise AssertionError('Presented UI geometry is missing')
