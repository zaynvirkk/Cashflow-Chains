"""Adapter to the exact shared C# domain, not a second economic implementation."""
from pathlib import Path
import subprocess,tempfile,json,sys,os
ROOT=Path(__file__).resolve().parents[2]
sys.path.insert(0,str(ROOT/'Tools'))
from cc2_domain import UNITY
class Domain:
 def __init__(self):
  if os.environ.get('CC2_DOMAIN_DLL'):
   self.host=os.environ.get('CC2_DOTNET','dotnet');self.assembly=os.environ['CC2_DOMAIN_DLL'];return
  self.host=str(UNITY/'NetCoreRuntime/dotnet');self.assembly=str(ROOT/'Builds/CC2Tools/CC2Tools.dll')
  subprocess.run([sys.executable,str(ROOT/'Tools/cc2_domain.py'),'validate',str(ROOT/'Assets/_Game/Data/CC2/balance-v1.json')],check=True,cwd=ROOT,stdout=subprocess.DEVNULL)
 def apply(self,request):
  with tempfile.NamedTemporaryFile(mode='w',suffix='.json',encoding='utf-8') as file:
   json.dump(request,file);file.flush()
   result=subprocess.run([self.host,self.assembly,'apply',str(ROOT/'Assets/_Game/Data/CC2/balance-v1.json'),file.name],cwd=ROOT,text=True,capture_output=True,timeout=20)
  if result.returncode:raise ValueError('Domain command rejected')
  return json.loads(result.stdout)
