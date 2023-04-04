#!/bin/bash
set -uo pipefail

# ARGS
ACTUAL_FILE=$1

# START

/srv/lib/args/get-args.sh \
  --string "Hello, world!" \
  --int 1 \
  --flag \
  --optional value \
  /path/to/file \
  path/to/file \
  > "${ACTUAL_FILE}"
