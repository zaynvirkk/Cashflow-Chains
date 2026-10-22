#!/usr/bin/env python3
"""Install and interact with the real ARM64 release APK on a connected Android test device."""
from ui_map import map_point
from pathlib import Path
import subprocess,time,datetime,json,hashlib,math,struct,wave,shlex,re,xml.etree.ElementTree as ET
from PIL import Image
p=Path(__file__).resolve().parent.parent;cc=p.name=='CashflowChains';package='com.virkgames.'+p.name.lower();apk=p/'Builds/Release'/(p.name+'-1.0.0.apk');adb='/home/virk/Android/Sdk/platform-tools/adb';stamp=datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ');out=p/'Artifacts/Android'/stamp;out.mkdir(parents=True);remote='/sdcard/Android/data/'+package+'/files/Review-'+stamp;actions=[]
def command(*args,timeout=60):return subprocess.check_output([adb,*map(str,args)],text=True,stderr=subprocess.STDOUT,timeout=timeout).strip()
def shell(s):return command('shell','-n',s)
def state():return json.loads(shell('cat '+remote+'/campaign-state.json'))
def wait(name=None):
 for _ in range(120):
  try:
   s=state()
   if name is None or s['Screen']==name:return s
  except (json.JSONDecodeError,subprocess.CalledProcessError):pass
  time.sleep(1)
 raise AssertionError('Android state not reached: '+str(name))
def capture(name):
 time.sleep(.9)
 with open(out/(name+'.png'),'wb') as f:subprocess.run([adb,'exec-out','screencap','-p'],stdout=f,check=True,timeout=30)
 try:(out/(name+'.json')).write_text(json.dumps(state(),indent=2))
 except Exception:pass
 return out/(name+'.png')
def detect_layout(file):
 layout=json.loads(shell('cat '+remote+'/ui-layout.json'));assert layout['Elements'];return layout['Width']/720,0
