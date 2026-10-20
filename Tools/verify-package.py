#!/usr/bin/env python3
"""Byte integrity and structural checks only. Not a level-solvability certificate."""
import hashlib,json
from pathlib import Path
p=Path(__file__).resolve().parent.parent
manifest=json.loads((p/'Docs/SOURCE_PACKAGE_SHA256.json').read_text())
for name,digest in manifest['files'].items():
 assert hashlib.sha256((p/'Docs/SourcePackage'/name).read_bytes()).hexdigest()==digest,name
for name,key,count in [('cashflow_pieces_seed','pieces',24),('cashflow_72_missions_seed','missions',72),('blackbag_48_loot_seed','loot',48),('blackbag_72_contracts_seed','missions',72)]:
 data=json.loads((p/'Docs/SourcePackage/data'/(name+'.json')).read_text());rows=data[key]
 assert len(rows)==count,(name,len(rows));assert len({r['id'] for r in rows})==count
 for row in rows:
  if 'cells' in row:
   cells={tuple(c) for c in row['cells']};assert len(cells)==len(row['cells']);assert 0<len(cells)<=8
   visited={next(iter(cells))}
   while True:
    grown=visited|{(x+dx,y+dy) for x,y in visited for dx,dy in [(1,0),(-1,0),(0,1),(0,-1)] if (x+dx,y+dy) in cells}
    if grown==visited:break
    visited=grown
   assert visited==cells,row['id']
print(json.dumps({'sourcePackageFiles':len(manifest['files']),'integrity':'PASS','catalogCounts':[24,72,48,72],'structure':'PASS','solvability':'NOT_TESTED','releaseBalanced':False},indent=2))
