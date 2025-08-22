# --------------------------
# Stage 1: Build Nginx
# --------------------------
FROM debian:bullseye-slim AS builder

ARG NGINX_VERSION=1.24.0
ARG NGINX_CONFIGURE_ARGS="--prefix=/etc/nginx \
    --sbin-path=/usr/sbin/nginx \
    --modules-path=/usr/lib/nginx/modules \
    --conf-path=/etc/nginx/nginx.conf \
    --error-log-path=/var/log/nginx/error.log \
    --http-log-path=/var/log/nginx/access.log \
    --pid-path=/var/run/nginx.pid \
    --lock-path=/var/run/nginx.lock \
    --http-client-body-temp-path=/var/cache/nginx/client_temp \
    --http-proxy-temp-path=/var/cache/nginx/proxy_temp \
    --http-fastcgi-temp-path=/var/cache/nginx/fastcgi_temp \
    --http-uwsgi-temp-path=/var/cache/nginx/uwsgi_temp \
    --http-scgi-temp-path=/var/cache/nginx/scgi_temp \
    --with-http_ssl_module \
    --with-http_v2_module \
    --with-http_realip_module \
    --with-http_gunzip_module \
    --with-http_gzip_static_module \
    --with-threads"

# 安装构建依赖并清理缓存
RUN apt-get update && apt-get install -y --no-install-recommends \
    build-essential \
    libpcre3-dev \
    zlib1g-dev \
    libssl-dev \
    wget \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

WORKDIR /usr/src

# 下载并解压 Nginx
RUN wget http://nginx.org/download/nginx-${NGINX_VERSION}.tar.gz \
    && tar zxf nginx-${NGINX_VERSION}.tar.gz \
    && rm nginx-${NGINX_VERSION}.tar.gz

WORKDIR /usr/src/nginx-${NGINX_VERSION}

# 编译安装 Nginx并创建模块目录
RUN ./configure ${NGINX_CONFIGURE_ARGS} \
    && make -j$(nproc) \
    && make install \
    && mkdir -p /usr/lib/nginx/modules

# --------------------------
# Stage 2: Runtime Image
# --------------------------
FROM debian:bullseye-slim

# 复制 nginx 可执行文件和配置
COPY --from=builder /usr/sbin/nginx /usr/sbin/nginx
COPY --from=builder /etc/nginx /etc/nginx

# 安装运行依赖
RUN apt-get update && apt-get install -y --no-install-recommends \
    libpcre3 \
    openssl \
    ca-certificates \
    && rm -rf /var/lib/apt/lists/*

# 创建 nginx 用户
RUN addgroup --system nginx \
    && adduser --system --disabled-login --ingroup nginx --no-create-home --home /nonexistent --gecos "nginx user" --shell /bin/false nginx

# 创建日志目录并设置输出到 stdout/stderr
RUN mkdir -p /var/log/nginx \
    && ln -sf /dev/stdout /var/log/nginx/access.log \
    && ln -sf /dev/stderr /var/log/nginx/error.log

# 创建缓存目录并设置权限
RUN mkdir -p /var/cache/nginx/client_temp \
    /var/cache/nginx/proxy_temp \
    /var/cache/nginx/fastcgi_temp \
    /var/cache/nginx/uwsgi_temp \
    /var/cache/nginx/scgi_temp \
    && chown -R nginx:nginx /var/cache/nginx

EXPOSE 80

STOPSIGNAL SIGQUIT

# 默认启动命令
CMD ["nginx", "-g", "daemon off;"]
