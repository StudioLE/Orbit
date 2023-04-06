#!/bin/bash
set -uo pipefail

# ARGS

ACTUAL_FILE=$1

# START

if [[ "" ]]
then
  echo "INCORRECT"
else
  echo "CORRECT"
fi

if [[ "TRUE" ]]
then
  echo "CORRECT" >> "${ACTUAL_FILE}"
else
  echo "INCORRECT" >> "${ACTUAL_FILE}"
fi

if [[ "0" ]]
then
  echo "CORRECT"
else
  echo "INCORRECT" >> "${ACTUAL_FILE}"
fi

if [[ "1" ]]
then
  echo "CORRECT" >> "${ACTUAL_FILE}"
else
  echo "INCORRECT" >> "${ACTUAL_FILE}"
fi

RESULT=$(parent)

echo "${RESULT}" > "${ACTUAL_FILE}"
