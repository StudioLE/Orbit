#!/bin/bash
set -euo pipefail

function create-vm {

  # IMPORTS

  source /srv/lib/args/get-args.sh
  source /srv/lib/args/echo-args.sh
  source /srv/lib/args/get-arg.sh
  source /srv/lib/echo/echo-error.sh
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
  TYPE=$(get-arg "--type")

  declare MEMORY
  MEMORY=$(get-arg "--memory")

  declare CORES
  CORES=$(get-arg "--cores")

  declare ROLE
  ROLE=$(get-arg "--role" "vm")

  declare CLUSTER_ID
  CLUSTER_ID=$(get-arg "--cluster-id")

  declare NODE_ID
  NODE_ID=$(get-arg "--node-id")

  # VALIDATE

  if [[ "${TYPE}" == "" ]];
  then

    if [[ "${MEMORY}" == "" ]]
    then
      echo-error "Neither --type or --memory were set."
      exit 1
    fi
    
    if [[ "${CORES}" == "" ]]
    then
      echo-error "Neither --type or --cores were set."
      exit 1
    fi

    source /srv/lib/vm/set-type-by-args.sh
    set-type-by-args "${CORES}" "${MEMORY}"

  else

    if [[ "${MEMORY}" != "" ]]
    then
      echo-error "--type and --memory can't both be set."
      exit 1
    fi
    if [[ "${CORES}" != "" ]]
    then
      echo-error "--type and --cores can't both be set."
      exit 1
    fi

    source /srv/lib/vm/set-args-by-type.sh
    set-args-by-type "${TYPE}"

  fi

  if [[ "${CLUSTER_ID}" == "" && "${NODE_ID}" == "" ]];
  then
    CLUSTER_ID=$(/srv/lib/api/get-next-cluster-id.sh)
    NODE_ID="00"
  elif [[ "${CLUSTER_ID}" == "" && "${NODE_ID}" != "" ]];
  then
      echo-error "--node was provided but not --cluster"
      exit 1
  elif [[ "${NODE_ID}" = "" ]]
  then
    NODE_ID=$(/srv/lib/api/get-next-node-id.sh "${CLUSTER_ID}")
  fi

  if [[ "${CLUSTER_ID}" == "" || "${CLUSTER_ID}" -lt "1" || "${CLUSTER_ID}" -gt "99" ]]
  then
      echo-error "Invalid CLUSTER_ID: ${CLUSTER_ID}"
      exit 1
  elif [[ "${NODE_ID}" == "" || "${NODE_ID}" -lt "0" || "${NODE_ID}" -gt "99" ]]
  then
      echo-error "Invalid NODE_ID: ${NODE_ID}"
      exit 1
  elif [[ "${#NODE_ID}" != 2 ]]
  then
    NODE_ID="0${NODE_ID}"
  fi

  # CONSTANTS
  declare -r ID="${CLUSTER_ID}${NODE_ID}"
  declare -r HOSTNAME="vm-${CLUSTER_ID}-${NODE_ID}"
  declare -r STORAGE_NAME="local"
  declare TYPE_LOWER
  TYPE_LOWER=$(echo "${TYPE}" | tr '[:upper:]' '[:lower:]')
  declare -r NAME="${ROLE}-${TYPE_LOWER}"
  declare TAGS="${ROLE},${TYPE_LOWER},${OS_NAME},${OS_NAME}-${OS_VERSION}"

  # VALIDATE

  if [[ "${CORES}" == "" || "${CORES}" -lt "1" || "${CORES}" -gt "4" ]];
  then
      echo "❗  Invalid CORES: ${CORES}" >&2
      exit 1
  fi

  if [[ "${MEMORY}" == "" || "${MEMORY}" -lt "1" || "${MEMORY}" -gt "32" ]];
  then
      echo "❗  Invalid MEMORY: ${MEMORY}" >&2
      exit 1
  else
      MEMORY=$((MEMORY * 1024))
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
