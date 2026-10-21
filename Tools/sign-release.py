#!/usr/bin/env python3
"""Sign local release artifacts using ignored credentials, without logging secrets."""
from pathlib import Path
import json,subprocess,os,zipfile,hashlib
p=Path(__file__).resolve().parent.parent
sdk=Path(os.environ.get('ANDROID_SDK_ROOT','/home/virk/Android/Sdk'));bt=sdk/'build-tools/36.0.0'
jdk=Path('/home/virk/Unity/Hub/Editor/6000.3.26f1/Editor/Data/PlaybackEngines/AndroidPlayer/OpenJDK/bin')
c=json.loads((p/'.local-signing/credentials.json').read_text());env={**os.environ,'CF_STORE_PASSWORD':c['store_password'],'CF_KEY_PASSWORD':c['key_password']}
key=Path(c['keystore']);key=key if key.is_absolute() else p/key
out=p/'Builds/Release';out.mkdir(parents=True,exist_ok=True)
def run(args):return subprocess.check_output(list(map(str,args)),env=env,stderr=subprocess.STDOUT,text=True)
def unsigned(src,dst):
 with zipfile.ZipFile(src) as a,zipfile.ZipFile(dst,'w') as z:
  for i in a.infolist():
   n=i.filename.upper()
   if n.startswith('META-INF/') and (n.endswith(('.SF','.RSA','.DSA','.EC')) or n=='META-INF/MANIFEST.MF'):continue
   z.writestr(i,a.read(i.filename))
apk=out/'CashflowChains-1.0.0.apk';raw=out/'unsigned.apk';aligned=out/'aligned.apk'
unsigned(p/'Builds/Android/CashflowChains-Playtest-1.0.apk',raw)
run([bt/'zipalign','-f','-P','16','4',raw,aligned])
run([bt/'apksigner','sign','--ks',key,'--ks-key-alias',c['alias'],'--ks-pass','env:CF_STORE_PASSWORD','--key-pass','env:CF_KEY_PASSWORD','--out',apk,aligned])
verify=run([bt/'apksigner','verify','--verbose','--print-certs',apk]);(out/'apk-signature.txt').write_text(verify)
run([bt/'zipalign','-c','-P','16','4',apk]);raw.unlink();aligned.unlink()
aab=out/'CashflowChains-1.0.0.aab';unsigned(p/'Builds/Android/CashflowChains-1.0.0-build.aab',aab)
run([jdk/'jarsigner','-keystore',key,'-storepass:env','CF_STORE_PASSWORD','-keypass:env','CF_KEY_PASSWORD','-sigalg','SHA256withRSA','-digestalg','SHA-256',aab,c['alias']])
audit=run([jdk/'jarsigner','-verify','-verbose','-certs',aab]);assert 'jar verified.' in audit;(out/'aab-signature.txt').write_text(audit)
report={'signed_with':'Local upload key; private key and passwords excluded from Git','files':{f.name:{'bytes':f.stat().st_size,'sha256':hashlib.sha256(f.read_bytes()).hexdigest()} for f in [apk,aab]},'apk_signature':verify}
(out/'SIGNING.json').write_text(json.dumps(report,indent=2));print('Upload-key-signed APK and AAB verified:',out)
