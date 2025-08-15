# .NET MVC 中间件测试项目

## 项目概述
该项目用于测试各种中间件的集成和使用，包括：
- 数据库：MySQL, PostgreSQL, SQL Server, Oracle, Redis，Memcached，RabbitMQ
- 服务通信：HTTP

## 项目结构
```
Controllers/    # 各中间件测试控制器
Data/           # 数据库上下文
Models/         # 数据模型
Views/          # 视图文件
wwwroot/        # 静态资源
```

## 使用指南
1. 配置连接字符串(appsettings.json)
2. 运行需要测试的控制器方法
3. 查看测试结果

## Kubernetes 部署

### 准备工作
1. 确保已安装kubectl并配置好Kubernetes集群
2. 创建命名空间：
   ```bash
   kubectl create namespace dotnet-test
   ```

### 部署步骤

1. 部署共享存储：
   ```bash
   kubectl apply -f k8s/shared-storage.yaml
   
   sudo mkdir -p /mnt/data/dotnet_mvc/sqlserver
   sudo mkdir -p /mnt/data/dotnet_mvc/oracle
   sudo chown -R 10001:0 /mnt/data/dotnet_mvc/sqlserver
   sudo chown -R 54321:54321 /mnt/data/dotnet_mvc/oracle
   ```

2. 部署中间件服务：

- 所有中间件使用统一密码: Middleware123
- 数据存储使用共享PVC，通过subPath隔离

   ```bash
   # MySQL
   kubectl apply -f k8s/mysql.yaml

   # PostgreSQL
   kubectl apply -f k8s/postgres.yaml
   
   # Oracle
   kubectl apply -f k8s/oracle.yaml
   
   # SqlServer
   kubectl apply -f k8s/sqlserver.yaml

   # RabbitMQ
   kubectl apply -f k8s/rabbitmq.yaml
   - 默认用户名/密码: guest/guest
   - 管理界面端口: 15672 (通过rabbitmq-service访问)
   - AMQP端口: 5672
   - 数据持久化在共享PVC的rabbitmq子目录
   - 管理界面URL: http://rabbitmq-service.dotnet-test.svc.cluster.local:15672

   # Memcached
   kubectl apply -f k8s/memcached.yaml

   # Redis
   kubectl apply -f k8s/redis.yaml
   ```

3. 构建.NET应用镜像：
   ```bash
   # 在项目根目录执行
   docker build -t docker.io/library/dotnet-mvc-app:latest .
   
   # (可选) 如果你的Kubernetes节点无法直接访问Docker Hub或本地镜像，
   # 你可能需要将镜像推送到一个节点可以访问的镜像仓库，
   # 或者像 k8s/update.sh 脚本中那样，将镜像导出并导入到containerd中。
   # 例如：
   # docker save -o mvc.tar docker.io/library/dotnet-mvc-app:latest
   # sudo ctr -n k8s.io images import mvc.tar
   ```

4. 部署.NET应用：
   ```bash
   kubectl apply -f k8s/dotnet-app.yaml
   ```

### 访问应用
1. 通过Ingress访问：
   ```bash
   # 确保在本地hosts文件中添加DNS解析
   # 例如：<集群IP> dotnet.ziyou.com
   
   # 获取Ingress IP
   kubectl get ingress -n dotnet-test
   ```
2. 访问地址：http://dotnet.ziyou.com



