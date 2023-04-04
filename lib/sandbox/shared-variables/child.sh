#!/bin/bash
set -euo pipefail

echo "> > ${OUTPUT}"

OUTPUT="CHILD VALUE" /srv/lib/sandbox/shared-variables/grand-child.sh

echo "> > ${OUTPUT}"
