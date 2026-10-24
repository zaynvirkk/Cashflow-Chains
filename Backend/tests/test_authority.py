import sys,json,uuid,sqlite3
from pathlib import Path
from concurrent.futures import ThreadPoolExecutor
import pytest
from fastapi.testclient import TestClient
sys.path.insert(0,str(Path(__file__).resolve().parents[1]))
from cc2.app import create_app
ROOT=Path(__file__).resolve().parents[2]
@pytest.fixture
def service(tmp_path):
 clock=[1800000000];path=tmp_path/'state.sqlite';app=create_app(path,clock=lambda:clock[0]);client=TestClient(app);guest=client.post('/v1/auth/guest').json();headers={'Authorization':'Bearer '+guest['accessToken']};return client,headers,clock,path,guest

# Freeze complete fixtures once: Unity may regenerate its export during a build.
REFERENCE_PLANS=(ROOT/'Artifacts/Campaign/witnesses.jsonl').read_text().splitlines()
assert len(REFERENCE_PLANS)==104, 'Generate the complete reference campaign before testing'
def board(level):return json.loads(REFERENCE_PLANS[level])
def snapshot(c,h):return c.get('/v1/account/snapshot',headers=h).json()
def amount(state,item):return sum(b['Earned']+b['Purchased'] for b in state['Economy']['Balances'] if b['ItemId']==item)
def win(c,h,level):
 ticket=c.post('/v1/puzzle/challenge/'+str(level),headers=h);assert ticket.status_code==200,ticket.text
 body={'requestId':uuid.uuid4().hex,'expectedVersion':snapshot(c,h)['version'],'challengeId':ticket.json()['challengeId'],'board':board(level)};r=c.post('/v1/puzzle/verify-and-claim',headers=h,json=body);assert r.status_code==200,r.text;return body,r.json()
def command(c,h,kind,**extra):return c.post('/v1/economy/command',headers=h,json={'requestId':uuid.uuid4().hex,'expectedVersion':snapshot(c,h)['version'],'kind':kind,**extra})

def test_authenticated_accounts_and_rotating_tokens(service):
 c,h,clock,path,g=service;assert c.get('/v1/account/snapshot').status_code==401;assert snapshot(c,h)['accountId']==g['accountId'];r=c.post('/v1/auth/refresh',json={'refreshToken':g['refreshToken']});assert r.status_code==200;assert c.get('/v1/account/snapshot',headers=h).status_code==401;assert c.post('/v1/auth/refresh',json={'refreshToken':g['refreshToken']}).status_code==401

def test_expired_session_cannot_mutate_wallet(service):
 c,h,clock,_,_=service;clock[0]+=3601;assert c.get('/v1/account/snapshot',headers=h).status_code==401

def test_verified_plans_are_account_bound_and_survive_restart(service):
 c,h,clock,path,_=service;assert c.get('/v1/puzzle/plans').status_code==401;body,result=win(c,h,0);assert c.get('/v1/puzzle/plans',headers=h).json()['plans']==[body['board']]
 other=c.post('/v1/auth/guest').json();assert not c.get('/v1/puzzle/plans',headers={'Authorization':'Bearer '+other['accessToken']}).json()['plans']
 restarted=TestClient(create_app(path,clock=lambda:clock[0]));assert restarted.get('/v1/puzzle/plans',headers=h).json()['plans']==[body['board']]
 bad=board(1);bad['Budget']+=100;ticket=c.post('/v1/puzzle/challenge/1',headers=h).json();rejected=c.post('/v1/puzzle/verify-and-claim',headers=h,json={'requestId':uuid.uuid4().hex,'expectedVersion':result['version'],'challengeId':ticket['challengeId'],'board':bad});assert rejected.status_code==409;assert len(c.get('/v1/puzzle/plans',headers=h).json()['plans'])==1

