#!/usr/bin/env python3
"""Compile the exact pure C# game domain with Unity's pinned compiler/runtime. No new SDK required."""
from pathlib import Path
import subprocess,hashlib,json,os
ROOT=Path(__file__).resolve().parent.parent
UNITY=Path('/home/virk/Unity/Hub/Editor/6000.3.26f1/Editor/Data')
def run(*args):
 runtime=UNITY/'NetCoreRuntime';refs=runtime/'shared/Microsoft.NETCore.App/6.0.21';out=ROOT/'Builds/CC2Tools';out.mkdir(parents=True,exist_ok=True)
 sources=sorted((ROOT/'Assets/_Game/Scripts/Domain').rglob('*.cs'))+sorted((ROOT/'Tools/CC2').glob('*.cs'))
 digest=hashlib.sha256(b''.join(str(f.relative_to(ROOT)).encode()+f.read_bytes() for f in sources)).hexdigest();assembly=out/'CC2Tools.dll';marker=out/'source.sha256'
 if not marker.exists() or marker.read_text()!=digest or not assembly.exists():
  rsp=out/'compiler.rsp';rsp.write_text('\n'.join(['-nologo','-target:exe','-langversion:9','-optimize+','-out:'+str(assembly),*['-r:'+str(f) for f in refs.glob('*.dll')],*[str(f) for f in sources]]))
  subprocess.run([str(runtime/'dotnet'),str(UNITY/'DotNetSdkRoslyn/csc.dll'),'@'+str(rsp)],check=True,cwd=ROOT)
  (out/'CC2Tools.runtimeconfig.json').write_text(json.dumps({'runtimeOptions':{'tfm':'net6.0','framework':{'name':'Microsoft.NETCore.App','version':'6.0.21'}}}));marker.write_text(digest)
 return subprocess.run([str(runtime/'dotnet'),str(assembly),*map(str,args)],check=True,cwd=ROOT)
if __name__=='__main__':
 import sys
 run(*sys.argv[1:])
