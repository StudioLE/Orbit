#!/bin/bash
set -uo pipefail

# ARGS
ACTUAL_FILE=$1

# SETUP

source /srv/lib/args/get-args.sh
source /srv/lib/args/echo-args.sh

# START

get-args --hello

echo-args > "${ACTUAL_FILE}"
