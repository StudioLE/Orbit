#!/bin/bash
set -uo pipefail

# ARGS
ACTUAL_FILE=$1

# SETUP

source /srv/lib/args/get-args.sh

# START

get_args \
  --string "Hello, world!" \
  --int 1 \
  --flag \
  --optional value \
  /path/to/file \
  path/to/file \
  > "${ACTUAL_FILE}"
