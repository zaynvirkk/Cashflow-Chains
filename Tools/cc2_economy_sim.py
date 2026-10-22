#!/usr/bin/env python3
"""Run the shared C# preliminary economy model; synthetic output is not commercial evidence."""
import argparse,json,hashlib
from cc2_domain import run,ROOT
p=argparse.ArgumentParser();p.add_argument('--accounts',type=int,default=10000);p.add_argument('--days',type=int,default=180);p.add_argument('--output',default=str(ROOT/'Artifacts/CC2/EconomySimulation/report.json'));a=p.parse_args();run('simulate',ROOT/'Assets/_Game/Data/CC2/balance-v1.json',a.accounts,a.days,a.output)

file=__import__('pathlib').Path(a.output);report=json.loads(file.read_text());report['catalog_sha256']=hashlib.sha256((ROOT/'Assets/_Game/Data/CC2/balance-v1.json').read_bytes()).hexdigest();report['model_source_sha256']=hashlib.sha256((ROOT/'Tools/CC2/Simulation.cs').read_bytes()).hexdigest();file.write_text(json.dumps(report,indent=2)+'\n')
