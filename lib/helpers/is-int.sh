#!/bin/bash
set -euo pipefail

function is-int {
  if [[ -v "1" && "$1" =~ ^-?[0-9]+$ ]]
  then
    echo "TRUE"
  else
    echo ""
  fi
}
