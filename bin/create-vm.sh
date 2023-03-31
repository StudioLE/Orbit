# ARGS

VERSION="${1}"

# CONSTANTS

VERSION="jammy"
FILENAME="${VERSION}-server-cloudimg-amd64.img"
VM_ID=$(get /cluster/nextid)
STORAGE_NAME="local"
MEMORY="8192"
CORES="4"
DISK_SIZE="50G"

# VALIDATE

if [ "${VM_ID}" = "" |  "${VM_ID}" -lt "100" | "${VM_ID}" -gt "253" ]; then
    echo "ERROR: Invalid VM_ID: ${VM_ID}"
    exit 1
fi

# OUTPUT

echo "VERSION: ${VERSION}"
echo "FILENAME: ${FILENAME}"
echo "VM_ID: ${VM_ID}"
echo "STORAGE_NAME: ${STORAGE_NAME}"
echo "MEMORY: ${MEMORY}"
echo "CORES: ${CORES}"
echo "DISK_SIZE: ${DISK_SIZE}"

# START

# Exit on failure
set -e

echo "##### Create a virtual machine:"
qm create ${VM_ID} --memory ${MEM_SIZE} --name ubuntu-cloud-${UBUNTU_DISTRO} --net0 virtio,bridge=vmbr0

echo "##### Import the disk into the proxmox storage"
qm importdisk ${VM_ID} ./${UBUNTU_DISTRO}-server-cloudimg-amd64.img ${STORAGE_NAME}

echo "##### Add the new, imported disk to the VM"
qm set ${VM_ID} --scsihw virtio-scsi-pci --scsi0 ${STORAGE_NAME}:${VM_ID}/vm-${VM_ID}-disk-0.raw

echo "##### Add a CD-ROM for cloud-init"
qm set ${VM_ID} --ide2 ${STORAGE_NAME}:cloudinit

echo "##### Specify the boot disk:"
qm set ${VM_ID} --boot c --bootdisk scsi0

echo "##### Add support for VNC and a serial console"
qm set ${VM_ID} --serial0 socket --vga serial0

echo "##### Set cores"
qm set ${VM_ID}  --cores ${CORES}

echo "##### Set cloud-config"
qm set ${VM_ID} --cicustom "user=local:snippets/cloud-config.yml"

echo "##### Resize boot disk"
qm resize ${VM_ID} scsi0 ${DISK_SIZE}
