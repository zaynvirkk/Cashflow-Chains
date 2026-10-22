#!/usr/bin/env python3
"""Package the explicitly labelled developer preview separately from the production player."""
from pathlib import Path
import json,hashlib,zipfile
p=Path(__file__).resolve().parent.parent;folder=Path((p/'Artifacts/latest-content-qa.txt').read_text().strip());report=json.loads((folder/'report.json').read_text());assert report['passed'] and report['production_rejects_fixture_command'] and report['fixture_purchases']==0
for file,h in report['source'].items():assert hashlib.sha256((p/file).read_bytes()).hexdigest()==h
for file,h in report['build'].items():assert hashlib.sha256((p/'Builds/ContentQA'/file).read_bytes()).hexdigest()==h
out=p/'Builds/Deliverables';out.mkdir(parents=True,exist_ok=True);archive=out/'CashflowChains-ContentQA-1.0.zip'
with zipfile.ZipFile(archive,'w',zipfile.ZIP_DEFLATED,compresslevel=6) as z:
 for f in [p/'PlayContentQA.sh',p/'Docs/CONTENT_QA.md',p/'Docs/FONT_LICENSE.txt',*(p/'Docs/Licenses').glob('*.txt'),*(p/'Builds/ContentQA').rglob('*')]:
  if f.is_file() and not any(x in str(f) for x in ['LocalPreview','DoNotShip']):z.write(f,'CashflowChains/'+str(f.relative_to(p)))
 z.writestr('CashflowChains/README-CONTENT-QA.txt','DEVELOPER CONTENT PREVIEW, NOT PRODUCTION\n\nExtract the whole folder and run PlayContentQA.sh. This separate development build prepares an all-content domain fixture in an isolated save, with a permanent QA watermark. No genuine purchase or human progression is demonstrated. Real Google Play billing tests remain separate. Play.sh and the regular archive are the production candidate.\n')
with zipfile.ZipFile(archive) as z:assert z.testzip() is None
(out/'CONTENT-QA.json').write_text(json.dumps({'scope':'Separate development preview; not a payment test or production binary','report':str(folder.relative_to(p)/'report.json'),'file':archive.name,'bytes':archive.stat().st_size,'sha256':hashlib.sha256(archive.read_bytes()).hexdigest()},indent=2));print(archive)
