#!/usr/bin/env python3
"""Synthetic economic diagnostics,using the exact C# domain;not a human or revenue test."""
from pathlib import Path
import subprocess,hashlib,json
import cc2_domain
root=Path(__file__).resolve().parent.parent
cc2_domain.run('validate',root/'Assets/_Game/Data/CC2/balance-v1.json')
out=root/'Artifacts/CC2/CityEconomySimulation';out.mkdir(exist_ok=True)
runtime=cc2_domain.UNITY/'NetCoreRuntime';refs=runtime/'shared/Microsoft.NETCore.App/6.0.21';build=root/'Builds/CC2Tools';harness=build/'CityEconomyHarness.dll';rsp=build/'city-harness.rsp';source=root/'Tools/CityEconomyHarness.cs'
rsp.write_text('\n'.join(['-nologo','-target:exe','-langversion:9','-optimize+','-out:'+str(harness),'-r:'+str(build/'CC2Tools.dll'),*['-r:'+str(p) for p in refs.glob('*.dll')],str(source)]))
subprocess.run([str(runtime/'dotnet'),str(cc2_domain.UNITY/'DotNetSdkRoslyn/csc.dll'),'@'+str(rsp)],check=True)
(harness.with_suffix('.runtimeconfig.json')).write_text(json.dumps({'runtimeOptions':{'tfm':'net6.0','framework':{'name':'Microsoft.NETCore.App','version':'6.0.21'}}}))
subprocess.run([str(runtime/'dotnet'),str(harness),str(root/'Assets/_Game/Data/CC2/balance-v1.json'),str(out/'report.json')],check=True)
files=[*sorted((root/'Assets/_Game/Scripts/Domain').rglob('*.cs')),root/'Assets/_Game/Data/CC2/balance-v1.json',source,Path(__file__)]
(out/'source.json').write_text(json.dumps({str(p.relative_to(root)):hashlib.sha256(p.read_bytes()).hexdigest() for p in files},indent=2))
