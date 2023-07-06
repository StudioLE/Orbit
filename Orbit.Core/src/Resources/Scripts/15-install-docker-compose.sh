#!/bin/bash
set -euo pipefail

# Install docker-compose-plugin
# Source: https://docs.docker.com/compose/install/linux/#install-using-the-repository

echo "➡️  Install docker compose"
sudo apt-get update
sudo apt-get install docker-compose-plugin
