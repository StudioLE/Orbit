#!/bin/bash
set -euo pipefail


function write-yaml-array {

  # IMPORTS

  source /srv/lib/yaml/write-yaml-string.sh

  # ARGS

  declare -r INPUT=${1}
  declare -r FIND=${2}
  declare -r REPLACE=${3}
  declare -ri INDENT_COUNT="${4}"

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

  if [[ "${INDENT_COUNT}" == "" ]]; then
    echo "❗  Invalid INDENT_COUNT: ${INDENT_COUNT}" >&2
    exit 1
  fi

  # START

  declare INDENT=""

  # Create the correct number of indents
  for i in $(seq 1 "${INDENT_COUNT}");
  do
    INDENT+="  "
  done

  declare ARRAY=""

  # Loop through each line of the replacement content and indent it
  while read -r line;
  do
    ARRAY+=$'\n'
    ARRAY+="${INDENT}- $line"
  done <<< "${REPLACE}"

  write-yaml-string "${INPUT}" "${FIND}" "${ARRAY}"
}
