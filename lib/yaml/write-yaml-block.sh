#!/bin/bash
set -euo pipefail

function write-yaml-block {

  # ARGS
  declare -r SOURCE_FILE=${1}
  declare -r INSERT_FROM_FILE=${2}
  declare -r FIND=${3}

  # CONSTANTS
  declare TEMP_FILE
  TEMP_FILE=$(mktemp)

  # VALIDATE

  if [[ ! -f "${SOURCE_FILE}" ]]; then
    echo "❗  Invalid SOURCE_FILE: ${SOURCE_FILE}" >&2
    exit 1
  fi

  if [[ ! -f "${INSERT_FROM_FILE}" ]]; then
    echo "❗  Invalid INSERT_FROM_FILE: ${INSERT_FROM_FILE}" >&2
    exit 1
  fi

  if [[ "${FIND}" == "" ]]; then
    echo "❗  Invalid FIND: ${FIND}" >&2
    exit 1
  fi

  # START

  declare INSERT="|"

  # Loop through each line of the insert file and indent it
  while read -r line;
  do
    INSERT="$INSERT"$'\n'"      $line"
  done < "${INSERT_FROM_FILE}"

  # Loop through each line of the source file and replace occurances
  while IFS= read -r line;
  do
    if [[ $line == *"$FIND"* ]];
    then
      echo "${line/$FIND/$INSERT}" >> "${TEMP_FILE}"
    else
      echo "${line}" >> "${TEMP_FILE}"
    fi
  done < "${SOURCE_FILE}"

  cp "${TEMP_FILE}" "${SOURCE_FILE}"
  rm "${TEMP_FILE}"
}
