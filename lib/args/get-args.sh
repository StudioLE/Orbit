#!/bin/bash
set -euo pipefail

declare -Ax NAMED_ARGS=()
declare -ax MISC_ARGS=()

function get-args {
  declare -a ARGS
  declare -i ARGS_COUNT
  ARGS=( "$@" )
  ARGS_COUNT=${#ARGS[@]}
  declare SKIP_NEXT=""

  for i in $(seq 0 $((ARGS_COUNT - 1)) )
  do
    declare ARG
    ARG="${ARGS[$i]}"

    # Skip if this argument as already been assigned
    if [[ ${SKIP_NEXT} != "" ]]
    then

      SKIP_NEXT=""
      continue

    # Match a long arg name
    elif [[ "${ARG}" =~ ^--.* ]]
    then
      
      declare NEXT_ARG

      # If it's not the final iteration then define the next arg
      if [[ $i -lt $((ARGS_COUNT - 1)) ]]
      then
        NEXT_ARG="${ARGS[$i+1]}"
      else
        NEXT_ARG=""
      fi

      # If the next is a named argument then this must be a flag
      if [[ "${NEXT_ARG}" =~ ^--.* ]]
      then
        NAMED_ARGS[$ARG]="TRUE"

      # Else it's a value so set it and skip the next arg
      else
        NAMED_ARGS[$ARG]="${NEXT_ARG}"
        SKIP_NEXT="TRUE"
      fi

    # Else it must be a misc arg
    else

      MISC_ARGS+=("${ARG}")

    fi
  done
}
