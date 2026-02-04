-- ========================================
-- 武器特效数据库集成脚本
-- ========================================
-- 
-- 说明：
-- 客户端通过 reserve[3] 字段判断武器特效类型
-- 特效类型范围：100-249 为自定义特效（20帧，索引30000+）
-- 
-- 特效类型对应关系：
--   100: 冰霜之刃 - 蓝色冰霜光环，冰晶粒子环绕
--   101: 冰魄 - 青色冰霜光芒，雪花飘落
--   102: 凝霜 - 淡蓝色霜气环绕
--   103: 暗影匕首 - 紫色暗影脉冲波纹
--   105: 亡灵法杖 - 绿色幽魂火焰
--   110: 地狱火 - 红色火焰光环
--   115: 龙之光环 - 旋转金色光芒
-- 
-- ========================================

-- ========================================
-- 方案A: 使用 Reference 字段存储JSON格式 (推荐)
-- ========================================
-- 在 Reference 字段中存储 JSON 格式的扩展属性
-- 格式: {"effect":特效类型值}
-- 
-- 注意：此方案需要服务端代码支持从 Reference 字段解析JSON
-- 并将特效类型写入到 reserve[3] 位置
-- ========================================

-- 冰霜之刃 (ID=900): 特效类型 100
UPDATE stditems SET Reference = '{"effect":100}' WHERE Id = 900;

-- 冰魄 (ID=901): 特效类型 101
UPDATE stditems SET Reference = '{"effect":101}' WHERE Id = 901;

-- 凝霜 (ID=902): 特效类型 102
UPDATE stditems SET Reference = '{"effect":102}' WHERE Id = 902;

-- 暗影匕首 (ID=910): 特效类型 103
UPDATE stditems SET Reference = '{"effect":103}' WHERE Id = 910;

-- 亡灵法杖 (ID=920): 特效类型 105
UPDATE stditems SET Reference = '{"effect":105}' WHERE Id = 920;

-- 地狱火 (ID=961): 特效类型 110
UPDATE stditems SET Reference = '{"effect":110}' WHERE Id = 961;

-- 龙珠 (ID=954): 特效类型 115
UPDATE stditems SET Reference = '{"effect":115}' WHERE Id = 954;

-- ========================================
-- 方案B: 添加新字段 WeaponEffect (可选)
-- ========================================
-- 如果方案A不可行，可以使用此方案添加专门的字段存储特效类型
-- 
-- 注意：执行此方案前，请确保服务端代码已更新以支持读取此字段
-- ========================================

-- 检查字段是否存在，如果不存在则添加
-- MySQL 5.7+ 语法
-- ALTER TABLE stditems 
-- ADD COLUMN IF NOT EXISTS WeaponEffect TINYINT UNSIGNED DEFAULT 0 
-- COMMENT '武器特效类型 (reserve[3] 值, 100-249为自定义特效)';

-- MySQL 5.6 及以下版本语法（需要手动检查字段是否存在）
-- ALTER TABLE stditems ADD COLUMN WeaponEffect TINYINT UNSIGNED DEFAULT 0 COMMENT '武器特效类型';

-- 使用新字段更新武器特效
-- UPDATE stditems SET WeaponEffect = 100 WHERE Id = 900;  -- 冰霜之刃
-- UPDATE stditems SET WeaponEffect = 101 WHERE Id = 901;  -- 冰魄
-- UPDATE stditems SET WeaponEffect = 102 WHERE Id = 902;  -- 凝霜
-- UPDATE stditems SET WeaponEffect = 103 WHERE Id = 910;  -- 暗影匕首
-- UPDATE stditems SET WeaponEffect = 105 WHERE Id = 920;  -- 亡灵法杖
-- UPDATE stditems SET WeaponEffect = 110 WHERE Id = 961;  -- 地狱火
-- UPDATE stditems SET WeaponEffect = 115 WHERE Id = 954;  -- 龙珠

-- ========================================
-- 验证查询
-- ========================================
-- 执行以下查询可以验证更新是否成功：
-- 
-- SELECT Id, Name, Reference, WeaponEffect 
-- FROM stditems 
-- WHERE Id IN (900, 901, 902, 910, 920, 961, 954);
-- 
-- ========================================
-- 特效索引计算公式
-- ========================================
-- 自定义特效 (100-249) 的图像索引计算：
--   起始索引 = 30000 + (特效类型 - 100) * 20
--   帧索引 = 起始索引 + 当前帧数 (0-19)
-- 
-- 示例：
--   特效类型 100: 索引 30000-30019 (20帧)
--   特效类型 101: 索引 30020-30039 (20帧)
--   特效类型 102: 索引 30040-30059 (20帧)
--   特效类型 103: 索引 30060-30079 (20帧)
--   特效类型 105: 索引 30100-30119 (20帧)
--   特效类型 110: 索引 30200-30219 (20帧)
--   特效类型 115: 索引 30300-30319 (20帧)
-- 
-- ========================================
-- 注意事项
-- ========================================
-- 1. 执行此脚本前，请备份数据库
-- 2. 确保服务端代码已更新以支持从数据库读取特效类型
-- 3. 确保客户端已打包对应的特效图像到 State.data 或 StateEffect.data
-- 4. 特效图像应从索引 30000 开始按顺序导入
-- 5. 帧率默认为 200ms/帧，在客户端 FState.pas 中控制
-- 
-- ========================================
