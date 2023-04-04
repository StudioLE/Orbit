#!/bin/bash
set -euo pipefail

function write-network-config {
  
  # ARGS

  declare -r CONFIG_FILE="${1}"
  declare -ri VM_ID="${2}"

  # CONSTANTS

  declare -r CONFIG_SRC="/srv/lib/cloud-init/src/network-config-template.yml"
  declare -r VM_IP="10.10.10.${VM_ID}"
  declare -r GATEWAY_IP="10.10.10.1"

  # VALIDATE

  if [[ "${VM_ID}" == "" || "${VM_ID}" -lt "100" || "${VM_ID}" -gt "253" ]];
  then
      echo "❗  Invalid VM_ID: ${VM_ID}" >&2
      exit 1
  fi

  # START

  cp "${CONFIG_SRC}"  "${CONFIG_FILE}"
  sed -i "s/\${VM_IP}/${VM_IP}/" "${CONFIG_FILE}"
  sed -i "s/\${GATEWAY_IP}/${GATEWAY_IP}/" "${CONFIG_FILE}"
}
