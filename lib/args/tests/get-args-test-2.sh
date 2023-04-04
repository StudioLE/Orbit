#!/bin/bash
set -uo pipefail

# ARGS
ACTUAL_FILE=$1

# START

/srv/lib/args/get-args.sh \
  --hello \
  > "${ACTUAL_FILE}"
