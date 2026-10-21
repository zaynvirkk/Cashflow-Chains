#!/usr/bin/env bash
set -euo pipefail
project_dir="$(cd "$(dirname "$0")" && pwd)"
exec "$project_dir/Builds/Linux/CashflowChains.x86_64" -screen-fullscreen 0 -screen-width 540 -screen-height 960 "$@"
