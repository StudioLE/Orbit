#!/bin/bash
set -uo pipefail

# IMPORTS
source /srv/lib/cloud-init/write-user-config.sh

# ARGS
ACTUAL_FILE=$1

# START

write-user-config "${ACTUAL_FILE}" vm999
