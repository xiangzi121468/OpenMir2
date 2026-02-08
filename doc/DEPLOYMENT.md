# OpenMir2 完整部署指南

## 一、环境准备

### 1.1 服务器要求

| 配置项 | 最低要求 | 推荐配置 |
|--------|----------|----------|
| 操作系统 | Windows Server 2019 / Ubuntu 20.04 | Windows Server 2022 / Ubuntu 22.04 |
| CPU | 4核 | 8核+ |
| 内存 | 8GB | 16GB+ |
| 硬盘 | 50GB SSD | 100GB SSD |
| 网络 | 10Mbps | 100Mbps+ |

### 1.2 软件环境

```bash
# 必需软件
- .NET 8.0 SDK/Runtime
- MySQL 8.0+
- Git (可选，用于拉取代码)

# 可选软件
- Nginx (用于反向代理WebApi)
- Redis (用于缓存，可选)
```

### 1.3 安装 .NET 8.0

**Windows:**
```powershell
# 下载安装包
https://dotnet.microsoft.com/download/dotnet/8.0

# 或使用 winget
winget install Microsoft.DotNet.SDK.8
```

**Linux (Ubuntu):**
```bash
# 添加 Microsoft 包源
wget https://packages.microsoft.com/config/ubuntu/22.04/packages-microsoft-prod.deb -O packages-microsoft-prod.deb
sudo dpkg -i packages-microsoft-prod.deb
rm packages-microsoft-prod.deb

# 安装 .NET SDK
sudo apt-get update
sudo apt-get install -y dotnet-sdk-8.0
```

### 1.4 安装 MySQL

**Windows:**
```powershell
# 下载 MySQL Installer
https://dev.mysql.com/downloads/installer/

# 安装时选择 Server Only
# 设置 root 密码，记住这个密码
```

**Linux:**
```bash
sudo apt-get update
sudo apt-get install -y mysql-server

# 安全配置
sudo mysql_secure_installation

# 设置 root 密码
sudo mysql -u root -p
ALTER USER 'root'@'localhost' IDENTIFIED WITH mysql_native_password BY 'your_password';
FLUSH PRIVILEGES;
```

---

## 二、获取代码

```bash
# 克隆代码
git clone https://github.com/xiangzi121468/OpenMir2.git
cd OpenMir2

# 切换到 dev 分支（最新功能）
git checkout dev
```

---

## 三、数据库初始化

### 3.1 创建数据库

```sql
-- 连接 MySQL
mysql -u root -p

-- 创建数据库
CREATE DATABASE IF NOT EXISTS mir2_account CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
CREATE DATABASE IF NOT EXISTS mir2_data CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
CREATE DATABASE IF NOT EXISTS mir2_db CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;

-- 创建游戏专用用户（推荐）
CREATE USER 'mir2'@'localhost' IDENTIFIED BY 'your_game_password';
GRANT ALL PRIVILEGES ON mir2_account.* TO 'mir2'@'localhost';
GRANT ALL PRIVILEGES ON mir2_data.* TO 'mir2'@'localhost';
GRANT ALL PRIVILEGES ON mir2_db.* TO 'mir2'@'localhost';
FLUSH PRIVILEGES;
```

### 3.2 导入数据库

**方法一：使用自动化脚本（推荐）**

```bash
cd sql

# Windows
init_database.bat localhost root your_password

# Linux/Mac
chmod +x init_database.sh
./init_database.sh -h localhost -u root -p your_password

# Python (跨平台)
python init_database.py --host localhost --user root --password your_password
```

**方法二：手动导入**

```bash
cd sql

# 阶段1：核心数据库
mysql -u root -p mir2_account < mir2_account.sql
mysql -u root -p mir2_data < mir2_data.sql
mysql -u root -p --max_allowed_packet=512M mir2_db < mir2_db.sql

# 阶段2：索引优化
mysql -u root -p mir2_db < characters_indexes.sql
mysql -u root -p mir2_db < fix_gamegold_field.sql

# 阶段3：功能模块
mysql -u root -p mir2_db < shop_system.sql
mysql -u root -p mir2_db < mail_system.sql
mysql -u root -p mir2_db < notice_system.sql
mysql -u root -p mir2_db < activity_system.sql
mysql -u root -p mir2_db < castle_war.sql
mysql -u root -p mir2_db < appraisal_system.sql
mysql -u root -p mir2_db < vip_map_system.sql
mysql -u root -p mir2_db < market_system.sql
mysql -u root -p mir2_db < player_logs.sql
mysql -u root -p mir2_db < gm_admin.sql

# 阶段4：游戏内容
mysql -u root -p mir2_data < skill_books.sql
mysql -u root -p mir2_data < new_weapons.sql
mysql -u root -p mir2_data < new_armors.sql
mysql -u root -p mir2_data < new_accessories.sql
mysql -u root -p mir2_data < new_monsters.sql

# 阶段5：配置数据
mysql -u root -p mir2_db < vip_map_monsters_config.sql
mysql -u root -p mir2_data < all_weapon_effects.sql
```

