#!/bin/bash
set -uo pipefail

# IMPORTS

source /srv/lib/echo/echo-replace.sh

# START

for i in {1..5}
do
  echo "LINE $i"
done

echo-replace "REPLACE LINE 4" 2

echo "THIS SHOULD BE LINE 6"

echo ""

for i in {8..10}
do
  echo "LINE $i"
done

echo-replace "REPLACE LINE 9
REPLACE LINE 10" 2
