#!/bin/bash
set -euo pipefail

function get-all-vm {

  # Request all resources as JSON from the API
  declare ALL_RESOURCES
  ALL_RESOURCES=$(pvesh get /cluster/resources --human-readable 0 --output-format json-pretty)

  # Validate at least 1 resource is returned
  declare COUNT
  COUNT=$(echo "${ALL_RESOURCES}" | jq 'length')
  if [[ ${COUNT} -lt 1 ]];
  then
      echo "❗  No resources found" >&2
      exit 1
  fi

  # Filter to only VM
  declare ALL_VM
  ALL_VM=$(echo "${ALL_RESOURCES}" | jq 'map(select(has("vmid")))')

  echo "${ALL_VM}"
}

get-all-vm
