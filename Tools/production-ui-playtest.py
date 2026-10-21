from pathlib import Path
from ui_map import local_point
import subprocess,os,time,json,datetime,hashlib,shutil
p=Path(__file__).resolve().parent.parent;env={**os.environ,'DISPLAY':':1'};out=p/'Artifacts/ProductionUI'/datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ');out.mkdir(parents=True);earned=Path((p/'Artifacts/latest-first-session.txt').read_text().strip());shutil.copytree(earned/'Saves',out/'Saves');checks=[]
proc=subprocess.Popen([str(p/'Play.sh'),'-screen-fullscreen','0','-screen-width','540','-screen-height','960','-save-dir',str(out/'Saves'),'-capture-dir',str(out),'-logFile',str(out/'player.log')],env=env,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
def xd(*a):return subprocess.check_output(['xdotool',*map(str,a)],env=env,text=True,stderr=subprocess.DEVNULL,timeout=10).strip()
def state():return json.loads((out/'campaign-state.json').read_text())
def layout():return json.loads((out/'ui-layout.json').read_text())
def focus():
 if xd('getactivewindow')!=window:xd('windowactivate','--sync',window)
def wait(screen):
 for _ in range(150):
  try:
   if state()['Screen']==screen:return
  except (FileNotFoundError,json.JSONDecodeError):pass
  time.sleep(.1)
 raise AssertionError('Missing '+screen)
def click(x,y):focus();xd('mousemove','--window',window,x,y,'mousedown','1');time.sleep(.1);xd('mouseup','1');time.sleep(.5)
def named(text,index=0):
 time.sleep(.25);entries=[e for e in layout()['Elements'] if e['Kind']=='button' and e['Name']==text];assert entries,(text,[e['Name'] for e in layout()['Elements'] if e['Kind']=='button']);priority=max(e['Priority'] for e in entries);entries=[e for e in entries if e['Priority']==priority];r=entries[index]['Pixels'];click(round(r['x']+r['width']/2),round(r['y']+r['height']/2))
def key(k):focus();xd('keydown',k);time.sleep(.1);xd('keyup',k);time.sleep(.5)
def cap(name):
 before=set(out.glob('cashflow-*.png'));key('F12');time.sleep(.3);files=set(out.glob('cashflow-*.png'))-before;assert files;shutil.copyfile(next(iter(files)),out/(name+'.png'))
def desk():
 for _ in range(5):
  if state()['Screen']=='menu':return
  key('Escape')
 raise AssertionError('Menu unavailable')
try:
 for _ in range(150):
  try:window=xd('search','--onlyvisible','--pid',proc.pid).splitlines()[-1];wait('title');break
  except subprocess.CalledProcessError:time.sleep(.2)
 focus();xd('windowmove',window,2100,45);time.sleep(1);named('CONTINUE BUILDING  →');wait('board');wallet=state()['Save']['Profile']['Credits'];key('Escape');wait('menu');cap('01-city-desk')
 named('DISTRICTS & PROGRESS');wait('hub');cap('02-eight-district-atlas');named('STARTER TOWN\n3 / 12 CITIES');wait('map');cap('03-challenge-select');desk()
 named('COLLECTIONS');wait('collections');cap('04-real-building-collection');named('›');cap('05-advanced-collections');desk()
 named('COMBOS');wait('combos');cap('06-combo-discoveries');named('SEE HOW IT WORKS  →',0);cap('07-explained-combo');key('Escape');wait('combos')
 for page in range(3):named('›');cap('08-combos-page-'+str(page+2))
 desk();named('STORE');wait('store');cap('09-offline-store');named('DETAILS  →',0);cap('10-starter-eligibility');key('Escape');wait('store');named('›');cap('11-convenience-catalog');named('›');cap('12-cosmetics-expansions');named('DETAILS  →',0);cap('13-neon-details');key('Escape');named('RESTORE PURCHASES');cap('14-restore-offline');key('Escape');wait('store');named('DECORATIONS');wait('decorations');cap('15-cosmetic-finishes');assert state()['Save']['Profile']['Credits']==wallet and not state()['Save']['Profile']['Purchases'];checks.append('Unconfigured shop has no fabricated price or purchase success and cannot change money')
 desk();named('BUILDING ALMANAC');wait('library')
 for page in range(6):cap('16-almanac-'+str(page+1));named('›')
 desk();named('BLUEPRINTS');wait('blueprints');cap('17-earned-blueprints');desk();named('TODAY’S CITY');cap('18-daily-and-draft');named('DRAFT PRACTICE · ONE FREE REROLL');wait('board');assert state()['Save']['Board']['DraftMode'];cap('19-draft-practice');r=local_point(out,630,1030);click(*r);cap('20-draft-tray');named('REROLL\nFREE');wait('board');assert state()['Save']['Board']['DraftRoll']==1 and state()['Save']['Profile']['Credits']==wallet;cap('21-free-draft-reroll');checks.append('One free optional draft reroll changes tray state without spending coins or granting premium money')
 desk();named('DISTRICTS & PROGRESS');wait('hub');xd('windowsize',window,1280,720);time.sleep(1);cap('22-wide-atlas');xd('windowsize',window,720,992);time.sleep(1);cap('23-short-atlas');xd('windowsize',window,540,960);time.sleep(1);desk();named('SETTINGS & HELP');wait('settings');cap('24-settings');time.sleep(27)
finally:
 proc.terminate();proc.wait(timeout=15)
issues=json.loads((out/'text-overflows.json').read_text())['Issues'];assert not issues,issues
errors=[l for l in (out/'player.log').read_text().splitlines() if any(x in l for x in ['NullReferenceException','MissingReferenceException','ArgumentException','error CS'])];assert not errors,errors
report={'passed':True,'checks':checks,'method':'Ordinary native input; profile copied byte-for-byte from the current legitimately earned three-city replay, never edited. No purchase mock in the player.','source':{str(f.relative_to(p)):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Assets/_Game').rglob('*.cs')},'build':{str(f.relative_to(p/'Builds/Linux')):hashlib.sha256(f.read_bytes()).hexdigest() for f in (p/'Builds/Linux').rglob('*') if f.is_file()}};(out/'report.json').write_text(json.dumps(report,indent=2));(p/'Artifacts/latest-production-ui.txt').write_text(str(out));print(out)
