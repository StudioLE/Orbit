#!/bin/bash
#set -euo pipefail

SUCCESS="TRUE"

echo -ne "IPv4 Ping\t"
if ping 1.1.1.1 -4 -c 1 -q -w 1 > /dev/null
then
  echo "🟢"
else
  echo "🔴"
  SUCCESS=""
fi

echo -ne "IPv6 Ping\t"
if ping 2606:4700:4700::1111 -6 -c 1 -q -w 1 > /dev/null
then
  echo "🟢"
else
  echo "🔴"
  SUCCESS=""
fi

echo -ne "IPv4 DNS\t"
if ping google.com -4 -c 1 -q -w 1 > /dev/null
then
  echo "🟢"
else
  echo "🔴"
  SUCCESS=""
fi

echo -ne "IPv6 DNS\t"
if ping google.com -6 -c 1 -q -w 1 > /dev/null
then
  echo "🟢"
else
  echo "🔴"
  SUCCESS=""
fi

echo -ne 'IPv4 Address\t'
IPv4=$(curl ipinfo.io/ip -4 --connect-timeout 1 --silent)

if [[ $? == 0 ]]
then
  echo "${IPv4}"
else
  echo "🔴"
  SUCCESS=""
fi

echo -ne "IPv6 Address\t"
IPv4=$(curl v6.ipinfo.io/ip -6 --connect-timeout 1 --silent)
if [[ $? == 0 ]]
then
  echo "${IPv4}"
else
  echo "🔴"
  SUCCESS=""
fi


if [[ "${SUCCESS}" == "" ]]
then
  exit 1
fi
