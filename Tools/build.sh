#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
platform="${1:-linux}"
case "$platform" in empire-sandbox) method=EmpireSandbox; target=StandaloneLinux64;; content-qa) method=ContentQA; target=StandaloneLinux64;; linux) method=Linux; target=StandaloneLinux64;; android) method=Android; target=Android;; bundle) method=AndroidBundle; target=Android;; *) echo 'Use linux, content-qa, empire-sandbox, android or bundle' >&2; exit 2;; esac
cd "$root"
mkdir -p Artifacts/Logs
trap 'python3 "$root/Tools/sanitize-logs.py" "$root/Artifacts/Logs/$platform-release-build.log"' EXIT
# One Unity process at a time; cap workers after the laptop memory incident.
# Keep the hard6GiB cap;5500MiB soft pressure avoids repeated reclaim
# at the measured~5.1GiB code-generation working set.
# One compiler task at a time after a two-worker build hit system memory pressure.
# Bound the whole compiler tree, including IL2CPP/Bee subprocesses. CPU affinity
# alone did not bound native compiler jobs and oomd killed the terminal scope.
systemd-run --user --scope --quiet -p MemoryHigh=5500M -p MemoryMax=6G -p MemorySwapMax=512M -p CPUQuota=100% env DOTNET_PROCESSOR_COUNT=1 BEE_BUILD_THREADS=1 IL2CPP_ADDITIONAL_ARGS='--jobs=1 --bee-jobs=1' taskset -c 0 unity build "$root" --target "$target" --execute-method "BuildProject.$method" --args '-job-worker-count 1 -gc-helper-count 1 -desiredWorkerCount 1 -standbyWorkerCount 1' --log-file "$root/Artifacts/Logs/$platform-release-build.log" --no-tail
