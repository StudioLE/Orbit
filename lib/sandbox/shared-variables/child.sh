#!/bin/bash
set -euo pipefail

echo "> > ${OUTPUT}"

OUTPUT="CHILD VALUE"

source /srv/lib/sandbox/shared-variables/grand-child.sh

echo "> > ${OUTPUT}"
