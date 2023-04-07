#!/bin/bash
set -euo pipefail

function get-vm-by-cluster {

  # ARGS
  declare CLUSTER_ID=$1

  if [[ "${CLUSTER_ID}" == "" ||  "${CLUSTER_ID}" -lt "0" || "${CLUSTER_ID}" -gt "99" ]];
  then
      echo "❗  Invalid CLUSTER_ID: ${CLUSTER_ID}" >&2
      exit 1
  fi

  # Get all the VM
  declare ALL_VM
  ALL_VM=$(/srv/lib/api/get-all-vm.sh)

  # Get all in cluster
  declare MIN="${CLUSTER_ID}00"
  declare MAX="${CLUSTER_ID}99"
  declare CLUSTER_VM
  CLUSTER_VM=$(echo "$ALL_VM" | jq "map(select(.vmid >= ${MIN} and .vmid <= ${MAX}))")

  echo "${CLUSTER_VM}"
}

get-vm-by-cluster "$@"
