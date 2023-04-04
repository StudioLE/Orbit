#!/bin/bash
set -euo pipefail

echo "> > > ${OUTPUT}"
OUTPUT="GRAND CHILD VALUE" 
export OUTPUT
echo "> > > ${OUTPUT}"
