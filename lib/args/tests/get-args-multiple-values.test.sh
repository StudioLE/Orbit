#!/bin/bash
set -uo pipefail

# ARGS
ACTUAL_FILE=$1

# SETUP

source /srv/lib/args/get-args.sh
source /srv/lib/args/echo-args.sh

# START

get-args \
  --string "Value of --string" \
  --int -1 \
  --flag \
  --optional value \
  -s "Value of -s" \
  -a \
  -bc \
  -- \
  /path/to/file \
  path/to/file

echo-args > "${ACTUAL_FILE}"
