#!/bin/bash
set -euo pipefail

declare -A ARGS

function get-args {
  declare -a SRC_ARGS
  declare -i SRC_ARGS_COUNT
  declare -i POSITIONAL_INDEX
  SRC_ARGS=( "$@" )
  SRC_ARGS_COUNT=${#SRC_ARGS[@]}
  POSITIONAL_INDEX=0
  declare SKIP_NEXT=""
  declare LONG_REGEX="^--.+$"
  declare SHORT_REGEX="^-[A-Za-z]+$"
  declare ESCAPE_REGEX="^--$"

  for i in $(seq 0 $((SRC_ARGS_COUNT - 1)) )
  do
    declare ARG
    ARG="${SRC_ARGS[$i]}"

    # Skip if ESCAPE_REGEX or if this argument as already been assigned
    if [[  "${ARG}" =~ ${ESCAPE_REGEX} || ${SKIP_NEXT} != "" ]]
    then

      SKIP_NEXT=""
      continue

    # Match a long or short arg name
    elif [[ "${ARG}" =~ (${LONG_REGEX}|${SHORT_REGEX}) ]]
    then
      
      declare NEXT_ARG

      # If it's not the final iteration then define the next arg
      if [[ $i -lt $((SRC_ARGS_COUNT - 1)) ]]
      then
        NEXT_ARG="${SRC_ARGS[$i+1]}"
      else
        NEXT_ARG=""
      fi

      # If the next is a named argument then this must be a flag
      if [[ "${NEXT_ARG}" == "" || "${NEXT_ARG}" =~ (${LONG_REGEX}|${SHORT_REGEX}|${ESCAPE_REGEX}) ]]
      then

        if [[ "${ARG}" =~ ${LONG_REGEX} ]]
        then
          ARGS[$ARG]="TRUE"
        # Short regex should be processed as individual characters
        else
          for ((ii = 1; ii < ${#ARG}; ii++))
          do
            CHAR="${ARG:ii:1}"
            ARGS["-$CHAR"]="TRUE"
          done
        fi

      # Else it's a value so set it and skip the next arg
      else
        ARGS[$ARG]="${NEXT_ARG}"
        SKIP_NEXT="TRUE"
      fi

    # Else it must be a misc arg
    else
      ARGS[$POSITIONAL_INDEX]="${ARG}"
      POSITIONAL_INDEX+=1
    fi
  done
}
