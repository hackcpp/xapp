#!/bin/bash

# PROXY="192.168.0.14:5678"
# HTTP_PROXY_URL="http://$PROXY"
# HTTPS_PROXY_URL="https://$PROXY"

# echo "设置代理环境变量（大小写） ..."
# export http_proxy="$HTTP_PROXY_URL"
# export https_proxy="$HTTPS_PROXY_URL"
# export HTTP_PROXY="$HTTP_PROXY_URL"
# export HTTPS_PROXY="$HTTPS_PROXY_URL"

IMAGE="mcr.microsoft.com/mssql/server:2022-latest"
SLEEP_SECONDS=10

while true; do
    echo "Trying to pull image: $IMAGE ..."
    if docker pull "$IMAGE"; then
        echo "Successfully pulled $IMAGE"
        break
    else
        echo "Failed to pull $IMAGE, retrying in $SLEEP_SECONDS seconds..."
        sleep $SLEEP_SECONDS
    fi
done

docker save -o image.tar "$IMAGE"
sudo ctr -n k8s.io images import image.tar

