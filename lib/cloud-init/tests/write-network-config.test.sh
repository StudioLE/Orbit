#!/bin/bash
set -uo pipefail

# IMPORTS
source /srv/lib/cloud-init/write-network-config.sh

# ARGS
ACTUAL_FILE=$1

# START

write-network-config 253
echo "${RETURN_VALUE_2}" > "${ACTUAL_FILE}"