def test_acquisition_uses_authoritative_balances_and_promotes_affordable_upgrade(service):
 c,h,_,_,_=service;assert c.get('/v1/development/home/sources').status_code==401
 for level in range(3):win(c,h,level)
 before=snapshot(c,h);r=c.get('/v1/development/home/sources',headers=h);assert r.status_code==200;r=r.json();assert r['primaryAction']=='upgrade';assert r['beforeHourly']==0 and r['afterHourly']==8 and r['seconds']==30;assert all(x['missing']==0 and not x['sources'] for x in r['requirements']);assert not r['paidRoutes'];assert snapshot(c,h)==before
 assert c.get('/v1/development/unknown/sources',headers=h).status_code==404

def test_source_finder_lists_real_free_routes_without_future_gacha_or_fake_prices(service):
 c,h,clock,_,_=service
 for level in range(3):win(c,h,level)
 built=command(c,h,'build',businessId='home');assert built.status_code==200;assert c.get('/v1/development/home/sources',headers=h).json()['primaryAction']=='builder-busy';clock[0]+=30;assert command(c,h,'finish',projectId=built.json()['state']['Construction'][0]['Id']).status_code==200
 before=snapshot(c,h);r=c.get('/v1/development/home/sources',headers=h).json();assert r['primaryAction']=='find-resources';steel=next(x for x in r['requirements'] if x['itemId']=='STEEL');assert steel['missing']==5;routes=steel['sources'];assert any(x['kind']=='dispatch' and x['templateId']=='route-1' and x['unlocked'] for x in routes);assert any(x['kind']=='production' and not x['available'] for x in routes);assert not r['paidCommerceEnabled'];assert snapshot(c,h)==before

