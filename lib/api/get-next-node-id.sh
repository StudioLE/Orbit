#!/bin/bash
set -euo pipefail

function get-next-node-id {

  # ARGS
  declare CLUSTER_ID=$1

  if [[ "${CLUSTER_ID}" == "" ||  "${CLUSTER_ID}" -lt "0" || "${CLUSTER_ID}" -gt "99" ]];
  then
      echo "❗  Invalid CLUSTER_ID: ${CLUSTER_ID}" >&2
      exit 1
  fi

  # Get all the VM in the cluster
  declare CLUSTER_VM
  CLUSTER_VM=$(/srv/lib/api/get-vm-by-cluster.sh "${CLUSTER_ID}")

  # Determine the highest ID
  declare ID
  ID=$(echo "$CLUSTER_VM" | jq 'map(.vmid) | max')

  # Get the highest cluster ID
  declare NODE_ID
  NODE_ID=$(/srv/lib/api/get-node-id.sh "$ID")

  declare NEXT_NODE_ID
  NEXT_NODE_ID=$((NODE_ID + 1))

  echo "${NEXT_NODE_ID}"
}

get-next-node-id "$@"
