#!/bin/bash
set -uo pipefail

# IMPORTS

source /srv/lib/echo/echo-replace.sh

# ARGS

ID=100

# START


START_TIME="$(date -u +%s)"

echo ""
echo ""
echo ""
echo ""
echo ""
echo ""
echo ""

while :
do
  CURRENT_TIME="$(date -u +%s)"
  ELAPSED="$((CURRENT_TIME - START_TIME))"

  if [[ "${ELAPSED}" -lt "3" ]]
  then
    STAGE=1
  elif [[ "${ELAPSED}" -lt "6" ]]
  then
    STAGE=2
  elif [[ "${ELAPSED}" -lt "9" ]]
  then
    STAGE=3
  else
    STAGE=4
  fi
  
  echo-replace "STAGE: ${STAGE}" 7
  [[ ${STAGE} -lt 1 ]] && ICON="1️⃣" || ICON="✅"
  [[ ${STAGE} == 1 ]] && ICON="▶️"
  echo-replace "${ICON}  Booting" 6
  [[ ${STAGE} -lt 2 ]] && ICON="2️⃣" || ICON="✅"
  [[ ${STAGE} == 2 ]] && ICON="▶️"
  echo-replace "${ICON}  Responding to SSH" 5
  [[ ${STAGE} -lt 3 ]] && ICON="3️⃣" || ICON="✅"
  [[ ${STAGE} == 3 ]] && ICON="▶️"
  echo-replace "${ICON}  Running Cloud-Init" 4
  [[ ${STAGE} -lt 4 ]] && ICON="4️⃣" || ICON="✅"
  # [[ ${STAGE} == 4 ]] && ICON="▶️"
  echo-replace "${ICON}  Complete" 3
  echo-replace "⌛  ${ELAPSED} seconds" 1

  if [[ "${STAGE}" == "4" ]]
  then
    echo "🟢  VM ${ID} is online."
    break
  else
    sleep 1s
  fi
done
