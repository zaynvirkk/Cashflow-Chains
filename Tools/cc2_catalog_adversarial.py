#!/usr/bin/env python3
"""Reject malformed and exploitable catalogs without mutating production files."""
import copy,json,tempfile,subprocess,sys,hashlib
from pathlib import Path
from cc2_content_validate import check
from cc2_domain import ROOT,UNITY
canonical=ROOT/'Assets/_Game/Data/CC2/balance-v1.json';base=json.loads(canonical.read_text());schema=json.loads((canonical.parent/'balance-v1.schema.json').read_text());cases=[]
def case(name,change):
 value=copy.deepcopy(base);change(value)
 try:check(value,schema)
 except ValueError:cases.append({'case':name,'rejected':True,'layer':'schema'});return
 with tempfile.NamedTemporaryFile('w',suffix='.json') as f:
  json.dump(value,f);f.flush();r=subprocess.run([str(UNITY/'NetCoreRuntime/dotnet'),str(ROOT/'Builds/CC2Tools/CC2Tools.dll'),'validate',f.name],text=True,capture_output=True)
 assert r.returncode!=0,name;cases.append({'case':name,'rejected':True,'layer':'shared-CSharp'})
case('negative-cost',lambda c:c['Upgrades'][0]['Costs'][0].update(Quantity=-1))
case('zero-cost',lambda c:c['Upgrades'][0]['Costs'][0].update(Quantity=0))
case('boolean-money',lambda c:c['Upgrades'][0]['Costs'][0].update(Quantity=True))
case('duplicate-item',lambda c:c['Items'].append(copy.deepcopy(c['Items'][0])))
case('positive-arbitrage',lambda c:c['Exchanges'][1].update(Quantity=13))
case('purchased-gem-expiry',lambda c:next(x for x in c['Items'] if x['Id']=='GEMS').update(CanExpire=True))
case('missing-repeatable-free-source',lambda c:c.update(Sources=[s for s in c['Sources'] if s['ItemId']!='WOOD' or not s['Repeatable']]))
case('unknown-field',lambda c:c.update(UnverifiedPremiumGrant=9999))
case('unbounded-offline-cap',lambda c:c.update(OfflineCapSeconds=315576000))
case('unknown-recipe',lambda c:c['Businesses'][0].update(Recipe='not_a_piece'))
case('unknown-output-item',lambda c:c['Businesses'][0].update(Output='ROUND_SCORE'))
out=ROOT/'Artifacts/CC2/ContentValidation/adversarial.json';out.write_text(json.dumps({'passed':True,'cases':cases,'canonical_sha256':hashlib.sha256(canonical.read_bytes()).hexdigest()},indent=2)+'\n');print(str(len(cases))+' negative catalogs rejected')
