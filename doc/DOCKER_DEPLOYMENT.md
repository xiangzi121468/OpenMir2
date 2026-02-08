# OpenMir2 Docker 部署指南

## 一、环境要求

- Docker 20.10+
- Docker Compose 2.0+
- 内存: 4GB+
- 硬盘: 20GB+

### 安装 Docker

**Ubuntu/Debian:**
```bash
# 安装 Docker
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER

# 安装 Docker Compose
sudo apt install docker-compose-plugin
```

**CentOS/RHEL:**
```bash
sudo yum install -y yum-utils
sudo yum-config-manager --add-repo https://download.docker.com/linux/centos/docker-ce.repo
sudo yum install -y docker-ce docker-ce-cli containerd.io docker-compose-plugin
sudo systemctl start docker
sudo systemctl enable docker
```

**Windows:**
```
下载安装 Docker Desktop: https://www.docker.com/products/docker-desktop
```

---

## 二、快速部署

### 2.1 克隆代码

```bash
git clone https://github.com/xiangzi121468/OpenMir2.git
cd OpenMir2
```

### 2.2 配置环境变量

```bash
# 复制环境变量模板
cp .env.example .env

# 编辑配置
nano .env
```

修改 `.env` 文件:
```ini
MYSQL_ROOT_PASSWORD=your_secure_password
JWT_SECRET=your_jwt_secret_key_at_least_32_characters
```

### 2.3 启动服务

```bash
# 构建并启动所有服务
docker-compose up -d

# 查看日志
docker-compose logs -f

# 查看服务状态
docker-compose ps
```

### 2.4 验证部署

```bash
# 检查服务状态
docker-compose ps

# 测试 WebApi
curl http://localhost:5000/api/health

# 检查端口
netstat -tlnp | grep -E "3306|5000|6000|7000|7100"
```

---

## 三、服务说明

| 服务 | 端口 | 说明 |
|------|------|------|
| mysql | 3306 | 数据库 |
| dbsrv | 6000 | 数据库服务 |
| loginsrv | 7000 | 登录服务 |
| gamesrv | 7100 | 游戏服务 |
| webapi | 5000 | API服务 |
| nginx | 80/443 | 反向代理(可选) |

---

## 四、常用命令

### 4.1 启动/停止

```bash
# 启动所有服务
docker-compose up -d

# 停止所有服务
docker-compose down

# 重启某个服务
docker-compose restart gamesrv

# 停止并删除数据卷（慎用！会删除数据库）
docker-compose down -v
```

### 4.2 查看日志

```bash
# 查看所有日志
docker-compose logs -f

# 查看特定服务日志
docker-compose logs -f gamesrv

# 查看最近100行日志
docker-compose logs --tail=100 gamesrv
```

### 4.3 进入容器

```bash
# 进入 MySQL 容器
docker-compose exec mysql mysql -u root -p

# 进入 GameSrv 容器
docker-compose exec gamesrv /bin/bash
```

### 4.4 重新构建

```bash
# 重新构建所有镜像
docker-compose build --no-cache

# 重新构建并启动
docker-compose up -d --build
```

---

## 五、数据库初始化

首次启动时，MySQL 会自动执行 `sql/` 目录下的 SQL 文件。

如果需要手动初始化:

```bash
# 进入 MySQL 容器
docker-compose exec mysql mysql -u root -p

# 创建数据库
CREATE DATABASE IF NOT EXISTS mir2_account;
CREATE DATABASE IF NOT EXISTS mir2_data;
CREATE DATABASE IF NOT EXISTS mir2_db;

# 导入数据（在容器外执行）
docker-compose exec -T mysql mysql -u root -p$MYSQL_ROOT_PASSWORD mir2_account < sql/mir2_account.sql
docker-compose exec -T mysql mysql -u root -p$MYSQL_ROOT_PASSWORD mir2_data < sql/mir2_data.sql
docker-compose exec -T mysql mysql -u root -p$MYSQL_ROOT_PASSWORD mir2_db < sql/mir2_db.sql
```

