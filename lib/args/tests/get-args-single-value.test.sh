#!/bin/bash
set -uo pipefail

# ARGS
ACTUAL_FILE=$1

# SETUP

source /srv/lib/args/get-args.sh

# START

get-args --hello

declare OUTPUT

OUTPUT=""

for key in "${!NAMED_ARGS[@]}"
do
  OUTPUT+="$key: ${NAMED_ARGS[$key]}"
  OUTPUT+=$'\n'
done

for key in "${!MISC_ARGS[@]}"
do 
  OUTPUT+="$key: ${MISC_ARGS[$key]}"
  OUTPUT+=$'\n'
done

OUTPUT=$(echo "${OUTPUT}" | sed '$d')

echo "${OUTPUT}" > "${ACTUAL_FILE}"
