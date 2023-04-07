#!/bin/bash
set -euo pipefail

function write-network-config {
  
  # IMPORTS

  source /srv/lib/yaml/write-yaml-string.sh
  
  # ARGS

  declare -r CLUSTER_ID="${1}"
  declare -r NODE_ID="${2}"

  if [[ "${CLUSTER_ID}" == "" || "${CLUSTER_ID}" -lt "1" || "${CLUSTER_ID}" -gt "99" ]];
  then
      echo "❗  Invalid CLUSTER_ID: ${CLUSTER_ID}" >&2
      exit 1
  fi

  if [[ "${NODE_ID}" == "" || "${NODE_ID}" -lt "0" || "${NODE_ID}" -gt "99" ]];
  then
      echo "❗  Invalid NODE_ID: ${NODE_ID}" >&2
      exit 1
  fi

  # CONSTANTS

  declare NODE_IP_ID
  if [[ "${NODE_ID:0:1}" == "0" ]];
  then
    NODE_IP_ID="${NODE_ID:1}"
  else
    NODE_IP_ID="${NODE_ID}"
  fi

  declare -r CONFIG_SRC="/srv/lib/cloud-init/src/network-config-template.yml"
  declare -r VM_IP="10.10.${CLUSTER_ID}.${NODE_IP_ID}"
  declare -r GATEWAY_IP="10.10.0.1"

  # VALIDATE

  # START
  OUTPUT=$(cat "${CONFIG_SRC}")
  write-yaml-string "${OUTPUT}" "\${VM_IP}" "${VM_IP}"
  write-yaml-string "${OUTPUT}" "\${GATEWAY_IP}" "${GATEWAY_IP}"
}
