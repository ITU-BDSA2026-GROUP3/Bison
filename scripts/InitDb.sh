#!/usr/bin/env bash

set -euo pipefail

repo_root="$(cd -- "$(dirname -- "${BASH_SOURCE[0]}")/.." && pwd)"
temporary_directory="$(mktemp -d "${TMPDIR:-/tmp}/bison-db.XXXXXX")"
trap 'rm -rf -- "$temporary_directory"' EXIT

mkdir -p "$temporary_directory/data"

cd "$temporary_directory"
sqlite3 :memory: < "$repo_root/scripts/sql/setup.sql"