### 3.3 验证数据库

```bash
mysql -u root -p < verify_database.sql
```

---

## 四、编译项目

### 4.1 还原依赖

```bash
cd OpenMir2
dotnet restore OpenMir2.sln
```

### 4.2 编译发布

```bash
# Debug 模式（开发调试）
dotnet build OpenMir2.sln -c Debug

# Release 模式（正式部署）
dotnet build OpenMir2.sln -c Release

# 发布到指定目录
dotnet publish src/GameSrv/GameSrv.csproj -c Release -o publish/GameSrv
dotnet publish src/LoginSrv/LoginSrv.csproj -c Release -o publish/LoginSrv
dotnet publish src/DBSrv/DBSrv.csproj -c Release -o publish/DBSrv
dotnet publish src/WebApi/WebApi.csproj -c Release -o publish/WebApi
```

---

## 五、配置文件修改

### 5.1 DBSrv 配置

编辑 `MirServer/DBServer/dbsvr.conf`:

```ini
[Server]
ServerName=DBServer
ServerIndex=0

[Database]
Host=127.0.0.1
Port=3306
Database=mir2_db
User=mir2
Password=your_game_password

[Account]
Host=127.0.0.1
Port=3306
Database=mir2_account
User=mir2
Password=your_game_password
```

### 5.2 LoginSrv 配置

编辑 `MirServer/LoginSrv/config.conf`:

```ini
[Server]
ServerName=LoginServer
GatePort=7000

[DBServer]
Host=127.0.0.1
Port=6000
```

### 5.3 GameSrv 配置

编辑 `src/GameSrv/appsettings.json`:

```json
{
  "GameServer": {
    "ServerName": "OpenMir2",
    "ServerIndex": 0,
    "GatePort": 7100
  },
  "Database": {
    "ConnectionString": "Server=127.0.0.1;Database=mir2_db;User=mir2;Password=your_game_password;"
  },
  "GameData": {
    "ConnectionString": "Server=127.0.0.1;Database=mir2_data;User=mir2;Password=your_game_password;"
  }
}
```

### 5.4 WebApi 配置

编辑 `src/WebApi/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=127.0.0.1;Database=mir2_db;User=mir2;Password=your_game_password;",
    "GameDataConnection": "Server=127.0.0.1;Database=mir2_data;User=mir2;Password=your_game_password;",
    "AccountConnection": "Server=127.0.0.1;Database=mir2_account;User=mir2;Password=your_game_password;"
  },
  "Jwt": {
    "Secret": "your_jwt_secret_key_at_least_32_characters",
    "Issuer": "OpenMir2",
    "Audience": "OpenMir2Admin"
  },
  "Urls": "http://0.0.0.0:5000"
}
```

---

## 六、启动服务

### 6.1 启动顺序（重要！）

```
1. DBSrv     (数据库服务)
2. LoginSrv  (登录服务)
3. GameSrv   (游戏服务)
4. WebApi    (API服务，可选)
```

### 6.2 Windows 启动

```powershell
# 打开多个命令行窗口，分别执行：

# 窗口1 - DBSrv
cd publish/DBSrv
dotnet DBSrv.dll

# 窗口2 - LoginSrv
cd publish/LoginSrv
dotnet LoginSrv.dll

# 窗口3 - GameSrv
cd publish/GameSrv
dotnet GameSrv.dll

# 窗口4 - WebApi (可选)
cd publish/WebApi
dotnet WebApi.dll
```

### 6.3 Linux 启动（使用 systemd）

创建服务文件 `/etc/systemd/system/openmir2-dbsrv.service`:

```ini
[Unit]
Description=OpenMir2 Database Server
After=mysql.service

[Service]
WorkingDirectory=/opt/openmir2/publish/DBSrv
ExecStart=/usr/bin/dotnet DBSrv.dll
Restart=always
RestartSec=10
User=mir2
Environment=DOTNET_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

类似创建其他服务文件，然后启动：

```bash
sudo systemctl daemon-reload
sudo systemctl enable openmir2-dbsrv openmir2-loginsrv openmir2-gamesrv openmir2-webapi
sudo systemctl start openmir2-dbsrv
sudo systemctl start openmir2-loginsrv
sudo systemctl start openmir2-gamesrv
sudo systemctl start openmir2-webapi
```

### 6.4 一键启动脚本

**Windows (start_server.bat):**
```batch
@echo off
echo Starting OpenMir2 Servers...

start "DBSrv" cmd /k "cd /d %~dp0publish\DBSrv && dotnet DBSrv.dll"
timeout /t 5

start "LoginSrv" cmd /k "cd /d %~dp0publish\LoginSrv && dotnet LoginSrv.dll"
timeout /t 3

start "GameSrv" cmd /k "cd /d %~dp0publish\GameSrv && dotnet GameSrv.dll"
timeout /t 3

