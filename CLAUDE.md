# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Common Commands

### Build & Deploy
- Build API image: `bash Scripts/build_api_image.sh`
- Build MVC image: `bash Scripts/build_mvc_image.sh`
- Deploy middleware: `bash Scripts/deploy_middleware.sh`
- Update API: `bash Scripts/update_api.sh`
- Update MVC: `bash Scripts/update_mvc.sh`

### Kubernetes
- Create namespace: `kubectl create namespace dotnet-test`
- Apply configurations: `kubectl apply -f k8s/<file>.yaml`
- Get ingress details: `kubectl get ingress -n dotnet-test`


## Architecture Overview

### Projects
- **api/**: Web API for testing middleware integrations.
- **mvc/**: MVC application for testing middleware usage.
- **k8s/**: Kubernetes deployment files for middleware and applications.
- **Scripts/**: Build and deployment scripts.

### Middleware
- Supported databases: MySQL, PostgreSQL, SQL Server, Oracle, Redis, Memcached.
- Supported message queues: RabbitMQ, ActiveMQ.
- Service communication: HTTP.