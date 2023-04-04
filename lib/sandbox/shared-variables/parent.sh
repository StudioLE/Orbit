#!/bin/bash
set -euo pipefail

declare OUTPUT

OUTPUT="PARENT VALUE"

echo "> ${OUTPUT}"

OUTPUT="PARENT VALUE" /srv/lib/sandbox/shared-variables/child.sh

echo "> ${OUTPUT}"
