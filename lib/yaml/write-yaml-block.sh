#!/bin/bash
set -euo pipefail

function write-yaml-block {

  # IMPORTS

  source /srv/lib/yaml/write-yaml-string.sh

  # ARGS

  declare -r INPUT=${1}
  declare -r FIND=${2}
  declare -r REPLACE=${3}

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

  declare BLOCK="|"

  # Loop through each line of the insert file and indent it
  while read -r line;
  do
    BLOCK+=$'\n'
    BLOCK+="      $line"
  done <<< "${REPLACE}"

  echo "$BLOCK"

  write-yaml-string "${INPUT}" "${FIND}" "${BLOCK}"
}
