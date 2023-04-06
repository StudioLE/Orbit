#!/bin/bash
set -uo pipefail

# IMPORTS

source /srv/lib/echo/echo-replace.sh

# CONSTANTS

declare -i ID=100
declare -i STAGE_INTERVAL=4
declare SLEEP_INTERVAL=0.5
declare CLOCK_ICONS="🕛🕐🕑🕒🕓🕔🕕🕖🕗🕘🕙🕚"

# START

# Create a blank canvas to overwrite
# shellcheck disable=SC2034
for i in {1..7}
do
  echo ""
done

declare START_TIME
START_TIME="$(date -u +%s)"
declare -i FRAME=0
while :
do
  FRAME=$((FRAME + 1))
  declare CURRENT_TIME
  CURRENT_TIME="$(date -u +%s)"
  declare ELAPSED
  ELAPSED="$((CURRENT_TIME - START_TIME))"
  declare STAGE
  STAGE=$((ELAPSED / STAGE_INTERVAL + 1))
  declare CLOCK_NUMBER
  CLOCK_NUMBER=$((FRAME % ${#CLOCK_ICONS}))
  
  echo-replace "STAGE: ${STAGE}; FRAME: ${FRAME}; CLOCK_NUMBER: ${CLOCK_NUMBER}" 7

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
  else
    sleep "${SLEEP_INTERVAL}s"
  fi
done
