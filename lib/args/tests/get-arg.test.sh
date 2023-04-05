#!/bin/bash
set -uo pipefail

# ARGS
ACTUAL_FILE=$1

# SETUP

source /srv/lib/args/get-args.sh
source /srv/lib/args/get-arg.sh

get-args \
  --string "Value of --string" \
  --int -1 \
  --flag \
  --optional value \
  -s "Value of -s" \
  -a \
  -bc \
  -- \
  /path/to/file \
  path/to/file

# START

declare OUTPUT

OUTPUT=""

STRING=$(get-arg "--string")
OUTPUT+="--string: ${STRING}"
OUTPUT+=$'\n'

FLAG=$(get-arg "--flag")
OUTPUT+="--flag: ${FLAG}"
OUTPUT+=$'\n'

NOT_SET=$(get-arg "--not-set")
OUTPUT+="--no-set: ${NOT_SET}"
OUTPUT+=$'\n'

ZERO=$(get-arg "0")
OUTPUT+="0: ${ZERO}"
OUTPUT+=$'\n'

A=$(get-arg "-a")
OUTPUT+="-a: ${A}"

echo "${OUTPUT}" > "${ACTUAL_FILE}"
