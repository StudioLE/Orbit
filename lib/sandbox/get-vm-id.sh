#!/bin/bash
set -euo pipefail

function get-vm-id {

  # IMPORTS

  source /srv/lib/args/get-args.sh
  source /srv/lib/args/get-arg.sh

  # ARGS

  get-args "$@"

  declare CLUSTER_ID
  CLUSTER_ID=$(get-arg "--cluster")

  if [[ "${CLUSTER_ID}" == "" ||  "${CLUSTER_ID}" -lt "1" || "${CLUSTER_ID}" -gt "99" ]];
  then
      echo "❗  Invalid CLUSTER_ID: ${CLUSTER_ID}" >&2
      exit 1
  fi

  declare NODE_ID
  NODE_ID=$(get-arg "--node")

  if [[ "${NODE_ID}" == "" ||  "${NODE_ID}" -lt "0" || "${NODE_ID}" -gt "99" ]];
  then
      echo "❗  Invalid NODE_ID: ${NODE_ID}" >&2
      exit 1
  elif [[ "${#NODE_ID}" != 2 ]]
  then
      NODE_ID="0${NODE_ID}"
  fi

  echo "${CLUSTER_ID}${NODE_ID}"

}

get-vm-id "$@"
