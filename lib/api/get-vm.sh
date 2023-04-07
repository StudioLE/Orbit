#!/bin/bash
set -euo pipefail

function get-vm {

  # ARGS
  declare ID=$1

  # VALIDATE

  if [[ "${ID}" == "" ||  "${ID}" -lt "100" || "${ID}" -gt "9999" ]];
  then
      echo "❗  Invalid ID: ${ID}" >&2
      exit 1
  fi

  # START

  # Get all vm
  declare ALL_VM
  ALL_VM=$(/srv/lib/api/get-all-vm.sh)

  # Select by VM ID
  declare VM_WITH_ID
  VM_WITH_ID=$(echo "${ALL_VM}" | jq "map(select(.vmid == ${ID}))")

  # Validate only one VM has that ID
  COUNT=$(echo "${VM_WITH_ID}" | jq 'length')
  if [[ ${COUNT} -lt 1 ]];
  then
      echo "❗  VM ${ID} does not exist" >&2
      exit 1
  elif [[ ${COUNT} -gt 1 ]];
  then
      echo "❗  Multiple VMs with ${ID} were found" >&2
      exit 1
  fi

  # Select the VM
  declare STATUS
  STATUS=$(echo "${VM_WITH_ID}" | jq '.[]')
  echo "$STATUS"

}

get-vm "$@"

