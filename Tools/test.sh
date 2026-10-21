#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
cd "$root"
export DISPLAY="${CASHFLOW_DISPLAY:-:1}"
mkdir -p Artifacts/Tests
DOTNET_PROCESSOR_COUNT=4 taskset -c 0-3 unity test "$root" --mode EditMode --output "$root/Artifacts/Tests/ci.xml" --timeout 600 -- -job-worker-count 2
python3 - "$root/Artifacts/Tests/ci.xml" <<'PYTEST'
import sys,xml.etree.ElementTree as ET
r=ET.parse(sys.argv[1]).getroot();print({k:r.get(k) for k in ['result','total','passed','failed']})
assert r.get('result')=='Passed' and int(r.get('failed','1'))==0
PYTEST
