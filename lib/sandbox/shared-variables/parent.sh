#!/bin/bash
set -euo pipefail

OUTPUT="PARENT VALUE"

echo "> ${OUTPUT}"

source /srv/lib/sandbox/shared-variables/child.sh

echo "> ${OUTPUT}"
