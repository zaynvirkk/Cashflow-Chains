#!/usr/bin/env python3
"""Generate illustrative preproduction content catalogues, NOT certified launch balance.
Run domain solvers and human-curate every mandatory level before release.
"""
import json, pathlib, math
D=pathlib.Path(__file__).resolve().parent.parent/'data';D.mkdir(exist_ok=True)
def save(name,obj):
 with open(D/name,'w') as f: json.dump(obj,f,indent=2,ensure_ascii=False)

# Fixed starter content. Values and input recipes are tunable guesses, not shipped balance.
raw=[
('home','Residential Home','residential','people',3,0,'source'),
('cafe','Neighborhood Cafe','retail','credits',80,1,'people+goods'),
('grocer','Corner Grocer','retail','credits',110,2,'2people+goods'),
('warehouse','Warehouse','logistics','goods',6,3,'router'),
('farm','Urban Farm','supply','goods',2,1,'source'),
('power','Power Station','supply','energy',4,2,'source'),
('factory','Assembly Factory','industry','goods',3,4,'energy+goods'),
('market','Open Market','retail','credits',220,2,'2people+2goods'),
('office','Business Office','services','people',1,2,'source+adjacent10pct'),
('bank','Bank','services','credits',0,3,'adjacent25pct'),
('logistics','Logistics Hub','logistics','goods',5,4,'router_range2'),
('apartment','Apartments','residential','people',6,3,'source'),
('boutique','Luxury Boutique','retail','credits',240,2,'people+2goods'),
('park','City Park','special','credits',0,1,'nearhome15pct'),
('port','Trade Port','supply','goods',4,4,'import'),
('landmark','Landmark','special','credits',0,5,'diversity25pct'),
('diner','Late Diner','retail','credits',125,2,'people+goods'),
('shopping_mall','Shopping Mall','retail','credits',350,4,'3people+3goods'),
('tech_office','Tech Office','services','people',2,4,'source+researchtag'),
('recycler','Recycler','industry','goods',2,3,'2goods+energy'),
('freight_yard','Freight Yard','logistics','goods',8,4,'router'),
('hotel','City Hotel','services','credits',190,4,'2people+goods'),
('stadium','Sports Stadium','special','credits',280,5,'5people+goods'),
('solar_array','Solar Array','supply','energy',3,3,'source'),
]
ccpieces=[]
for i,(id,name,family,produces,rate,tier,rule) in enumerate(raw):
 ccpieces.append(dict(id=id,displayName=name,family=family,tier=tier,scoreOutput=produces,baseRate=rate,ruleHint=rule,artKey='art/pieces/'+id,rotationOptions=[0,90,180,270],maxPerBoard=4 if family not in ['special','services'] else 2,productionModelStatus='prototype_guess_validate_recipe',sourceSpec='01_CASHFLOW_CHAINS_MASTER_SPEC.md#C04'))
save('cashflow_pieces_seed.json',dict(meta=dict(status='PROTOTYPE_SEED_NOT_RELEASE_BALANCED',version=1,require='define fully normalized port/recipe validation in editor'),pieces=ccpieces))

