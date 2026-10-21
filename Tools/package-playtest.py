#!/usr/bin/env python3
"""Package the already-built local player; this never publishes or contacts a store."""
from pathlib import Path
import json,hashlib,zipfile,datetime,subprocess,sys,xml.etree.ElementTree as ET
p=Path(__file__).resolve().parent.parent
apk=p/'Builds/Android'/(p.name+'-Playtest-1.0.apk');assert apk.exists()
def digest(f):return hashlib.sha256(f.read_bytes()).hexdigest()
proofs={}
for key,marker in [('native','latest-playtest.txt'),('campaign','latest-campaign-replay.txt'),('android','latest-android-review.txt'),('earned','latest-earned-playtest.txt'),('gesture','latest-android-gesture.txt')]:
 folder=Path((p/'Artifacts'/marker).read_text().strip());r=json.loads((folder/'report.json').read_text());assert r['passed'];proofs[key]=str(folder.relative_to(p))+'/report.json'
 if key in ['android','gesture']:assert r['apk_sha256']==digest(apk)
 else:
  for file,h in r['source'].items():assert digest(p/file)==h,('Source drift',file)
  for file,h in r.get('build',{}).items():assert digest(p/'Builds/Linux'/file)==h,('Binary drift',file)
 for f in folder.glob('text-overflows*.json'):assert not json.loads(f.read_text())['Issues'],f
r=ET.parse(p/'Artifacts/Tests/ci.xml').getroot();assert int(r.get('failed','1'))==0 and r.get('result')=='Passed'
out=p/'Builds/Deliverables';out.mkdir(parents=True,exist_ok=True);archive=out/(p.name+'-Linux-Playtest-1.0.zip')
readme=f'''{p.name} — local playtest edition 1.0

Extract the complete folder, then run Play.sh on Linux. Keep Builds/Linux beside the launcher. The window opens at 540x960 and can be resized. Gameplay works offline; saves normally use Unity's persistent application data folder.

Cashflow: drag/select buildings; rotate with R, undo with Z, run/skip with Space. Black Bag: select/drag from the shelf; rotate with R; explicitly Take with Space; extract with E. Escape opens the desk or closes a dialog. All controls also have visible buttons.

Settings include independent music/effects/haptics, reduced motion and high contrast. Save recovery can archive an existing local game after confirmation. English only. This edition has no configured real store checkout. No accounts, ads, cash redemption or online gameplay. This is not a published Steam/Google Play/iOS release. Physical phone, controller and real-player commercial acceptance remain separate.

Unity 6000.3.26f1. Original art/music; DejaVu font license included. Detailed test evidence remains in the project Artifacts folder and Docs/MOBILE_ACCEPTANCE.md.
'''
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
 for f in [p/'Play.sh',p/'Docs/FONT_LICENSE.txt',p/'Docs/THIRD_PARTY.md',*(p/'Builds/Linux').rglob('*')]:
  if f.is_file():z.write(f,p.name+'/'+str(f.relative_to(p)))
 z.writestr(p.name+'/README-PLAYTEST.txt',readme)
with zipfile.ZipFile(archive) as z:assert z.testzip() is None
subprocess.run([sys.executable,str(p/'Tools/verify-portable.py')],check=True)
portable=Path((p/'Artifacts/latest-portable-review.txt').read_text().strip());pr=json.loads((portable/'report.json').read_text());assert pr['passed'] and pr['archive_sha256']==digest(archive);proofs['portable']=str(portable.relative_to(p))+'/report.json'
manifest={'created_utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'scope':'Local offline playtest; no publication or real-store acceptance','tests':{k:r.get(k) for k in ['total','passed','failed']},'proofs':proofs,'files':{str(f.relative_to(p)):{'bytes':f.stat().st_size,'sha256':digest(f)} for f in [apk,archive]},'build_inputs':{str(f.relative_to(p)):digest(f) for f in [*(p/'Assets/Plugins/Android').rglob('*.java'),p/'Packages/manifest.json',p/'Packages/packages-lock.json',p/'ProjectSettings/ProjectSettings.asset',p/'ProjectSettings/ProjectVersion.txt']},'runtime_source':{str(f.relative_to(p)):digest(f) for f in (p/'Assets/_Game/Scripts').rglob('*.cs')}}
(out/'DELIVERY.json').write_text(json.dumps(manifest,indent=2)+'\n');print(archive)
