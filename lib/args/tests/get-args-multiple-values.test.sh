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
  -s "Short, value" \
  -a \
  -bc \
  -1 "Short int" \
  -23 \
  -- \
  /path/to/file \
  path/to/file

declare OUTPUT

OUTPUT=""

for key in "${!ARGS[@]}"
do
  OUTPUT+="$key: ${ARGS[$key]}"
  OUTPUT+=$'\n'
done

OUTPUT=$(echo "${OUTPUT}" | sed '$d')

echo "${OUTPUT}" > "${ACTUAL_FILE}"
