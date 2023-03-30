#!/bin/bash
set -euo pipefail

function set-type-by-args {

  # IMPORTS

  source /srv/lib/echo/echo-error.sh

  # ARGS

  declare CORES="$1"
  declare MEMORY="$2"

  # START

  declare MULTIPLIER
  MULTIPLIER=$((MEMORY / CORES))
  declare MOD
  MOD=$((MEMORY % CORES))
  declare CATEGORY

  if [[ "${MOD}" == 0 && "${MULTIPLIER}" == 2 ]]
  then
    CATEGORY="C"
  elif [[ "${MOD}" == 0 &&  "${MULTIPLIER}" == 4 ]]
  then
    CATEGORY="G"
  elif [[ "${MOD}" == 0 &&  "${MULTIPLIER}" == 8 ]]
  then
    CATEGORY="M"
  else
    echo-error "A valid TYPE could not be determined. MEMORY / CORES must be either 2, 4, or 8."
    exit 1
  fi
  
  TYPE="${CATEGORY}${CORES}"

}
