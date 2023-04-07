#!/bin/bash
set -euo pipefail

function get-node-id {

  # ARGS
  declare ID=$1

  if [[ "${ID}" == "" ||  "${ID}" -lt "100" || "${ID}" -gt "9999" ]];
  then
      echo "❗  Invalid ID: ${ID}" >&2
      exit 1
  fi

  # START
  
  declare CLUSTER_ID="${ID:2}"

  echo "${CLUSTER_ID}"
}

get-node-id "$@"
