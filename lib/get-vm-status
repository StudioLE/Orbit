#!/bin/bash
set -euo pipefail

# ARGS

ID=${1}

# VALIDATE

if [[ "${ID}" == "" ||  "${ID}" -lt "100" || "${ID}" -gt "253" ]];
then
    echo "❗  Invalid ID: ${ID}" >&2
    exit 1
fi

# START

# Request all resources as JSON from the API
ALL_RESOURCES=$(pvesh get /cluster/resources --human-readable 0 --output-format json-pretty)

# Validate at least 1 resource is returned
COUNT=$(echo "${ALL_RESOURCES}" | jq 'length')
if [[ ${COUNT} -lt 1 ]];
then
    echo "❗  No resources found" >&2
    exit 1
fi

# Select by VM ID
VM_WITH_ID=$(echo "${ALL_RESOURCES}" | jq "map(select(.vmid == ${ID}))")

# Validate only one VM has that ID
COUNT=$(echo "${VM_WITH_ID}" | jq 'length')
if [[ ${COUNT} -lt 1 ]];
then
    echo "❗  VM ${ID} does not exist" >&2
    exit 1
elif [[ ${COUNT} -gt 1 ]];
then
    echo "❗  Multiple VMs with ${ID} were found" >&2
    exit 1
fi

# Select the VM
STATUS=$(echo "${VM_WITH_ID}" | jq '.[]')
echo "$STATUS"
