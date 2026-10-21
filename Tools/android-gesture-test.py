#!/usr/bin/env python3
"""Regression for the actual Android edge-Back gesture on the local emulator."""
from ui_map import map_point
from pathlib import Path
from PIL import Image
import subprocess,time,datetime,json,hashlib,shlex
p=Path(__file__).resolve().parent.parent;cc=p.name=='CashflowChains';pkg='com.virkgames.'+p.name.lower();adb='/home/virk/Android/Sdk/platform-tools/adb';apk=p/'Builds/Release'/(p.name+'-1.0.0.apk');out=p/'Artifacts/AndroidGesture'/datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ');out.mkdir(parents=True);remote='/sdcard/Android/data/'+pkg+'/files/Gesture-'+out.name

def cmd(*v):return subprocess.check_output([adb,*v],text=True,stderr=subprocess.STDOUT,timeout=60).strip()
def shell(v):return cmd('shell','-n',v)
def state():return json.loads(shell('cat '+remote+'/campaign-state.json'))
def wait(screen):
 for _ in range(60):
  try:
   s=state()
   if s['Screen']==screen:return s
  except (subprocess.CalledProcessError,json.JSONDecodeError):pass
  time.sleep(.5)
 raise AssertionError('Gesture failed to reach '+screen)
def capture(name):
 time.sleep(.6)
 with open(out/(name+'.png'),'wb') as f:subprocess.run([adb,'exec-out','screencap','-p'],stdout=f,check=True,timeout=30)
 (out/(name+'.json')).write_text(json.dumps(state(),indent=2))
assert cmd('get-serialno').startswith('emulator-'),'Use the local emulator for this regression'
mode=shell('cmd overlay list');assert '[x] com.android.internal.systemui.navbar.gestural' in mode
activity=shell('cmd package resolve-activity --brief '+pkg).splitlines()[-1]
shell('am force-stop '+pkg);shell('am start -n '+activity+' --es unity '+shlex.quote('-capture-dir '+remote+' -save-dir '+remote+'/Saves'));wait('title');time.sleep(2);capture('01-title')
# The state probe can precede Android's first presented frame after a cold start.
# Wait for the actual title pixels; retain the final capture used for calibration.
for attempt in range(20):
 im=Image.open(out/'01-title.png').convert('RGB');w,h=im.size;scale=w/720;want=(255,211,42) if cc else (218,182,109);ys=[y for y in range(int(h*.6),h) if max(abs(a-b) for a,b in zip(im.getpixel((round(50*scale),y)),want))<13]
 if ys:break
 time.sleep(.5);capture('01-title')
else:raise AssertionError('Android title did not present its primary button')
layout=json.loads(shell('cat '+remote+'/ui-layout.json'));px,py=map_point(layout,360,1060)
shell(f'input tap {px} {py}');wait('board');capture('02-before-gesture');wallet=state()['Save']['Profile']['Credits']
# Immersive Android hides navigation and disables Back until a bottom-edge reveal.
shell(f'input touchscreen swipe {w//2} {h-2} {w//2} {h-250} 180');time.sleep(.15)
system=shell('dumpsys activity service com.android.systemui/.SystemUIService');(out/'system-navigation-revealed.txt').write_text(system);assert 'backGestureDisabled=false' in system
shell(f'input touchscreen swipe 8 {h//2} {w//2} {h//2} 220');wait('menu');capture('03-native-gesture-desk')
(out/'back-gesture-logcat.txt').write_text(cmd('logcat','-d','-t','500'))
assert state()['Save']['Profile']['Credits']==wallet
report={'passed':True,'method':'Android system gestural navigation enabled; revealed immersive system navigation with a bottom-edge swipe, then actual left-edge swipe through adb input from x=8 to half-screen over 220 ms. Board -> menu observed, no KEYCODE_BACK substitution.','activity':activity,'api':shell('getprop ro.build.version.sdk'),'page_size':int(shell('getconf PAGE_SIZE')),'apk_sha256':hashlib.sha256(apk.read_bytes()).hexdigest(),'before':'board','after':state()['Screen']}
(out/'report.json').write_text(json.dumps(report,indent=2));(p/'Artifacts/latest-android-gesture.txt').write_text(str(out));shell('input keyevent 3');print(out)
