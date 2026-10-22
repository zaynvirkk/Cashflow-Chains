#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
cd "$root"
export DISPLAY="${CASHFLOW_DISPLAY:-:1}"
mkdir -p Artifacts/Tests
systemd-run --user --scope --quiet -p MemoryHigh=4G -p MemoryMax=5G -p MemorySwapMax=512M -p CPUQuota=200% env DOTNET_PROCESSOR_COUNT=2 BEE_BUILD_THREADS=2 taskset -c 0-1 unity test "$root" --mode EditMode --output "$root/Artifacts/Tests/ci.xml" --timeout 600 -- -job-worker-count 1 -gc-helper-count 1 -desiredWorkerCount 1 -standbyWorkerCount 1
python3 - "$root/Artifacts/Tests/ci.xml" <<'PYTEST'
import sys,xml.etree.ElementTree as ET
r=ET.parse(sys.argv[1]).getroot();print({k:r.get(k) for k in ['result','total','passed','failed']})
assert r.get('result')=='Passed' and int(r.get('failed','1'))==0
PYTEST
