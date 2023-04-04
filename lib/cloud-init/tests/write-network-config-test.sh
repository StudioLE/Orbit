#!/bin/bash
set -uo pipefail

# ARGS
ACTUAL_FILE=$1

# START

/srv/lib/cloud-init/write-network-config.sh "${ACTUAL_FILE}" 253
