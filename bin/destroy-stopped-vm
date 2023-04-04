#!/bin/bash
set -euo pipefail

# CONSTANTS

JSON_FILE=$(mktemp)

# START

# Request JSON from API
pvesh get /cluster/resources --human-readable 0 --output-format json-pretty > "${JSON_FILE}"

# Filter data with jq
ID_ARRAY=$(jq 'map(select(.status == "stopped" and .template == 0)) | map(.vmid) | .[]' \
    "${JSON_FILE}" \
    --compact-output)

for ID in ${ID_ARRAY}
do
  echo "🗑️  Destroy VM ${ID}"
  qm destroy "${ID}" --purge
done

# Remove the TEMP file
rm "${JSON_FILE}"
