#!/bin/bash
set -euo pipefail

# Install cri-dockerd

echo "➡️  Download"
wget https://github.com/Mirantis/cri-dockerd/releases/download/v0.3.1/cri-dockerd_0.3.1.3-0.ubuntu-jammy_amd64.deb --no-verbose

echo "➡️  Install"
sudo dpkg -i "cri-dockerd_0.3.1.3-0.ubuntu-jammy_amd64.deb"
