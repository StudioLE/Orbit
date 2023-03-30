#!/bin/bash
set -uo pipefail

# ARGS
ACTUAL_FILE=$1

source /srv/lib/echo/echo-error.sh

declare OUTPUT=""

set +e
OUTPUT+=$(echo-error "An error has occurred." 2>&1)
EXIT_CODE=$?
set -e
OUTPUT+=$'\n'
OUTPUT+="EXIT_CODE: ${EXIT_CODE}"

OUTPUT+=$'\n'

set +e
OUTPUT+=$(echo-error "An error has occurred." 2>&1; exit 99)
EXIT_CODE=$?
set -e
OUTPUT+=$'\n'
OUTPUT+="EXIT_CODE: ${EXIT_CODE}"

echo "${OUTPUT}" > "${ACTUAL_FILE}"
