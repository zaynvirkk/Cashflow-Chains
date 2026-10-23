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

def board(level):return json.loads((ROOT/'Artifacts/Campaign/witnesses.jsonl').read_text().splitlines()[level])
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
 before=snapshot(c,h);r=c.get('/v1/development/home/sources',headers=h);assert r.status_code==200;r=r.json();assert r['primaryAction']=='upgrade';assert all(x['missing']==0 and not x['sources'] for x in r['requirements']);assert not r['paidRoutes'];assert snapshot(c,h)==before
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
