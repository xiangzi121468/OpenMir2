# 武器特效最终集成指南

## 已生成的资源文件

### 武器特效
| 文件 | 大小 | 说明 |
|-----|------|------|
| `MirClient/Data/NewShineEffect.data` | 961 KB | 包含7种武器特效，共140帧 |

### 索引映射关系

客户端代码 `GetWStateImg(idx)` 的索引映射：
- `30000-39999` → `ShineEffect.data[idx - 30000]`

| 特效类型 | 武器 | GetWStateImg索引 | ShineEffect索引 |
|---------|------|-----------------|----------------|
| 100 | 冰霜之刃 | 30000-30019 | 0-19 |
| 101 | 冰魄 | 30020-30039 | 20-39 |
| 102 | 凝霜 | 30040-30059 | 40-59 |
| 103 | 暗影匕首 | 30060-30079 | 60-79 |
| 105 | 亡灵法杖 | 30100-30119 | 100-119 |
| 110 | 地狱火 | 30200-30219 | 200-219 |
| 115 | 龙之光环 | 30300-30319 | 300-319 |

## 集成步骤

### 方案A: 替换现有文件 (推荐用于测试)

```bash
# 1. 备份原文件
copy MirClient\Data\ShineEffect.data MirClient\Data\ShineEffect.data.bak

# 2. 使用新文件
copy MirClient\Data\NewShineEffect.data MirClient\Data\ShineEffect.data
```

### 方案B: 合并到现有文件

如果现有 `ShineEffect.data` 中有其他重要特效，需要使用专门的合并工具。

### 数据库配置

执行SQL更新武器的特效类型：

```sql
-- 更新武器特效类型 (reserve[3] 字段)
-- 需要根据服务端代码确定具体字段名

-- 示例：如果使用 Reference 字段存储JSON
UPDATE stditems SET Reference = '{"effect":100}' WHERE Id = 900;  -- 冰霜之刃
UPDATE stditems SET Reference = '{"effect":101}' WHERE Id = 901;  -- 冰魄
UPDATE stditems SET Reference = '{"effect":102}' WHERE Id = 902;  -- 凝霜
UPDATE stditems SET Reference = '{"effect":103}' WHERE Id = 910;  -- 暗影匕首
UPDATE stditems SET Reference = '{"effect":105}' WHERE Id = 920;  -- 亡灵法杖
UPDATE stditems SET Reference = '{"effect":110}' WHERE Id = 961;  -- 地狱火
UPDATE stditems SET Reference = '{"effect":115}' WHERE Id = 954;  -- 龙珠
```

## 客户端代码参考

特效渲染逻辑 (`FState.pas`):

```pascal
// 自定义特效 (reserve[3] = 100-249)
if (g_UseItems[U_WEAPON].s.reserve[3] in [100..249]) then begin
  if GetTickCount - g_sWeaponEffectTick > 200 then begin
    g_sWeaponEffectTick := GetTickCount;
    Inc(g_sWeaponEffectIdx);
    if g_sWeaponEffectIdx >= 20 then
      g_sWeaponEffectIdx := 0;
  end;
  // 索引计算: 30000 + (type - 100) * 20 + frame
  d := frmMain.GetWStateImg(30000 + (g_UseItems[U_WEAPON].s.reserve[3]+1 - 100) * 20-20+g_sWeaponEffectIdx, ax, ay);
  if d <> nil then begin
    dsurface.DrawBlend(SurfaceX(bbx + ax), SurfaceY(bby + ay), d, 1);
  end;
end;
```

## 测试验证

1. 启动游戏服务端和客户端
2. 使用GM命令获取测试武器:
   ```
   @make 冰霜之刃
   @make 暗影匕首
   @make 地狱火
   ```
3. 装备武器后打开人物状态界面
4. 确认武器特效正常显示

## 工具脚本

| 脚本 | 功能 |
|-----|------|
| `tools/pack_all_weapon_effects.py` | 打包所有特效到ShineEffect.data |
| `tools/pack_to_data.py` | 单独打包PNG到.data格式 |
| `tools/generate_weapon_effects_v2.py` | 生成特效PNG图像 |
| `tools/weapon_effect_config.py` | 特效配置管理 |

## 文件清单

```
assets/weapon_effects_v2/
├── type_100/          # 冰霜之刃特效 (20帧)
├── type_101/          # 冰魄特效 (20帧)
├── type_102/          # 凝霜特效 (20帧)
├── type_103/          # 暗影匕首特效 (20帧)
├── type_105/          # 亡灵法杖特效 (20帧)
├── type_110/          # 地狱火特效 (20帧)
└── type_115/          # 龙之光环特效 (20帧)

MirClient/Data/
├── NewShineEffect.data  # 打包后的特效文件 (需替换为ShineEffect.data)
└── Mon41-46.data        # 新怪物精灵图
```
