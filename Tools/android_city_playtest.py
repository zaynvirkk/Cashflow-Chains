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
source={str(f.relative_to(p)):hashlib.sha256(f.read_bytes()).hexdigest() for f in [*(p/'Assets/_Game').rglob('*.cs'),p/'Assets/_Game/Data/CC2/balance-v1.json',p/'Assets/_Game/Resources/CC2/balance-v1.json',*[f for folder in ['Art/V6','Art/V7','Art/V8','Fonts/V6'] for f in (p/'Assets/_Game/Resources'/folder).rglob('*') if f.is_file()],Path(__file__)]}
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
 shell('rm -f '+ ' '.join(remote+'/'+name for name in ['campaign-state.json','empire-state.json','ui-layout.json','text-overflows.json','performance.json','traffic-state.json','city-world-state.json','actual-mix.wav']))
 shell('am start -n '+activity+' --es unity '+shlex.quote('-capture-dir '+remote+' -save-dir '+remote+'/Saves'))
 until(lambda:state()['Screen']=='city-world');time.sleep(2)
 shell('uiautomator dump '+remote+'/system-ui.xml');tree=ET.fromstring(shell('cat '+remote+'/system-ui.xml'))
 if any(n.get('resource-id')=='android:id/immersive_cling_title' for n in tree.iter('node')):
  n=next(n for n in tree.iter('node') if n.get('resource-id')=='android:id/ok');x1,y1,x2,y2=map(int,re.findall(r'\d+',n.get('bounds')));shell(f'input tap {(x1+x2)//2} {(y1+y2)//2}');actions.append({'system':'Dismiss Android full-screen help'});time.sleep(1)
