#!/bin/bash
set -euo pipefail

echo "ARGS"
echo "$@"
echo "COUNT"
echo ${#}

function child {
  echo "ARGS"
  echo "$@"
  echo "COUNT"
  echo ${#}
}

child "$@"

