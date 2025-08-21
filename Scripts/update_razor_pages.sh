#!/bin/bash
set -e 

# IMAGE_NAME="dotnet-api"

function info() {
  echo -e "\033[1;34m[INFO]\033[0m $*"
}

bash ./build_razor_pages_image.sh
kubectl rollout restart deployment dotnet-razor-pages -n dotnet-test

info "Update razor pages deployment done."