assert cmd('get-serialno').splitlines()[-1].startswith('emulator-'),'This test is isolated to the local emulator'
print(cmd('install','--no-incremental','-r',apk,timeout=180));activity=shell('cmd package resolve-activity --brief '+pkg).splitlines()[-1]
try:
 shell('wm size 720x1280');launch();assert state()['EmpireSandbox'] is False;assert not city()['Properties'];initial=read('city-world-state.json');population=initial['Population'];assert initial['Region']==2 and initial['PrivateResidents']==0 and population>80;assert initial['Revenue']==0 and initial['Net']==0 and initial['Cash']==200;assert initial['VacantLots']==2 and initial['ExistingBuildings']>=62;capture('01-dense-city-zero-owned-assets')
 tap('Metro fit');capture('01a-whole-established-city');tap('Public building 1102',kind='pointer');capture('01b-public-hospital-not-owned');assert not any(e['Name'] in ('MOVE','PAUSE BUSINESS','COMPARE UPGRADE') for e in layout()['Elements']);tap('BACK TO MY PORTFOLIO');assert read('city-world-state.json')['Cash']==200
 tap('Metro career');capture('01c-visual-investor-journey');tap('CHOOSE MY FIRST DEAL');capture('01d-existing-business-opportunities');tap('DEAL 12027');capture('02-rental-acquisition-forecast');wood=next(v['Earned']+v['Purchased'] for v in read('empire-state.json')['Economy']['Balances'] if v['ItemId']=='WOOD');tap('BUY · $80');until(lambda:any(p['AcquiredFrom']==12027 and p['Level']==1 for p in city()['Properties']));assert read('city-world-state.json')['Population']==population;assert next(v['Earned']+v['Purchased'] for v in read('empire-state.json')['Economy']['Balances'] if v['ItemId']=='WOOD')==wood;capture('02a-existing-rental-private-ownership')
 until(lambda:city()['Day']>=2 and city()['CareerStage']>=2);assert not any(b['Kind']=='cafe' for b in city()['Properties']);assert city()['EarnedCashflow']>0;assert len([r for r in read('empire-state.json')['Economy']['Receipts'] if r['CommandId']=='city-day-settle:1'])==1;capture('02b-real-rental-income-no-cafe-required')
 tap('Public building 12028',kind='pointer');capture('02c-cafe-acquisition-preview');tap('BUY · $100');until(lambda:any(b['Kind']=='cafe' and b['Level']==1 for b in city()['Properties']));capture('03-private-cafe-in-a-working-city');assert read('city-world-state.json')['Population']==population
 tap('Owned plot 4 3',kind='pointer');capture('03a-visual-coffee-and-payment');tap('COSTS & BILLS');capture('03aa-inspect-actual-business-costs');tap('BACK TO BUSINESS');back();assert not any('SKIP' in e['Name'] for e in layout()['Elements'] if e['Kind']=='button')
 tap('Metro day');capture('03ab-real-market-calendar');tap('BACK TO MY PORTFOLIO')
 recording=subprocess.Popen([adb,'shell','-n','screenrecord --time-limit 20 --size 720x1280 --bit-rate 3000000 '+remote+'/real-city-motion.mp4'],stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
 tap('Metro census');capture('03b-real-city-demographics');tap('INSPECT A HOME');capture('03c-preserved-tenant-profiles');back();back()
 recording.wait(timeout=35);cmd('pull',remote+'/real-city-motion.mp4',out/'real-city-motion.mp4')
 until(lambda:city()['Day']>=3 and sum(b['Earned']+b['Purchased'] for b in read('empire-state.json')['Economy']['Balances'] if b['ItemId']=='CASH')>=120,200)
 tap('Public building 12033',kind='pointer');tap('BUY · $120');tap('Owned plot 1 4',kind='pointer');capture('04a-real-ownership-transfer-progress');until(lambda:len([b for b in city()['Properties'] if b['Level']>0])==3);assert read('city-world-state.json')['Population']==population;capture('04-three-acquired-assets-same-city')

 # Ordinary Android touches claim earned rewards through the same durable commands.
 tap('Metro rewards');capture('04d-daily-seven-claim-calendar');tap('Daily reward 7');capture('04e-daily-free-and-club-benefits');tap('BACK TO DAILY SUPPLIES');tap('CLAIM DAILY SUPPLIES');capture('04f-committed-daily-reward');tap('CONTINUE')
 daily=[x for x in read('empire-state.json')['Investor']['Claims'] if x['Kind']=='daily'];assert len(daily)==1
 assert any(e['Name']=='CLAIM DAILY SUPPLIES' and not e['Interactable'] for e in layout()['Elements'])
 tap('Rewards Growth');capture('04g-real-investor-milestones');tap('CLAIM GROWTH 0');tap('CONTINUE');tap('CLAIM GROWTH 1');capture('04h-earned-milestone-chest');tap('CONTINUE');tap('Rewards Supplies');tap('CHEST ODDS');capture('04i-exact-supply-chest-odds');tap('BACK TO MY SUPPLIES');tap('OPEN EARNED CHEST');capture('04j-committed-earned-random-outcome');tap('CONTINUE');tap('REWARD HISTORY');capture('04k-persistent-reward-history');tap('BACK')
 assert len([x for x in read('empire-state.json')['Investor']['Claims'] if x['Kind']=='chest'])==1
 tap('Rewards Season');capture('04l-season-free-and-premium-track');elements=layout()['Elements'];eligible=next(e for e in elements if e['Name'].startswith('SEASON 0 TIER 0 False') and e['Interactable']);tap(eligible['Name']);tap('CONTINUE');tap('REWARDS NEXT');capture('04m-later-season-milestones');tap('Rewards Extras');capture('04n-contextual-investor-products')
 tap('INVESTOR PRODUCT cc2.investorclub');capture('04o-optional-daily-upgrade-unavailable-checkout');assert any(e['Name']=='GOOGLE PLAY UNAVAILABLE' and not e['Interactable'] for e in layout()['Elements']);tap('NEXT BENEFITS');capture('04p-complete-daily-upgrade-catalog');back();tap('REWARDS NEXT');tap('INVESTOR PRODUCT cc2.seasonpass');capture('04q-explicit-season-pass-terms');back();back();until(lambda:state()['Screen']=='city-world')

 # Real touches must reach both limits without changing ownership or creating grants.
 for _ in range(8):tap('Metro zoom in')
 until(lambda:abs(read('city-world-state.json')['Zoom']-read('city-world-state.json')['MaximumZoom'])<.001 and any(e['Name']=='Metro zoom in' and e['DisplayText']=='MAX' and not e['Interactable'] for e in layout()['Elements']))
 assert next(e for e in layout()['Elements'] if e['Name']=='Metro zoom in')['DisplayText']=='MAX'
 assert not next(e for e in layout()['Elements'] if e['Name']=='Metro zoom in')['Interactable']
 capture('04b-real-maximum-zoom')
 for _ in range(12):tap('Metro zoom out')
 until(lambda:abs(read('city-world-state.json')['Zoom']-read('city-world-state.json')['MinimumZoom'])<.001 and any(e['Name']=='Metro zoom out' and e['DisplayText']=='MIN' for e in layout()['Elements']))
 assert next(e for e in layout()['Elements'] if e['Name']=='Metro zoom out')['DisplayText']=='MIN'
 capture('04c-real-minimum-zoom-and-growing-access-streets');tap('Metro fit')
 tap('Metro books');until(lambda:state()['Screen']=='city-ledger');capture('05-real-population-bills-and-demand');tap('MY CITY');tap('Metro zoom out');capture('06-zoom-out');tap('Metro fit')
 tap('Metro assets');until(lambda:state()['Screen']=='city-desk');capture('07-owned-city-desk');tap('SETTINGS');tap('SAVE RECOVERY');capture('07b-safe-city-recovery');assert not any(e['Name'] in ('NEW LOCAL GAME','ARCHIVE & START OVER') for e in layout()['Elements']);tap('KEEP MY CITY');tap('FINANCING & BUSINESS STRATEGY');tap('COMPARE FINANCING');tap('REVIEW $100 TERMS');capture('08-financing-terms');back();back();back();until(lambda:state()['Screen']=='city-desk');tap('RETURN TO MY CITY')
 for size,name in [('720x1280','09-compact-mobile'),('720x1600','10-tall-mobile')]:
  shell('wm size '+size);time.sleep(2);capture(name)
 shell('wm size reset');time.sleep(2)
 # Actual touch pan on the map. No domain placement changes may result.
 owned=[(b['Id'],b['X'],b['Y']) for b in city()['Properties']];elements=layout()['Elements'];rect=next(e['Pixels'] for e in elements if e['Name']=='Owned plot 3 3' and e['Kind']=='pointer');x=round(rect['x']+rect['width']/2);y=round(rect['y']+rect['height']/2)
 shell(f'input touchscreen swipe {x} {y} {x+80} {y+100} 500');actions.append({'pan':[x,y,x+80,y+100]});assert [(b['Id'],b['X'],b['Y']) for b in city()['Properties']]==owned;capture('11-actual-touch-pan')
 tap('Metro fit')
 for _ in range(8):tap('Metro zoom in')
 before=city();capture('12-before-offline-resume');flush_audit();preserve_process_log();shell('input keyevent 3');shell('cmd connectivity airplane-mode enable');launch();assert [(b['Id'],b['X'],b['Y']) for b in city()['Properties']]==owned;assert city()['Day']>=before['Day'];until(lambda:next(e for e in layout()['Elements'] if e['Name']=='Metro zoom in')['DisplayText']=='MAX');capture('13-offline-cold-resume')
 # Preserve the Android edge-Back journey separately from KEYCODE_BACK.
 mode=shell('cmd overlay list');assert '[x] com.android.internal.systemui.navbar.gestural' in mode
 dims=layout();w,h=dims['Width'],dims['Height']
 # Immersive Android can consume the first edge swipe to reveal system bars.
 # Keep the app foreground and deliver the second edge gesture while bars show.
 foreground=shell('dumpsys activity activities');assert any(pkg in line for line in foreground.splitlines() if 'topResumedActivity=' in line)
 for attempt in range(2):
  shell(f'input touchscreen swipe 2 {h//2} {w//2} {h//2} 350');actions.append({'system':'Actual edge Back','stage':attempt});time.sleep(.25)
  if state()['Screen']=='city-desk':break
  assert state()['Screen']=='city-world'
 until(lambda:state()['Screen']=='city-desk',15)
 nav=shell('dumpsys activity service com.android.systemui/.SystemUIService');(out/'system-navigation.txt').write_text(nav);assert 'backGestureDisabled=false' in nav
 capture('14-actual-system-back-gesture')
 # Audit every captured screen, preserving cumulative runtime text issues.
 flush_audit()
 issues=[v for a in audit for v in a['Issues']];assert not issues,issues
 (out/'meminfo.txt').write_text(shell('dumpsys meminfo '+pkg));preserve_process_log();log='\n'.join(process_logs);(out/'logcat.txt').write_text(log)
 errors=[line for line in log.splitlines() if re.search(r'FATAL EXCEPTION|Fatal signal|NullReferenceException|MissingReferenceException|MissingMethodException|DllNotFoundException|InvalidOperationException|IndexOutOfRangeException|ArgumentException',line)];assert not errors,errors
 for f,h in source.items():assert hashlib.sha256((p/f).read_bytes()).hexdigest()==h,'Source changed during Android replay: '+f
 report={'passed':True,'scope':'Actual touch of the ARM64 APK through emulator native translation: dense established city with no owned assets or public income,existing-property acquisitions,preserved tenants/population,real private income,visual career/calendar,idempotent daily and investor milestone claims,earned chest odds/opening/history,free and optional premium season tracks,unavailable paid checkout with complete benefit previews,ownership-transfer timing,finite demand/costs,financing review,bounded zoom/MAX-MIN restoration,public and private property-connected access streets,visual business exchange and inspectable real costs,compact/tall layouts,offline cold resume and actual system edge-Back gesture. No edited save/clock/grant/purchase.','api':shell('getprop ro.build.version.sdk'),'page_size':int(shell('getconf PAGE_SIZE')),'abi':shell('getprop ro.product.cpu.abilist'),'emulatorPrimaryAbi':shell('getprop ro.product.cpu.abi'),'nativeBridge':shell('getprop ro.dalvik.vm.native.bridge'),'apk_sha256':hashlib.sha256(apk.read_bytes()).hexdigest(),'source':source,'actions':actions,'textIssues':issues,'runtimeErrors':errors,'audioSignalChecks':audio}
 (out/'report.json').write_text(json.dumps(report,indent=2));(p/'Artifacts/latest-android-city.txt').write_text(str(out));print(out)
finally:
 shell('wm size reset');shell('cmd connectivity airplane-mode disable');shell('input keyevent 3')
