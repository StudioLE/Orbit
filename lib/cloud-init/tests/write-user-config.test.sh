#!/bin/bash
set -uo pipefail

# IMPORTS
source /srv/lib/cloud-init/write-user-config.sh

# ARGS
ACTUAL_FILE=$1

# START
OUTPUT=""
write-user-config vm999
echo "${OUTPUT}" > "${ACTUAL_FILE}"
