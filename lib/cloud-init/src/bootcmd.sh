echo "*********************************"
echo "Executing bootcmd"
echo "*********************************"
if [ ! -f /root/first-run-complete ]; then echo "IS FIRST RUN"; else echo "NOT FIRST RUN"; fi
if [ ! -f /root/first-run-complete ]; then netplan apply; fi
echo "###### IP Addresses"
ip -c -br addr
echo "###### IP Routes"
ip -c route
