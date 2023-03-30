#!/bin/bash
set -euo pipefail

echo "➡️  Install package dependencies"
apt-get install python3-pip jq colordiff -y

echo "➡️  Set SSH config"
cat > /root/.ssh/config <<- EOM
Host 10.10.10.*
    LogLevel ERROR
    StrictHostKeyChecking no
    UserKnownHostsFile /dev/null
    Port 4444
    User radiko
EOM

echo "➡️  Add /srv/bin to path"
# shellcheck disable=SC2016
echo 'export PATH="/srv/bin:$PATH"' >> /root/.bashrc

echo "➡️  Configure networking"
cat > /etc/network/interfaces <<- EOM
auto lo
iface lo inet loopback

auto eno1
#real IP address
iface eno1 inet static
        address  ${EXTERNAL_IP}/24
        gateway  ${EXTERNAL_GATEWAY}

auto vmbr0
#private sub network
iface vmbr0 inet static
        address  10.10.0.1/18
        bridge-ports none
        bridge-stp off
        bridge-fd 0

        post-up   echo 1 > /proc/sys/net/ipv4/ip_forward
        post-up   iptables -t nat -A POSTROUTING -s '10.10.0.0/18' -o eno1 -j MASQUERADE
        post-down iptables -t nat -D POSTROUTING -s '10.10.0.0/18' -o eno1 -j MASQUERADE

        post-up   iptables -t raw -I PREROUTING -i fwbr+ -j CT --zone 1
        post-down iptables -t raw -D PREROUTING -i fwbr+ -j CT --zone 1
EOM

echo "➡️  Reload networking"
ifreload -a
