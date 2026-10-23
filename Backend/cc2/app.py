"""Authenticated, durable local service slice. Paid commerce/live features remain unavailable.
This app is not configured or deployed for production. TLS, hosting, external identity,
Play verification and operational acceptance are separate release gates.
"""
from pathlib import Path
import hashlib,hmac,json,secrets,sqlite3,time,uuid,os
from contextlib import contextmanager
from typing import Literal
from fastapi import FastAPI,HTTPException,Header,Request
from pydantic import BaseModel,ConfigDict,Field
from .domain import Domain,ROOT
from .acquisition import development_sources

def compact(value):return json.dumps(value,separators=(',',':'),sort_keys=True)
def digest(value):return hashlib.sha256(value.encode()).hexdigest()
class Strict(BaseModel):model_config=ConfigDict(extra='forbid',strict=True)
class Node(Strict):
 Id:int=Field(ge=1,le=1000000);X:int=Field(ge=0,le=6);Y:int=Field(ge=0,le=6);Rotation:int=Field(ge=0,le=3);Kind:str=Field(min_length=1,max_length=40);InitialGoods:int=Field(default=0,ge=0,le=64);InitialEnergy:int=Field(default=0,ge=0,le=64)
class Board(Strict):
 Width:int=Field(ge=1,le=7);Height:int=Field(ge=1,le=7);Budget:int=Field(ge=0,le=1000000);Challenge:int=Field(ge=0,le=103);ImportQuota:int=Field(default=0,ge=0,le=64);ImportCost:int=Field(default=0,ge=0,le=10);DraftRoll:int=Field(default=0,ge=0,le=0);DraftMode:Literal[False]=False;DraftOperations:list[str]=Field(default_factory=list,max_length=0);Blocked:int=Field(default=0,ge=0,le=2**49-1);Nodes:list[Node]=Field(max_length=49)
class Command(Strict):
 requestId:str=Field(min_length=16,max_length=100,pattern=r'^[A-Za-z0-9_-]+$');expectedVersion:int=Field(ge=0);kind:Literal['build','finish','collect','dispatch','dispatch-claim','research-start','research-finish','speedup'];businessId:str|None=Field(default=None,max_length=40);projectId:str|None=Field(default=None,max_length=160);template:str|None=Field(default=None,max_length=80);nodeId:str|None=Field(default=None,max_length=80);sourceItem:str|None=Field(default=None,max_length=80);minutes:int=Field(default=0,ge=0,le=1000000);board:Board|None=None
class RoundClaim(Strict):
 requestId:str=Field(min_length=16,max_length=100,pattern=r'^[A-Za-z0-9_-]+$');expectedVersion:int=Field(ge=0);challengeId:str=Field(min_length=16,max_length=100);board:Board
class Refresh(Strict):refreshToken:str=Field(min_length=32,max_length=200)

