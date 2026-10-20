#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
platform="${1:-linux}"
case "$platform" in linux) method=Linux;; android) method=Android;; *) echo 'Use linux or android' >&2;exit 2;; esac
mkdir -p "$root/Artifacts/Logs"
unity command eval --caller plugin --skill unity-cli --project-path "$root" --timeout 1800 --format json -- --code "BuildProject.$method(); return \"Build complete\";" --timeout 1800000 > "$root/Artifacts/Logs/build-$platform.json"
python3 - "$root/Artifacts/Logs/build-$platform.json" <<'PYBUILD'
import json,sys
r=json.load(open(sys.argv[1]));print(r)
if not r.get('success') or not r.get('data',{}).get('result',{}).get('success'):raise SystemExit(1)
PYBUILD
