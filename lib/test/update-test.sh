#!/bin/bash
set -uo pipefail

# SETUP

source /srv/lib/test/setup-test.sh

# START

/srv/lib/test/diff-results.sh "${EXPECTED_FILE}" "${ACTUAL_FILE}"
cp "${ACTUAL_FILE}" "${EXPECTED_FILE}"
