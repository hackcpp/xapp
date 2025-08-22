#!/bin/bash
set -e 

IMAGE_NAME="dotnet-api"

function info() {
  echo -e "\033[1;34m[INFO]\033[0m $*"
}

bash ./build_api_image.sh
kubectl rollout restart deployment ${IMAGE_NAME} -n dotnet-test

info "Update Api deployment done."
