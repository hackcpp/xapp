#!/bin/bash
set -e 

IMAGE_NAME="dotnet-mvc-app"

function info() {
  echo -e "\033[1;34m[INFO]\033[0m $*"
}

# function remove_old_images() {
#   info "Removing Docker image ..."
#   docker images |grep ${IMAGE_NAME} |awk '{print $3}'|xargs -r docker rmi -f || true
#   info "Removing image from containerd via crictl..."
#   sudo crictl images | grep ${IMAGE_NAME} | awk '{print $3}' | xargs -r sudo crictl rmi || true
# }


bash ./build_mvc_image.sh
kubectl rollout restart deployment ${IMAGE_NAME} -n dotnet-test
# kubectl wait --for=condition=ready pod -l app=${IMAGE_NAME} -n dotnet-test --timeout=300s

info "Update MVC deployment done."
