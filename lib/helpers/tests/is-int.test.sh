#!/bin/bash
set -uo pipefail

# ARGS
ACTUAL_FILE=$1

source /srv/lib/helpers/is-int.sh

declare OUTPUT=""

for i in -1 0 1 10 100000 0.5 -0.5 1.0 .0
do
  OUTPUT+="$i: "
  OUTPUT+=$(is_int "$i")
  OUTPUT+=$'\n'
done

echo "${OUTPUT}" > "${ACTUAL_FILE}"
