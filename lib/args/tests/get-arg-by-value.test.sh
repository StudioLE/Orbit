#!/bin/bash
set -uo pipefail

# ARGS
ACTUAL_FILE=$1

# START

OUTPUT=$(/srv/lib/args/tests/get-arg-by-value.sh \
  --string "Value of --string" \
  --int -1 \
  --flag \
  --optional value \
  -s "Value of -s" \
  -a \
  -bc \
  -- \
  /path/to/file \
  path/to/file)

echo "${OUTPUT}" > "${ACTUAL_FILE}"
