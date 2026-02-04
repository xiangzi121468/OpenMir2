# GM管理后台

## 系统概述

完整的Web管理后台系统，包括：
- 管理员认证与权限
- 玩家管理
- 商城/充值/礼包码管理
- 数据统计
- 操作日志

## 功能模块

```
┌────────────────────────────────────────────────────────┐
│                    GM管理后台                          │
├────────────┬────────────┬────────────┬────────────────┤
│  认证系统   │  玩家管理   │  商城管理   │   数据统计     │
├────────────┼────────────┼────────────┼────────────────┤
│ • JWT认证   │ • 玩家搜索  │ • 商品管理  │ • 仪表盘概览   │
│ • 角色权限  │ • 修改元宝  │ • 订单查询  │ • 趋势图表    │
│ • 操作日志  │ • 修改等级  │ • 充值记录  │ • 分布统计    │
│ • 密码管理  │ • 封禁/解封 │ • 礼包码    │ • 排行榜      │
└────────────┴────────────┴────────────┴────────────────┘
```

## 数据库

导入SQL文件：
```bash
mysql -u root -p mir2_db < sql/gm_admin.sql
```

### 核心表

| 表名 | 说明 |
|------|------|
| admin_users | 管理员账号 |
| admin_logs | 操作日志 |
| system_notices | 系统公告 |
| ban_records | 封禁记录 |
| mail_records | 邮件发送记录 |
| stats_snapshot | 统计快照 |

## API接口

### 认证API `/api/admin/auth`

| 方法 | 路径 | 说明 |
|------|------|------|
| POST | /login | 管理员登录 |
| GET | /info | 获取当前用户信息 |
| POST | /password | 修改密码 |
| POST | /logout | 退出登录 |

### 玩家管理 `/api/admin/player`

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /search | 搜索玩家 |
| GET | /{id} | 玩家详情 |
| POST | /{id}/gold | 修改元宝 |
| POST | /{id}/level | 修改等级 |
| POST | /{id}/ban | 封禁玩家 |
| POST | /unban/{banId} | 解封 |
| GET | /bans | 封禁列表 |
| POST | /{id}/teleport | 传送玩家 |

### 商城管理 `/api/admin/shop`

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /items | 商品列表 |
| POST | /items | 添加商品 |
| PUT | /items/{id} | 更新商品 |
| DELETE | /items/{id} | 删除商品 |
| GET | /orders | 订单列表 |
| GET | /recharges | 充值记录 |
| GET | /recharges/stats | 充值统计 |
| GET | /giftcodes | 礼包码列表 |
| POST | /giftcodes/generate | 批量生成礼包码 |

### 数据统计 `/api/admin/stats`

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /dashboard | 仪表盘概览 |
| GET | /level-distribution | 等级分布 |
| GET | /job-distribution | 职业分布 |
| GET | /trend | 近30天趋势 |
| GET | /vip-distribution | VIP分布 |
| GET | /recharge-ranking | 充值排行榜 |

## 管理员角色

| 角色 | 权限 |
|------|------|
| superadmin | 超级管理员，所有权限 |
| admin | 管理员，除删除外的权限 |
| gm | GM，基本操作权限 |
| viewer | 只读权限 |

## 默认账号

```
用户名: admin
密码: admin123
```

## 部署步骤

### 1. 导入数据库

```bash
mysql -u root -p mir2_db < sql/gm_admin.sql
```

### 2. 配置WebApi

在 `Program.cs` 中注册服务：

```csharp
// 添加JWT认证
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => {
        options.TokenValidationParameters = new TokenValidationParameters {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.ASCII.GetBytes("OpenMir2_Admin_Secret_Key_2024_Very_Long_String")),
            ValidateIssuer = false,
            ValidateAudience = false
        };
    });

// 注册服务
var connStr = "server=127.0.0.1;uid=root;pwd=;database=mir2_db;";
builder.Services.AddSingleton(new AdminService(connStr));
builder.Services.AddSingleton<IShopRepository>(new MySqlShopRepository(connStr));
builder.Services.AddSingleton<IShopService, ShopService>();
```

### 3. 启用静态文件

```csharp
app.UseStaticFiles();
```

### 4. 访问后台

```
http://localhost:5000/admin/index.html
```

## 前端界面

管理后台使用 Vue 3 + Element Plus 构建，支持：

- 响应式布局
- 数据图表 (ECharts)
- 表格分页
- 对话框操作
- 消息提示

## 安全建议

1. **修改默认密码** - 首次登录后立即修改
2. **配置HTTPS** - 生产环境必须使用HTTPS
3. **IP白名单** - 限制后台访问IP
4. **操作日志** - 所有操作自动记录
5. **JWT过期** - Token默认24小时过期

## 截图预览

### 登录页
- 用户名/密码登录
- JWT Token认证

### 仪表盘
- 数据概览卡片
- 近30天趋势图
- 等级/职业分布饼图

### 玩家管理
- 搜索玩家
- 修改元宝/等级
- 封禁/解封操作

### 商城管理
- 商品CRUD
- 订单查询
- 充值记录和统计

### 礼包码
- 批量生成
- 使用情况查看
- 状态管理
