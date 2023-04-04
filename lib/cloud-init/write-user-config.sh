#!/bin/bash
set -euo pipefail

function write-user-config {
  
  # IMPORTS
  source /srv/lib/yaml/write-yaml-string.sh
  source /srv/lib/yaml/write-yaml-array.sh
  source /srv/lib/yaml/write-yaml-block.sh

  # ARGS
  declare -r HOSTNAME="${1}"

  # CONSTANTS

  declare -r CONFIG_SRC="/srv/lib/cloud-init/src/user-config-template.yml"
  declare -r USER="stivisto"
  declare -r SUDO_USER="radiko"
  declare -ri SSH_PORT="4444"

  # VALIDATE

  if [[ "${HOSTNAME}" == "" ]];
  then
      echo "❗  Invalid HOSTNAME: ${HOSTNAME}" >&2
      exit 1
  fi

  # START

  OUTPUT=$(cat "${CONFIG_SRC}")
  write-yaml-string "${OUTPUT}" "\${HOSTNAME}" "${HOSTNAME}"
  write-yaml-string "${OUTPUT}" "\${USER}" "${USER}"
  write-yaml-string "${OUTPUT}" "\${SUDO_USER}" "${SUDO_USER}"
  SSH_AUTHORIZED_KEYS=$(cat "/root/.ssh/id_rsa.pub")
  write-yaml-array "${OUTPUT}" "\${SSH_AUTHORIZED_KEYS}" "${SSH_AUTHORIZED_KEYS}" 3
  write-yaml-string "${OUTPUT}" "\${SSH_PORT}" "${SSH_PORT}"
  HELLO_WORLD_CONTENT=$(cat "/srv/lib/cloud-init/src/50-hello-world")
  write-yaml-block "${OUTPUT}" "\${HELLO_WORLD_CONTENT}" "${HELLO_WORLD_CONTENT}"
  BOOTCMD_CONTENT=$(cat "/srv/lib/cloud-init/src/bootcmd.sh")
  write-yaml-array "${OUTPUT}" "\${BOOTCMD_CONTENT}" "${BOOTCMD_CONTENT}" 1
  RUNCMD_CONTENT=$(cat "/srv/lib/cloud-init/src/runcmd.sh")
  write-yaml-array "${OUTPUT}" "\${RUNCMD_CONTENT}" "${RUNCMD_CONTENT}" 1

}

