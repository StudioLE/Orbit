#!/bin/bash
set -euo pipefail

function create-vm {

  # IMPORTS

  source /srv/lib/args/get-args.sh
  source /srv/lib/args/echo-args.sh
  source /srv/lib/args/get-arg.sh
  source /srv/lib/boot-disk/create-boot-disk.sh
  source /srv/lib/cloud-init/write-network-config.sh
  source /srv/lib/cloud-init/write-user-config.sh

  # ARGS

  get-args "$@"

  declare OS_NAME
  OS_NAME=$(get-arg "--os-name" "ubuntu")

  declare OS_VERSION
  OS_VERSION=$(get-arg "--os-version" "jammy")

  declare DISK_SIZE
  DISK_SIZE=$(get-arg "--disk-size" "20G")

  declare TYPE
  TYPE=$(get-arg "--type" "c1")

  declare MEMORY
  MEMORY=$(get-arg "--memory" "8")

  declare CORES
  CORES=$(get-arg "--cores" "4")

  declare PURPOSE
  PURPOSE=$(get-arg "--purpose" "worker")

  # CONSTANTS

  declare -ri ID=$(pvesh get /cluster/nextid)
  declare -r HOSTNAME="vm${ID}"
  declare -r STORAGE_NAME="local"
  declare -r NAME="${PURPOSE}-${TYPE}"
  declare TAGS="${PURPOSE},${TYPE},${OS_NAME},${OS_NAME}-${OS_VERSION}"

  # VALIDATE

  if [[ "${OS_NAME}" == "" ]];
  then
      echo "❗  Invalid OS_NAME: ${OS_NAME}" >&2
      exit 1
  fi

  if [[ "${OS_VERSION}" == "" ]];
  then
      echo "❗  Invalid OS_VERSION: ${OS_VERSION}" >&2
      exit 1
  fi

  if [[ "${CORES}" == "" || "${CORES}" -lt "1" || "${CORES}" -gt "4" ]];
  then
      echo "❗  Invalid CORES: ${CORES}" >&2
      exit 1
  fi

  if [[ "${DISK_SIZE}" == "" || "${DISK_SIZE}" != *"G" ]];
  then
      echo "❗  Invalid DISK_SIZE: ${DISK_SIZE}" >&2
      exit 1
  fi

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

  if [[ "${NAME}" == "" ]];
  then
      NAME="${HOSTNAME}"
  fi

  if [[ "${TAGS}" == "" ]];
  then
      TAGS="unknown"
  fi

  # START

  
  echo "➡️  Create boot disk"
  create-boot-disk "${OS_NAME}" "${OS_VERSION}" "${DISK_SIZE}"
  DISK_FILE="${OUTPUT}"
  echo "$DISK_FILE"

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
