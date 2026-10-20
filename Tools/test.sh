#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
mkdir -p "$root/Artifacts/Tests"
unity command run_tests --caller plugin --skill unity-cli --project-path "$root" --timeout 300 --format json -- --mode editor --filter CashflowChains.Tests --filter_type assembly --timeout 300 > "$root/Artifacts/Tests/latest.json"
python3 - "$root/Artifacts/Tests/latest.json" <<'PYTEST'
import json,sys
x=json.load(open(sys.argv[1]));r=x.get('data',{}).get('result',{});s=r.get('Summary',r.get('summary',{}));print(s)
if not x.get('success') or not s or s.get('Failed',s.get('failed',-1))!=0 or s.get('Passed',s.get('passed',0))<1:raise SystemExit(1)
PYTEST
