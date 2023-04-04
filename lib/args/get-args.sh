#!/bin/bash
set -euo pipefail

SRC_ARRAY=( "$@" )
SRC_ARRAY_COUNT=${#SRC_ARRAY[@]}

declare -A NAMED_ARGS=()
declare -a MISC_ARGS=()

SKIP_NEXT=""
for i in $(seq 0 $((SRC_ARRAY_COUNT - 1)) )
do
  ARG="${SRC_ARRAY[$i]}"

  # Skip if this argument as already been assigned
  if [[ ${SKIP_NEXT} != "" ]]
  then

    SKIP_NEXT=""
    continue

  # Match a long arg name
  elif [[ "${ARG}" =~ ^--.* ]]
  then
  
    # If it's not the final iteration then define the next arg
    if [[ $i -lt $((SRC_ARRAY_COUNT - 1)) ]]
    then
      NEXT_ARG="${SRC_ARRAY[$i+1]}"
    else
      NEXT_ARG=""
    fi

    # If the next is a named argument then this must be a flag
    if [[ "${NEXT_ARG}" =~ ^--.* ]]
    then
      NAMED_ARGS[${ARG}]="TRUE"

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

for key in "${!NAMED_ARGS[@]}"
do 
  echo "$key: ${NAMED_ARGS[$key]}"
done

for key in "${!MISC_ARGS[@]}"
do 
  echo "$key: ${MISC_ARGS[$key]}"
done
