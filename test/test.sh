PROXY_NEW_LINE="\r"
PROXY_SPACE="\f"
PREFIX_LINE="CCCCCC"

# Delete all trailing blank lines at end of file (only).
sed -z s/.$// include > include.tmp

# Replace \n and \s for sed to work
ESCAPED=$(cat include.tmp | tr "\n" ${PROXY_NEW_LINE} | tr "\s" ${PROXY_SPACE})

# Pad the output
ESCAPED=$(echo "${PROXY_NEW_LINE}${ESCAPED}" | sed -e 's/'${PROXY_NEW_LINE}'/'${PROXY_NEW_LINE}${PROXY_SPACE}${PROXY_SPACE}${PROXY_SPACE}${PROXY_SPACE}${PROXY_SPACE}'-'${PROXY_SPACE}'/g')

# echo ${ESCAPED}
rm dest
cp src dest.tmp
sed -i 's/REPLACE_ME/'${ESCAPED}'/' dest.tmp
(cat dest.tmp | tr ${PROXY_NEW_LINE} "\n" | tr ${PROXY_SPACE} " ") > dest
cat dest
