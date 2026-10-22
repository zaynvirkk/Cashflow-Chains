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
