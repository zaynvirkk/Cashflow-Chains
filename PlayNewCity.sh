#!/usr/bin/env bash
set -euo pipefail
project_dir="$(cd "$(dirname "$0")" && pwd)"
city_save_dir="${XDG_DATA_HOME:-$HOME/.local/share}/virkgames/CashflowChains-city3"
mkdir -p "$city_save_dir"
# Ordinary release gameplay,with its own persistent save. Existing cities are preserved.
exec "$project_dir/Builds/Linux/CashflowChains.x86_64" -save-dir "$city_save_dir" -screen-fullscreen 1 -screen-width 1280 -screen-height 720 "$@"
