#!/bin/bash
set -euo pipefail

function create-boot-disk {

  # ARGS

  declare -r OS_NAME="${1}"
  declare -r OS_VERSION="${2}"
  declare -r DISK_SIZE="${3}"

  # CONSTANTS

  declare -r DIR="/srv/artifacts/${OS_NAME}/${OS_VERSION}"
  declare -r IGNORE_CACHE=false

  # VALIDATE

  if [ "${OS_NAME}" == "" ]; then
    echo "❗  Invalid OS_NAME: ${OS_NAME}" >&2
    exit 1
  fi

  if [ "${OS_VERSION}" == "" ]; then
    echo "❗  Invalid OS_VERSION: ${OS_VERSION}" >&2
    exit 1
  fi

  if [ "${DISK_SIZE}" == "" ]; then
    echo "❗  Invalid DISK_SIZE: ${DISK_SIZE}" >&2
    exit 1
  fi

  # START

  declare FILENAME
  declare SRC_FORMAT
  declare SRC_URL
  declare SHA_URL

  if [[ "${OS_NAME}" == "ubuntu" ]]
  then
    FILENAME="${OS_VERSION}-server-cloudimg-amd64"
    SRC_FORMAT="img"
    SRC_URL="https://cloud-images.ubuntu.com/${OS_VERSION}/current/${FILENAME}.${SRC_FORMAT}"
    SHA_URL="https://cloud-images.ubuntu.com/${OS_VERSION}/current/SHA256SUMS"
  else
    echo "❗  Only ubuntu is currently supported" >&2
    exit 1
  fi

  declare OUTPUT_FILE
  OUTPUT_FILE="${DIR}/${FILENAME}-${DISK_SIZE}.qcow2"


  # Download the source file if it doesn't exist
  if ${IGNORE_CACHE} || [[ ! -f "${DIR}/${FILENAME}.${SRC_FORMAT}" ]]
  then
    # Create output directory if it doesn't exist
    if [[ ! -e "${DIR}" ]]
    then
      mkdir -p "${DIR}"
    fi

    echo "➡️  Download the disk file: ${SRC_URL}"
    curl --silent --show-error --output "${DIR}/${FILENAME}.${SRC_FORMAT}" "${SRC_URL}"
    
    echo "➡️  Download the SHA256 file: ${SHA_URL}"
    curl --silent --show-error "${SHA_URL}" \
      | grep "\*${FILENAME}\.${SRC_FORMAT}$" \
      > "${DIR}/SHA256SUMS"
    
    echo "➡️  Check the SHA256"
    (cd "${DIR}" && sha256sum -c "${DIR}/SHA256SUMS")
  fi

  # Convert to qcow2 format if required
  if ${IGNORE_CACHE} || [[ "${SRC_FORMAT}" != "qcow2" && ! -f "${DIR}/${FILENAME}.qcow2" ]]
  then
    echo "➡️  Convert the disk file to qcow2"
    qemu-img convert -O qcow2 "${DIR}/${FILENAME}.${SRC_FORMAT}" "${DIR}/${FILENAME}.qcow2"
  fi

  # Resize if required
  if ${IGNORE_CACHE} || [[ ! -f "${OUTPUT_FILE}" ]]
  then
    echo "➡️  Copy the disk file"
    cp "${DIR}/${FILENAME}.qcow2" "${OUTPUT_FILE}"

    echo "➡️  Resize the disk file"
    qemu-img resize "${OUTPUT_FILE}" "${DISK_SIZE}"
  fi

  # Return the output
  OUTPUT="${OUTPUT_FILE}"
}
