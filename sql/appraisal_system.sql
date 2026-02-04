-- ============================================
-- 装备鉴定系统数据库表
-- OpenMir2 Item Appraisal System
-- ============================================

-- 鉴定属性配置表
CREATE TABLE IF NOT EXISTS `appraisal_attributes` (
    `id` INT PRIMARY KEY AUTO_INCREMENT,
    `name` VARCHAR(50) NOT NULL COMMENT '属性名称',
    `description` VARCHAR(200) COMMENT '属性描述',
    `effect_type` VARCHAR(50) NOT NULL COMMENT '效果类型: paralysis, rebirth, dodge, detect, teleport, block',
    `effect_value` INT DEFAULT 0 COMMENT '效果数值',
    `max_level` INT DEFAULT 5 COMMENT '最大等级',
    `scroll_level` INT DEFAULT 1 COMMENT '需要卷轴等级',
    `probability` INT DEFAULT 10 COMMENT '出现概率(百分比)',
    `is_rare` TINYINT(1) DEFAULT 0 COMMENT '是否稀有',
    `broadcast` TINYINT(1) DEFAULT 0 COMMENT '是否全服广播',
    `is_enabled` TINYINT(1) DEFAULT 1,
    
    INDEX `idx_scroll` (`scroll_level`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='鉴定属性配置表';

-- 卷轴商品配置表
CREATE TABLE IF NOT EXISTS `scroll_items` (
    `id` INT PRIMARY KEY AUTO_INCREMENT,
    `item_name` VARCHAR(50) NOT NULL COMMENT '物品名称',
    `display_name` VARCHAR(50) NOT NULL COMMENT '显示名称',
    `scroll_level` INT DEFAULT 1 COMMENT '卷轴等级',
    `price_single` INT DEFAULT 1000 COMMENT '单个价格(元宝)',
    `price_batch` INT DEFAULT 10000 COMMENT '批量价格(10个)',
    `batch_count` INT DEFAULT 10 COMMENT '批量数量',
    `description` VARCHAR(200) COMMENT '描述',
    `icon` VARCHAR(50) COMMENT '图标',
    `sort_order` INT DEFAULT 0 COMMENT '排序',
    `is_enabled` TINYINT(1) DEFAULT 1,
    
    INDEX `idx_level` (`scroll_level`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='卷轴商品配置表';

-- 鉴定记录日志表
CREATE TABLE IF NOT EXISTS `appraisal_logs` (
    `id` BIGINT PRIMARY KEY AUTO_INCREMENT,
    `account_id` VARCHAR(50) NOT NULL,
    `char_name` VARCHAR(50) NOT NULL,
    `item_name` VARCHAR(50) NOT NULL COMMENT '被鉴定装备',
    `scroll_name` VARCHAR(50) COMMENT '使用卷轴',
    `result_attribute` VARCHAR(50) COMMENT '获得属性',
    `result_level` INT DEFAULT 0 COMMENT '属性等级',
    `is_success` TINYINT(1) DEFAULT 1 COMMENT '是否成功',
    `is_broadcast` TINYINT(1) DEFAULT 0 COMMENT '是否广播',
    `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    INDEX `idx_account` (`account_id`),
    INDEX `idx_char` (`char_name`),
    INDEX `idx_time` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='鉴定记录日志表';

-- 属性转移记录表
CREATE TABLE IF NOT EXISTS `transfer_logs` (
    `id` BIGINT PRIMARY KEY AUTO_INCREMENT,
    `account_id` VARCHAR(50) NOT NULL,
    `char_name` VARCHAR(50) NOT NULL,
    `source_item` VARCHAR(50) NOT NULL COMMENT '源装备',
    `target_item` VARCHAR(50) NOT NULL COMMENT '目标装备',
    `transferred_attr` VARCHAR(50) COMMENT '转移属性',
    `is_success` TINYINT(1) DEFAULT 1,
    `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    INDEX `idx_char` (`char_name`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='属性转移日志';

-- 插入预设鉴定属性
INSERT INTO `appraisal_attributes` (`name`, `description`, `effect_type`, `max_level`, `scroll_level`, `probability`, `is_rare`, `broadcast`) VALUES
('重生神技', '死亡后有几率原地复活', 'rebirth', 3, 3, 5, 1, 1),
('八卦护身', '格挡物理攻击几率提升', 'block', 5, 2, 15, 0, 0),
('麻痹神技', '物理攻击有几率麻痹敌人', 'paralysis', 5, 2, 10, 1, 1),
('魔道麻痹', '魔法攻击有几率麻痹敌人', 'magic_paralysis', 5, 2, 10, 1, 1),
('战意麻痹', '道术攻击有几率麻痹敌人', 'tao_paralysis', 5, 2, 10, 1, 1),
('探测技能', '探测周围隐身玩家', 'detect', 3, 1, 20, 0, 0),
('传送神技', '受到攻击时随机传送', 'teleport', 3, 3, 8, 1, 1),
('吸血神技', '攻击时吸取敌人生命', 'lifesteal', 5, 2, 12, 0, 0),
('反弹伤害', '反弹部分物理伤害', 'reflect', 5, 2, 15, 0, 0),
('幸运加持', '提升暴击几率', 'lucky', 5, 1, 25, 0, 0),
('神圣护盾', '减少魔法伤害', 'magic_shield', 5, 2, 15, 0, 0),
('疾风步', '提升移动速度', 'speed', 3, 1, 20, 0, 0);

-- 插入卷轴商品配置
INSERT INTO `scroll_items` (`item_name`, `display_name`, `scroll_level`, `price_single`, `price_batch`, `batch_count`, `description`, `sort_order`) VALUES
('一级卷轴', '一级卷轴', 1, 1000, 10000, 10, '基础鉴定卷轴，可解读普通属性', 1),
('二级卷轴', '二级卷轴', 2, 1200, 12000, 10, '中级鉴定卷轴，可解读稀有属性', 2),
('三级卷轴', '三级卷轴', 3, 1500, 15000, 10, '高级鉴定卷轴，可解读神级属性', 3),
('幸运符', '幸运符', 1, 1000, 10000, 10, '提升鉴定成功率', 4),
('神秘卷轴', '神秘卷轴', 1, 1000, 10000, 10, '随机解读任意等级属性', 5),
('属性转移石', '属性转移石', 0, 2000, 18000, 10, '用于转移装备属性', 6),
('鉴定保护符', '鉴定保护符', 0, 500, 4500, 10, '鉴定失败不损坏装备', 7);
