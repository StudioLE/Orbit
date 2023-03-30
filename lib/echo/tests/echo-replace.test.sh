#!/bin/bash
set -uo pipefail

# ARGS
ACTUAL_FILE=$1

# START

OUTPUT=$(/srv/lib/echo/tests/echo-replace-test.sh)

echo "${OUTPUT}" > "${ACTUAL_FILE}"
