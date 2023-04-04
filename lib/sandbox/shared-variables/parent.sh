#!/bin/bash
set -euo pipefail

source /srv/lib/sandbox/shared-variables/child.sh

function parent {

  OUTPUT="PARENT VALUE"

  echo "> ${OUTPUT}"

  child

  echo "> ${OUTPUT}"
}
