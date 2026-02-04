# OpenMir2 数据库初始化指南

## 一、数据库要求

- MySQL 8.0+ 或 MariaDB 10.5+
- 字符集: utf8mb4
- 排序规则: utf8mb4_general_ci

## 二、创建数据库

```sql
CREATE DATABASE IF NOT EXISTS mir2_account CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
CREATE DATABASE IF NOT EXISTS mir2_data CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
CREATE DATABASE IF NOT EXISTS mir2_db CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;
```

## 三、SQL脚本执行顺序

### 阶段1: 核心数据库 (必须按顺序执行)

| 序号 | 文件 | 目标数据库 | 说明 |
|------|------|-----------|------|
| 1 | mir2_account.sql | mir2_account | 账号系统表 |
| 2 | mir2_data.sql | mir2_data | 游戏基础数据（技能、物品、怪物） |
| 3 | mir2_db.sql | mir2_db | 角色数据表 |

### 阶段2: 索引优化 (阶段1完成后执行)

| 序号 | 文件 | 目标数据库 | 说明 |
|------|------|-----------|------|
| 4 | characters_indexes.sql | mir2_db | 角色表索引优化 |
| 5 | fix_gamegold_field.sql | mir2_db | 元宝字段修复 |

### 阶段3: 功能模块 (可并行执行)

| 序号 | 文件 | 目标数据库 | 说明 |
|------|------|-----------|------|
| 6 | shop_system.sql | mir2_db | 商城系统 |
| 7 | mail_system.sql | mir2_db | 邮件系统 |
| 8 | notice_system.sql | mir2_db | 公告系统 |
| 9 | activity_system.sql | mir2_db | 活动系统 |
| 10 | castle_war.sql | mir2_db | 攻城战系统 |
| 11 | appraisal_system.sql | mir2_db | 装备鉴定系统 |
| 12 | vip_map_system.sql | mir2_db | VIP地图系统 |
| 13 | market_system.sql | mir2_db | 寄售系统 |
| 14 | player_logs.sql | mir2_db | 玩家日志系统 |
| 15 | gm_admin.sql | mir2_db | GM后台管理 |

### 阶段4: 游戏内容 (阶段2完成后执行)

| 序号 | 文件 | 目标数据库 | 说明 |
|------|------|-----------|------|
| 16 | skill_books.sql | mir2_data | 技能书数据 |
| 17 | new_weapons.sql | mir2_data | 新武器数据 |
| 18 | new_armors.sql | mir2_data | 新防具数据 |
| 19 | new_accessories.sql | mir2_data | 新配饰数据 |
| 20 | new_monsters.sql | mir2_data | 新怪物数据 |

### 阶段5: 配置数据 (阶段4完成后执行)

| 序号 | 文件 | 目标数据库 | 说明 |
|------|------|-----------|------|
| 21 | vip_map_monsters_config.sql | mir2_db | VIP地图怪物配置 |
| 22 | all_weapon_effects.sql | mir2_data | 武器特效配置 |
| 23 | update_accessory_looks.sql | mir2_data | 配饰外观更新 |

### 可选脚本 (按需执行)

| 文件 | 目标数据库 | 说明 |
|------|-----------|------|
| weapon_effects_data.sql | mir2_data | 武器特效数据（备用） |
| weapon_effects_update.sql | mir2_data | 武器特效更新（备用） |

## 四、快速初始化

### Windows (PowerShell)

```powershell
# 设置数据库连接信息
$DB_HOST = "localhost"
$DB_USER = "root"
$DB_PASS = "your_password"

# 执行初始化脚本
.\init_database.bat $DB_HOST $DB_USER $DB_PASS
```

### Linux/Mac (Bash)

```bash
# 设置数据库连接信息
export DB_HOST=localhost
export DB_USER=root
export DB_PASS=your_password

# 执行初始化脚本
./init_database.sh
```

### 使用Python脚本

```bash
python init_database.py --host localhost --user root --password your_password
```

## 五、验证安装

执行以下SQL验证表是否创建成功：

```sql
-- 验证核心表
SELECT 'mir2_account' AS db, COUNT(*) AS tables FROM information_schema.tables WHERE table_schema = 'mir2_account';
SELECT 'mir2_data' AS db, COUNT(*) AS tables FROM information_schema.tables WHERE table_schema = 'mir2_data';
SELECT 'mir2_db' AS db, COUNT(*) AS tables FROM information_schema.tables WHERE table_schema = 'mir2_db';

-- 验证关键表存在
SELECT table_name FROM information_schema.tables 
WHERE table_schema = 'mir2_db' 
AND table_name IN ('characters', 'shop_items', 'game_mails', 'game_notices', 'game_activities', 'castles');
```

## 六、常见问题

### Q1: 执行大文件(mir2_db.sql)超时
```bash
mysql --max_allowed_packet=512M -u root -p mir2_db < mir2_db.sql
```

### Q2: 字符编码问题
```bash
mysql --default-character-set=utf8mb4 -u root -p < script.sql
```

### Q3: 重复执行报错
大部分脚本支持重复执行（使用 IF NOT EXISTS），但建议先备份数据库。

## 七、数据库表清单

### mir2_account 数据库
- account - 账号表

### mir2_data 数据库
- magics - 技能表
- stditems - 物品表
- monsters - 怪物表
- goldsales - 金币寄售

### mir2_db 数据库
- characters - 角色表
- characters_item - 角色装备
- characters_magic - 角色技能
- characters_bagitem - 背包物品
- characters_storageitem - 仓库物品
- shop_categories - 商品分类
- shop_items - 商城商品
- shop_orders - 商城订单
- recharge_records - 充值记录
- recharge_packages - 充值档位
- gift_codes - 礼包码
- vip_levels - VIP等级
- user_vip - 用户VIP
- game_mails - 邮件
- player_mails - 玩家邮件
- mail_templates - 邮件模板
- game_notices - 公告
- game_activities - 活动
- activity_logs - 活动日志
- castles - 城堡
- castle_war_records - 攻城记录
- castle_war_kills - 击杀记录
- appraisal_attributes - 鉴定属性
- appraisal_logs - 鉴定日志
- vip_maps - VIP地图
- vip_map_monsters - VIP地图怪物
- market_items - 寄售物品
- market_transactions - 交易记录
- player_login_logs - 登录日志
- player_item_logs - 物品日志
- player_currency_logs - 货币日志
- admin_users - 管理员
- admin_logs - 管理日志
- ban_records - 封禁记录

---

**版本**: v1.0  
**更新日期**: 2026-02-04
