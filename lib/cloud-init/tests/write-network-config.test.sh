#!/bin/bash
set -uo pipefail

# IMPORTS
source /srv/lib/cloud-init/write-network-config.sh

# ARGS
ACTUAL_FILE=$1

# START

write-network-config "${ACTUAL_FILE}" 253
