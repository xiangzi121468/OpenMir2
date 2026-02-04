-- ============================================
-- 定时活动系统数据库表
-- OpenMir2 Activity System
-- ============================================

-- 活动配置表
CREATE TABLE IF NOT EXISTS `game_activities` (
    `id` INT PRIMARY KEY AUTO_INCREMENT,
    `name` VARCHAR(100) NOT NULL COMMENT '活动名称',
    `activity_type` VARCHAR(50) NOT NULL COMMENT '活动类型: exp_boost, drop_boost, boss_spawn, gift_rain, merchant, party',
    `description` VARCHAR(500) COMMENT '活动描述',
    `icon` VARCHAR(100) DEFAULT 'default' COMMENT '活动图标',
    `color` VARCHAR(20) DEFAULT '#FFD700' COMMENT '显示颜色',
    
    -- 时间配置
    `schedule_type` VARCHAR(20) NOT NULL DEFAULT 'cron' COMMENT '调度类型: once, daily, weekly, interval, cron',
    `schedule_rule` VARCHAR(100) COMMENT '调度规则 (cron表达式或间隔分钟)',
    `start_time` TIME COMMENT '每日开始时间',
    `end_time` TIME COMMENT '每日结束时间',
    `duration_minutes` INT DEFAULT 30 COMMENT '活动持续时间(分钟)',
    `interval_minutes` INT DEFAULT 0 COMMENT '间隔时间(分钟)',
    
    -- 周期配置 (用于weekly类型)
    `week_days` VARCHAR(20) DEFAULT '1,2,3,4,5,6,7' COMMENT '生效星期 1-7',
    
    -- 活动参数 (JSON格式)
    `params` TEXT COMMENT '活动参数JSON',
    
    -- 状态
    `is_enabled` TINYINT(1) DEFAULT 1 COMMENT '是否启用',
    `is_running` TINYINT(1) DEFAULT 0 COMMENT '是否正在运行',
    `priority` INT DEFAULT 100 COMMENT '优先级(数字越小越高)',
    
    -- 限制条件
    `min_level` INT DEFAULT 1 COMMENT '最低等级限制',
    `max_level` INT DEFAULT 999 COMMENT '最高等级限制',
    `map_id` VARCHAR(50) COMMENT '限定地图(空=全服)',
    `vip_only` TINYINT(1) DEFAULT 0 COMMENT '仅VIP可参与',
    
    -- 统计
    `total_runs` INT DEFAULT 0 COMMENT '总运行次数',
    `last_run_time` DATETIME COMMENT '上次运行时间',
    `next_run_time` DATETIME COMMENT '下次运行时间',
    
    -- 审计
    `created_by` VARCHAR(50) COMMENT '创建人',
    `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `updated_at` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    
    INDEX `idx_type` (`activity_type`),
    INDEX `idx_enabled` (`is_enabled`),
    INDEX `idx_running` (`is_running`),
    INDEX `idx_next_run` (`next_run_time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='游戏活动配置表';

-- 活动运行日志表
CREATE TABLE IF NOT EXISTS `activity_logs` (
    `id` BIGINT PRIMARY KEY AUTO_INCREMENT,
    `activity_id` INT NOT NULL COMMENT '活动ID',
    `activity_name` VARCHAR(100) COMMENT '活动名称',
    `activity_type` VARCHAR(50) COMMENT '活动类型',
    `action` VARCHAR(20) NOT NULL COMMENT '动作: start, end, trigger, error',
    `status` VARCHAR(20) DEFAULT 'success' COMMENT '状态: success, failed, cancelled',
    `message` VARCHAR(500) COMMENT '消息',
    `participants` INT DEFAULT 0 COMMENT '参与人数',
    `rewards_given` INT DEFAULT 0 COMMENT '发放奖励数',
    `details` TEXT COMMENT '详细信息JSON',
    `created_at` DATETIME DEFAULT CURRENT_TIMESTAMP,
    
    INDEX `idx_activity` (`activity_id`),
    INDEX `idx_action` (`action`),
    INDEX `idx_time` (`created_at`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='活动运行日志表';

-- 活动奖励配置表
CREATE TABLE IF NOT EXISTS `activity_rewards` (
    `id` INT PRIMARY KEY AUTO_INCREMENT,
    `activity_id` INT NOT NULL COMMENT '活动ID',
    `reward_type` VARCHAR(20) NOT NULL COMMENT '奖励类型: item, gold, exp, gamegold',
    `reward_value` VARCHAR(100) NOT NULL COMMENT '奖励值(物品名/数量)',
    `quantity` INT DEFAULT 1 COMMENT '数量',
    `probability` INT DEFAULT 100 COMMENT '概率(1-100)',
    `is_broadcast` TINYINT(1) DEFAULT 0 COMMENT '是否全服广播',
    
    INDEX `idx_activity` (`activity_id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='活动奖励配置表';

-- 插入示例活动数据
INSERT INTO `game_activities` (`name`, `activity_type`, `description`, `schedule_type`, `schedule_rule`, `start_time`, `duration_minutes`, `params`, `priority`, `color`) VALUES
('经验派送', 'exp_boost', '开区40分钟击杀超级经验猪爆超级经验卷', 'interval', '40', '00:00:00', 30, 
 '{"exp_rate": 2, "monster_name": "超级经验猪", "drop_item": "超级经验卷", "spawn_count": 50, "map": "0"}', 
 10, '#00FF00'),

('财宝金蛋', 'boss_spawn', '开区60分钟击杀大财主获得大量财宝', 'interval', '60', '00:00:00', 30,
 '{"boss_name": "大财主", "spawn_map": "3", "spawn_x": 330, "spawn_y": 330, "drop_gold": 10000}',
 20, '#FFD700'),

('天降豪礼', 'gift_rain', '每隔90分钟天降豪礼洒落海量物资全场奉送', 'interval', '90', '00:00:00', 10,
 '{"items": ["金条", "祝福油", "强效太阳水"], "count_per_item": 20, "maps": ["0", "3"]}',
 30, '#FF69B4'),

('真假教主', 'boss_spawn', '开区俩个半小时自动开启王者衣服武器爆爆', 'interval', '150', '00:00:00', 60,
 '{"boss_name": "真假教主", "spawn_map": "3", "spawn_x": 330, "spawn_y": 330, "fake_count": 3, "real_count": 1}',
 40, '#FF4500'),

('黑市商人', 'merchant', '每120分钟黑市商人将高价收购各种装备物资', 'interval', '120', '00:00:00', 30,
 '{"npc_name": "黑市商人", "spawn_map": "3", "spawn_x": 338, "spawn_y": 338, "buy_rate": 1.5}',
 50, '#8B4513'),

('激情派对', 'party', '每天多场派对自动运行超高经验，大量元宝', 'daily', NULL, '20:00:00', 60,
 '{"exp_rate": 3, "drop_rate": 2, "gamegold_bonus": 100}',
 60, '#FF1493'),

('双倍经验时间', 'exp_boost', '每晚8点至10点双倍经验', 'daily', NULL, '20:00:00', 120,
 '{"exp_rate": 2}',
 70, '#32CD32'),

('周末狂欢', 'exp_boost', '周末全天三倍经验', 'weekly', NULL, '00:00:00', 1440,
 '{"exp_rate": 3, "drop_rate": 1.5}',
 80, '#9400D3');

-- 插入示例奖励配置
INSERT INTO `activity_rewards` (`activity_id`, `reward_type`, `reward_value`, `quantity`, `probability`, `is_broadcast`) VALUES
(1, 'item', '超级经验卷', 1, 100, 0),
(2, 'gold', '10000', 1, 100, 0),
(2, 'item', '金条', 5, 50, 1),
(3, 'item', '金条', 1, 30, 0),
(3, 'item', '祝福油', 1, 40, 0),
(3, 'item', '强效太阳水', 2, 60, 0),
(4, 'item', '王者战甲(男)', 1, 5, 1),
(4, 'item', '屠龙', 1, 3, 1),
(6, 'gamegold', '100', 1, 100, 0);
