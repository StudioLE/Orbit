#!/bin/bash
set -uo pipefail

# ARGS

TEST_FILE=${1}

# CONSTANTS

TEST_DIR=$(dirname "${TEST_FILE}")
TEST_FILENAME=$(basename "${TEST_FILE}")
EXPECTED_FILE="${TEST_DIR}/verify/${TEST_FILENAME}.expected"
ACTUAL_FILE="${TEST_DIR}/verify/${TEST_FILENAME}.actual"

# VALIDATE

if [[ ! -f ${TEST_FILE} ]];
then
  echo "❗  Invalid TEST_FILE: ${TEST_FILE}" >&2
  exit 1
fi

if [[ "${TEST_FILENAME}" != *".test.sh" ]];
then
  echo "❗  Invalid TEST_FILENAME: ${TEST_FILENAME}" >&2
  exit 1
fi

if [[ ! -f ${EXPECTED_FILE} ]];
then
  mkdir -p "$(dirname "${EXPECTED_FILE}")"
  touch "${EXPECTED_FILE}"
fi

if [[ ! -f ${ACTUAL_FILE} ]];
then
  mkdir -p "$(dirname "${ACTUAL_FILE}")"
  touch "${ACTUAL_FILE}"
fi
