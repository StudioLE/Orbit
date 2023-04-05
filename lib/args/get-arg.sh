#!/bin/bash
set -euo pipefail

function get-arg {

  declare KEY
  [[ -v "1" ]] && KEY="$1" || KEY=""

  declare DEFAULT
  [[ -v "2" ]] && DEFAULT="$2" || DEFAULT=""

  if [[ "${KEY}" == "" ]] 
  then
    echo "❗  Invalid KEY: ${KEY}" >&2
    exit 1
  fi

  declare VALUE
  [[ -v "ARGS[${KEY}]" ]] && VALUE="${ARGS[${KEY}]}" || VALUE="${DEFAULT}"

  echo "${VALUE}"

}
