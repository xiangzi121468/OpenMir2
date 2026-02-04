-- ============================================
-- characters表索引优化
-- 提升常用查询性能
-- ============================================

-- 登录ID索引 - 用于按账号查询角色
ALTER TABLE `characters` ADD INDEX IF NOT EXISTS `idx_loginid` (`LoginID`);

-- 角色名索引 - 用于按角色名查询
ALTER TABLE `characters` ADD INDEX IF NOT EXISTS `idx_chrname` (`ChrName`);

-- 等级索引 - 用于等级筛选和排行榜
ALTER TABLE `characters` ADD INDEX IF NOT EXISTS `idx_level` (`Level`);

-- 地图索引 - 用于按地图查询玩家
ALTER TABLE `characters` ADD INDEX IF NOT EXISTS `idx_mapname` (`MapName`);

-- 删除标记索引 - 用于筛选未删除角色
ALTER TABLE `characters` ADD INDEX IF NOT EXISTS `idx_deleted` (`Deleted`);

-- 元宝索引 - 用于充值排行等
ALTER TABLE `characters` ADD INDEX IF NOT EXISTS `idx_gamegold` (`GameGold`);

-- 职业索引 - 用于职业统计
ALTER TABLE `characters` ADD INDEX IF NOT EXISTS `idx_job` (`Job`);

-- 复合索引：登录ID + 删除标记 - 常用查询
ALTER TABLE `characters` ADD INDEX IF NOT EXISTS `idx_loginid_deleted` (`LoginID`, `Deleted`);

-- 复合索引：等级 + 删除标记 - 排行榜查询
ALTER TABLE `characters` ADD INDEX IF NOT EXISTS `idx_level_deleted` (`Level` DESC, `Deleted`);

-- ============================================
-- 其他表索引优化
-- ============================================

-- 角色物品表索引
ALTER TABLE `characters_item` ADD INDEX IF NOT EXISTS `idx_playerid` (`PlayerId`);

-- 角色技能表索引
ALTER TABLE `characters_magic` ADD INDEX IF NOT EXISTS `idx_playerid` (`PlayerId`);

-- 角色背包表索引
ALTER TABLE `characters_bagitem` ADD INDEX IF NOT EXISTS `idx_playerid` (`PlayerId`);

-- 角色仓库表索引
ALTER TABLE `characters_storageitem` ADD INDEX IF NOT EXISTS `idx_playerid` (`PlayerId`);

-- ============================================
-- 验证索引创建
-- ============================================
-- SHOW INDEX FROM characters;
-- SHOW INDEX FROM characters_item;
-- SHOW INDEX FROM characters_magic;
-- SHOW INDEX FROM characters_bagitem;
-- SHOW INDEX FROM characters_storageitem;
