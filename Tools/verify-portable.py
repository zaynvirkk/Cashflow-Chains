#!/usr/bin/env python3
"""Launch the delivered archive from a fresh directory without altering user saves."""
from pathlib import Path
from PIL import ImageGrab
import tempfile,subprocess,time,json,hashlib,zipfile,os,datetime
p=Path(__file__).resolve().parent.parent
archive=p/'Builds/Deliverables'/(p.name+'-Linux-Playtest-1.0.zip')
out=p/'Artifacts/Portable'/datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ');out.mkdir(parents=True)
with tempfile.TemporaryDirectory(prefix=p.name+'-portable-') as temp:
 with zipfile.ZipFile(archive) as z:
  assert z.testzip() is None
  z.extractall(temp)
  for f in z.infolist():
   path=Path(temp)/f.filename
   if path.is_file():path.chmod((f.external_attr>>16)&0o777 or 0o644)
 player=Path(temp)/p.name/'Play.sh'
 proc=subprocess.Popen([str(player),'-force-glcore','-save-dir',str(out/'Saves'),'-capture-dir',str(out),'-logFile',str(out/'player.log')],env={**os.environ,'DISPLAY':':93'},stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
 try:
  for _ in range(160):
   assert proc.poll() is None,'Extracted player exited'
   f=out/'performance.json'
   if f.exists():
    state=json.loads((out/'campaign-state.json').read_text());perf=json.loads(f.read_text())
    if state['Screen']=='title' and perf['Frames']>=600:break
   time.sleep(.2)
  else:raise AssertionError('Extracted player failed to advance 600 frames')
  ImageGrab.grab(xdisplay=':93').save(out/'actual-desktop.png')
 finally:
  proc.terminate();proc.wait(timeout=15)
 errors=[s for s in (out/'player.log').read_text().splitlines() if any(t in s for t in ['NullReferenceException','MissingReferenceException','DllNotFoundException','Shader error','Failed to load'])];assert not errors,errors
 assert not json.loads((out/'text-overflows.json').read_text())['Issues']
 report={'passed':True,'archive_sha256':hashlib.sha256(archive.read_bytes()).hexdigest(),'method':'Fresh archive extraction, preserved executable modes, ordinary Play.sh launch, isolated save, actual X11 pixels and 600+ advancing frames. No pointer input or profile edits.','performance':perf,'screen':state['Screen'],'errors':errors}
 (out/'report.json').write_text(json.dumps(report,indent=2));(p/'Artifacts/latest-portable-review.txt').write_text(str(out));print(out)
