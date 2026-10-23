#!/usr/bin/env python3
"""Package the successful APK's exact payload through Unity's Android Gradle export.

Fallback for memory-constrained hosts: no second IL2CPP conversion. AAB acceptance
requires byte-identical game assets and native libraries,not merely a Gradle exit0.
"""
from pathlib import Path
import subprocess,shutil,zipfile,json,hashlib,os,re
root=Path(__file__).resolve().parent.parent
apk=root/'Builds/Android/CashflowChains-Playtest-1.0.apk'
project=root/'Library/Bee/Android/Prj/IL2CPP/Gradle'
export=root/'Builds/AndroidBundleGradle'
editor=Path('/home/virk/Unity/Hub/Editor/6000.3.26f1/Editor/Data')
proof=json.loads((root/'Artifacts/Logs/android-release-build.provenance.json').read_text())
assert proof['outcome']=='success' and proof['exitCode']==0,'Successful current APK build required'
inputs=[*sorted((root/'Assets/_Game').rglob('*.cs')),root/'Assets/_Game/Data/CC2/balance-v1.json',root/'Assets/_Game/Resources/CC2/balance-v1.json',root/'Packages/manifest.json',root/'Packages/packages-lock.json',*[f for f in (root/'Assets/_Game/Resources').rglob('*') if f.is_file()],*[f for f in (root/'Assets/StreamingAssets').rglob('*') if f.is_file()]]
assert all(f.stat().st_mtime_ns<=apk.stat().st_mtime_ns for f in inputs),'APK predates game inputs;build Android first'
source={str(f.relative_to(root)):hashlib.sha256(f.read_bytes()).hexdigest() for f in inputs}
assert project.is_dir()
if export.exists():shutil.rmtree(export) # Only this tool's disposable export.
shutil.copytree(project,export,ignore=shutil.ignore_patterns('build','.gradle','.cxx'))
assets=export/'unityLibrary/src/main/assets'
if assets.exists():shutil.rmtree(assets)
assets.mkdir(parents=True)
with zipfile.ZipFile(apk) as z:
 expected={n:hashlib.sha256(z.read(n)).hexdigest() for n in z.namelist() if not n.endswith('/') and (n.startswith('assets/') or n.startswith('lib/') and n.endswith('.so')) and not n.startswith('assets/dexopt/')}
 assert hashlib.sha256((export/'unityLibrary/src/main/jniLibs/arm64-v8a/libil2cpp.so').read_bytes()).hexdigest()==expected['lib/arm64-v8a/libil2cpp.so'],'Exported C# binary does not match successful APK'
 for name in expected:
  if name.startswith('assets/'):
   target=assets/name[7:];target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(z.read(name))
# Use all proven native binaries too. Rebuilding Unity's two tiny platform bridges
# embeds different export paths;that would violate the exact-payload contract.
with zipfile.ZipFile(apk) as z:
 for name in expected:
  if name.startswith('lib/'):
   target=export/'unityLibrary/src/main/jniLibs'/name[4:];target.parent.mkdir(parents=True,exist_ok=True);target.write_bytes(z.read(name))
gradle_file=export/'unityLibrary/build.gradle';text=gradle_file.read_text()
while True:
 match=re.search(r'(?:android\.)?externalNativeBuild\s*\{',text)
 if not match:break
 depth=1;end=match.end()
 while depth:
  if text[end]=='{':depth+=1
  elif text[end]=='}':depth-=1
  end+=1
 text=text[:match.start()]+text[end:]
gradle_file.write_text(text)
java=editor/'PlaybackEngines/AndroidPlayer/OpenJDK/bin/java'
gradle=editor/'PlaybackEngines/AndroidPlayer/Tools/gradle/lib/gradle-launcher-9.3.1.jar'
args=[str(java),'-classpath',str(gradle),'org.gradle.launcher.GradleMain','--offline','--no-daemon','--max-workers=1','-Dorg.gradle.jvmargs=-Xmx768m','bundleRelease']
subprocess.run(args,cwd=export,check=True)
aab=export/'launcher/build/outputs/bundle/release/launcher-release.aab';assert aab.is_file()
with zipfile.ZipFile(aab) as z:
 actual={n[5:]:hashlib.sha256(z.read(n)).hexdigest() for n in z.namelist() if n.startswith(('base/assets/','base/lib/')) and not n.endswith('/') and not n.startswith('base/assets/dexopt/')}
assert actual==expected,{'missing':sorted(expected.keys()-actual.keys()),'extra':sorted(actual.keys()-expected.keys()),'changed':[n for n in expected.keys()&actual.keys() if expected[n]!=actual[n]]}
assert all(hashlib.sha256((root/f).read_bytes()).hexdigest()==h for f,h in source.items()),'Game inputs changed while packaging'
output=root/'Builds/Android/CashflowChains-1.0.0-build.aab';shutil.copyfile(aab,output)
record={'passed':True,'producer':'Offline Gradle bundleRelease from Unity export;no second IL2CPP conversion','command':args,'apk':{'sha256':hashlib.sha256(apk.read_bytes()).hexdigest()},'aab':{'sha256':hashlib.sha256(output.read_bytes()).hexdigest()},'payloadFiles':len(expected),'payload':expected,'source':source,'apkBuildProvenance':proof,'limits':'Byte identity and packaging proof,not Play delivery,physical-phone or purchase acceptance.'}
(root/'Artifacts/CC2/PersistentCityAndroidVerified/bundle-build-provenance.json').write_text(json.dumps(record,indent=2))
print(json.dumps({'passed':True,'identicalPayloadFiles':len(expected),'output':str(output)}))
