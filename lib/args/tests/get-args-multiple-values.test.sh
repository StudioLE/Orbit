#!/bin/bash
set -uo pipefail

# ARGS
ACTUAL_FILE=$1

# SETUP

source /srv/lib/args/get-args.sh

# START

get-args \
  --string "Hello, world!" \
  --int 1 \
  --flag \
  --optional value \
  /path/to/file \
  path/to/file

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

echo "${OUTPUT}"
echo "${OUTPUT}" > "${ACTUAL_FILE}"
