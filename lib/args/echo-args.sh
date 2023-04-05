#!/bin/bash
set -euo pipefail

function echo-args {

  OUTPUT=""

  for key in "${!ARGS[@]}"
  do
    OUTPUT+="$key: ${ARGS[$key]}"
    OUTPUT+=$'\n'
  done

  OUTPUT=$(echo "${OUTPUT}" | sed '$d')

  echo "${OUTPUT}"
    
}

echo-args
