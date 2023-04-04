#!/bin/bash
set -euo pipefail

source /srv/lib/sandbox/shared-variables/grand-child.sh

function child {
  
  echo "> > ${OUTPUT}"

  OUTPUT="CHILD VALUE"

  grand-child

  echo "> > ${OUTPUT}"

}
