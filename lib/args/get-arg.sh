#!/bin/bash
set -euo pipefail

function get-arg {

  declare KEY
  [[ -v "1" ]] && KEY="$1" || KEY=""

  if [[ "${KEY}" == "" ]] 
  then
    echo "❗  Invalid KEY: ${KEY}" >&2
    exit 1
  fi

  declare VALUE
  [[ -v "ARGS[${KEY}]" ]] && VALUE="${ARGS[${KEY}]}" || VALUE=""

  echo "${VALUE}"

}
