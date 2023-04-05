#!/bin/bash
set -euo pipefail

function move-cursor-up {
  declare -i N
  [[ -v "1" ]] && N="$1" || N="1"
  echo -ne '\e['"${N}"'A'
}

function move-cursor-down {
  declare -i N
  [[ -v "1" ]] && N="$1" || N="1"
  echo -ne '\e['"${N}"'B'
}

  # shellcheck disable=SC2120
function erase-in-line {
  declare -i N
  [[ -v "1" ]] && N="$1" || N="0"
  echo -ne '\e['"${N}"'K'
}

function echo-replace {

  declare VALUE
  [[ -v "1" ]] && VALUE="$1" || VALUE=""

  declare LINE
  [[ -v "2" ]] && LINE="$2" || LINE="1"

  move-cursor-up "${LINE}"

  # shellcheck disable=SC2119
  erase-in-line

  echo "${VALUE}"

  move-cursor-down "${LINE}"
}
