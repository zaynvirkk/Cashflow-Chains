#!/usr/bin/env bash
set -euo pipefail
project_dir="$(cd "$(dirname "$0")" && pwd)"
exec "$project_dir/Builds/EmpireSandbox/CashflowChains-Empire.x86_64" -cc2-empire-sandbox yes -save-dir "$project_dir/Builds/EmpireSandbox/LocalEmpire" "$@"
