# .NET 解决方案 - 中间件测试平台

## 项目概述
该解决方案包含API和MVC两个项目，用于测试各种中间件的集成和使用，包括：
- 数据库：MySQL, PostgreSQL, SQL Server, Oracle, Redis, Memcached
- 消息队列：RabbitMQ, ActiveMQ
- 服务通信：HTTP

## 项目结构
```
xapp/
├── api/            # Web API 项目
│   ├── Controllers/
│   └── Properties/
├── k8s/            # Kubernetes 部署文件
├── mvc/            # MVC Web 应用
│   ├── Controllers/ # 各中间件测试控制器
│   ├── Models/      # 数据模型
│   ├── Views/       # 视图文件
│   └── wwwroot/     # 静态资源
└── Scripts/        # 构建和部署脚本
```

## 使用指南
1. 配置连接字符串(api/appsettings.json 和 mvc/appsettings.json)
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


1. 部署中间件服务：

- 所有中间件使用统一密码: Middleware123

   ```bash
   # MySQL
   kubectl apply -f k8s/mysql.yaml

   # PostgreSQL
   kubectl apply -f k8s/postgres.yaml
   
   # Oracle
   kubectl apply -f k8s/oracle.yaml
   
   # SqlServer
   kubectl apply -f k8s/sqlserver.yaml

   # Memcached
   kubectl apply -f k8s/memcached.yaml

   # Redis
   kubectl apply -f k8s/redis.yaml
   
   # RabbitMQ
   kubectl apply -f k8s/rabbitmq.yaml
   - 默认用户名/密码: guest/guest
   - 管理界面端口: 15672 (通过rabbitmq-service访问)
   - AMQP端口: 5672
   - 管理界面URL: http://rabbitmq-service.dotnet-test.svc.cluster.local:15672

   # ActiveMQ
   kubectl apply -f k8s/activemq.yaml
   - 默认用户名/密码: admin/admin
   - 管理界面端口: 8161 (通过activemq-service访问)
   - 消息端口: 61616
   - 管理界面URL: http://activemq-service.dotnet-test.svc.cluster.local:8161

   # 部署所有的中间件服务
   bash Scripts/deploy_middleware.sh
   ```

2. 构建.NET应用镜像：
   ```bash
   # 构建API镜像
   bash Scripts/build_api_image.sh

   # 构建MVC镜像
   bash Scripts/build_mvc_image.sh
   ```

3. 部署.NET应用：
   ```bash
   kubectl apply -f k8s/dotnet-api.yaml
   kubectl apply -f k8s/dotnet-app.yaml
   ```

### 更新应用
当代码变更后，可以使用以下脚本更新Kubernetes中的部署：

1. 更新API服务：
```bash
bash ./Scripts/update_api.sh
```

2. 更新MVC应用：
```bash
bash ./Scripts/update_mvc.sh
```

这些脚本会自动：
- 删除旧部署
- 清理旧镜像
- 构建新镜像
- 重新部署应用
- 等待新Pod就绪

### 访问应用
1. 通过Ingress访问：
    ```bash
    # 确保在本地hosts文件中添加DNS解析
    # 例如：<集群IP> dotnet.ziyou.com
    
    # 获取Ingress IP和端口
    kubectl get ingress -n dotnet-test
    kubectl get svc ingress-nginx-controller -n ingress-nginx
    
    # 示例输出：
    # NAME                       TYPE       CLUSTER-IP      EXTERNAL-IP   PORT(S)                      AGE
    # ingress-nginx-controller   NodePort   10.104.238.69   <none>        80:30706/TCP,443:31770/TCP   20d
    ```
   访问地址：
   - HTTP: http://dotnet.ziyou.com:30706
   - HTTPS: https://dotnet.ziyou.com:31770
   - 直接访问节点: http://<节点IP>:30706 或 https://<节点IP>:31770