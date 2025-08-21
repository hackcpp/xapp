#!/bin/bash
set -e

CURRENT_TAG=${2:-latest}
IMAGE_NAME="dotnet-razor-pages"
TAR_FILE="razor-pages.tar"

function info() {
  echo -e "\033[1;34m[INFO]\033[0m $*"
}

function build_image() {
  info "Building Docker image ${IMAGE_NAME}:${CURRENT_TAG}..."
  if [ -d "../RazorPages/publish" ]; then
    docker build -t ${IMAGE_NAME}:${CURRENT_TAG} ../RazorPages -f ../RazorPages/Dockerfile-dev
  else
    docker build -t ${IMAGE_NAME}:${CURRENT_TAG} ../RazorPages -f ../RazorPages/Dockerfile
  fi
}

function save_and_import_image() {
  info "Saving image ${IMAGE_NAME}:${CURRENT_TAG} to ${TAR_FILE}..."
  docker save -o ${TAR_FILE} ${IMAGE_NAME}:${CURRENT_TAG}

  info "Importing image tar into containerd..."
  sudo ctr -n k8s.io images import ${TAR_FILE}
  rm ${TAR_FILE} -fr
  docker rmi ${IMAGE_NAME}:${CURRENT_TAG}
}

build_image
save_and_import_image