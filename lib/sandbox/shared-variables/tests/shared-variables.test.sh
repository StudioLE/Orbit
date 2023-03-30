#!/bin/bash
set -uo pipefail

# ARGS
ACTUAL_FILE=$1

source /srv/lib/sandbox/shared-variables/parent.sh

RESULT=$(parent)

echo "${RESULT}" > "${ACTUAL_FILE}"
