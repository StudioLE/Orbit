#!/bin/bash
set -euo pipefail

# Setup control plane
# Source: https://kubernetes.io/docs/setup/production-environment/tools/kubeadm/create-cluster-kubeadm/#initializing-your-control-plane-node

# Install calico networking
# Source: https://docs.tigera.io/calico/latest/getting-started/kubernetes/quickstart

echo "➡️  Initialize the control plane"
sudo kubeadm init \
  --cri-socket=unix:///var/run/cri-dockerd.sock \
  --pod-network-cidr=192.168.0.0/16
  # TODO: --control-plane-endpoint \
  # TODO: store the join command (final 2 lines of output)

echo "➡️  Create config directory"
mkdir -p $HOME/.kube
sudo cp -i /etc/kubernetes/admin.conf $HOME/.kube/config
sudo chown $(id -u):$(id -g) $HOME/.kube/config

# ONLY RUN ON CONTROL PLANE

echo "➡️  Install the Tigera Calico operator and custom resource definitions."
kubectl create -f https://raw.githubusercontent.com/projectcalico/calico/v3.25.1/manifests/tigera-operator.yaml

echo "➡️  Install Calico by creating the necessary custom resource."
kubectl create -f https://raw.githubusercontent.com/projectcalico/calico/v3.25.1/manifests/custom-resources.yaml

# echo "➡️  Remove the taints on the control plane so that you can schedule pods on it."
# TODO: Only required if we wish to schedule on the control plane
# https://kubernetes.io/docs/setup/production-environment/tools/kubeadm/create-cluster-kubeadm/#control-plane-node-isolation
# kubectl taint nodes --all node-role.kubernetes.io/control-plane-
# kubectl taint nodes --all node-role.kubernetes.io/master-5

# TODO: Install the calicoctl command line tool to manage Calico resources and perform administrative functions.
# https://docs.tigera.io/calico/latest/operations/calicoctl/install
