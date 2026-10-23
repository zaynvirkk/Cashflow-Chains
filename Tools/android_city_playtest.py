#!/usr/bin/env python3
"""Real touch test of the permanent-city ARM64 player. No save or clock injection."""
from pathlib import Path
import subprocess,time,datetime,json,hashlib,shlex,re,wave,struct,math,xml.etree.ElementTree as ET
p=Path(__file__).resolve().parent.parent
adb='/home/virk/Android/Sdk/platform-tools/adb';pkg='com.virkgames.cashflowchains'
apk=p/'Builds/Release/CashflowChains-1.0.0.apk'
out=p/'Artifacts/AndroidCity'/datetime.datetime.now(datetime.timezone.utc).strftime('%Y%m%dT%H%M%SZ');out.mkdir(parents=True)
remote='/sdcard/Android/data/'+pkg+'/files/CityReview-'+out.name
actions=[];audit=[];process_logs=[];audio=[]
source={str(f.relative_to(p)):hashlib.sha256(f.read_bytes()).hexdigest() for f in [*(p/'Assets/_Game').rglob('*.cs'),p/'Assets/_Game/Data/CC2/balance-v1.json',p/'Assets/_Game/Resources/CC2/balance-v1.json',Path(__file__)]}
def cmd(*args,timeout=60):return subprocess.check_output([adb,*map(str,args)],text=True,stderr=subprocess.STDOUT,timeout=timeout).strip()
def shell(s):return cmd('shell','-n',s)
def read(name):
 for _ in range(25):
  try:return json.loads(shell('cat '+remote+'/'+name))
  except json.JSONDecodeError:time.sleep(.04)
 raise AssertionError('Valid read-only Android probe missing: '+name)
def state():return read('campaign-state.json')
def city():return read('empire-state.json')['City']
def layout():return read('ui-layout.json')
def until(test,timeout=60):
 deadline=time.monotonic()+timeout
 while time.monotonic()<deadline:
  try:
   if test():return
  except (subprocess.CalledProcessError,json.JSONDecodeError,KeyError):pass
  time.sleep(.3)
 raise AssertionError('Real touch / city condition timed out')
def tap(name,index=0,kind=None):
 until(lambda:any(e['Name']==name and (kind is None or e['Kind']==kind) for e in layout()['Elements']),15)
 items=[e for e in layout()['Elements'] if e['Name']==name and (kind is None or e['Kind']==kind)]
 assert items,(name,[e['Name'] for e in layout()['Elements']]);priority=max(e['Priority'] for e in items);items=[e for e in items if e['Priority']==priority];r=items[index]['Pixels'];x=round(r['x']+r['width']/2);y=round(r['y']+r['height']/2)
 shell(f'input tap {x} {y}');actions.append({'tap':name,'index':index,'pixels':[x,y]});time.sleep(.4)
def tap_prefix(prefix):
 until(lambda:any(e['Name'].startswith(prefix) and e['Kind']=='button' for e in layout()['Elements']),15)
 tap(next(e['Name'] for e in layout()['Elements'] if e['Name'].startswith(prefix) and e['Kind']=='button'))
def back():shell('input keyevent 4');actions.append({'system':'Android Back'});time.sleep(.5)
def preserve_process_log():
 until(lambda:shell('test -f '+remote+'/actual-mix.wav && echo ready || true')=='ready',60)
 name='session-'+str(len(process_logs))+'-actual-mix.wav';cmd('pull',remote+'/actual-mix.wav',out/name)
 with wave.open(str(out/name)) as f:
  count=f.getnframes();channels=f.getnchannels();rate=f.getframerate();samples=struct.unpack('<'+'h'*(count*channels),f.readframes(count));measurement={'file':name,'seconds':count/rate,'sampleRate':rate,'peak':max(map(abs,samples))/32768,'rms':math.sqrt(sum(x*x for x in samples)/len(samples))/32768,'sha256':hashlib.sha256((out/name).read_bytes()).hexdigest()};assert measurement['rms']>.001 and measurement['peak']<.999,measurement;audio.append(measurement)
 pid=shell('pidof '+pkg).split()[0];value=cmd('logcat','--pid='+pid,'-d','-t','3500');process_logs.append(value);(out/('session-'+str(len(process_logs))+'-logcat.txt')).write_text(value)
