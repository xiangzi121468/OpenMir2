# 通知系统

## 系统概述

完整的游戏公告通知系统，支持多种类型的公告和定时发送功能。

## 功能架构

```
┌─────────────────────────────────────────────────────────────────┐
│                        通知系统                                  │
├─────────────┬─────────────┬─────────────┬─────────────────────┤
│   公告类型   │   定时调度   │   GM后台    │   游戏集成          │
├─────────────┼─────────────┼─────────────┼─────────────────────┤
│ • 登录公告  │ • 滚动循环   │ • 公告列表  │ • 登录时发送        │
│ • 全服广播  │ • Cron定时   │ • 增删改查  │ • 定时广播          │
│ • 滚动公告  │ • 间隔控制   │ • 立即发送  │ • 缓存机制          │
│ • 定时公告  │ • 次数限制   │ • 发送记录  │ • 颜色支持          │
│ • 地图公告  │             │             │                     │
└─────────────┴─────────────┴─────────────┴─────────────────────┘
```

## 公告类型

| 类型 | 说明 | 用途 |
|------|------|------|
| `login` | 登录公告 | 玩家登录时显示 |
| `broadcast` | 全服广播 | 立即发送给所有在线玩家 |
| `scroll` | 滚动公告 | 按间隔循环发送 |
| `schedule` | 定时公告 | 按Cron表达式定时发送 |
| `map` | 地图公告 | 发送给指定地图玩家 |

## 数据库表

### game_notices - 公告表

```sql
CREATE TABLE game_notices (
    Id INT PRIMARY KEY,
    Title VARCHAR(200),           -- 标题
    Content TEXT,                  -- 内容
    NoticeType VARCHAR(30),        -- 类型
    Priority TINYINT,              -- 优先级(0普通,1重要,2紧急)
    Color VARCHAR(20),             -- 颜色
    TargetMap VARCHAR(50),         -- 目标地图
    RepeatCount INT,               -- 重复次数
    RepeatInterval INT,            -- 重复间隔(秒)
    StartTime DATETIME,            -- 生效时间
    EndTime DATETIME,              -- 失效时间
    ScheduleCron VARCHAR(100),     -- Cron表达式
    IsEnabled TINYINT,             -- 是否启用
    ...
);
```

### notice_send_logs - 发送记录

```sql
CREATE TABLE notice_send_logs (
    Id BIGINT PRIMARY KEY,
    NoticeId INT,
    NoticeType VARCHAR(30),
    TargetCount INT,
    Content VARCHAR(500),
    SendTime DATETIME
);
```

## 模块结构

```
src/Modules/NoticeModule/
├── Models/
│   └── NoticeModels.cs      -- 数据模型
├── INoticeService.cs        -- 服务接口
├── NoticeService.cs         -- 服务实现
├── NoticeScheduler.cs       -- 定时调度器
├── NoticeIntegration.cs     -- 集成帮助类
└── NoticeModule.csproj      -- 项目文件
```

## API接口

### 公告管理 `/api/admin/notice`

| 方法 | 路径 | 说明 |
|------|------|------|
| GET | /list | 获取公告列表 |
| GET | /{id} | 获取公告详情 |
| POST | / | 添加公告 |
| PUT | /{id} | 更新公告 |
| DELETE | /{id} | 删除公告 |
| POST | /{id}/toggle | 启用/禁用 |
| POST | /send | 立即发送 |
| GET | /logs | 发送记录 |

## Cron表达式

支持简化的Cron表达式：`分 时 日 月 周`

| 示例 | 说明 |
|------|------|
| `0 12 * * *` | 每天12:00 |
| `30 20 * * *` | 每天20:30 |
| `0 9-18 * * *` | 每天9点到18点整点 |
| `0 12 * * 1-5` | 周一到周五12:00 |
| `*/30 * * * *` | 每30分钟 |
| `0 12 1 * *` | 每月1日12:00 |

## 游戏服务器集成

### 1. 初始化

在游戏启动时初始化通知系统：

```csharp
// GameApp.cs 或 AppService.cs
await NoticeIntegration.InitializeAsync(
    connectionString,
    // 广播回调
    (content, color, priority) => {
        SystemShare.WorldEngine.SendBroadCastMsg(content, MsgType.System);
    },
    // 地图公告回调
    (map, content, color) => {
        // 发送到指定地图
    }
);
```

### 2. 定时处理

在TimedService中每秒调用：

```csharp
// TimedService.cs
private async void ProcessNotices()
{
    await NoticeIntegration.ProcessAsync();
}
```

### 3. 登录公告

在玩家登录时发送：

```csharp
// PlayObject.cs 登录处理
NoticeIntegration.SendLoginNoticesToPlayer(msg => {
    SysMsg(msg, MsgColor.Yellow, MsgType.Hint);
});
```

## 颜色映射

| 颜色 | 前景色 | 说明 |
|------|:------:|------|
| red | 249 | 红色(紧急) |
| green | 1 | 绿色(成功) |
| blue | 6 | 蓝色(提示) |
| yellow | 252 | 黄色(默认) |
| white | 255 | 白色 |

## 部署步骤

### 1. 导入数据库

```bash
mysql -u root -p mir2_db < sql/notice_system.sql
```

### 2. 添加项目引用

在 `GameSrv.csproj` 添加：

```xml
<ProjectReference Include="..\Modules\NoticeModule\NoticeModule.csproj" />
```

### 3. 配置连接字符串

在 `appsettings.json` 中配置数据库连接。

### 4. 访问GM后台

```
http://localhost:5000/admin/index.html
```

进入"公告管理"页面进行管理。

## 使用示例

### 添加登录公告

1. 进入GM后台 → 公告管理
2. 点击"添加公告"
3. 类型选择"登录公告"
4. 填写标题和内容
5. 点击确定

### 设置滚动公告

1. 类型选择"滚动公告"
2. 设置重复间隔（如300秒）
3. 启用公告

### 定时公告

1. 类型选择"定时公告"
2. 填写Cron表达式（如 `0 12 * * *` 每天12点）
3. 启用公告

### 立即广播

1. 点击"立即发送"按钮
2. 输入内容
3. 选择颜色
4. 点击发送

## 注意事项

1. **缓存刷新** - 公告修改后5分钟内生效，或手动调用刷新
2. **时间范围** - 支持设置生效和失效时间
3. **优先级** - 紧急公告优先显示
4. **发送记录** - 所有发送都会记录日志
5. **权限控制** - 需要管理员权限才能操作公告
