# ARGS

CONFIG_FILE="${1}"

# VARIABLES
REPLACE="\${SSH_AUTHORIZED_KEYS}"
INSERT_FROM_FILE="/root/.ssh/id_rsa.pub"
SOURCE_FILE=${CONFIG_FILE}
DESTINATION_FILE=dest

# SCRIPT CONSTANTS
TEMP_FILE=$(mktemp)
PROXY_NEW_LINE="\r"
PROXY_SPACE="\f"

# VALIDATE

if [ "${CONFIG_FILE}" = "" ]; then
    echo "ERROR: Invalid CONFIG_FILE: ${CONFIG_FILE}"
    exit 1
fi

# START

# Delete all trailing blank lines at end of file (only).
sed -z s/.$// ${INSERT_FROM_FILE} > ${TEMP_FILE}

cat ${TEMP_FILE}

# # Replace \n and \s with escape values for sed to work
# INSERT=$(cat ${TEMP_FILE} | tr "\n" ${PROXY_NEW_LINE} | tr "\s" ${PROXY_SPACE})

# # Format each line as a yaml array
# INSERT=$(echo "${PROXY_NEW_LINE}${INSERT}" | sed -e 's/'${PROXY_NEW_LINE}'/'${PROXY_NEW_LINE}${PROXY_SPACE}${PROXY_SPACE}${PROXY_SPACE}${PROXY_SPACE}${PROXY_SPACE}'-'${PROXY_SPACE}'/g')

# echo "${INSERT}"

# Replace values with insert value
# sed -e 's/'${REPLACE}'/'${INSERT}'/' ${SOURCE_FILE} > ${TEMP_FILE}

# # Revert the escape value replacement
# cat ${TEMP_FILE} | tr ${PROXY_NEW_LINE} "\n" | tr ${PROXY_SPACE} " "

# # Remove the temp file
# rm ${TEMP_FILE}
