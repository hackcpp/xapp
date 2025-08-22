#!/bin/bash
set -e

CURRENT_TAG=${2:-latest}
IMAGE_NAME="dotnet-api"
TAR_FILE="api.tar"

function info() {
  echo -e "\033[1;34m[INFO]\033[0m $*"
}

function build_image() {
  info "Building Docker image ${IMAGE_NAME}:${CURRENT_TAG}..."
  if [ -d "../api/publish" ]; then
    docker build -t ${IMAGE_NAME}:${CURRENT_TAG} ../api -f ../api/Dockerfile-dev
  else
    docker build -t ${IMAGE_NAME}:${CURRENT_TAG} ../api -f ../api/Dockerfile
  fi
}

function save_and_import_image() {
  info "Saving and importing image ${IMAGE_NAME}:${CURRENT_TAG} directly to containerd..."
  docker save ${IMAGE_NAME}:${CURRENT_TAG} | sudo ctr -n k8s.io images import -
  
  info "Removing local image..."
  docker rmi ${IMAGE_NAME}:${CURRENT_TAG}
}

build_image
save_and_import_image