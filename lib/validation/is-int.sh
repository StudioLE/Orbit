#!/bin/bash
set -euo pipefail

function is-int {

  # IMPORTS

  source /srv/lib/echo/echo-error.sh

  # START

  if [[ ! -v "1" ]]
  then
    echo-error "is-int: Argument \$1 was not set."
    exit 1
  elif [[ "$1" =~ ^-?[0-9]+$ ]]
  then
    echo "TRUE"
  else
    echo ""
  fi
}
