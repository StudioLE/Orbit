#!/bin/bash
set -uo pipefail

# IMPORTS
source /srv/lib/cloud-init/write-network-config.sh

# ARGS
ACTUAL_FILE=$1

# START
OUTPUT=""
write-network-config 253
echo "${OUTPUT}" > "${ACTUAL_FILE}"
echo "${OUTPUT}"
