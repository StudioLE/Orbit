#!/bin/bash
set -uo pipefail

# ARGS

ID=${1}

# VALIDATE

if [[ "${ID}" == "" ||  "${ID}" -lt "100" || "${ID}" -gt "253" ]];
then
    echo "❗  Invalid ID: ${ID}" >&2
    exit 1
fi

# START

echo "➡️  SSH to VM ${ID}"
ssh "10.10.10.${ID}"
