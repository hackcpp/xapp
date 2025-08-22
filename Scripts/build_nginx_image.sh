#!/bin/bash
set -e

NGINX_VERSION=${1:-1.27.4}
IMAGE_NAME="nginx-app"
TAR_FILE="nginx-app.tar"
CURRENT_TAG=${2:-latest}

function info() {
  echo -e "\033[1;34m[INFO]\033[0m $*"
}

function build_image() {
  info "Building Docker image ${IMAGE_NAME}:${CURRENT_TAG}..."
  docker build \
    --build-arg NGINX_VERSION=${NGINX_VERSION} \
    -t ${IMAGE_NAME}:${CURRENT_TAG} \
    ../nginx
}

function save_and_import_image() {
  info "Saving and importing image ${IMAGE_NAME}:${CURRENT_TAG} directly to containerd..."
  docker save ${IMAGE_NAME}:${CURRENT_TAG} ${TAR_FILE} 
  sudo ctr -n k8s.io images import ${TAR_FILE} 
  
  info "Removing local image..."
  rm -fr ${TAR_FILE} 
  # docker rmi ${IMAGE_NAME}:${CURRENT_TAG}
}

build_image
save_and_import_image
