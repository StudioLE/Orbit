#!/bin/bash
set -euo pipefail

function get-next-cluster-id {

  # Get all the VM
  declare ALL_VM
  ALL_VM=$(/srv/lib/api/get-all-vm.sh)

  # Determine the highest ID
  declare ID
  ID=$(echo "$ALL_VM" | jq 'map(.vmid) | sort | last')

  # Get the highest cluster ID
  declare CLUSTER_ID
  CLUSTER_ID=$(/srv/lib/api/get-cluster-id.sh "$ID")

  declare NEXT_CLUSTER_ID
  NEXT_CLUSTER_ID=$((CLUSTER_ID + 1))

  echo "${NEXT_CLUSTER_ID}"
}

get-next-cluster-id
