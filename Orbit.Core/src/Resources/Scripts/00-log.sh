#!/bin/bash
set -euo pipefail

date --rfc-3339 ns | tr '\n' ' ' | sudo tee -a /var/log/orbit.log
echo "Executing installers" | sudo tee -a /var/log/orbit.log
