#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
cd "$root"
python3 Tools/verify-package.py
mkdir -p Artifacts/Tests
unity test "$root" --mode EditMode --output "$root/Artifacts/Tests/ci.xml" --timeout 600
