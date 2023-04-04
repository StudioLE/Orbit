#!/bin/bash
set -euo pipefail

function grand-child {
  
  echo "> > > ${OUTPUT}"

  OUTPUT="GRAND CHILD VALUE"

  echo "> > > ${OUTPUT}"
  
}
