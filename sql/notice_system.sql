-- ========================================
-- 完整通知系统数据库
-- ========================================

-- 删除旧表（如果存在）
DROP TABLE IF EXISTS `game_notices`;

-- 游戏公告表（重新设计）
CREATE TABLE IF NOT EXISTS `game_notices` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Title` VARCHAR(200) NOT NULL COMMENT '标题',
    `Content` TEXT NOT NULL COMMENT '内容',
    `NoticeType` VARCHAR(30) NOT NULL DEFAULT 'broadcast' COMMENT '类型:login登录,broadcast全服,scroll滚动,map地图,schedule定时',
    `Priority` TINYINT DEFAULT 0 COMMENT '优先级:0普通,1重要,2紧急',
    `Color` VARCHAR(20) DEFAULT 'yellow' COMMENT '颜色:red,green,blue,yellow,white',
    `TargetMap` VARCHAR(50) DEFAULT NULL COMMENT '目标地图(map类型时)',
    `TargetLevel` INT DEFAULT 0 COMMENT '目标等级(>=此等级)',
    `TargetVip` INT DEFAULT 0 COMMENT '目标VIP(>=此等级)',
    `RepeatCount` INT DEFAULT 1 COMMENT '重复次数(0无限)',
    `RepeatInterval` INT DEFAULT 0 COMMENT '重复间隔(秒)',
    `StartTime` DATETIME DEFAULT NULL COMMENT '开始时间',
    `EndTime` DATETIME DEFAULT NULL COMMENT '结束时间',
    `ScheduleCron` VARCHAR(100) DEFAULT NULL COMMENT '定时表达式(如:0 12 * * * 每天12点)',
    `LastSentTime` DATETIME DEFAULT NULL COMMENT '上次发送时间',
    `SentCount` INT DEFAULT 0 COMMENT '已发送次数',
    `IsEnabled` TINYINT DEFAULT 1 COMMENT '是否启用',
    `AdminId` INT DEFAULT NULL COMMENT '创建管理员',
    `AdminName` VARCHAR(50) DEFAULT NULL COMMENT '管理员名',
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `UpdateTime` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX `idx_type` (`NoticeType`),
    INDEX `idx_enabled` (`IsEnabled`, `StartTime`, `EndTime`),
    INDEX `idx_schedule` (`NoticeType`, `IsEnabled`, `ScheduleCron`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='游戏公告表';

-- 公告发送记录
CREATE TABLE IF NOT EXISTS `notice_send_logs` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `NoticeId` INT NOT NULL COMMENT '公告ID',
    `NoticeType` VARCHAR(30) NOT NULL COMMENT '公告类型',
    `TargetCount` INT DEFAULT 0 COMMENT '目标人数',
    `Content` VARCHAR(500) DEFAULT NULL COMMENT '发送内容',
    `SendTime` DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '发送时间',
    INDEX `idx_notice` (`NoticeId`),
    INDEX `idx_time` (`SendTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='公告发送记录';

-- ========================================
-- 初始化数据
-- ========================================

-- 登录公告
INSERT INTO `game_notices` (`Title`, `Content`, `NoticeType`, `Priority`, `Color`, `IsEnabled`) VALUES
('欢迎公告', '欢迎来到传奇世界！祝您游戏愉快！\n请遵守游戏规则，文明游戏。', 'login', 0, 'yellow', 1),
('新手指引', '新玩家可前往新手村领取新手礼包！\n输入@help查看帮助命令。', 'login', 0, 'green', 1);

-- 滚动公告
INSERT INTO `game_notices` (`Title`, `Content`, `NoticeType`, `Priority`, `Color`, `RepeatCount`, `RepeatInterval`, `IsEnabled`) VALUES
('充值优惠', '首充双倍元宝！VIP享专属折扣！', 'scroll', 1, 'yellow', 0, 300, 1),
('活动预告', '每晚20:00攻城战，21:00世界BOSS刷新！', 'scroll', 0, 'green', 0, 600, 1);

-- 定时公告
INSERT INTO `game_notices` (`Title`, `Content`, `NoticeType`, `Priority`, `Color`, `ScheduleCron`, `IsEnabled`) VALUES
('午间提醒', '中午了，记得休息一下哦~', 'schedule', 0, 'blue', '0 12 * * *', 1),
('维护预告', '服务器将于每周三凌晨3:00进行例行维护', 'schedule', 1, 'red', '0 2 * * 3', 1);

-- 全服广播示例
INSERT INTO `game_notices` (`Title`, `Content`, `NoticeType`, `Priority`, `Color`, `IsEnabled`) VALUES
('系统公告', '服务器运行正常，感谢您的支持！', 'broadcast', 0, 'yellow', 0);
