# 武器特效集成指南

## 概述

本文档描述如何将自定义武器特效集成到OpenMir2项目中。

## 特效系统架构

### 客户端特效机制

客户端通过 `TClientStdItem.reserve[3]` 字段判断武器特效类型：

| reserve[3] 值 | 特效类型 | 图像索引 | 帧数 |
|--------------|---------|---------|------|
| 1 | 静态发光 | 1403 | 1 |
| 2 | 倚天剑特效 | 1890-1899 | 10 |
| 3 | 静态发光2 | 2427 | 1 |
| 4 | 传奇神剑 | 2530-2537 | 8 |
| 5 | 传奇神剑2 | 2550-2559 | 10 |
| 6 | 传奇神剑3 | 2560-2569 | 10 |
| 7-9 | 高级特效 | 3480+ | 14 |
| 10-12 | 顶级特效 | 3610+ | 18 |
| 13-15 | 更高级特效 | 3820+ | 18 |
| 16 | 特殊特效 | 2850-2865 | 16 |
| 100-249 | 自定义特效 | 30000+ | 20 |

### 索引计算公式

- 高级特效 (7-9): `3480 + (type-7) * 20 + frame`
- 顶级特效 (10-12): `3610 + (type-10) * 20 + frame`
- 自定义特效 (100-249): `30000 + (type-100) * 20 + frame`

### 帧率

默认 200ms/帧，在 `FState.pas` 中控制：
```pascal
if GetTickCount - g_sWeaponEffectTick > 200 then begin
  g_sWeaponEffectTick := GetTickCount;
  Inc(g_sWeaponEffectIdx);
  ...
end;
```

## 生成的特效文件

### 目录结构

```
assets/weapon_effects_v2/
├── type_100/              # 冰霜之刃 (20帧)
│   ├── effect_30000.png
│   ├── effect_30001.png
│   ├── ...
│   ├── effect_30019.png
│   └── effect_type_100_sprite.png  # 精灵图
├── type_101/              # 冰魄 (20帧)
├── type_102/              # 凝霜 (20帧)
├── type_103/              # 暗影匕首 (20帧)
├── type_105/              # 亡灵法杖 (20帧)
├── type_110/              # 地狱火 (20帧)
└── type_115/              # 龙之光环 (20帧)
```

### 特效风格

| 类型 | 武器 | 风格描述 | 颜色 |
|-----|------|---------|------|
| 100 | 冰霜之刃 | 光环+闪烁星星 | 蓝色 |
| 101 | 冰魄 | 飘动粒子 | 青色 |
| 102 | 凝霜 | 迷雾环绕 | 青色 |
| 103 | 暗影匕首 | 暗影脉冲波纹 | 紫色 |
| 105 | 亡灵法杖 | 幽魂火焰 | 绿色 |
| 110 | 地狱火 | 火焰光环 | 红色 |
| 115 | 龙之光环 | 旋转金色光芒 | 金色 |

## 集成步骤

### 1. 打包特效图像到 State.data

使用WIL编辑器将PNG图像打包：

1. 打开 `Data/State.data` 或 `Data/StateEffect.data`
2. 从索引 30000 开始导入特效图像
3. 按顺序导入: `effect_30000.png`, `effect_30001.png`, ...

### 2. 数据库配置

客户端的 `reserve[3]` 数据来源需要通过服务端传递。目前的数据流：

```
数据库 stditems 表 → 服务端 StdItem → 客户端 TClientStdItem.reserve
```

#### 方案A: 使用 Reference 字段传递 (推荐)

在 `Reference` 字段中存储JSON格式的扩展属性：

```sql
UPDATE stditems SET Reference = '{"effect":100}' WHERE Id = 900;
```

#### 方案B: 扩展数据库表

添加新字段存储特效类型：

```sql
ALTER TABLE stditems ADD COLUMN WeaponEffect TINYINT DEFAULT 0;
UPDATE stditems SET WeaponEffect = 100 WHERE Id = 900;
```

### 3. 服务端代码修改

需要修改 `ClientItem.cs` 的 `WritePacket` 方法，将特效类型写入到对应的字节位置。

文件: `src/OpenMir2/Packets/ClientPackets/ClientItem.cs`

在 `WritePacket` 方法中添加 reserve 数组的写入逻辑。

### 4. 新武器配置

| 物品ID | 名称 | 特效类型 |
|-------|------|---------|
| 900 | 冰霜之刃 | 100 |
| 901 | 冰魄 | 101 |
| 902 | 凝霜 | 102 |
| 910 | 暗影匕首 | 103 |
| 920 | 亡灵法杖 | 105 |
| 961 | 地狱火 | 110 |
| 954 | 龙珠 | 115 |

## 工具说明

### generate_weapon_effects_v2.py

生成武器特效帧图像的Python脚本：

```bash
python tools/generate_weapon_effects_v2.py
```

输出到 `assets/weapon_effects_v2/` 目录。

### weapon_effect_config.py

特效配置管理脚本：

```bash
python tools/weapon_effect_config.py
```

显示所有特效类型的索引映射关系。

## 客户端代码参考

特效渲染逻辑位于 `FState.pas`:

```pascal
// 自定义特效 (100-249)
if (g_UseItems[U_WEAPON].s.reserve[3] in [100..249]) then begin
  if GetTickCount - g_sWeaponEffectTick > 200 then begin
    g_sWeaponEffectTick := GetTickCount;
    Inc(g_sWeaponEffectIdx);
    if g_sWeaponEffectIdx >= 20 then
      g_sWeaponEffectIdx := 0;
  end;
  d := frmMain.GetWStateImg(30000 + (g_UseItems[U_WEAPON].s.reserve[3]+1 - 100) * 20-20+g_sWeaponEffectIdx, ax, ay);
  if d <> nil then begin
    dsurface.DrawBlend(SurfaceX(bbx + ax), SurfaceY(bby + ay), d, 1);
  end;
end;
```

## 测试验证

1. 使用GM命令给角色发放测试武器
2. 装备武器后在人物状态界面查看特效
3. 确认动画帧率和特效显示正确

## 注意事项

1. 特效图像尺寸建议 80x100 像素
2. 使用PNG格式，支持透明通道
3. 索引号必须与客户端代码对应
4. 帧数固定为20帧（自定义特效）
