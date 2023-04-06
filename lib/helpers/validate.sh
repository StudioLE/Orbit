#!/bin/bash
set -euo pipefail

function validate {
  
  # IMPORTS

  source /srv/lib/helpers/echo-error.sh

  # START

  if [[ ! -v "1" ]]
  then
    echo-error "validate: \$1 was not set."
    exit 1
  
  elif [[ "$1" =~ ^-?[0-9]+$ ]]
  then
    echo "TRUE"
  else
    echo ""
  fi
}