start "WebApi" cmd /k "cd /d %~dp0publish\WebApi && dotnet WebApi.dll"

echo All servers started!
pause
```

---

## 七、防火墙配置

### 7.1 端口清单

| 端口 | 服务 | 说明 |
|------|------|------|
| 6000 | DBSrv | 数据库服务内部通信 |
| 7000 | LoginSrv | 登录服务器（客户端连接） |
| 7100 | GameGate | 游戏网关（客户端连接） |
| 5000 | WebApi | HTTP API |
| 3306 | MySQL | 数据库（仅内部） |

### 7.2 Windows 防火墙

```powershell
# 开放端口
netsh advfirewall firewall add rule name="OpenMir2 Login" dir=in action=allow protocol=tcp localport=7000
netsh advfirewall firewall add rule name="OpenMir2 Game" dir=in action=allow protocol=tcp localport=7100
netsh advfirewall firewall add rule name="OpenMir2 WebApi" dir=in action=allow protocol=tcp localport=5000
```

### 7.3 Linux 防火墙

```bash
# Ubuntu/Debian (ufw)
sudo ufw allow 7000/tcp
sudo ufw allow 7100/tcp
sudo ufw allow 5000/tcp
sudo ufw reload

# CentOS/RHEL (firewalld)
sudo firewall-cmd --permanent --add-port=7000/tcp
sudo firewall-cmd --permanent --add-port=7100/tcp
sudo firewall-cmd --permanent --add-port=5000/tcp
sudo firewall-cmd --reload
```

---

## 八、客户端配置

### 8.1 修改服务器地址

编辑客户端目录下的 `!setup.txt` 或 `Mir2.ini`:

```ini
[Server]
Title=OpenMir2
ServerName=OpenMir2
ServerAddr=你的服务器IP
ServerPort=7000
```

### 8.2 客户端资源

确保以下资源文件存在于客户端 `Data` 目录:

```
Data/
├── AccessoryItems.data    (配饰图标)
├── WeaponEffect100.data   (武器特效)
├── NewHum.data            (新衣服)
├── Mon41.data - Mon46.data (新怪物)
└── ... (其他资源)
```

---

## 九、验证部署

### 9.1 检查服务状态

```bash
# Windows
netstat -an | findstr "7000 7100 5000 6000"

# Linux
netstat -tlnp | grep -E "7000|7100|5000|6000"
```

### 9.2 测试 WebApi

```bash
# 测试 API 是否正常
curl http://localhost:5000/api/health

# 或浏览器访问
http://localhost:5000/swagger
```

### 9.3 客户端连接测试

1. 启动客户端
2. 注册账号
3. 创建角色
4. 进入游戏

---

## 十、常见问题

### Q1: 服务启动失败，提示端口被占用

```bash
# Windows 查看端口占用
netstat -ano | findstr :7000
taskkill /PID <进程ID> /F

# Linux
lsof -i :7000
kill -9 <进程ID>
```

### Q2: 数据库连接失败

1. 检查 MySQL 服务是否启动
2. 检查用户名密码是否正确
3. 检查防火墙是否阻止 3306 端口

### Q3: 客户端无法连接

1. 检查服务器 IP 和端口配置
2. 检查防火墙是否开放 7000、7100 端口
3. 检查服务是否都已启动

### Q4: mir2_db.sql 导入超时

```bash
# 增加超时时间
mysql --max_allowed_packet=512M --connect_timeout=600 -u root -p mir2_db < mir2_db.sql
```

---

## 十一、运维建议

### 11.1 日志管理

日志文件位于各服务目录的 `logs` 文件夹：

```
publish/
├── GameSrv/logs/
├── LoginSrv/logs/
├── DBSrv/logs/
└── WebApi/logs/
```

### 11.2 数据备份

```bash
# 每日备份脚本
#!/bin/bash
DATE=$(date +%Y%m%d)
mysqldump -u mir2 -p mir2_account > /backup/mir2_account_$DATE.sql
mysqldump -u mir2 -p mir2_data > /backup/mir2_data_$DATE.sql
mysqldump -u mir2 -p mir2_db > /backup/mir2_db_$DATE.sql
```

### 11.3 性能监控

- 监控服务器 CPU、内存使用率
- 监控 MySQL 连接数和慢查询
- 监控网络流量

---

## 十二、快速部署清单

```
□ 1. 安装 .NET 8.0 SDK
□ 2. 安装 MySQL 8.0
□ 3. 克隆代码仓库
□ 4. 执行数据库初始化脚本
□ 5. 编译项目 (dotnet build)
□ 6. 修改配置文件（数据库连接）
□ 7. 配置防火墙端口
□ 8. 按顺序启动服务
□ 9. 配置客户端服务器地址
□ 10. 测试连接
```

---

**版本**: v1.0  
**更新日期**: 2026-02-04  
**作者**: OpenMir2 Team
