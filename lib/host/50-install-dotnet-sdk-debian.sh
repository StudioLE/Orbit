#!/bin/bash
set -euo pipefail

# Install .NET SDK
# Source: https://learn.microsoft.com/en-us/dotnet/core/install/linux-debian

echo "➡️  Add the Microsoft package repository"
# Download Microsoft signing key and repository
wget https://packages.microsoft.com/config/debian/11/packages-microsoft-prod.deb -O packages-microsoft-prod.deb

# Install Microsoft signing key and repository
sudo dpkg -i packages-microsoft-prod.deb

# Clean up
rm packages-microsoft-prod.deb

# Update packages
sudo apt-get update

echo "➡️  Install .NET SDK"
sudo apt-get install dotnet-sdk-7.0 --yes --quiet
