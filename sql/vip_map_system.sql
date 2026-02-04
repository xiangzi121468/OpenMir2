-- ============================================
-- VIP专属地图系统数据库表
-- OpenMir2 VIP Map System
-- ============================================

-- VIP地图配置表
CREATE TABLE IF NOT EXISTS `vip_maps` (
    `id` INT PRIMARY KEY AUTO_INCREMENT,
    `map_id` VARCHAR(20) NOT NULL COMMENT '地图ID',
    `map_name` VARCHAR(50) NOT NULL COMMENT '地图名称',
    `description` VARCHAR(500) COMMENT '地图描述',
    `min_vip_level` INT DEFAULT 1 COMMENT '最低VIP等级(0=充值金额判断)',
    `min_recharge` INT DEFAULT 0 COMMENT '最低充值金额(元)',
    `entry_fee` INT DEFAULT 0 COMMENT '每次进入费用(元宝)',
    `daily_limit` INT DEFAULT -1 COMMENT '每日进入次数限制(-1=无限)',
    `level_limit` INT DEFAULT 1 COMMENT '最低等级限制',
    `is_random_entry` TINYINT(1) DEFAULT 1 COMMENT '是否支持随机进入',
    `is_fixed_entry` TINYINT(1) DEFAULT 1 COMMENT '是否支持定点进入',
    `random_x_min` INT DEFAULT 100 COMMENT '随机进入X范围最小',
    `random_x_max` INT DEFAULT 200 COMMENT '随机进入X范围最大',
    `random_y_min` INT DEFAULT 100 COMMENT '随机进入Y范围最小',
    `random_y_max` INT DEFAULT 200 COMMENT '随机进入Y范围最大',
    `fixed_x` INT DEFAULT 150 COMMENT '定点进入X坐标',
    `fixed_y` INT DEFAULT 150 COMMENT '定点进入Y坐标',
    `is_enabled` TINYINT(1) DEFAULT 1 COMMENT '是否启用',
    `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    INDEX `idx_map` (`map_id`),
    INDEX `idx_vip` (`min_vip_level`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='VIP地图配置表';

-- VIP地图怪物配置表
CREATE TABLE IF NOT EXISTS `vip_map_monsters` (
    `id` INT PRIMARY KEY AUTO_INCREMENT,
    `vip_map_id` INT NOT NULL COMMENT 'VIP地图ID',
    `monster_name` VARCHAR(50) NOT NULL COMMENT '怪物名称',
    `description` VARCHAR(200) COMMENT '怪物描述',
    `spawn_count` INT DEFAULT 1 COMMENT '刷新数量',
    `spawn_interval` INT DEFAULT 30 COMMENT '刷新间隔(分钟)',
    `spawn_x` INT DEFAULT 150 COMMENT '刷新X坐标',
    `spawn_y` INT DEFAULT 150 COMMENT '刷新Y坐标',
    `spawn_range` INT DEFAULT 10 COMMENT '刷新范围',
    `is_boss` TINYINT(1) DEFAULT 0 COMMENT '是否BOSS',
    `drop_description` VARCHAR(200) COMMENT '掉落描述',
    `is_enabled` TINYINT(1) DEFAULT 1,
    
    INDEX `idx_map` (`vip_map_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='VIP地图怪物配置';

-- VIP地图进入日志
CREATE TABLE IF NOT EXISTS `vip_map_entry_logs` (
    `id` BIGINT PRIMARY KEY AUTO_INCREMENT,
    `account_id` VARCHAR(50) NOT NULL,
    `char_name` VARCHAR(50) NOT NULL,
    `map_id` VARCHAR(20) NOT NULL,
    `map_name` VARCHAR(50),
    `entry_type` VARCHAR(20) DEFAULT 'random' COMMENT 'random/fixed',
    `entry_fee` INT DEFAULT 0,
    `entry_time` DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    INDEX `idx_account` (`account_id`),
    INDEX `idx_char` (`char_name`),
    INDEX `idx_date` (`entry_time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='VIP地图进入日志';

-- 插入示例VIP地图
INSERT INTO `vip_maps` (`map_id`, `map_name`, `description`, `min_vip_level`, `min_recharge`, `entry_fee`, `daily_limit`, `level_limit`) VALUES
('VIPHALL1', '贵族VIP大厅', '充值30元及以上免费进入，30分刷新普通怪，120分刷新超级BOSS', 1, 30, 0, -1, 35),
('VIPHALL2', '黄金VIP殿堂', '充值100元及以上进入，顶级装备产出地', 2, 100, 0, 3, 40),
('VIPHALL3', '钻石VIP圣殿', '充值500元及以上进入，神装专属产出', 3, 500, 0, 2, 45);

-- 插入示例怪物配置
INSERT INTO `vip_map_monsters` (`vip_map_id`, `monster_name`, `description`, `spawn_count`, `spawn_interval`, `spawn_x`, `spawn_y`, `is_boss`, `drop_description`) VALUES
(1, '黄金狮子', '普通小怪，30分钟刷新', 20, 30, 150, 150, 0, '金币、祝福油'),
(1, '陈天桥老婆', '【免费挖倚天武器】超级BOSS，120分钟刷新', 1, 120, 180, 180, 1, '王者、炎龙、金牛装备、恐龙蛋'),
(1, '财神爷', '节日BOSS，60分钟刷新', 1, 60, 160, 160, 1, '大量金币、元宝'),
(2, '黄金教主', '黄金VIP专属BOSS', 1, 90, 150, 150, 1, '屠龙、裁决、龙纹'),
(3, '钻石龙王', '钻石VIP专属终极BOSS', 1, 180, 150, 150, 1, '神装全套');
