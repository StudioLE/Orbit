#!/bin/bash
set -uo pipefail

# ARGS
ACTUAL_FILE=$1

/srv/lib/sandbox/shared-variables/parent.sh > "${ACTUAL_FILE}"
