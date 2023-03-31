VM_ID="299"
UBUNTU_DISTRO="jammy"
MEM_SIZE="8192"
CORES="4"
DISK_SIZE="50G"
STORAGE_NAME="local"
CI_USERNAME="le"
CI_PASSWORD="le"
LAUNCHPAD_ID="studiole"
SEARCH_DOMAIN="localdomain"
SSH_KEYS="./keys"
SSH_KEY="ssh-ed25519 AAAAC3NzaC1lZDI1NTE5AAAAIK2yRBr6Jq082zG9l40fX70jGEdUCdbmE1WABcbs8NO3 le@LE-7501"


# echo "[*] Download SSH keys from Launchpad"
wget https://launchpad.net/~${LAUNCHPAD_ID}/+sshkeys -O ./keys
# See: https://www.techbythenerd.com/posts/using-launchpad-for-ssh-keys/

echo "[*] Download the Ubuntu 'cloud image'"
# wget https://cloud-images.ubuntu.com/${UBUNTU_DISTRO}/current/${UBUNTU_DISTRO}-server-cloudimg-amd64.img

echo "[*] From the same command-line, create a virtual machine:"
qm create ${VM_ID} --memory ${MEM_SIZE} --name ubuntu-cloud-${UBUNTU_DISTRO} --net0 virtio,bridge=vmbr0

echo "[*] Import the disk into the proxmox storage, into '${STORAGE_NAME}' in this case."
qm importdisk ${VM_ID} ./${UBUNTU_DISTRO}-server-cloudimg-amd64.img ${STORAGE_NAME}

echo "[*] Add the new, imported disk to the VM:"
qm set ${VM_ID} --scsihw virtio-scsi-pci --scsi0 ${STORAGE_NAME}:${VM_ID}/vm-${VM_ID}-disk-0.raw

echo "[*] Add a CD-ROM:"
qm set ${VM_ID} --ide2 ${STORAGE_NAME}:cloudinit

echo "[*] Specify the boot disk:"
qm set ${VM_ID} --boot c --bootdisk scsi0

echo "[*] Add support for VNC and a serial console:"
qm set ${VM_ID} --serial0 socket --vga serial0

echo "[*] Set other template variables"
# qm set ${VM_ID} --ciuser ${CI_USERNAME} --cipassword ${CI_PASSWORD} --cores ${CORES} --searchdomain ${SEARCH_DOMAIN} --sshkeys ${SSH_KEYS} --description "Virtual machine based on the Ubuntu '${UBUNTU_DISTRO}' Cloud image." --ipconfig0 ip=dhcp --onboot 1 --ostype l26 --agent 1
qm set ${VM_ID} --cicustom "user=local:snippets/cloud-config.yml"

echo "[*] Resize boot disk to ${DISK_SIZE}B"
qm resize ${VM_ID} scsi0 ${DISK_SIZE}

echo "[*] Convert VM to a template"
qm template ${VM_ID}

# rm ./keys
echo "[+] Done."
