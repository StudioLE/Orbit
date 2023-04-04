#!/bin/bash
set -uo pipefail

# ARGS
ACTUAL_FILE=$1

# SETUP

source /srv/lib/args/get-args.sh

# START

get-args \
  --hello \
  > "${ACTUAL_FILE}"
