#!/bin/bash
set -euo pipefail

# IMPORTS

source /srv/lib/echo/echo-replace.sh

# ARGS

ID=${1}

# VALIDATE

if [[ "${ID}" == "" ||  "${ID}" -lt "100" || "${ID}" -gt "253" ]];
then
    echo "❗  Invalid ID: ${ID}" >&2
    exit 1
fi

# START

echo "➡️  Start VM ${ID}"
qm start "${ID}"

START_TIME="$(date -u +%s)"

echo ""
echo ""
echo ""
echo ""
echo ""
echo ""
echo ""

# Wait for the cloud-init status to show done
while :
do
  CURRENT_TIME="$(date -u +%s)"
  ELAPSED="$((CURRENT_TIME - START_TIME))"
  set +e
  RESPONSE=$(ssh "10.10.10.${ID}" -o 'LogLevel=FATAL' -o 'ConnectTimeout=1' 'cloud-init status')
  EXIT_CODE=$?
  set -e

  if [[ "${EXIT_CODE}" == "255" ]]
  then
    STAGE="2"
  elif [[ "${RESPONSE}" == "status: running" ]]
  then
    STAGE="3"
  elif [[ "${RESPONSE}" == "status: done" ]]
  then
    STAGE="4"
  else
    STAGE="1"
  fi
  
  [[ ${STAGE} -lt 1 ]] && ICON="1️⃣" || ICON="✅"
  [[ ${STAGE} == 1 ]] && ICON="▶️"
  echo-replace "${ICON}  Starting" 6
  [[ ${STAGE} -lt 2 ]] && ICON="2️⃣" || ICON="✅"
  [[ ${STAGE} == 2 ]] && ICON="▶️"
  echo-replace "${ICON}  Waiting for SSH" 5
  [[ ${STAGE} -lt 3 ]] && ICON="3️⃣" || ICON="✅"
  [[ ${STAGE} == 3 ]] && ICON="▶️"
  echo-replace "${ICON}  Waiting for Cloud-Init" 4
  [[ ${STAGE} -lt 4 ]] && ICON="4️⃣" || ICON="✅"
  # [[ ${STAGE} == 4 ]] && ICON="▶️"
  echo-replace "${ICON}  Complete" 3
  echo-replace "⌛  ${ELAPSED} seconds" 1

  if [[ "${STAGE}" == "4" ]]
  then
    echo ""
    echo "🟢  VM ${ID} is online."
    break
  else
    sleep 1s
  fi
done
