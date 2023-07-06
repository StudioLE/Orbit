echo "*********************************"
echo "Executing bootcmd"
echo "*********************************"
echo "###### IP Addresses"
ip -br addr
echo "###### IP Routes"
ip route
date --rfc-3339 ns | tr '\n' ' ' | sudo tee -a /var/log/orbit.log
echo "Executed bootcmd" | sudo tee -a /var/log/orbit.log
