#!/bin/bash
set -euo pipefail

function get-cluster-id {

  # ARGS
  declare ID=$1

  if [[ "${ID}" == "" ||  "${ID}" -lt "100" || "${ID}" -gt "9999" ]];
  then
      echo "❗  Invalid ID: ${ID}" >&2
      exit 1
  fi

  # START
  
  declare CLUSTER_ID

  if [[ "${#ID}" == 4 ]]
  then
    CLUSTER_ID="${ID:(-4):2}"
  elif [[ "${#ID}" == 3 ]]
  then
    CLUSTER_ID="${ID:(-3):1}"
  fi

  echo "${CLUSTER_ID}"
}

get-cluster-id "$@"
