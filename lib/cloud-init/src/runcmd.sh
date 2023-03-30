echo "*********************************"
echo "Executing runcmd"
echo "*********************************"
systemctl restart ssh
systemctl start qemu-guest-agent
touch /root/first-run-complete
