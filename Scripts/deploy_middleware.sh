#!/bin/bash
set -e

function info() {
  echo -e "\033[1;34m[INFO]\033[0m $*"
}

info "Step 1: Creating namespace 'dotnet-test'..."
kubectl create namespace dotnet-test || info "Namespace 'dotnet-test' already exists."

info "Step 2: Deploying shared storage..."
kubectl apply -f ../k8s/shared-storage.yaml

info "Creating directories for persistent volumes on the host..."
sudo mkdir -p /mnt/data/dotnet_mvc/sqlserver
sudo mkdir -p /mnt/data/dotnet_mvc/oracle

info "Changing ownership of persistent volume directories..."
sudo chown -R 10001:0 /mnt/data/dotnet_mvc/sqlserver
sudo chown -R 54321:54321 /mnt/data/dotnet_mvc/oracle

info "Step 3: Deploying middleware services..."
kubectl apply -f ../k8s/mysql.yaml
kubectl apply -f ../k8s/postgres.yaml
kubectl apply -f ../k8s/oracle.yaml
kubectl apply -f ../k8s/sqlserver.yaml
kubectl apply -f ../k8s/rabbitmq.yaml
kubectl apply -f ../k8s/memcached.yaml
kubectl apply -f ../k8s/redis.yaml

info "Waiting for all middleware pods to be ready..."
kubectl wait --for=condition=ready pod --all -n dotnet-test --timeout=600s 