#!/bin/bash
set -euo pipefail

function create-vm {

  # IMPORTS

  source /srv/lib/cloud-init/write-network-config.sh
  source /srv/lib/cloud-init/write-user-config.sh
  source /srv/lib/args/get-args.sh
  source /srv/lib/args/echo-args.sh
  source /srv/lib/args/get-arg.sh

  # ARGS

  get-args "$@"

  declare MEMORY
  MEMORY=$(get-arg "--memory")

  declare CORES
  CORES=$(get-arg "--cores")

  declare DISK_FILE
  DISK_FILE=$(get-arg "--disk-file")

  declare OS_NAME
  OS_NAME=$(get-arg "--os-name")

  declare OS_VERSION
  OS_VERSION=$(get-arg "--os-version")

  declare TYPE
  TYPE=$(get-arg "--type")

  declare PURPOSE
  PURPOSE=$(get-arg "--purpose")

  # CONSTANTS

  declare -ri ID=$(pvesh get /cluster/nextid)
  declare -r HOSTNAME="vm${ID}"
  declare -r STORAGE_NAME="local"
  declare -r NAME="${PURPOSE}-${TYPE}"
  declare TAGS="${PURPOSE},${TYPE},${OS_NAME},${OS_NAME}-${OS_VERSION}"

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
  write-user-config "${HOSTNAME}"
  echo "${OUTPUT}" > "/var/lib/vz/snippets/${ID}-user-config.yml"

  echo "➡️  Write network config"
  write-network-config "${ID}"
  echo "${OUTPUT}" > "/var/lib/vz/snippets/${ID}-network-config.yml"

  echo "🆕  Create a virtual machine"
  qm create "${ID}" --name "${NAME}" --tags "${TAGS}"
  qm set "${ID}" --memory "${MEMORY}"
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

}

create-vm "$@"
