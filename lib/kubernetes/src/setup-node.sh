#!/bin/bash
set -euo pipefail

# Setup node
# Source: https://kubernetes.io/docs/setup/production-environment/tools/kubeadm/create-cluster-kubeadm/#join-nodes

echo "➡️  Join the node to this cluster"
kubeadm join 10.10.10.100:6443 --token f8xqkm.wdno54yho01bp8k1 \
  --discovery-token-ca-cert-hash sha256:784b978ce121f3a93c0558876824314eb548f4aa54bebff66bd30f06a68e5be1 \
  --cri-socket=unix:///var/run/cri-dockerd.sock

