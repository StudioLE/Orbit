#!/bin/bash
set -euo pipefail

# IMPORTS

source /srv/lib/cloud-init/write-network-config.sh
source /srv/lib/cloud-init/write-user-config.sh

# ARGS

MEMORY=${1}
CORES=${2}
DISK_FILE=${3}
NAME=${4}
TAGS=${5}

# CONSTANTS

ID=$(pvesh get /cluster/nextid)
HOSTNAME="vm${ID}"
STORAGE_NAME="local"

# VALIDATE

if [[ "${ID}" == "" ||  "${ID}" -lt "100" || "${ID}" -gt "253" ]];
then
    echo "❗  Invalid ID: ${ID}" >&2
    exit 1
fi

if [[ "${MEMORY}" == "" || "${MEMORY}" -lt "1" || "${MEMORY}" -gt "32" ]];
then
    echo "❗  Invalid MEMORY: ${MEMORY}" >&2
    exit 1
else
    MEMORY=$((MEMORY * 1024))
fi

if [[ "${CORES}" == "" || "${CORES}" -lt "1" || "${CORES}" -gt "4" ]];
then
    echo "❗  Invalid CORES: ${CORES}" >&2
    exit 1
fi

if [[ "${DISK_FILE}" == "" || ! -f "${DISK_FILE}" ]];
then
    echo "❗  Invalid DISK_FILE: ${DISK_FILE}" >&2
    exit 1
fi

if [[ "${NAME}" == "" ]];
then
    NAME="${HOSTNAME}"
fi

if [[ "${TAGS}" == "" ]];
then
    TAGS="unknown"
fi

# START

echo "➡️  Write user config"
write-user-config "/var/lib/vz/snippets/${ID}-user-config.yml" "${HOSTNAME}"

echo "➡️  Write network config"
write-network-config "/var/lib/vz/snippets/${ID}-network-config.yml" "${ID}"

echo "🆕  Create a virtual machine"
qm create "${ID}" --name "${NAME}" --tags "${TAGS}"
qm set "${ID}" --memory ${MEMORY}
qm set "${ID}" --sockets 1 --cores "${CORES}"
qm set "${ID}" --net0 virtio,bridge=vmbr0
qm set "${ID}" --agent enabled=1

echo "➡️  Import the disk into the proxmox storage"
qm disk import "${ID}" "${DISK_FILE}" "${STORAGE_NAME}" --format qcow2

echo "➡️  Add the imported disk to the VM"
qm set "${ID}" \
    --scsihw virtio-scsi-pci \
    --scsi0 "${STORAGE_NAME}:${ID}/vm-${ID}-disk-0.qcow2"

echo "➡️  Specify the boot disk"
qm set "${ID}" --boot c --bootdisk scsi0

echo "➡️  Add support for VNC and a serial console"
qm set "${ID}" --serial0 socket --vga serial0

echo "➡️  Add a CD-ROM for cloud-init"
qm set "${ID}" --ide2 "${STORAGE_NAME}:cloudinit"

echo "➡️  Set cloud-init"
qm set "${ID}" --cicustom \
    "network=local:snippets/${ID}-network-config.yml,user=local:snippets/${ID}-user-config.yml"

echo "✅  Created VM ${ID}"
