#!/usr/bin/env python3
"""Fail closed on packaging, permission, ABI and 16 KB ELF/ZIP requirements."""
from pathlib import Path
import subprocess,json,zipfile,struct,hashlib,xml.etree.ElementTree as ET
p=Path(__file__).resolve().parent.parent;sdk=Path('/home/virk/Android/Sdk');bt=sdk/'build-tools/36.0.0';java=Path('/home/virk/Unity/Hub/Editor/6000.3.26f1/Editor/Data/PlaybackEngines/AndroidPlayer/OpenJDK/bin/java');bundle=p/'Builds/Tools/bundletool-all-1.18.3.jar';apk=p/'Builds/Release/CashflowChains-1.0.0.apk';aab=apk.with_suffix('.aab');out=p/'Release/Verification';out.mkdir(parents=True,exist_ok=True)
def run(args):return subprocess.check_output(list(map(str,args)),text=True,stderr=subprocess.STDOUT)
manifest=run([sdk/'cmdline-tools/latest/bin/apkanalyzer','manifest','print',apk]);(out/'AndroidManifest.xml').write_text(manifest);root=ET.fromstring(manifest);ns='{http://schemas.android.com/apk/res/android}'
assert root.get('package')=='com.virkgames.cashflowchains';assert root.get(ns+'versionCode')=='1';assert root.get(ns+'versionName')=='1.0.0'
uses=root.find('uses-sdk');assert int(uses.get(ns+'targetSdkVersion'))>=36 and int(uses.get(ns+'minSdkVersion'))==26
assert root.find('application').get(ns+'debuggable','false')=='false'
permissions=[e.get(ns+'name') for e in root.findall('uses-permission')];assert not any(any(word in x for word in ['BILLING','INTERNET','AD_ID','RECORD_AUDIO','ACCESS_FINE_LOCATION']) for x in permissions),permissions
for name in ['Packages/manifest.json','Packages/packages-lock.json']:
 text=(p/name).read_text();assert not any(x in text for x in ['com.unity.purchasing','com.unity.services.analytics','com.unity.ads'])
def elf_alignment(data):
 assert data[:5]==b'\x7fELF\x02' and data[5]==1,'Expected 64 bit little endian ELF'
 offset=struct.unpack_from('<Q',data,32)[0];size,count=struct.unpack_from('<HH',data,54);loads=[]
 for i in range(count):
  h=struct.unpack_from('<IIQQQQQQ',data,offset+i*size)
  if h[0]==1:
   assert h[7]>=16384 and h[2]%16384==h[3]%16384,('ELF load segment not 16KB aligned',h)
   loads.append(h[7])
 assert loads;return min(loads)
native=[]
with apk.open('rb') as raw,zipfile.ZipFile(apk) as z:
 for i in z.infolist():
  if i.filename.startswith('lib/') and i.filename.endswith('.so'):
   alignment=elf_alignment(z.read(i.filename));raw.seek(i.header_offset);header=raw.read(30);n,e=struct.unpack_from('<HH',header,26);offset=i.header_offset+30+n+e
   if i.compress_type==zipfile.ZIP_STORED:assert offset%16384==0,(i.filename,offset)
   native.append({'file':i.filename,'elf_load_alignment':alignment,'zip_compressed':i.compress_type!=zipfile.ZIP_STORED,'zip_data_offset':offset})
assert native and {x['file'].split('/')[1] for x in native}=={'arm64-v8a'}
run([bt/'zipalign','-c','-P','16','4',apk]);signature=run([bt/'apksigner','verify','--verbose','--print-certs',apk]);assert 'Verified' in signature
validate=run([java,'-jar',bundle,'validate','--bundle='+str(aab)]);(out/'bundletool-validation.txt').write_text(validate)
config=run([java,'-jar',bundle,'dump','config','--bundle='+str(aab)]);(out/'BundleConfig.json').write_text(config);assert 'PAGE_ALIGNMENT_16K' in config,config
bundle_manifest=run([java,'-jar',bundle,'dump','manifest','--bundle='+str(aab)]);(out/'BundleManifest.xml').write_text(bundle_manifest)
with zipfile.ZipFile(aab) as z:
 aab_libs=[i for i in z.namelist() if i.startswith('base/lib/') and i.endswith('.so')];assert aab_libs
 for i in aab_libs:elf_alignment(z.read(i))
report={'passed':True,'target_sdk':36,'min_sdk':26,'debuggable':False,'permissions':permissions,'abis':['arm64-v8a'],'native_libraries':native,'bundle_alignment':'PAGE_ALIGNMENT_16K','bundletool_version':'1.18.3','bundletool_sha256':hashlib.sha256(bundle.read_bytes()).hexdigest(),'files':{str(f.relative_to(p)):hashlib.sha256(f.read_bytes()).hexdigest() for f in [apk,aab]},'limits':'Static compatibility checks complement emulator tests; physical ARM64 phone behavior and Play-generated splits require external testing.'}
(out/'report.json').write_text(json.dumps(report,indent=2));print(json.dumps({k:report[k] for k in ['passed','target_sdk','permissions','abis','bundle_alignment']}))
