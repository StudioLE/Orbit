#!/bin/bash
set -euo pipefail

# ARGS
ACTUAL_FILE=$1

source /srv/lib/helpers/validate.sh
source /srv/lib/helpers/is-int.sh

declare OUTPUT=""

for i in -1 0 1 10 100000 0.5 -0.5 1.0 .0 "" 1
do
  OUTPUT+="$i: "
  set +e
  OUTPUT+=$(validate "is-int" "$i" 2>&1)
  EXIT_CODE=$?
  set -e
  OUTPUT+="; EXIT_CODE: ${EXIT_CODE}"
  OUTPUT+=$'\n'
done

echo "${OUTPUT}" > "${ACTUAL_FILE}"
