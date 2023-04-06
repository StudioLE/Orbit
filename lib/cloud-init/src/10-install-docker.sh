#!/bin/bash
set -euo pipefail

# Install docker
# Source: https://docs.docker.com/engine/install/ubuntu/

echo "➡️  Uninstall old versions"
sudo apt-get remove \
  docker \
  docker-engine \
  docker.io \
  containerd \
  runc

echo "➡️  Install apt dependencies"
sudo apt-get update
sudo apt-get install \
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
sudo apt-get install \
  docker-ce \
  docker-ce-cli \
  containerd.io \
  docker-buildx-plugin \
  docker-compose-plugin


# Post install steps
# Source: https://docs.docker.com/engine/install/linux-postinstall/

echo "➡️  Create the docker group"
sudo groupadd docker

echo "➡️  Add user to group"
sudo usermod -aG docker "${SUDO_USER}"


# TODO: Change docker logging provider
# Source: https://docs.docker.com/engine/install/linux-postinstall/#configure-default-logging-driver