---

## 六、数据备份与恢复

### 6.1 备份数据库

```bash
# 备份所有数据库
docker-compose exec mysql mysqldump -u root -p --all-databases > backup_$(date +%Y%m%d).sql

# 备份单个数据库
docker-compose exec mysql mysqldump -u root -p mir2_db > mir2_db_$(date +%Y%m%d).sql
```

### 6.2 恢复数据库

```bash
# 恢复数据库
docker-compose exec -T mysql mysql -u root -p < backup_20260208.sql
```

### 6.3 备份数据卷

```bash
# 备份 MySQL 数据卷
docker run --rm -v openmir2_mysql_data:/data -v $(pwd):/backup alpine tar czf /backup/mysql_data.tar.gz /data
```

---

## 七、生产环境优化

### 7.1 启用 Nginx

```bash
# 启动时包含 Nginx
docker-compose --profile with-nginx up -d
```

### 7.2 配置 SSL

1. 将 SSL 证书放入 `docker/nginx/ssl/` 目录
2. 编辑 `docker/nginx/nginx.conf` 启用 HTTPS 配置
3. 重启 Nginx: `docker-compose restart nginx`

### 7.3 资源限制

在 `docker-compose.yml` 中添加资源限制:

```yaml
services:
  gamesrv:
    # ... 其他配置
    deploy:
      resources:
        limits:
          cpus: '2'
          memory: 4G
        reservations:
          cpus: '1'
          memory: 2G
```

---

## 八、故障排查

### 8.1 服务无法启动

```bash
# 查看详细日志
docker-compose logs gamesrv

# 检查容器状态
docker-compose ps -a

# 检查网络
docker network ls
docker network inspect openmir2_openmir2-network
```

### 8.2 数据库连接失败

```bash
# 检查 MySQL 是否就绪
docker-compose exec mysql mysqladmin ping -h localhost

# 检查连接
docker-compose exec gamesrv ping mysql
```

### 8.3 端口冲突

```bash
# 检查端口占用
netstat -tlnp | grep 7000

# 修改 docker-compose.yml 中的端口映射
ports:
  - "17000:7000"  # 改为其他端口
```

---

## 九、架构图

```
                    ┌─────────────────────────────────────────────┐
                    │               Docker Network                │
                    │                                             │
   ┌────────────┐   │   ┌─────────┐    ┌──────────┐              │
   │   Client   │───┼──▶│ Nginx   │───▶│  WebApi  │              │
   │ (Windows)  │   │   │  :80    │    │  :5000   │              │
   └────────────┘   │   └─────────┘    └────┬─────┘              │
         │          │                       │                     │
         │          │   ┌───────────────────┼───────────────┐    │
         │          │   │                   ▼               │    │
         ▼          │   │             ┌──────────┐          │    │
   ┌──────────┐     │   │             │  MySQL   │          │    │
   │ LoginSrv │◀────┼───┤             │  :3306   │          │    │
   │  :7000   │     │   │             └────┬─────┘          │    │
   └────┬─────┘     │   │                  │                │    │
        │           │   │                  ▼                │    │
        ▼           │   │            ┌──────────┐           │    │
   ┌──────────┐     │   │            │  DBSrv   │           │    │
   │ GameSrv  │◀────┼───┼───────────▶│  :6000   │           │    │
   │  :7100   │     │   │            └──────────┘           │    │
   └──────────┘     │   │                                   │    │
                    │   └───────────────────────────────────┘    │
                    │                                             │
                    └─────────────────────────────────────────────┘
```

---

## 十、快速参考

```bash
# 一键启动
docker-compose up -d

# 一键停止
docker-compose down

# 查看状态
docker-compose ps

# 查看日志
docker-compose logs -f

# 重新构建
docker-compose up -d --build

# 进入数据库
docker-compose exec mysql mysql -u root -p
```

---

**版本**: v1.0  
**更新日期**: 2026-02-08
