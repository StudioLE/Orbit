
# VARIABLES
REPLACE="REPLACE_ME"
INSERT_FROM_FILE=include
SOURCE_FILE=src
DESTINATION_FILE=dest

# SCRIPT CONSTANTS
TEMP_FILE=$(mktemp)
PROXY_NEW_LINE="\r"
PROXY_SPACE="\f"
PREFIX_LINE="CCCCCC"


# Delete all trailing blank lines at end of file (only).
sed -z s/.$// ${INSERT_FROM_FILE} > ${TEMP_FILE}

# Replace \n and \s with escape values for sed to work
INSERT=$(cat ${TEMP_FILE} | tr "\n" ${PROXY_NEW_LINE} | tr "\s" ${PROXY_SPACE})

# Format each line as a yaml array
INSERT=$(echo "${PROXY_NEW_LINE}${INSERT}" | sed -e 's/'${PROXY_NEW_LINE}'/'${PROXY_NEW_LINE}${PROXY_SPACE}${PROXY_SPACE}${PROXY_SPACE}${PROXY_SPACE}${PROXY_SPACE}'-'${PROXY_SPACE}'/g')

# Replace values with insert value
sed -e 's/'${REPLACE}'/'${INSERT}'/' ${SOURCE_FILE} > ${TEMP_FILE}

# Revert the escape value replacement
cat ${TEMP_FILE} | tr ${PROXY_NEW_LINE} "\n" | tr ${PROXY_SPACE} " " > ${DESTINATION_FILE}

# Remove the temp file
rm ${TEMP_FILE}

# Display the output
cat ${DESTINATION_FILE}
