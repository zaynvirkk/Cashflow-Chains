#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
platform="${1:-linux}"
case "$platform" in content-qa) method=ContentQA; target=StandaloneLinux64;; linux) method=Linux; target=StandaloneLinux64;; android) method=Android; target=Android;; bundle) method=AndroidBundle; target=Android;; *) echo 'Use linux, content-qa, android or bundle' >&2; exit 2;; esac
cd "$root"
mkdir -p Artifacts/Logs
trap 'python3 "$root/Tools/sanitize-logs.py" "$root/Artifacts/Logs/$platform-release-build.log"' EXIT
# One Unity process at a time; cap workers after the laptop memory incident.
DOTNET_PROCESSOR_COUNT=4 taskset -c 0-3 unity build "$root" --target "$target" --execute-method "BuildProject.$method" --args '-job-worker-count 2' --log-file "$root/Artifacts/Logs/$platform-release-build.log" --no-tail
