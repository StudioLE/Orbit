#!/bin/bash
set -euo pipefail

# Install docker
# Source: https://docs.docker.com/engine/install/ubuntu/

echo "➡️  Uninstall old versions"
set +e
for PACKAGE in docker docker-engine docker.io containerd runc
do
  sudo apt-get remove "$PACKAGE" --quiet --yes
done
set -e

echo "➡️  Install apt dependencies"
sudo apt-get update
sudo apt-get install --quiet --yes \
  ca-certificates \
  curl \
  gnupg

echo "➡️  Add Docker official GPG key"
sudo mkdir -m 0755 -p /etc/apt/keyrings
curl -fsSL https://download.docker.com/linux/ubuntu/gpg | sudo gpg --dearmor -o /etc/apt/keyrings/docker.gpg

echo "➡️  Set up the repository"
echo \
  "deb [arch="$(dpkg --print-architecture)" signed-by=/etc/apt/keyrings/docker.gpg] https://download.docker.com/linux/ubuntu \
  "$(. /etc/os-release && echo "$VERSION_CODENAME")" stable" | \
  sudo tee /etc/apt/sources.list.d/docker.list > /dev/null

echo "➡️  Install Docker Engine"
sudo apt-get update
sudo apt-get install --quiet --yes \
  docker-ce \
  docker-ce-cli \
  containerd.io \
  docker-buildx-plugin \
  docker-compose-plugin


# Post install steps
# Source: https://docs.docker.com/engine/install/linux-postinstall/

echo "➡️  Create the docker group"
set +e
sudo groupadd docker
set -e

echo "➡️  Add user to group"
sudo usermod -aG docker "${SUDO_USER}"

echo "➡️  Reload groups"
newgrp docker

# TODO: Change docker logging provider
# Source: https://docs.docker.com/engine/install/linux-postinstall/#configure-default-logging-driver
