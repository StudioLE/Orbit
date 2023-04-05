#!/bin/bash
set -uo pipefail

# ARGS
ACTUAL_FILE=$1

# SETUP

source /srv/lib/boot-disk/create-boot-disk.sh

# START

create-boot-disk "ubuntu" "jammy" "20G"
echo "${OUTPUT}" > "${ACTUAL_FILE}"
