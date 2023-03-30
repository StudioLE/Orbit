#!/bin/bash
set -euo pipefail

# Setup host to control control plane
# Source: https://kubernetes.io/docs/setup/production-environment/tools/kubeadm/create-cluster-kubeadm/#optional-controlling-your-cluster-from-machines-other-than-the-control-plane-node

echo "➡️  Initialize the control plane"
scp radiko@10.10.10.100:/home/radiko/.kube/config ./admin.conf
kubectl --kubeconfig ./admin.conf get nodes

# WORK IN PROGRESS

# TODO: Need to install kubectl...
