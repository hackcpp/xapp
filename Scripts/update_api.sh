#!/bin/bash
set -e 

CURRENT_TAG=${2:-latest}
IMAGE_NAME="dotnet-api"
TAR_FILE="api.tar"

function info() {
  echo -e "\033[1;34m[INFO]\033[0m $*"
}

function remove_old_images() {
  info "Removing Docker image ..."
  docker images |grep ${IMAGE_NAME} |awk '{print $3}'|xargs -r docker rmi -f || true
  info "Removing image from containerd via crictl..."
  sudo crictl images | grep ${IMAGE_NAME} | awk '{print $3}' | xargs -r sudo crictl rmi || true
}

# 主流程
kubectl get -f ../k8s/dotnet-api.yaml &>/dev/null && kubectl delete -f ../k8s/dotnet-api.yaml || true
remove_old_images

bash ./build_api_image.sh
kubectl apply -f ../k8s/dotnet-api.yaml
kubectl wait --for=condition=ready pod -l app=${IMAGE_NAME} -n dotnet-test --timeout=300s

info "Update deployment done."
