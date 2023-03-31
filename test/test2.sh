#!/bin/bash

# VARIABLES
FIND="REPLACE_ME"
INSERT_FROM_FILE=include
SOURCE_FILE=src


# START

INSERT=""

while read line;
do
  INSERT="$INSERT"$'\n'"   - $line"
done < "${INSERT_FROM_FILE}"

# echo "$INSERT"

# Loop through each line of the source file and replace occurances
while IFS= read -r line;
do
  if [[ $line == *"$FIND"* ]];
  then
    echo "${line/$FIND/$INSERT}"
  else
    echo "${line}"
  fi
done < "${SOURCE_FILE}"
