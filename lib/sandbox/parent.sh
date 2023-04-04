#!/bin/bash
set -euo pipefail

declare OUTPUT

OUTPUT="ONE"

echo "> ${OUTPUT}"

OUTPUT="TWO" ./child.sh

echo "> ${OUTPUT}"
