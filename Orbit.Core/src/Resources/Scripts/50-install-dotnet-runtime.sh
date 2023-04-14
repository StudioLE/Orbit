#!/bin/bash
set -euo pipefail

# Install .NET Runtime
# Source: https://learn.microsoft.com/en-us/dotnet/core/install/linux-ubuntu

echo "➡️  Add the Microsoft package repository"
declare repo_version=$(if command -v lsb_release &> /dev/null; then lsb_release -r -s; else grep -oP '(?<=^VERSION_ID=).+' /etc/os-release | tr -d '"'; fi)

# Download Microsoft signing key and repository
wget https://packages.microsoft.com/config/ubuntu/$repo_version/packages-microsoft-prod.deb -O packages-microsoft-prod.deb

# Install Microsoft signing key and repository
sudo dpkg -i packages-microsoft-prod.deb

# Clean up
rm packages-microsoft-prod.deb

# Update packages
sudo apt-get update

echo "➡️  Install .NET Runtime"
sudo apt-get install dotnet-runtime-7.0 --yes --quiet