def test_every_dispatch_template_has_a_free_verified_plan_and_exact_return(service, record_property):
 """Replay all free cities and every route; no inventory injection or paid unlock."""
 c,h,clock,_,guest=service
 config=json.loads((ROOT/'Assets/_Game/Data/CC2/balance-v1.json').read_text())
 plans=[board(level) for level in range(72)]
 for level in range(72):
  clock[0]+=4  # Test service clock: respect the challenge request window.
  win(c,h,level)
 built=command(c,h,'build',businessId='home');assert built.status_code==200
 clock[0]+=30;assert command(c,h,'finish',projectId=built.json()['state']['Construction'][0]['Id']).status_code==200
 bindings={}
 for definition in config['Operations']['Dispatches']:
  current=snapshot(c,h)['state']
  if sum(d['StartedAt']//86400==clock[0]//86400 for d in current['Dispatches'])>=config['Operations']['DailyDispatchStarts']:
   clock[0]=(clock[0]//86400+1)*86400
  renewed=c.post('/v1/auth/refresh',json={'refreshToken':guest['refreshToken']});assert renewed.status_code==200
  guest=renewed.json();h={'Authorization':'Bearer '+guest['accessToken']}
  for plan in plans:
   if not any(n['Kind']==definition['RequiredRecipe'] for n in plan['Nodes']):continue
   before=snapshot(c,h)
   started=command(c,h,'dispatch',template=definition['Id'],board=plan)
   if started.status_code==409:
    assert snapshot(c,h)==before
    continue
   assert started.status_code==200,started.text
   break
  else:pytest.fail('No freely earned contributing plan for '+definition['Id'])
  bindings[definition['Id']]=plan['Challenge']
  dispatched=started.json()['state'];job=dispatched['Dispatches'][-1]
  for cost in definition['Costs']:
   assert amount(dispatched,cost['ItemId'])==amount(before['state'],cost['ItemId'])-cost['Quantity']
  assert job['FinishAt']-job['StartedAt']==definition['Seconds']
  clock[0]=job['FinishAt']
  renewed=c.post('/v1/auth/refresh',json={'refreshToken':guest['refreshToken']});assert renewed.status_code==200
  guest=renewed.json();h={'Authorization':'Bearer '+guest['accessToken']}
  returned=command(c,h,'dispatch-claim',projectId=job['Id']);assert returned.status_code==200,returned.text
  for reward in definition['Rewards']:
   assert amount(returned.json()['state'],reward['ItemId'])==amount(dispatched,reward['ItemId'])+reward['Quantity']
  assert all(b['Purchased']==0 for b in returned.json()['state']['Economy']['Balances'])
 record_property('free_plan_bindings',json.dumps(bindings,sort_keys=True))
 late=[{'route':d['Id'],'advertisedClears':d['UnlockClears'],'firstAuthoredPlanClears':bindings[d['Id']]+1}
       for d in config['Operations']['Dispatches'] if bindings[d['Id']]>=d['UnlockClears']]
 assert not late, 'Route appears before its authored teaching plan: '+json.dumps(late)

def test_proof_replay_and_changed_request_payload(service):
 c,h,_,_,_=service;body,r=win(c,h,0);assert amount(r['state'],'CASH')==80;assert c.post('/v1/puzzle/verify-and-claim',headers=h,json=body).json()==r
 body['board']['Nodes'][0]['Rotation']=1;assert c.post('/v1/puzzle/verify-and-claim',headers=h,json=body).status_code==409;assert amount(snapshot(c,h)['state'],'CASH')==80

def test_other_account_cannot_use_challenge(service):
 c,h,_,_,_=service;ticket=c.post('/v1/puzzle/challenge/0',headers=h).json();other=c.post('/v1/auth/guest').json();other_h={'Authorization':'Bearer '+other['accessToken']};body={'requestId':uuid.uuid4().hex,'expectedVersion':0,'challengeId':ticket['challengeId'],'board':board(0)};assert c.post('/v1/puzzle/verify-and-claim',headers=other_h,json=body).status_code==409

def test_invalid_constraints_never_grant_resources(service):
 c,h,_,_,_=service;ticket=c.post('/v1/puzzle/challenge/0',headers=h).json();b=board(0);b['Budget']+=1000;body={'requestId':uuid.uuid4().hex,'expectedVersion':0,'challengeId':ticket['challengeId'],'board':b};assert c.post('/v1/puzzle/verify-and-claim',headers=h,json=body).status_code==409;assert snapshot(c,h)['version']==0

def test_client_cannot_submit_balance_grant_or_time(service):
 c,h,_,_,_=service
 for field,value in [('CASH',1000000),('TrustedNow',2000000000),('AccountId','other')]:
  body={'requestId':uuid.uuid4().hex,'expectedVersion':0,'kind':'collect',field:value};assert c.post('/v1/economy/command',headers=h,json=body).status_code==422
 assert command(c,h,'build',businessId='home').status_code==409;assert snapshot(c,h)['version']==0

def test_stale_version_rejects_without_replacing_state(service):
 c,h,_,_,_=service;win(c,h,0);r=c.post('/v1/economy/command',headers=h,json={'requestId':uuid.uuid4().hex,'expectedVersion':0,'kind':'collect'});assert r.status_code==409;assert amount(snapshot(c,h)['state'],'CASH')==80

def test_concurrent_same_request_and_database_restart_are_idempotent(service):
 c,h,clock,path,_=service;body,response=win(c,h,0)
 with ThreadPoolExecutor(max_workers=4) as pool:results=list(pool.map(lambda _:c.post('/v1/puzzle/verify-and-claim',headers=h,json=body),range(8)))
 assert all(r.status_code==200 and r.json()==response for r in results);restarted=TestClient(create_app(path,clock=lambda:clock[0]));assert amount(snapshot(restarted,h)['state'],'CASH')==80;assert snapshot(restarted,h)['version']==1

def test_first_empire_loop_uses_server_clock_and_durable_construction(service):
 c,h,clock,path,g=service
 for level in range(3):win(c,h,level)
 r=command(c,h,'build',businessId='home');assert r.status_code==200,r.text;state=r.json()['state'];assert amount(state,'CASH')==180;project=state['Construction'][0]
 assert command(c,h,'finish',projectId=project['Id']).status_code==409;clock[0]=project['FinishAt'];assert command(c,h,'finish',projectId=project['Id']).status_code==200;clock[0]+=86400;renew=c.post('/v1/auth/refresh',json={'refreshToken':g['refreshToken']});assert renew.status_code==200;h={'Authorization':'Bearer '+renew.json()['accessToken']};assert command(c,h,'collect').status_code==200;assert amount(snapshot(c,h)['state'],'CASH')==244;assert command(c,h,'collect').status_code==200;assert amount(snapshot(c,h)['state'],'CASH')==244

def test_failed_domain_command_rolls_back_request_and_wallet(service):
 c,h,_,path,_=service
 for level in range(3):win(c,h,level)
 before=snapshot(c,h);assert command(c,h,'build',businessId='exchange').status_code==409;after=snapshot(c,h);assert before['state']==after['state'];db=sqlite3.connect(path);assert db.execute('SELECT count(*) FROM requests').fetchone()[0]==3;db.close()

def test_expired_challenge_and_locked_expansion(service):
 c,h,clock,_,g=service;ticket=c.post('/v1/puzzle/challenge/0',headers=h).json();assert c.post('/v1/puzzle/challenge/71',headers=h).status_code==403;assert c.post('/v1/puzzle/challenge/72',headers=h).status_code==403;clock[0]=ticket['expiresAt']+1;refresh=c.post('/v1/auth/refresh',json={'refreshToken':g['refreshToken']}).json();headers={'Authorization':'Bearer '+refresh['accessToken']};body={'requestId':uuid.uuid4().hex,'expectedVersion':0,'challengeId':ticket['challengeId'],'board':board(0)};assert c.post('/v1/puzzle/verify-and-claim',headers=headers,json=body).status_code==409

def test_billing_unconfigured_is_not_a_success_or_a_grant(service):
 c,h,_,_,_=service;assert c.post('/v1/iap/validate-and-fulfill',headers=h).status_code==503;assert snapshot(c,h)['version']==0;assert c.get('/v1/config/manifest').json()['paidCommerceEnabled'] is False

def test_config_drift_requires_an_explicit_database_migration(service):
 c,h,clock,path,_=service
 with sqlite3.connect(path) as db:db.execute("UPDATE metadata SET value='different' WHERE key='content_hash'")
 with pytest.raises(RuntimeError):create_app(path,clock=lambda:clock[0])

def test_dispatch_and_research_are_durable_and_cannot_be_double_claimed(service):
 c,h,clock,path,g=service
 for level in range(6):win(c,h,level)
 built=command(c,h,'build',businessId='home').json()['state']['Construction'][0];clock[0]=built['FinishAt'];assert command(c,h,'finish',projectId=built['Id']).status_code==200
 for n in range(3):
  sent=command(c,h,'dispatch',template='route-1',board=board(0));assert sent.status_code==200,sent.text;d=sent.json()['state']['Dispatches'][-1];clock[0]=d['FinishAt'];returned=command(c,h,'dispatch-claim',projectId=d['Id']);assert returned.status_code==200,returned.text;cash=amount(returned.json()['state'],'CASH');assert command(c,h,'dispatch-claim',projectId=d['Id']).status_code==200;assert amount(snapshot(c,h)['state'],'CASH')==cash
 started=command(c,h,'research-start',nodeId='operations-1');assert started.status_code==200,started.text;research=started.json()['state']['ResearchQueue'][0];assert command(c,h,'research-finish',projectId=research['Id']).status_code==409;clock[0]=research['FinishAt'];finished=command(c,h,'research-finish',projectId=research['Id']);assert finished.status_code==200;assert 'operations-1' in finished.json()['state']['Research'];assert amount(finished.json()['state'],'RECRUIT_TICKET')==1

def test_dispatch_rejects_unverified_layout_and_wrong_speedup_currency(service):
 c,h,clock,path,g=service
 for level in range(3):win(c,h,level)
 built=command(c,h,'build',businessId='home').json()['state']['Construction'][0];clock[0]=built['FinishAt'];command(c,h,'finish',projectId=built['Id']);bad=board(0);bad['Nodes'][1]['InitialGoods']=64;before=snapshot(c,h)['state'];assert command(c,h,'dispatch',template='route-1',board=bad).status_code==409;assert snapshot(c,h)['state']==before
 sent=command(c,h,'dispatch',template='route-1',board=board(0)).json()['state']['Dispatches'][0];assert command(c,h,'speedup',projectId=sent['Id'],sourceItem='GEMS',minutes=1).status_code==409


def found_home(c,h,clock):
 assert command(c,h,'city-build',businessId='home',x=1,y=1).status_code==200
 job=snapshot(c,h)['state']['City']['Jobs'][0];clock[0]=job['FinishAt']
 assert command(c,h,'city-finish',projectId=job['Id']).status_code==200
 assert amount(snapshot(c,h)['state'],'CASH')==120


def test_permanent_city_has_finite_demand_real_bills_and_no_early_settlement(service):
 c,h,clock,_,_=service
 assert c.get('/v1/city/forecast').status_code==401
 assert c.get('/v1/city/forecast',headers=h).status_code==409
 assert command(c,h,'city-open').status_code==200
 assert amount(snapshot(c,h)['state'],'CASH')==200
 assert command(c,h,'city-open').status_code==200
 assert amount(snapshot(c,h)['state'],'CASH')==200
 found_home(c,h,clock)
 assert command(c,h,'city-build',businessId='cafe',x=2,y=1).status_code==200
 project=snapshot(c,h)['state']['City']['Jobs'][0]
 assert command(c,h,'city-finish',projectId=project['Id']).status_code==409
 clock[0]=project['FinishAt']
 assert command(c,h,'city-finish',projectId=project['Id']).status_code==200
 before=snapshot(c,h)
 forecast=c.get('/v1/city/forecast',headers=h).json()
 assert forecast['Population']==4 and forecast['Adults']==2 and forecast['Children']==2 and forecast['CoffeeDemand']==2
 assert 0<forecast['CoffeeServed']<=2
 assert forecast['Net']==forecast['Revenue']-forecast['Bills']-forecast['ImportBills']-forecast['Tax']
 assert snapshot(c,h)==before
 assert command(c,h,'city-start-day').status_code==200
 frozen=c.get('/v1/city/forecast?current=true',headers=h).json()
 assert command(c,h,'city-settle-day',day=1).status_code==409
 assert command(c,h,'city-price',propertyId=2,priceBasisPoints=12500).status_code==200
 assert c.get('/v1/city/forecast?current=true',headers=h).json()==frozen
 clock[0]=snapshot(c,h)['state']['City']['Trading']['FinishAt']
 settled=command(c,h,'city-settle-day',day=1);assert settled.status_code==200,settled.text
 after=snapshot(c,h);assert after['state']['City']['Day']==2
 assert amount(after['state'],'CASH')==amount(before['state'],'CASH')+frozen['Net']
 assert command(c,h,'city-settle-day',day=1).status_code==200
 assert snapshot(c,h)==after


def test_city_clock_and_investment_quote_use_authoritative_time_and_actual_materials(service):
 c,h,clock,_,_=service
 assert command(c,h,'city-open').status_code==200
 found_home(c,h,clock)
 before=snapshot(c,h);q=c.get('/v1/city/investment?kind=cafe&x=2&y=1',headers=h).json()
 assert q['Affordable'] and q['BuilderAvailable']
 assert {x['ItemId']:x['Quantity'] for x in q['Costs']}=={'CASH':100,'WOOD':4}
 assert snapshot(c,h)==before
 assert command(c,h,'city-build',businessId='cafe',x=2,y=1).status_code==200
 clock[0]+=8
 assert command(c,h,'city-advance').status_code==200
 a=snapshot(c,h);assert a['state']['City']['Day']==2
 assert amount(a['state'],'CASH')==42
 assert command(c,h,'city-advance').status_code==200
 assert snapshot(c,h)==a
 assert c.get('/v1/city/investment?kind=not-a-business',headers=h).status_code==409
 assert command(c,h,'city-materials',itemId='GEMS',quantity=1).status_code==409
 assert snapshot(c,h)==a

def test_financing_repayment_and_material_orders_are_server_ledger_transactions(service):
 c,h,clock,_,_=service;assert command(c,h,'city-open').status_code==200
 found_home(c,h,clock)
 assert command(c,h,'city-build',businessId='cafe',x=2,y=1).status_code==200
 clock[0]+=30;assert command(c,h,'city-advance').status_code==200
 assert snapshot(c,h)['state']['City']['Day']>=4
 cash=amount(snapshot(c,h)['state'],'CASH')
 borrowed=command(c,h,'city-finance',amount=100);assert borrowed.status_code==200,borrowed.text
 loan=borrowed.json()['state']['City']['Loans'][0];assert loan['Remaining']==110
 assert amount(snapshot(c,h)['state'],'CASH')==cash+100
 assert command(c,h,'city-repay',loanId=loan['Id'],amount=50).status_code==200
 assert snapshot(c,h)['state']['City']['Loans'][0]['Remaining']==60
 assert amount(snapshot(c,h)['state'],'CASH')==cash+50
 before=snapshot(c,h);assert command(c,h,'city-repay',loanId=loan['Id'],amount=61).status_code==409
 assert snapshot(c,h)==before
 assert command(c,h,'city-materials',itemId='WOOD',quantity=2).status_code==200
 assert amount(snapshot(c,h)['state'],'CASH')==cash+44


def test_one_home_census_and_vacancies_are_server_derived_not_client_claims(service):
 c,h,clock,_,_=service
 assert command(c,h,'city-open').status_code==200
 empty=c.get('/v1/city/forecast',headers=h).json();assert empty['Population']==0 and empty['HousingCapacity']==0
 assert not snapshot(c,h)['state']['City']['Properties']
 found_home(c,h,clock)
 before=snapshot(c,h)
 census=c.get('/v1/city/forecast',headers=h).json()
 assert (census['Households'],census['Population'],census['Adults'],census['Children'])==(1,4,2,2)
 assert census['CoffeeDemand']==2 and census['HousingCapacity']==4
 assert snapshot(c,h)==before
 body={'requestId':'forged-household-count','expectedVersion':before['version'],'kind':'city-open','population':12}
 assert c.post('/v1/economy/command',headers=h,json=body).status_code==422
 assert snapshot(c,h)==before


def test_existing_city_context_has_public_people_but_no_unowned_income(service):
 c,h,clock,_,_=service
 assert command(c,h,'city-open').status_code==200
 assert command(c,h,'city-enter-region').status_code==200
 before=snapshot(c,h);r=c.get('/v1/city/forecast',headers=h).json()
 assert (r['PublicResidents'],r['Population'],r['PublicJobs'],r['Revenue'],r['Net'])==(48,48,24,0,0)
 assert not before['state']['City']['Properties'] and amount(before['state'],'CASH')==200
 assert command(c,h,'city-enter-region').status_code==200
 assert snapshot(c,h)['state']['Economy']['Revision']==before['state']['Economy']['Revision']
 assert command(c,h,'city-build',businessId='cafe',x=2,y=1).status_code==409
 assert command(c,h,'city-build',businessId='home',x=1,y=1).status_code==200
 clock[0]+=9
 assert command(c,h,'city-advance').status_code==200
 after=snapshot(c,h)['state'];assert after['City']['CareerStage']>=2
 assert after['City']['EarnedCashflow']>0 and amount(after,'CASH')>120
 assert not any(p['Kind']=='cafe' for p in after['City']['Properties'])
 assert command(c,h,'city-toggle',propertyId=1102,open=False).status_code==409


def test_dense_city_acquisition_has_no_free_assets_and_retains_existing_people(service):
 c,h,clock,_,_=service
 assert command(c,h,'city-open').status_code==200
 assert command(c,h,'city-enter-region').status_code==200
 assert command(c,h,'city-enter-metropolis').status_code==200
 before=c.get('/v1/city/forecast',headers=h).json();assert before['Population']>80 and before['Revenue']==0
 assert not snapshot(c,h)['state']['City']['Properties']
 assert command(c,h,'city-acquire',propertyId=1100).status_code==409
 assert command(c,h,'city-acquire',propertyId=12027).status_code==200
 state=snapshot(c,h)['state'];assert amount(state,'CASH')==120 and len(state['City']['Properties'])==1
 clock[0]+=9
 assert command(c,h,'city-advance').status_code==200
 after=c.get('/v1/city/forecast',headers=h).json();assert after['Population']==before['Population']
 assert after['PrivateResidents']>0 and after['Revenue']>0
 assert snapshot(c,h)['state']['City']['CareerStage']>=2


def test_investor_daily_is_account_bound_idempotent_and_keeps_its_place_after_a_missed_visit(service):
 c,h,clock,_,_=service
 for kind in ('city-open','city-enter-region','city-enter-metropolis','investor-open'):
  assert command(c,h,kind).status_code==200
 assert command(c,h,'investor-daily').status_code==200
 first=snapshot(c,h);assert amount(first['state'],'CASH')==210
 assert command(c,h,'investor-daily').status_code==200
 assert snapshot(c,h)['state']['Economy']['Revision']==first['state']['Economy']['Revision']
 clock[0]+=4*86400
 fresh=c.post('/v1/auth/refresh',json={'refreshToken':service[4]['refreshToken']}).json();h={'Authorization':'Bearer '+fresh['accessToken']}
 assert command(c,h,'investor-daily').status_code==200
 claims=snapshot(c,h)['state']['Investor']['Claims'];assert [x['Index'] for x in claims]==[0,1]
 before=snapshot(c,h)
 for kind in ('investor-daily-premium','investor-verified-purchase','investor-season-premium'):
  assert c.post('/v1/economy/command',headers=h,json={'requestId':uuid.uuid4().hex,'expectedVersion':before['version'],'kind':kind}).status_code==422
 assert snapshot(c,h)==before


def test_real_acquisition_advances_investor_tracks_and_earned_chest_only_once(service):
 c,h,clock,_,_=service
 for kind in ('city-open','city-enter-region','city-enter-metropolis','investor-open'):
  assert command(c,h,kind).status_code==200
 assert command(c,h,'investor-milestone',level=0).status_code==409
 assert command(c,h,'city-acquire',propertyId=12027).status_code==200
 clock[0]+=9
 assert command(c,h,'city-advance').status_code==200
 state=snapshot(c,h)['state'];assert sum(x['Xp'] for x in state['Investor']['Activity'])==40
 assert command(c,h,'investor-season',day=0,level=0).status_code==200
 assert command(c,h,'investor-milestone',level=1).status_code==200
 body={'requestId':uuid.uuid4().hex,'expectedVersion':snapshot(c,h)['version'],'kind':'investor-chest'}
 first=c.post('/v1/economy/command',headers=h,json=body);assert first.status_code==200
 assert c.post('/v1/economy/command',headers=h,json=body).json()==first.json()
 assert amount(snapshot(c,h)['state'],'CHEST:bronze')==0
 assert command(c,h,'investor-chest').status_code==409
 assert c.post('/v1/iap/validate-and-fulfill',headers=h,json={'product':'cc2.seasonpass','verified':True}).status_code==503
