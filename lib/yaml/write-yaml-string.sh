#!/bin/bash
set -euo pipefail

declare -x OUTPUT

function write-yaml-string {

  # ARGS
  declare -r INPUT=${1}
  declare -r FIND=${2}
  declare -r REPLACE=${3}

  # CONSTANTS
  declare TEMP_FILE
  TEMP_FILE=$(mktemp)

  # VALIDATE

  if [[ "${INPUT}" == "" ]]; then
    echo "❗  Invalid INPUT: ${INPUT}" >&2
    exit 1
  fi

  if [[ "${FIND}" == "" ]]; then
    echo "❗  Invalid FIND: ${FIND}" >&2
    exit 1
  fi

  if [[ "${REPLACE}" == "" ]]; then
    echo "❗  Invalid REPLACE: ${REPLACE}" >&2
    exit 1
  fi

  # START

  # Loop through each line of the source file and replace occurances
  while IFS= read -r line;
  do
    if [[ $line == *"$FIND"* ]];
    then
      echo "${line/$FIND/$REPLACE}" >> "${TEMP_FILE}"
    else
      echo "${line}" >> "${TEMP_FILE}"
    fi
  done <<< "${INPUT}"

  OUTPUT=$(cat "${TEMP_FILE}")
}