csdistricts=[('starter','Starter Street',['home','farm','cafe','grocer','warehouse'],(4,4),(220,1600)),('downtown','Downtown Office',['home','cafe','grocer','warehouse','farm','office','bank','park','diner'],(5,5),(800,4400)),('industry','Industrial Bay',['home','cafe','farm','warehouse','power','factory','market','logistics','office'],(6,6),(1800,9000)),('retail','Retail Festival',['home','cafe','farm','market','grocer','boutique','apartment','shopping_mall','bank','park'],(6,6),(2400,13000)),('harbor','Harbor Trade',['home','cafe','power','factory','warehouse','logistics','port','freight_yard','hotel','park','market'],(6,6),(5000,23000)),('finance','Financial Heights',[x[0] for x in raw],(6,6),(9000,38000))]
mods=['none','limited_goods','high_housing','retail_festival','energy_shortage','industrial_bonus','bank_bonus','road_block','cheap_logistics','luxury_demand','fragile_supply','office_boost']
levels=[]
for di,(did,dname,allow,sz,rng) in enumerate(csdistricts):
 for li in range(12):
  idx=li+1;levelid=f'cc.{did}.{idx:02d}';goal=int(rng[0]+(rng[1]-rng[0])*(li/11)**1.15)
  levels.append(dict(id=levelid,district=did,index=idx,displayName=f'{dname}: Contract {idx}',grid={'width':sz[0], 'height':sz[1]},targetScore=goal,startingBudget=300+di*220+li*75,allowedPieceIds=allow[:min(len(allow),max(3,3+li//2))],modifier=mods[(li+di)%len(mods)],seed=104729+di*101+li*7919,secondStar={'objective':'finish_under_budget','factorBasisPoints':9000},thirdStar={'objective':'activate_unique_combo','minimum':1 if li<6 else 2},mandatoryOneStarFree=True,validation='UNVALIDATED_REQUIRES_SOLVER_REFERENCE_LAYOUT_AND_HUMAN_PLAYTEST'))
save('cashflow_72_missions_seed.json',dict(meta=dict(status='PREPRODUCTION_BALANCE_SEED',mandatory=72,warning='Targets are guesses. Codex MUST run constrained solver, produce witness layout for every mission and re-tune; never ship unsolved seeds.',version=1),districts=[dict(id=d,displayName=n) for d,n,*_ in csdistricts],missions=levels))

shapes={'single':[(0,0)],'domino':[(0,0),(1,0)],'long3':[(0,0),(1,0),(2,0)],'long4':[(0,0),(1,0),(2,0),(3,0)],'square':[(0,0),(1,0),(0,1),(1,1)],'L3':[(0,0),(0,1),(1,1)],'L4':[(0,0),(0,1),(0,2),(1,2)],'T4':[(0,0),(1,0),(2,0),(1,1)],'S4':[(1,0),(2,0),(0,1),(1,1)],'rect2x3':[(0,0),(1,0),(0,1),(1,1),(0,2),(1,2)],'hook5':[(0,0),(0,1),(0,2),(1,2),(2,2)],'plus5':[(1,0),(0,1),(1,1),(2,1),(1,2)]}
groups={
'jewelry':['Ruby Ring','Sapphire Ring','Pearl Brooch','Diamond Collar','Emerald Pendant','Gilded Tiara','Queen Bracelet','Meteor Opal'],
'heritage':['Pocket Watch','Bronze Compass','Carved Bust','Imperial Egg','Old Manuscript','Grand Chalice','Silver Reliquary','Ivory Miniature'],
'art':['Miniature Landscape','Golden Frame','Ceramic Vase','Oil Portrait','Art Scroll','Collector Bust','Antique Triptych','Obsidian Mask'],
'gadgets':['Film Camera','Vintage Radio','Encrypted Tablet','Mechanical Cipher','Prototype Phone','Microfilm Box','Crystal Clock','Encrypted Ledger'],
'gold':['Cash Roll','Gold Bar','Rare Coin','Coin Case','Gold Chain','Minted Ingot','Treasury Bonds','Royal Seal'],
'artifacts':['Sunstone','Runic Compass','Arcane Atlas','Star Globe','Astral Key','Imperial Crown','Emperor Scepter','Celestial Reliquary']}
shape_rotation=['single','domino','L3','long3','square','T4','S4','rect2x3','L4','hook5','plus5','long4']
loot=[]
for gi,(group,names) in enumerate(groups.items()):
 for i,name in enumerate(names):
  shape=shape_rotation[(i+gi*2)%len(shape_rotation)]; count=len(shapes[shape]); tier=i//2+gi//2;value=int((450+gi*400)*(1.8**tier)*(1+count*.25));weight=min(10,max(1,round(count*(.55 if group in ['jewelry','artifacts'] else 1.1))))
  id=group+'.'+name.lower().replace(' ','_')
  tags=[group]+(['fragile'] if group=='art' and i%2==0 else [])+(['uniqueArtifact'] if group in ['heritage','artifacts'] and i>=5 else [])
  loot.append(dict(id=id,name=name,family=group,shapeId=shape,cells=shapes[shape],weight=weight,baseCredits=value,alertCost=1+(1 if i==7 else 0),tags=tags,setId=['imperial','masquerade','forbidden','celestial','jewels','inventors','heirlooms','antiquities','aristocrats','artmasters'][(gi*2+i//3)%10],rarity=['common','uncommon','rare','legendary'][min(3,tier//2)],artKey='art/loot/'+id,balanced='TUNING_REQUIRED'))
assert len(loot)==48
save('blackbag_48_loot_seed.json',dict(meta=dict(status='UNVALIDATED_PREPRODUCTION',version=1,require='verify masks match drawn silhouettes and bag solver before ship'),shapes={k:[list(p) for p in v] for k,v in shapes.items()},loot=loot))

locations=['private_study','antique_auction','art_gallery','imperial_museum','grand_hotel','collector_archive'];types=['value_run','set_retrieval','fragile_freight','heavy_cargo','tight_suitcase','low_alarm','curators_choice','split_objectives','luxury_gala','mixed_vault']
missions=[]
for li,loc in enumerate(locations):
 for mi in range(12):
  diff=li*12+mi; area='tutorial' if li==0 and mi<2 else 'standard'
  grid=[4,5] if area=='tutorial' else ([5,6] if li==0 else [7,8]); goal=500+li*850+(mi*180)
  pool=[r['id'] for j,r in enumerate(loot) if j%6<=min(5,li+2) and len(r['cells'])<=6]
  missions.append(dict(id=f'bb.{loc}.{mi+1:02d}',displayName=f'{loc.replace("_"," ").title()} - {mi+1}',location=loc,index=mi+1,objective=types[(mi+li)%len(types)],grid={'width':grid[0],'height':grid[1]},bagClass='starter' if li==0 else 'standard',startingAlert=1+(li//3),maxAlert=6,targetCredits=goal,lootPoolIds=pool[:min(len(pool),18)],seed=30503+diff*4999,minimumFreeSolvable=True,validation='UNVALIDATED_REQUIRES_EXACT_POLYOMINO_SOLVER_WITNESS'))
save('blackbag_72_contracts_seed.json',dict(meta=dict(status='PREPRODUCTION_BALANCE_SEED',version=1,warning='Goals and item pools NOT guaranteed feasible. Must solve exact shape+weight+alert constraints for each mission; replace wrong values and human-test.'),missions=missions))

products={'meta':{'status':'WORKING_TEST_PRICES_NOT_STORE_APPROVED','rule':'Display actual localized native store price, never productList price here'},'cashflow':[
('cc.start.01','nonconsumable','1.99','Founder Starter'),('cc.gems.small','consumable','0.99','90 Gems'),('cc.gems.medium','consumable','4.99','520 Gems'),('cc.gems.large','consumable','9.99','1150 Gems'),('cc.reroll.5','consumable','1.99','5 Draft Rerolls'),('cc.blueprint.4','nonconsumable','3.99','4 Blueprint Slots'),('cc.skin.neon','nonconsumable','4.99','Neon City Skin Pack'),('cc.scenario.harbor','nonconsumable','6.99','Harbor Scenarios')],
'blackbag':[
('bb.start.01','nonconsumable','1.99','Initiate Starter'),('bb.coins.small','consumable','0.99','Coin Pack'),('bb.tools.3','consumable','1.99','3 Gadgets'),('bb.tools.10','consumable','4.99','10 Gadgets'),('bb.loadouts.3','nonconsumable','3.99','3 More Saved Loadouts'),('bb.cosmetic.noir','nonconsumable','4.99','Noir Case Set'),('bb.gallery.gilded','nonconsumable','5.99','Gilded Gallery'),('bb.campaign.auction','nonconsumable','7.99','Auction Missions')]}
for g in ('cashflow','blackbag'):
 products[g]=[dict(id=id,type=ty,USD_ILLUSTRATIVE=float(p),title=title,status='DISABLED_UNTIL_APPROVED_AND_FULFILLMENT_TESTED') for id,ty,p,title in products[g]]
save('iap_catalog_proposal.json',products)

report={'contentCounts':{'ccPieceCount':len(ccpieces),'ccMissionCount':len(levels),'bbLootCount':len(loot),'bbMissionCount':len(missions)},'CAUTION':'Seed data generated. Some mechanical models and all 72-level objectives remain unverified until Unity-domain solver and real human playtesting. Do not present as release-complete.'}
save('validation_summary.json',report)
print(report)