def create_app(database:Path,clock=time.time):
 database=Path(database);database.parent.mkdir(parents=True,exist_ok=True,mode=0o700);domain=Domain();config=json.loads((ROOT/'Assets/_Game/Data/CC2/balance-v1.json').read_text());content_hash=digest(''.join(str(f.relative_to(ROOT))+f.read_text() for f in sorted((ROOT/'Assets/_Game/Scripts/Domain').rglob('*.cs')))+(ROOT/'Assets/_Game/Data/CC2/balance-v1.json').read_text());secret_file=database.parent/'server-key.bin'
 if not secret_file.exists():
  fd=os.open(secret_file,os.O_WRONLY|os.O_CREAT|os.O_EXCL,0o600)
  with os.fdopen(fd,'wb') as f:f.write(secrets.token_bytes(32))
 secret=secret_file.read_bytes()
 @contextmanager
 def connect(write=False):
  conn=sqlite3.connect(database,timeout=25,isolation_level=None);conn.row_factory=sqlite3.Row;conn.execute('PRAGMA foreign_keys=ON');conn.execute('PRAGMA synchronous=FULL')
  try:
   if write:conn.execute('BEGIN IMMEDIATE')
   yield conn
   if write:conn.commit()
  except BaseException:
   if write:conn.rollback()
   raise
  finally:conn.close()
 with connect() as db:
  db.execute('PRAGMA journal_mode=WAL')
  db.executescript('''CREATE TABLE IF NOT EXISTS metadata(key TEXT PRIMARY KEY,value TEXT NOT NULL);
  CREATE TABLE IF NOT EXISTS accounts(id TEXT PRIMARY KEY,state TEXT NOT NULL,version INTEGER NOT NULL);
  CREATE TABLE IF NOT EXISTS sessions(token TEXT PRIMARY KEY,refresh TEXT UNIQUE NOT NULL,account TEXT NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,expires INTEGER NOT NULL,refresh_expires INTEGER NOT NULL);
  CREATE TABLE IF NOT EXISTS requests(account TEXT NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,id TEXT NOT NULL,hash TEXT NOT NULL,response TEXT NOT NULL,PRIMARY KEY(account,id));
  CREATE TABLE IF NOT EXISTS challenges(id TEXT PRIMARY KEY,account TEXT NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,level INTEGER NOT NULL,content_hash TEXT NOT NULL,expires INTEGER NOT NULL);
  CREATE TABLE IF NOT EXISTS puzzle_plans(account TEXT NOT NULL REFERENCES accounts(id) ON DELETE CASCADE,level INTEGER NOT NULL,board TEXT NOT NULL,PRIMARY KEY(account,level));
  CREATE TABLE IF NOT EXISTS rates(bucket TEXT NOT NULL,window INTEGER NOT NULL,count INTEGER NOT NULL,PRIMARY KEY(bucket,window));''')
 with connect(True) as db:
  previous=db.execute("SELECT value FROM metadata WHERE key='content_hash'").fetchone()
  if previous and previous['value']!=content_hash:raise RuntimeError('Domain/catalog changed: explicit migration required before using this database')
  db.execute("INSERT OR IGNORE INTO metadata VALUES('content_hash',?)",(content_hash,))
 os.chmod(database,0o600)
 app=FastAPI(title='Cashflow CC2 local authority slice',docs_url=None,redoc_url=None,openapi_url=None)
 @app.middleware('http')
 async def bound_request(request:Request,call_next):
  from fastapi.responses import JSONResponse
  try:length=int(request.headers.get('content-length','0'))
  except ValueError:return JSONResponse({'detail':'Invalid body length'},status_code=400)
  if length>100000 or len(await request.body())>100000:return JSONResponse({'detail':'Request too large'},status_code=413)
  return await call_next(request)
 def now():return int(clock())
 def rate(db,bucket,limit,period=60):
  window=now()//period*period;row=db.execute('SELECT count FROM rates WHERE bucket=? AND window=?',(bucket,window)).fetchone()
  if row and row['count']>=limit:raise HTTPException(429,'Request limit reached')
  db.execute('INSERT INTO rates VALUES(?,?,1) ON CONFLICT(bucket,window) DO UPDATE SET count=count+1',(bucket,window));db.execute('DELETE FROM rates WHERE window<?',(now()-172800,))
 def identity(db,authorization):
  if not authorization or not authorization.startswith('Bearer '):raise HTTPException(401,'Authentication required')
  row=db.execute('SELECT account,expires FROM sessions WHERE token=?',(digest(authorization[7:]),)).fetchone()
  if not row or row['expires']<now():raise HTTPException(401,'Session expired or invalid')
  return row['account']
 def session(db,account):
  token=secrets.token_urlsafe(40);refresh=secrets.token_urlsafe(40);db.execute('INSERT INTO sessions VALUES(?,?,?,?,?)',(digest(token),digest(refresh),account,now()+3600,now()+30*86400));return {'accountId':account,'accessToken':token,'refreshToken':refresh,'expiresAt':now()+3600}
 @app.get('/health')
 def health():return {'status':'local-service-slice','paidCommerceEnabled':False,'configRevision':config['Revision']}
 @app.post('/v1/auth/guest')
 def guest(request:Request):
  with connect(True) as db:
   peer=request.client.host if request.client else 'unknown';bucket=hmac.new(secret,peer.encode(),hashlib.sha256).hexdigest();rate(db,'guest:'+bucket,20,3600);account=uuid.uuid4().hex;state=domain.apply({'AccountId':account,'Command':'create','TrustedNow':now()});db.execute('INSERT INTO accounts VALUES(?,?,0)',(account,compact(state)));return session(db,account)
 @app.post('/v1/auth/refresh')
 def refresh(body:Refresh):
  with connect(True) as db:
   row=db.execute('SELECT account,refresh_expires FROM sessions WHERE refresh=?',(digest(body.refreshToken),)).fetchone()
   if not row or row['refresh_expires']<now():raise HTTPException(401,'Invalid refresh')
   db.execute('DELETE FROM sessions WHERE refresh=?',(digest(body.refreshToken),));return session(db,row['account'])
 @app.get('/v1/account/snapshot')
 def snapshot(authorization:str|None=Header(default=None)):
  with connect() as db:account=identity(db,authorization);row=db.execute('SELECT state,version FROM accounts WHERE id=?',(account,)).fetchone();return {'accountId':account,'version':row['version'],'serverTime':now(),'state':json.loads(row['state'])}
 @app.get('/v1/config/manifest')
 def manifest():return {'revision':config['Revision'],'contentHash':content_hash,'enabledFeatures':['certified-rewards','construction','production','dispatch','research','completed-plans','acquisition-guidance'],'paidCommerceEnabled':False}
 @app.get('/v1/puzzle/plans')
 def plans(authorization:str|None=Header(default=None)):
  with connect() as db:
   account=identity(db,authorization);rows=db.execute('SELECT board FROM puzzle_plans WHERE account=? ORDER BY level',(account,)).fetchall();return {'plans':[json.loads(row['board']) for row in rows],'serverTime':now(),'contentHash':content_hash}
 @app.get('/v1/development/{business_id}/sources')
 def sources(business_id:str,authorization:str|None=Header(default=None)):
  with connect() as db:
   account=identity(db,authorization);state=json.loads(db.execute('SELECT state FROM accounts WHERE id=?',(account,)).fetchone()['state'])
   if not any(b['Id']==business_id for b in config['Businesses']):raise HTTPException(404,'Unknown business')
   quote=domain.apply({'AccountId':account,'Command':'development-quote','BusinessId':business_id,'State':state,'TrustedNow':now()})
   return development_sources(config,state,business_id,now(),quote)
 @app.post('/v1/puzzle/challenge/{level}')
 def challenge(level:int,authorization:str|None=Header(default=None)):
  if not 0<=level<72:raise HTTPException(403,'Expansion ownership is not yet server-verified')
  with connect(True) as db:
   account=identity(db,authorization);rate(db,'challenge:'+account,20);state=json.loads(db.execute('SELECT state FROM accounts WHERE id=?',(account,)).fetchone()['state']);clears=sum(r['CommandId'].startswith('certified-first-clear:') for r in state['Economy']['Receipts'])
   if level>min(71,clears+2):raise HTTPException(403,'Challenge locked')
   id=secrets.token_urlsafe(24);expires=now()+72*3600;db.execute('INSERT INTO challenges VALUES(?,?,?,?,?)',(id,account,level,content_hash,expires));return {'challengeId':id,'accountId':account,'level':level,'expiresAt':expires,'contentHash':content_hash}
 def execute(body,kind,authorization,board=None):
  with connect(True) as db:
   account=identity(db,authorization);payload=body.model_dump();request_hash=digest(compact(payload));prior=db.execute('SELECT hash,response FROM requests WHERE account=? AND id=?',(account,body.requestId)).fetchone()
   if prior:
    if prior['hash']!=request_hash:raise HTTPException(409,'Request id reused with different payload')
    return json.loads(prior['response'])
   rate(db,'command:'+account,60);row=db.execute('SELECT state,version FROM accounts WHERE id=?',(account,)).fetchone()
   if row['version']!=body.expectedVersion:raise HTTPException(409,'Snapshot version changed; refresh before retrying')
   command={'AccountId':account,'State':json.loads(row['state']),'Command':kind,'RequestId':body.requestId,'TrustedNow':now()}
   if kind=='round':
    ticket=db.execute('SELECT * FROM challenges WHERE id=? AND account=?',(body.challengeId,account)).fetchone()
    if not ticket or ticket['expires']<now() or ticket['content_hash']!=content_hash:raise HTTPException(409,'Challenge expired or invalid')
    if body.board.Challenge!=ticket['level']:raise HTTPException(409,'Challenge proof mismatch')
    command.update(Level=ticket['level'],Board=body.board.model_dump())
   else:command.update(BusinessId=body.businessId,ProjectId=body.projectId,Template=body.template,NodeId=body.nodeId,SourceItem=body.sourceItem,Minutes=body.minutes,Board=body.board.model_dump() if body.board else None)
   try:state=domain.apply(command)
   except (ValueError,TimeoutError):raise HTTPException(409,'Command rejected; no resources changed')
   version=state['Economy']['Revision'];response={'version':version,'serverTime':now(),'state':state};db.execute('UPDATE accounts SET state=?,version=? WHERE id=?',(compact(state),version,account));db.execute('INSERT INTO requests VALUES(?,?,?,?)',(account,body.requestId,request_hash,compact(response)))
   if kind=='round':db.execute('INSERT INTO puzzle_plans VALUES(?,?,?) ON CONFLICT(account,level) DO UPDATE SET board=excluded.board',(account,body.board.Challenge,compact(body.board.model_dump())))
   return response
 @app.post('/v1/puzzle/verify-and-claim')
 def claim(body:RoundClaim,authorization:str|None=Header(default=None)):return execute(body,'round',authorization)
 @app.post('/v1/economy/command')
 def command(body:Command,authorization:str|None=Header(default=None)):return execute(body,body.kind,authorization)
 @app.post('/v1/iap/validate-and-fulfill')
 def iap_unavailable(authorization:str|None=Header(default=None)):
  with connect() as db:identity(db,authorization)
  raise HTTPException(503,'Play server verification is not configured; no grant or confirmation occurred')
 return app
