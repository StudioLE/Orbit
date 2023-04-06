#!/bin/bash
set -euo pipefail

function validate {
  
  # IMPORTS

  source /srv/lib/helpers/echo-error.sh

  # ARGS

  if [[ ! -v "1" ]]
  then
    echo-error "validate: \$1 was not set."
    exit 1
  elif [[ "$1" == "" ]]
  then
    echo-error "validate: \$1 was empty."
    exit 1
  fi

  # START

  declare COMMAND=$1
  set +e
  OUTPUT=$(${COMMAND} 2>&1)
  EXIT_CODE=$?
  set -e

  if [[ "${EXIT_CODE}" != 0 ]]
  then
    echo-error "${OUTPUT}"
    exit ${EXIT_CODE}
  else
    echo "${OUTPUT}"
  fi
}
