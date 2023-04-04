#!/bin/bash
set -uo pipefail

# ARGS

SCRIPT_FILE=${1}

# CONSTANTS

EXPECTED_FILE="/srv/tests/expected${SCRIPT_FILE}"
ACTUAL_FILE="/srv/tests/actual${SCRIPT_FILE}"

# VALIDATE

if [[ ! -f ${SCRIPT_FILE} ]];
then
  echo "❗  Invalid SCRIPT_FILE: ${SCRIPT_FILE}" >&2
  exit 1
fi

if [[ ! -f ${EXPECTED_FILE} ]];
then
  echo "❗  Invalid EXPECTED_FILE: ${EXPECTED_FILE}" >&2
  exit 1
fi

if [[ ! -f ${ACTUAL_FILE} ]];
then
  echo "❗  Invalid ACTUAL_FILE: ${ACTUAL_FILE}" >&2
  exit 1
fi

# START

/srv/tests/lib/diff-results "${EXPECTED_FILE}" "${ACTUAL_FILE}"
cp "${ACTUAL_FILE}" "${EXPECTED_FILE}"
