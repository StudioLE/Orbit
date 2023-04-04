#!/bin/bash
set -euo pipefail

declare -x RETURN_VALUE

function write-yaml-string {

  # ARGS
  declare -r SOURCE=${1}
  declare -r INSERT=${2}
  declare -r FIND=${3}

  # echo "SOURCE: ${SOURCE}"
  # echo "INSERT: ${INSERT}"
  # echo "FIND: ${FIND}"

  # CONSTANTS
  declare TEMP_FILE
  TEMP_FILE=$(mktemp)

  # VALIDATE

  if [[ "${SOURCE}" == "" ]]; then
    echo "❗  Invalid SOURCE: ${SOURCE}" >&2
    exit 1
  fi

  if [[ "${INSERT}" == "" ]]; then
    echo "❗  Invalid INSERT: ${INSERT}" >&2
    exit 1
  fi

  if [[ "${FIND}" == "" ]]; then
    echo "❗  Invalid FIND: ${FIND}" >&2
    exit 1
  fi

  # START

  # Loop through each line of the source file and replace occurances
  while IFS= read -r line;
  do
    if [[ $line == *"$FIND"* ]];
    then
      echo "${line/$FIND/$INSERT}" >> "${TEMP_FILE}"
    else
      echo "${line}" >> "${TEMP_FILE}"
    fi
  done <<< "${SOURCE}"

  RETURN_VALUE=$(cat "${TEMP_FILE}")
}
