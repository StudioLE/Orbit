#!/bin/bash
set -euo pipefail

function get-arg {

  # ARGS

  if [[ ! -v "ARGS" ]]
  then
    echo "❗  get-arg has been called before get-args!" >&2
    exit 1
  fi

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
