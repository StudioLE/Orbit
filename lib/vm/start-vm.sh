#!/bin/bash
set -euo pipefail

# IMPORTS

source /srv/lib/echo/echo-replace.sh

# ARGS

ID=${1}

# VALIDATE

if [[ "${ID}" == "" ||  "${ID}" -lt "100" || "${ID}" -gt "9999" ]];
then
    echo "❗  Invalid ID: ${ID}" >&2
    exit 1
fi

# START

echo "➡️  Start VM ${ID}"
qm start "${ID}"

declare START_TIME
START_TIME="$(date -u +%s)"
declare -ri STAGE_INTERVAL=4
declare -r SLEEP_INTERVAL=0.5
declare -r CLOCK_ICONS="🕛🕐🕑🕒🕓🕔🕕🕖🕗🕘🕙🕚"

# Create a blank canvas to overwrite
# shellcheck disable=SC2034
for i in {1..7}
do
  echo ""
done

declare RESPONSE
declare EXIT_CODE=255
declare -i FRAME=0

# Wait for the cloud-init status to show done
while :
do
  FRAME=$((FRAME + 1))
  declare CURRENT_TIME
  CURRENT_TIME="$(date -u +%s)"
  declare ELAPSED
  ELAPSED="$((CURRENT_TIME - START_TIME))"
  declare CLOCK_NUMBER
  CLOCK_NUMBER=$((FRAME % ${#CLOCK_ICONS}))

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
  [[ ${STAGE} == 1 ]] && ICON="⌛"
  echo-replace "${ICON}  Booting" 6

  [[ ${STAGE} -lt 2 ]] && ICON="2️⃣" || ICON="✅"
  [[ ${STAGE} == 2 ]] && ICON="⌛"
  echo-replace "${ICON}  Responding to SSH" 5

  [[ ${STAGE} -lt 3 ]] && ICON="3️⃣" || ICON="✅"
  [[ ${STAGE} == 3 ]] && ICON="⌛"
  echo-replace "${ICON}  Running Cloud-Init" 4

  [[ ${STAGE} -lt 4 ]] && ICON="4️⃣" || ICON="✅"
  # [[ ${STAGE} == 4 ]] && ICON="⌛"
  echo-replace "${ICON}  Complete" 3

  echo-replace "${CLOCK_ICONS:${CLOCK_NUMBER}:1}  ${ELAPSED} seconds" 1

  if [[ "${STAGE}" == "4" ]]
  then
    echo ""
    echo "🟢  VM ${ID} is online."
    break
  fi
    
  set +e
  RESPONSE=$(ssh "10.10.10.${ID}" -o 'LogLevel=FATAL' -o 'ConnectTimeout=1' 'cloud-init status')
  EXIT_CODE=$?
  set -e

  # Only sleep if SSH hasn't hit timeout
  # [[ "${EXIT_CODE}" != "255" ]] && sleep "${SLEEP_INTERVAL}s"
  sleep "${SLEEP_INTERVAL}s"
done
