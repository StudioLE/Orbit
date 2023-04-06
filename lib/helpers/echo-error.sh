#!/bin/bash
set -euo pipefail

function echo-error {

  # IMPORTS

  source /srv/lib/helpers/is-int.sh

  # ARGS

  [[ -v "1" ]] && MESSAGE="$1" || MESSAGE="ERROR"

  # START

  echo "❗  ${MESSAGE}" >&2
}
