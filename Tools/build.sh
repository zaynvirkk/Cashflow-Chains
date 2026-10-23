#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/.." && pwd)"
platform="${1:-linux}"
case "$platform" in empire-sandbox) method=EmpireSandbox; target=StandaloneLinux64;; content-qa) method=ContentQA; target=StandaloneLinux64;; linux) method=Linux; target=StandaloneLinux64;; android) method=Android; target=Android;; bundle) method=AndroidBundle; target=Android;; *) echo 'Use linux, content-qa, empire-sandbox, android or bundle' >&2; exit 2;; esac
cd "$root"
build_workers="${CASHFLOW_BUILD_WORKERS:-1}"
case "$build_workers" in 1|2) ;; *) echo 'CASHFLOW_BUILD_WORKERS must be1or2' >&2; exit 2;; esac
if [[ "$build_workers" == 2 ]]; then
  build_available_kib="$(awk '/MemAvailable:/{print $2}' /proc/meminfo)"
  if (( build_available_kib < 8388608 )); then echo 'Two workers require at least8GiB currently available RAM;use the default single worker.' >&2; exit 2; fi
fi
mkdir -p Artifacts/Logs
trap 'python3 "$root/Tools/sanitize-logs.py" "$root/Artifacts/Logs/$platform-release-build.log"' EXIT
# One Unity process at a time; cap workers after the laptop memory incident.
# Keep the hard6GiB cap;5500MiB soft pressure avoids repeated reclaim
# at the measured~5.1GiB code-generation working set.
# One compiler task at a time after a two-worker build hit system memory pressure.
# Bound the whole compiler tree, including IL2CPP/Bee subprocesses. CPU affinity
# alone did not bound native compiler jobs and oomd killed the terminal scope.
systemd-run --user --scope --quiet -p MemoryHigh=5500M -p MemoryMax=6G -p MemorySwapMax=512M -p CPUQuota="$((build_workers*100))%" env DOTNET_PROCESSOR_COUNT=1 BEE_BUILD_THREADS="$build_workers" IL2CPP_ADDITIONAL_ARGS="--jobs=1 --bee-jobs=$build_workers" taskset -c "0-$((build_workers-1))" unity build "$root" --target "$target" --execute-method "BuildProject.$method" --args '-job-worker-count 1 -gc-helper-count 1 -desiredWorkerCount 1 -standbyWorkerCount 1' --log-file "$root/Artifacts/Logs/$platform-release-build.log" --no-tail
