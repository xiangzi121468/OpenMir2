-- ============================================
-- 修复元宝字段问题
-- 为characters表添加GameGold字段
-- ============================================

-- 1. 检查并添加GameGold字段
ALTER TABLE `characters` ADD COLUMN IF NOT EXISTS `GameGold` INT NOT NULL DEFAULT 0 COMMENT '元宝' AFTER `Gold`;

-- 2. 如果有旧数据需要迁移（可选，根据实际情况执行）
-- 如果之前错误地将元宝存到了Gold字段，可以执行以下迁移
-- UPDATE `characters` SET `GameGold` = `Gold`, `Gold` = 0 WHERE `GameGold` = 0 AND `Gold` > 0;

-- 3. 添加索引优化查询
-- ALTER TABLE `characters` ADD INDEX `idx_gamegold` (`GameGold`);

-- ============================================
-- 验证
-- ============================================
-- DESCRIBE `characters`;
-- SELECT ChrName, Gold, GameGold FROM characters LIMIT 10;
