#!/bin/bash

IMAGE="container-registry.oracle.com/database/express:21.3.0-xe"
NAMESPACE="k8s.io"

while true; do
  echo "尝试导入镜像: $IMAGE -> containerd ($NAMESPACE)"
  if docker save "$IMAGE" | sudo ctr -n "$NAMESPACE" images import -; then
    echo "导入成功"
    exit 0
  else
    echo "导入失败，3 秒后重试..."
    sleep 3
  fi
done
