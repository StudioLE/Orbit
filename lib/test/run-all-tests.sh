#!/bin/bash
set -uo pipefail

# START

find /srv/lib/ -type f -wholename "*/tests/*.test.sh" | while read -r TEST_FILE
do
  /srv/lib/test/run-test.sh "${TEST_FILE}"
done
