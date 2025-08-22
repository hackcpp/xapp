#!/bin/bash
set -e

IMAGE_NAME="nginx-app"

function info() {
  echo -e "\033[1;34m[INFO]\033[0m $*"
}

bash ./build_nginx_image.sh
kubectl rollout restart deployment ${IMAGE_NAME} -n nginx-test

info "Update nginx-app deployment done."