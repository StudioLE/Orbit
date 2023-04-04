#!/bin/bash
set -euo pipefail

# IMPORTS

source /srv/lib/yaml/write-yaml-array.sh
source /srv/lib/yaml/write-yaml-block.sh

# ARGS

CONFIG_FILE="${1}"
HOSTNAME="${2}"

# CONSTANTS

CONFIG_SRC="/srv/lib/cloud-init/src/user-config-template.yml"
USER="stivisto"
SUDO_USER="radiko"
SSH_PORT="4444"

# VALIDATE

if [[ "${HOSTNAME}" == "" ]];
then
    echo "❗  Invalid HOSTNAME: ${HOSTNAME}" >&2
    exit 1
fi

# START

cp ${CONFIG_SRC}  "${CONFIG_FILE}"
sed -i "s/\${HOSTNAME}/${HOSTNAME}/" "${CONFIG_FILE}"
sed -i "s/\${USER}/${USER}/" "${CONFIG_FILE}"
sed -i "s/\${SUDO_USER}/${SUDO_USER}/" "${CONFIG_FILE}"
write-yaml-array "${CONFIG_FILE}" "/root/.ssh/id_rsa.pub" "\${SSH_AUTHORIZED_KEYS}" 3
sed -i "s/\${SSH_PORT}/${SSH_PORT}/" "${CONFIG_FILE}"
write-yaml-block "${CONFIG_FILE}" "/srv/lib/cloud-init/src/50-hello-world" "\${HELLO_WORLD_CONTENT}"
write-yaml-array "${CONFIG_FILE}" "/srv/lib/cloud-init/src/bootcmd.sh" "\${BOOTCMD_CONTENT}" 1
write-yaml-array "${CONFIG_FILE}" "/srv/lib/cloud-init/src/runcmd.sh" "\${RUNCMD_CONTENT}" 1
