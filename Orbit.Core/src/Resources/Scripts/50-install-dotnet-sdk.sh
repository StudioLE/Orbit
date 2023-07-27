#!/bin/bash
set -euo pipefail

date --rfc-3339 ns | tr '\n' ' ' | sudo tee -a /var/log/orbit.log
echo "Installing dotnet-sdk" | sudo tee -a /var/log/orbit.log

curl -fsS https://install.studiole.uk/dotnet-sdk | sudo bash
