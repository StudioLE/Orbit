#!/bin/bash
set -euo pipefail

function get-arg-by-value {

  # IMPORTS

  source /srv/lib/args/get-args.sh

  # START

  echo "SRC ARGS COUNT: ${#}"

  get-args "$@"

  echo "REVISED ARGS COUNT: ${#ARGS[@]}"

  echo "--int: ${ARGS["--int"]}"
    
}

get-arg-by-value "$@"

