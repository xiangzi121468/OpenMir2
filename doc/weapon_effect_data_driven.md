# 武器特效数据驱动系统

## 概述

武器特效系统已完全数据驱动化，无需修改代码即可添加新的武器特效。

## 数据流

```
┌─────────────────┐     ┌─────────────────┐     ┌─────────────────┐
│   数据库        │     │   服务端        │     │   客户端        │
│   stditems      │ ──► │   StdItem       │ ──► │   reserve[3]    │
│   Reference     │     │   WeaponEffect  │     │   特效渲染      │
└─────────────────┘     └─────────────────┘     └─────────────────┘
```

### 详细流程

1. **数据库配置** (`stditems.Reference`)
   ```sql
   UPDATE stditems SET Reference = '{"effect":100}' WHERE Name = '冰霜之刃';
   ```

2. **服务端加载** (`MySqlDB.LoadItemsDB`)
   - 读取 `Reference` 字段
   - 调用 `ParseWeaponEffect()` 解析 JSON
   - 设置 `StdItem.WeaponEffect`

3. **物品创建** (`GameItemSystem.CopyToUserItemFromName`)
   - 读取 `StdItem.WeaponEffect`
   - 设置 `UserItem.Desc[10]`

4. **网络传输** (`ClientItem.WritePacket`)
   - `Desc[14]` 数组发送到客户端

5. **客户端渲染** (`FState.pas`)
   - 读取 `reserve[3]` (对应 `Desc[10]`)
   - 计算特效图像索引: `30000 + (effect - 100) * 20`
   - 从 `ShineEffect.data` 加载并渲染

## 文件修改清单

| 文件 | 修改内容 |
|------|----------|
| `src/OpenMir2/Data/StdItem.cs` | 添加 `WeaponEffect` 字段 |
| `src/GameSrv/DB/MySqlDB.cs` | 添加 `ParseWeaponEffect()` 方法 |
| `src/M2Server/Items/GameItemSystem.cs` | 使用 `StdItem.WeaponEffect` |
| `sql/weapon_effects_data.sql` | 特效配置 SQL |

## 添加新武器特效

### 步骤 1: 数据库配置

```sql
-- 添加新武器的特效
UPDATE stditems SET Reference = '{"effect":120}' WHERE Name = '雷霆战锤';
```

### 步骤 2: 准备特效图片

创建 20 帧 PNG 动画:
```
weapon_effect_120_00.png
weapon_effect_120_01.png
...
weapon_effect_120_19.png
```

### 步骤 3: 打包资源

```bash
python tools/pack_to_data.py \
  --input assets/effects/thunder/ \
  --output MirClient/Data/ShineEffect.data \
  --start-index 400  # (120-100)*20 = 400
```

### 步骤 4: 部署

将 `ShineEffect.data` 复制到客户端 `Data` 目录。

## 特效类型分配

| 范围 | 类型 | 描述 |
|------|------|------|
| 0 | - | 无特效 |
| 1-99 | 系统 | 系统保留 |
| 100-109 | 冰霜 | 蓝色/青色系 |
| 110-119 | 火焰 | 红色/橙色系 |
| 120-129 | 雷电 | 黄色/紫色系 |
| 130-139 | 暗影 | 紫色/黑色系 |
| 140-149 | 神圣 | 金色/白色系 |
| 150-159 | 自然 | 绿色/棕色系 |
| 160-249 | 扩展 | 预留扩展 |

## JSON 格式支持

支持以下字段名:
```json
{"effect": 100}
{"Effect": 100}
{"WeaponEffect": 100}
{"weaponEffect": 100}
```

## 验证

```sql
-- 查看所有配置了特效的武器
SELECT Id, Name, Reference 
FROM stditems 
WHERE Reference LIKE '%effect%';
```

## 注意事项

1. 特效类型必须在 1-249 范围内
2. 自定义特效 (100-249) 需要对应的 ShineEffect.data 资源
3. 每个特效占用 20 帧图像
4. 重启服务端生效
