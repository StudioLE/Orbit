#!/bin/bash
set -euo pipefail

function exit-error {

  # IMPORTS

  source /srv/lib/helpers/is-int.sh

  # ARGS

  [[ -v "1" ]] && MESSAGE="$1" || MESSAGE="ERROR"
  [[ -v "2" && $(is-int "$2") != "" ]] && CODE="$2" || CODE="1"

  # START

  echo "❗  ${MESSAGE}" >&2
  exit "${CODE}"
}
