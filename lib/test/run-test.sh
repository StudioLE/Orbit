#!/bin/bash
set -uo pipefail

# SETUP

source /srv/lib/test/setup-test.sh

# START

if [[ -f "${ACTUAL_FILE}" ]]
then
  rm "${ACTUAL_FILE}"
fi

# Execute the test
${TEST_FILE} "${ACTUAL_FILE}"

# Determine the exit code.
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