def point(x,y):return map_point(json.loads(shell('cat '+remote+'/ui-layout.json')),x,y)
scale=1;top=0
def tap(x,y):shell('input tap '+str(point(x,y)[0])+' '+str(point(x,y)[1]));actions.append({'tap':[x,y]});time.sleep(.4)
def drag(x,y,tx,ty):shell('input swipe '+' '.join(str(round(v)) for v in [*point(x,y),*point(tx,ty)])+' 450');actions.append({'drag':[x,y,tx,ty]});time.sleep(.5)
def back():shell('input keyevent 4');actions.append({'key':'Android Back'});time.sleep(.7)
def launch():
 shell('am force-stop '+package);arguments='-capture-dir '+remote+' -save-dir '+remote+'/Saves';shell('am start -n '+activity+' --es unity '+shlex.quote(arguments));wait('title');time.sleep(2)
 shell('uiautomator dump '+remote+'/system-ui.xml');tree=ET.fromstring(shell('cat '+remote+'/system-ui.xml'))
 if any(n.get('resource-id')=='android:id/immersive_cling_title' for n in tree.iter('node')):
  node=next(n for n in tree.iter('node') if n.get('resource-id')=='android:id/ok');x1,y1,x2,y2=map(int,re.findall(r'\d+',node.get('bounds')));shell('input tap '+str((x1+x2)//2)+' '+str((y1+y2)//2));actions.append({'system':'Dismissed Android full-screen help'});time.sleep(2)
print(command('install','--no-incremental','-r',apk,timeout=180));activity=shell('cmd package resolve-activity --brief '+package).splitlines()[-1];assert '/' in activity,activity
try:
 launch();first=capture('01-title');scale,top=detect_layout(first);tap(360,1060);wait('board');capture('02-first-case')
 if not cc:
  # First shader/material presentation can trail the domain probe on ARM translation.
  # Require the actual wine-coloured Take control before delivering the first drag.
  for attempt in range(20):
   im=Image.open(out/'02-first-case.png').convert('RGB');col=im.getpixel(point(250,1174))
   if max(abs(a-b) for a,b in zip(col,(108,24,37)))<16:break
   capture('02-first-case')
  else:raise AssertionError('Black Bag board did not present its Take control')
 if cc:
  drag(602,712,441,712);assert state()['Save']['Board']['Nodes'][1]['X']==2
  tap(222,1198);assert state()['Save']['Board']['Nodes'][1]['Rotation']==1;tap(84,1198);assert state()['Save']['Board']['Nodes'][1]['Rotation']==0
  tap(375,1198);capture('03-forecast');tap(360,1069);tap(580,1198);wait('result');capture('04-three-stars');assert state()['Save']['Profile']['Stars'][0]==3
 else:
  drag(155,1028,210,368);assert not state()['Save']['Run']['Items'];tap(322,1205);assert len(state()['Save']['Run']['Items'])==1
  tap(428,1028);tap(440,368);tap(322,1205);assert len(state()['Save']['Run']['Items'])==2;alert=state()['Save']['Run']['Alert'];drag(210,368,210,520);assert state()['Save']['Run']['Alert']==alert;capture('03-packed-case')
  tap(580,1205);capture('04-extraction');tap(360,955);wait('result');capture('05-result');assert state()['Save']['Profile']['Stars'][0]>0 and len(state()['Save']['Profile']['Collection'])==2
 before=state()['Save']['Profile'];shell('input keyevent 3');time.sleep(2);shell('cmd connectivity airplane-mode enable');launch();tap(360,1060);after=wait()['Save']['Profile'];assert before['Stars']==after['Stars'] and before['Collection']==after['Collection'] and before['Credits']==after['Credits'];capture('06-offline-cold-resume')
 back();wait('menu');capture('07-native-back-desk');tap(360,1118);capture('13-free-edition-about');wallet=state()['Save']['Profile']['Credits'];layout=json.loads(shell('cat '+remote+'/ui-layout.json'));assert not any(any(word in e['Name'].upper() for word in ['PURCHASE','RESTORE','UNAVAILABLE']) for e in layout['Elements'] if e['Priority']>1000);back();wait('menu');assert state()['Save']['Profile']['Credits']==wallet and not state()['Save']['Profile']['Entitlements'];tap(360,1016);wait('settings');capture('08-settings');tap(360,915);capture('09-help');music=state()['Save']['Profile']['Music'];tap(360,320);assert state()['Save']['Profile']['Music']==music,'Touch leaked through help modal';back();wait('settings')
 tap(360,320);tap(360,658);tap(360,770);profile=state()['Save']['Profile'];assert not profile['Music'] and profile['ReducedMotion'] and profile['HighContrast'];capture('10-accessibility')
 for size,name in [('720x1600','tall'),('720x1280','compact')]:shell('wm size '+size);time.sleep(2);capture('11-'+name)
 shell('wm size reset');time.sleep(2);capture('12-restored-size')
 command('pull',remote+'/actual-mix.wav',out/'actual-mix.wav');
 try:command('pull',remote+'/performance.json',out/'performance.json')
 except subprocess.CalledProcessError:pass
 command('pull',remote+'/text-overflows.json',out/'text-overflows.json');assert not json.loads((out/'text-overflows.json').read_text())['Issues']
 (out/'meminfo.txt').write_text(shell('dumpsys meminfo '+package));(out/'logcat.txt').write_text(command('logcat','-d','-t','2500'))
 errors=[line for line in (out/'logcat.txt').read_text().splitlines() if re.search(r'FATAL EXCEPTION|AndroidRuntime.*(?:Exception|Error)|Fatal signal|NullReferenceException|MissingMethodException|DllNotFoundException',line)];assert not errors,errors
 with wave.open(str(out/'actual-mix.wav')) as f:samples=struct.unpack('<'+'h'*(f.getnframes()*f.getnchannels()),f.readframes(f.getnframes()));audio={'peak':max(abs(x) for x in samples)/32768,'rms':math.sqrt(sum(x*x for x in samples)/len(samples))/32768}
 assert audio['rms']>.001 and audio['peak']<.999,audio
 report={'passed':True,'package':package,'activity':activity,'device':shell('getprop ro.product.cpu.abilist'),'api':shell('getprop ro.build.version.sdk'),'page_size':int(shell('getconf PAGE_SIZE')),'apk_sha256':hashlib.sha256(apk.read_bytes()).hexdigest(),'text_overflow_count':0,'runtime_error_matches':errors,'layout':{'scale':scale,'top':top},'audio':audio,'actions':actions,'screenshots':[f.name for f in out.glob('[0-9][0-9]-*.png')]}
 (out/'report.json').write_text(json.dumps(report,indent=2));(p/'Artifacts/latest-android-review.txt').write_text(str(out));print(out)
finally:
 shell('wm size reset');shell('cmd connectivity airplane-mode disable')
