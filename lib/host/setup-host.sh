#!/bin/bash
set -euo pipefail

apt-get install python3-pip jq colordiff -y

cat > /root/.ssh/config <<- EOM
Host 10.10.10.*
    LogLevel ERROR
    StrictHostKeyChecking no
    UserKnownHostsFile /dev/null
    Port 4444
    User radiko
EOM

# shellcheck disable=SC2016
echo 'export PATH="/srv/bin:$PATH"' >> /root/.bashrc

