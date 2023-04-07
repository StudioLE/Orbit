#!/bin/bash
set -euo pipefail

function set-args-by-type {

  # ARGS

  declare TYPE="$1"

  if [[ "${#TYPE}" != 2 ]]
  then
    echo "❗  Invalid TYPE: " >&2
    exit 1
  fi

  # START

  declare CATEGORY="${TYPE:0:1}"
  CORES="${TYPE:1}"

  if [[ "${CATEGORY}" == "C" ]]
  then
    MEMORY=$((CORES * 2))
  elif [[ "${CATEGORY}" == "G" ]]
  then
    MEMORY=$((CORES * 4))
  elif [[ "${CATEGORY}" == "M" ]]
  then
    MEMORY=$((CORES * 8))
  else
    echo "❗  Invalid TYPE: " >&2
    exit 1
  fi

}
