#!/bin/bash
set -euo pipefail

function write-yaml-string {

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

  OUTPUT=""

  # Loop through each line of the source file and replace occurances
  while IFS= read -r line;
  do
    if [[ $line == *"$FIND"* ]];
    then
      OUTPUT+="${line/$FIND/$REPLACE}"
    else
      OUTPUT+="${line}"
    fi
    OUTPUT+=$'\n'
  done <<< "${INPUT}"
}
