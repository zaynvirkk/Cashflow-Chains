#!/usr/bin/env bash
set -euo pipefail
project_dir="$(cd "$(dirname "$0")" && pwd)"
exec "$project_dir/Builds/ContentQA/CashflowChains-QA.x86_64" -qa-content all -save-dir "$project_dir/Builds/ContentQA/LocalPreview" "$@"