def flush_audit():
 old=shell('cat '+remote+'/performance.json 2>/dev/null || true')
 until(lambda:shell('cat '+remote+'/performance.json 2>/dev/null || true') not in ('',old),90)
 stats=read('performance.json');(out/('session-'+str(len(audit))+'-performance.json')).write_text(json.dumps(stats,indent=2))
 value=read('text-overflows.json');audit.append(value);(out/('session-'+str(len(audit))+'-text-overflows.json')).write_text(json.dumps(value,indent=2))
def capture(name):
 until(lambda:bool(read('performance.json')),90)
 time.sleep(.6)
 with open(out/(name+'.png'),'wb') as f:subprocess.run([adb,'exec-out','screencap','-p'],stdout=f,check=True,timeout=30)
 for file in ['campaign-state.json','empire-state.json','ui-layout.json','text-overflows.json','performance.json','traffic-state.json','city-world-state.json']:
  value=read(file);(out/(name+'-'+file)).write_text(json.dumps(value,indent=2))
  if file=='text-overflows.json':audit.append(value)
def launch():
 shell('am force-stop '+pkg)
 # Clear derived capture files only;keep Saves untouched so a cold run cannot reuse an older process audit/audio.
 shell('rm -f '+ ' '.join(remote+'/'+name for name in ['campaign-state.json','empire-state.json','ui-layout.json','text-overflows.json','performance.json','traffic-state.json','actual-mix.wav']))
 shell('am start -n '+activity+' --es unity '+shlex.quote('-capture-dir '+remote+' -save-dir '+remote+'/Saves'))
 until(lambda:state()['Screen']=='city-world');time.sleep(2)
 shell('uiautomator dump '+remote+'/system-ui.xml');tree=ET.fromstring(shell('cat '+remote+'/system-ui.xml'))
 if any(n.get('resource-id')=='android:id/immersive_cling_title' for n in tree.iter('node')):
  n=next(n for n in tree.iter('node') if n.get('resource-id')=='android:id/ok');x1,y1,x2,y2=map(int,re.findall(r'\d+',n.get('bounds')));shell(f'input tap {(x1+x2)//2} {(y1+y2)//2}');actions.append({'system':'Dismiss Android full-screen help'});time.sleep(1)
