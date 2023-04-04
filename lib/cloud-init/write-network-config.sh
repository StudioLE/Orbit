#!/bin/bash
set -euo pipefail

declare -x RETURN_VALUE_2

function write-network-config {
  
  # IMPORTS

  source /srv/lib/yaml/write-yaml-string.sh
  
  # ARGS

  declare -ri VM_ID="${1}"

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
  OUTPUT=$(cat "${CONFIG_SRC}")
  write-yaml-string "${OUTPUT}" "\${VM_IP}" "${VM_IP}"
  write-yaml-string "${OUTPUT}" "\${GATEWAY_IP}" "${GATEWAY_IP}"
  RETURN_VALUE_2="${OUTPUT}"
}
