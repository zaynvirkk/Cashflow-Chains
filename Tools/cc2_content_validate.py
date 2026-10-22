#!/usr/bin/env python3
"""Validate the versioned balance schema and production C# semantic invariants."""
from pathlib import Path
import json,subprocess,sys,hashlib,datetime
from cc2_domain import run,ROOT

def check(value,schema,path='$'):
 kind=schema['type'];types={'object':dict,'array':list,'string':str,'integer':int,'boolean':bool}
 if type(value) is not types[kind]:raise ValueError(path+': expected '+kind)
 if kind=='object':
  missing=set(schema.get('required',[]))-set(value);extra=set(value)-set(schema.get('properties',{}))
  if missing or extra:raise ValueError(f'{path}: missing={sorted(missing)} extra={sorted(extra)}')
  for k,v in value.items():check(v,schema['properties'][k],path+'.'+k)
 elif kind=='array':
  if len(value)>schema.get('maxItems',1000):raise ValueError(path+': too many entries')
  for i,v in enumerate(value):check(v,schema['items'],path+f'[{i}]')
 elif kind=='integer':
  if not schema.get('minimum',0)<=value<=schema.get('maximum',1000000000):raise ValueError(path+': integer out of range')
 elif kind=='string':
  if not schema.get('minLength',0)<=len(value)<=schema.get('maxLength',1000000):raise ValueError(path+': invalid string length')

if __name__=='__main__':
 file=Path(sys.argv[1]) if len(sys.argv)>1 else ROOT/'Assets/_Game/Data/CC2/balance-v1.json';schema=ROOT/'Assets/_Game/Data/CC2/balance-v1.schema.json';check(json.loads(file.read_text()),json.loads(schema.read_text()));assert file.read_bytes()==(ROOT/'Assets/_Game/Resources/CC2/balance-v1.json').read_bytes(),'Runtime catalog differs from canonical data';run('validate',file)
 report={'passed':True,'time_utc':datetime.datetime.now(datetime.timezone.utc).isoformat(),'scope':'Balance-v1 schema and shared C# semantic validation only; future content schemas are not accepted','catalog_sha256':hashlib.sha256(file.read_bytes()).hexdigest(),'schema_sha256':hashlib.sha256(schema.read_bytes()).hexdigest()};out=ROOT/'Artifacts/CC2/ContentValidation';out.mkdir(parents=True,exist_ok=True);(out/'report.json').write_text(json.dumps(report,indent=2)+'\n')
