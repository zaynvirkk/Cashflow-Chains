#!/usr/bin/env python3
"""Regression for the actual Android edge-Back gesture on the local emulator."""
from pathlib import Path
from PIL import Image
import subprocess,time,datetime,json,hashlib,shlex
p=Path(__file__).resolve().parent.parent;cc=p.name=='CashflowChains';pkg='com.virkgames.'+p.name.lower();adb='/home/virk/Android/Sdk/platform-tools/adb';apk=p/'Builds/Android'/(p.name+'-Playtest-1.0.apk');out=p/'Artifacts/AndroidGesture'/datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ');out.mkdir(parents=True);remote='/sdcard/Android/data/'+pkg+'/files/Gesture-'+out.name

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
im=Image.open(out/'01-title.png').convert('RGB');w,h=im.size;scale=w/720;want=(98,227,187) if cc else (218,182,109);ys=[y for y in range(int(h*.6),h) if max(abs(a-b) for a,b in zip(im.getpixel((round(50*scale),y)),want))<13];assert ys;top=(min(ys)+max(ys))/2-1066*scale
shell(f'input tap {round(360*scale)} {round(top+1060*scale)}');wait('board');capture('02-before-gesture');wallet=state()['Save']['Profile']['Credits']
# Immersive Android hides navigation and disables Back until a bottom-edge reveal.
shell(f'input touchscreen swipe {w//2} {h-2} {w//2} {h-250} 180');time.sleep(.15)
system=shell('dumpsys activity service com.android.systemui/.SystemUIService');(out/'system-navigation-revealed.txt').write_text(system);assert 'backGestureDisabled=false' in system
shell(f'input touchscreen swipe 8 {h//2} {w//2} {h//2} 220');wait('menu');capture('03-native-gesture-desk')
(out/'back-gesture-logcat.txt').write_text(cmd('logcat','-d','-t','500'))
assert state()['Save']['Profile']['Credits']==wallet
report={'passed':True,'method':'Android system gestural navigation enabled; revealed immersive system navigation with a bottom-edge swipe, then actual left-edge swipe through adb input from x=8 to half-screen over 220 ms. Board -> menu observed, no KEYCODE_BACK substitution.','activity':activity,'apk_sha256':hashlib.sha256(apk.read_bytes()).hexdigest(),'before':'board','after':state()['Screen']}
(out/'report.json').write_text(json.dumps(report,indent=2));(p/'Artifacts/latest-android-gesture.txt').write_text(str(out));shell('input keyevent 3');print(out)
