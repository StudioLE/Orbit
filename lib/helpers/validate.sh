#!/bin/bash
set -euo pipefail

function validate {
  
  # IMPORTS

  source /srv/lib/helpers/echo-error.sh

  # ARGS

  if [[ ! -v "1" ]]
  then
    echo-error "validate: COMMAND was not set."
    exit 1
  elif [[ "$1" == "" ]]
  then
    echo-error "validate: COMMAND was empty."
    exit 1
  fi

  declare COMMAND="$1"

  if [[ ! -v "2" ]]
  then
    echo-error "validate: ARG was not set."
    exit 1
  fi

  declare ARG="$2"

  # START

  set +e
  OUTPUT=$("${COMMAND}" "${ARG}")
  EXIT_CODE=$?
  ERROR=$("${COMMAND}" "${ARG}" 2>&1 > /dev/null)
  set -e

  # If the command has errored then exit
  if [[ "${EXIT_CODE}" != 0 ]]
  then
    [[ "${OUTPUT}" != "" ]] && echo "${OUTPUT}"
    [[ "${ERROR}" != "" ]] && echo-error "${ERROR}"
    exit ${EXIT_CODE}
  # If the command returned invalid then exit
  elif [[ "${OUTPUT}" == "" ]]
  then
    echo-error "Failed validation ${COMMAND}: ${ARG}"
    exit 1
  # Else it has succeeded
  else
    echo "${OUTPUT}"
  fi
}