assert cmd('get-serialno').startswith('emulator-'),'This test is isolated to the local emulator'
print(cmd('install','--no-incremental','-r',apk,timeout=180));activity=shell('cmd package resolve-activity --brief '+pkg).splitlines()[-1]
try:
 shell('wm size 720x1280');launch();assert state()['EmpireSandbox'] is False;assert not city()['Properties'];assert read('city-world-state.json')['Forecast']['Population']==0;capture('01-empty-founding-city')
 tap('BUILD & DEVELOP');capture('01b-illustrated-building-tray');tap('BUILD',0);tap('START CONSTRUCTION');until(lambda:read('city-world-state.json')['Forecast']['Population']==4);capture('01c-paid-first-home')
 tap('Owned plot 2 1',kind='pointer');tap('BUILD',0);capture('02-materials-and-operating-comparison');tap('START CONSTRUCTION');until(lambda:any(b['Kind']=='cafe' and b['Level']==1 for b in city()['Properties']));until(lambda:city()['Day']>=2)
 assert len([r for r in read('empire-state.json')['Economy']['Receipts'] if r['CommandId']=='city-day-settle:1'])==1
 assert city()['LastDayRevenue']>city()['LastDayBills'];capture('03-real-cafe-trading');assert not any('SKIP' in e['Name'] for e in layout()['Elements'] if e['Kind']=='button')
 tap('4 RESIDENTS');capture('03b-real-resident-demographics');tap('INSPECT A HOME');capture('03c-individual-resident-profiles');back();back()
 until(lambda:sum(b['Earned']+b['Purchased'] for b in read('empire-state.json')['Economy']['Balances'] if b['ItemId']=='CASH')>=92,160)
 tap('Owned plot 0 1',kind='pointer');tap('BUILD',0);tap_prefix('FIND ');tap_prefix('BUY ');tap('START CONSTRUCTION');until(lambda:len([b for b in city()['Properties'] if b['Level']>0])==3);until(lambda:read('city-world-state.json')['Forecast']['Population']==6);capture('04-second-different-household')
 tap('CITY LEDGER');until(lambda:state()['Screen']=='city-ledger');capture('05-real-population-bills-and-demand');tap('MY CITY');tap('−');capture('06-zoom-out');tap('FIT CITY')
 tap('CITY DESK');until(lambda:state()['Screen']=='city-desk');capture('07-owned-city-desk');tap('SETTINGS');tap('SAVE RECOVERY');capture('07b-safe-city-recovery');assert not any(e['Name'] in ('NEW LOCAL GAME','ARCHIVE & START OVER') for e in layout()['Elements']);tap('KEEP MY CITY');tap('FINANCING & BUSINESS STRATEGY');tap('COMPARE FINANCING');tap('REVIEW $100 TERMS');capture('08-financing-terms');back();back();back();until(lambda:state()['Screen']=='city-desk');tap('RETURN TO MY CITY')
 for size,name in [('720x1280','09-compact-mobile'),('720x1600','10-tall-mobile')]:
  shell('wm size '+size);time.sleep(2);capture(name)
 shell('wm size reset');time.sleep(2)
 # Actual touch pan on the map. No domain placement changes may result.
 owned=[(b['Id'],b['X'],b['Y']) for b in city()['Properties']];elements=layout()['Elements'];rect=next(e['Pixels'] for e in elements if e['Name']=='Owned plot 1 1' and e['Kind']=='pointer');x=round(rect['x']+rect['width']/2);y=round(rect['y']+rect['height']/2)
 shell(f'input touchscreen swipe {x} {y} {x+80} {y+100} 500');actions.append({'pan':[x,y,x+80,y+100]});assert [(b['Id'],b['X'],b['Y']) for b in city()['Properties']]==owned;capture('11-actual-touch-pan')
 tap('FIT CITY');before=city();capture('12-before-offline-resume');flush_audit();preserve_process_log();shell('input keyevent 3');shell('cmd connectivity airplane-mode enable');launch();assert [(b['Id'],b['X'],b['Y']) for b in city()['Properties']]==owned;assert city()['Day']>=before['Day'];capture('13-offline-cold-resume')
 # Preserve the Android edge-Back journey separately from KEYCODE_BACK.
 mode=shell('cmd overlay list');assert '[x] com.android.internal.systemui.navbar.gestural' in mode
 dims=layout();w,h=dims['Width'],dims['Height'];shell(f'input touchscreen swipe {w//2} {h-2} {w//2} {h-250} 180');time.sleep(.2)
 nav=shell('dumpsys activity service com.android.systemui/.SystemUIService');(out/'system-navigation.txt').write_text(nav);assert 'backGestureDisabled=false' in nav
 shell(f'input touchscreen swipe 8 {h//2} {w//2} {h//2} 220');until(lambda:state()['Screen']=='city-desk');capture('14-actual-system-back-gesture')
 # Audit every captured screen, preserving cumulative runtime text issues.
 flush_audit()
 issues=[v for a in audit for v in a['Issues']];assert not issues,issues
 (out/'meminfo.txt').write_text(shell('dumpsys meminfo '+pkg));preserve_process_log();log='\n'.join(process_logs);(out/'logcat.txt').write_text(log)
 errors=[line for line in log.splitlines() if re.search(r'FATAL EXCEPTION|Fatal signal|NullReferenceException|MissingMethodException|DllNotFoundException',line)];assert not errors,errors
 for f,h in source.items():assert hashlib.sha256((p/f).read_bytes()).hexdigest()==h,'Source changed during Android replay: '+f
 report={'passed':True,'scope':'Actual ARM64 emulator touch: same-city construction/materials,real trading,second property,finite demand/costs,financing review,zoom/pan,compact/tall layouts,offline cold resume and actual system edge-Back gesture. No edited save/clock/grant/purchase.','api':shell('getprop ro.build.version.sdk'),'page_size':int(shell('getconf PAGE_SIZE')),'abi':shell('getprop ro.product.cpu.abilist'),'apk_sha256':hashlib.sha256(apk.read_bytes()).hexdigest(),'source':source,'actions':actions,'textIssues':issues,'runtimeErrors':errors,'audioSignalChecks':audio}
 (out/'report.json').write_text(json.dumps(report,indent=2));(p/'Artifacts/latest-android-city.txt').write_text(str(out));print(out)
finally:
 shell('wm size reset');shell('cmd connectivity airplane-mode disable');shell('input keyevent 3')
