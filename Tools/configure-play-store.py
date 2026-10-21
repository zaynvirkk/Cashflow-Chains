#!/usr/bin/env python3
"""Prepare native store configuration from the real app's public licensing key; no account calls."""
from pathlib import Path
import argparse,base64,json,subprocess,tempfile
p=Path(__file__).resolve().parent.parent;a=argparse.ArgumentParser();a.add_argument('--public-key-file',type=Path,required=True);a.add_argument('--enable',action='store_true');args=a.parse_args()
key=''.join(args.public_key_file.read_text().split());raw=base64.b64decode(key,validate=True)
with tempfile.NamedTemporaryFile() as f:
 f.write(raw);f.flush();subprocess.run(['openssl','pkey','-pubin','-inform','DER','-in',f.name,'-noout'],check=True,stdout=subprocess.DEVNULL,stderr=subprocess.DEVNULL)
file=p/'Assets/_Game/Resources/StoreConfiguration.json';file.write_text(json.dumps({'EnableGooglePlay':args.enable,'GooglePublicKeyBase64':key},indent=2)+'\n');print('Native store configuration prepared. Rebuild and complete real Play sandbox tests; no store was contacted.')
