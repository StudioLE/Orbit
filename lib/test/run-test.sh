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

if [[ ! -f ${EXPECTED_FILE} ]];
then
  mkdir -p $(dirname "${EXPECTED_FILE}")
  touch "${EXPECTED_FILE}"
fi

if [[ ! -f ${ACTUAL_FILE} ]];
then
  mkdir -p $(dirname "${ACTUAL_FILE}")
  touch "${ACTUAL_FILE}"
fi

# START

# Execute the test
${TEST_FILE} "${ACTUAL_FILE}"


SCRIPT_EXIT_CODE=$?
if [[ ${SCRIPT_EXIT_CODE} != 0 ]];
then
  echo "⚠️  Test failed. The script returned exit code: ${SCRIPT_EXIT_CODE}" >&2
  exit 1
fi

# Compare the expected and actual files. Store the exit code
cmp --silent -- "${EXPECTED_FILE}" "${ACTUAL_FILE}"
COMPARE_EXIT_CODE=$?

# Echo the status, the command and a diff if not matching
if [[ ${COMPARE_EXIT_CODE} == 0 ]];
then
  echo "✅  Test passed: ${TEST_FILE}" 
else
  echo "❌  Test failed: Actual and expected did not match: ${TEST_FILE}"
  /srv/lib/test/diff-results.sh "${EXPECTED_FILE}" "${ACTUAL_FILE}"
fi
