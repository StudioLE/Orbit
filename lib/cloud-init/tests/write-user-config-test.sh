#!/bin/bash
set -uo pipefail

# ARGS
ACTUAL_FILE=$1

# START

/srv/lib/cloud-init/write-user-config.sh "${ACTUAL_FILE}" vm999
