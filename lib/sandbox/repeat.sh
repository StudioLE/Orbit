#!/bin/bash
set -euo pipefail

function repeat {

  # IMPORTS

  source /srv/lib/validation/is-int.sh

  # ARGS
  
  declare N
  [[ -v "1" && $(is-int "$1") != "" ]] && N="$1"
  
  declare COMMAND
  [[ -v "2" ]] && COMMAND="$1" ||

  # # shellcheck disable=SC2034
  for i in {1..N}
  do
    "${COMMAND}"
  done
}
