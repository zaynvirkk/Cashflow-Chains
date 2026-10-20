#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")" && pwd)"
exec "$root/Builds/Linux/CashflowChains.x86_64" -screen-fullscreen 0 -screen-width 540 -screen-height 960 "$@"
