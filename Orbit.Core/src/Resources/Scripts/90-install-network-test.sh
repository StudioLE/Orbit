#!/bin/bash
set -euo pipefail

date --rfc-3339 ns | tr '\n' ' ' | sudo tee -a /var/log/orbit.log
echo "Installing network-test" | sudo tee -a /var/log/orbit.log

curl -fsS https://install.studiole.uk/network-test | sudo bash
