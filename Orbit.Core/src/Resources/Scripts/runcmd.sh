echo "*********************************"
echo "Executing runcmd"
echo "*********************************"
systemctl restart ssh
date --rfc-3339 ns | tr '\n' ' ' | sudo tee -a /var/log/orbit.log
echo "Executed runcmd" | sudo tee -a /var/log/orbit.log
