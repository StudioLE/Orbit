#!/bin/bash
set -uo pipefail

# ARGS
ACTUAL_FILE=$1

source /srv/lib/helpers/exit-error.sh

declare OUTPUT=""

set +e
OUTPUT+=$(exit-error "An error has occurred." 99 2>&1)
EXIT_CODE=$?
set -e
OUTPUT+=$'\n'
OUTPUT+="EXIT_CODE: ${EXIT_CODE}"

echo "${OUTPUT}" > "${ACTUAL_FILE}"
