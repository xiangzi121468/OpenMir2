-- ============================================
-- 邮件系统数据库表
-- OpenMir2 Mail System
-- ============================================

-- 邮件表
CREATE TABLE IF NOT EXISTS `game_mails` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `SenderId` VARCHAR(50) DEFAULT 'SYSTEM' COMMENT '发送者账号ID，SYSTEM表示系统邮件',
    `SenderName` VARCHAR(50) DEFAULT '系统' COMMENT '发送者名称',
    `ReceiverType` ENUM('single', 'all', 'level', 'vip', 'online') NOT NULL DEFAULT 'single' COMMENT '接收类型',
    `ReceiverId` VARCHAR(50) NULL COMMENT '接收者账号ID（单人发送时使用）',
    `ReceiverName` VARCHAR(50) NULL COMMENT '接收者角色名（单人发送时使用）',
    `ReceiverCondition` VARCHAR(200) NULL COMMENT '接收条件JSON（批量发送时使用）',
    `Title` VARCHAR(100) NOT NULL COMMENT '邮件标题',
    `Content` TEXT NOT NULL COMMENT '邮件内容',
    `Attachments` TEXT NULL COMMENT '附件JSON: [{itemId, itemName, count}]',
    `Gold` INT DEFAULT 0 COMMENT '附带金币',
    `GameGold` INT DEFAULT 0 COMMENT '附带元宝',
    `ExpireHours` INT DEFAULT 168 COMMENT '过期时间（小时），默认7天',
    `MailType` ENUM('system', 'reward', 'gm', 'activity', 'compensation') NOT NULL DEFAULT 'system' COMMENT '邮件类型',
    `Priority` TINYINT DEFAULT 0 COMMENT '优先级 0-普通 1-重要 2-紧急',
    `Status` ENUM('draft', 'pending', 'sent', 'cancelled') NOT NULL DEFAULT 'pending' COMMENT '状态',
    `SendTime` DATETIME NULL COMMENT '发送时间',
    `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `CreateBy` VARCHAR(50) NULL COMMENT '创建者（管理员）',
    `TotalReceivers` INT DEFAULT 0 COMMENT '总接收人数',
    `ClaimedCount` INT DEFAULT 0 COMMENT '已领取人数',
    INDEX `idx_receiver` (`ReceiverId`, `ReceiverName`),
    INDEX `idx_type` (`MailType`),
    INDEX `idx_status` (`Status`),
    INDEX `idx_time` (`CreateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='游戏邮件主表';

-- 玩家邮件接收表
CREATE TABLE IF NOT EXISTS `player_mails` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `MailId` INT NOT NULL COMMENT '邮件ID',
    `AccountId` VARCHAR(50) NOT NULL COMMENT '账号ID',
    `CharName` VARCHAR(50) NOT NULL COMMENT '角色名',
    `IsRead` TINYINT(1) DEFAULT 0 COMMENT '是否已读',
    `IsClaimed` TINYINT(1) DEFAULT 0 COMMENT '是否已领取附件',
    `IsDeleted` TINYINT(1) DEFAULT 0 COMMENT '是否已删除',
    `ReadTime` DATETIME NULL COMMENT '阅读时间',
    `ClaimTime` DATETIME NULL COMMENT '领取时间',
    `ExpireTime` DATETIME NOT NULL COMMENT '过期时间',
    `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_account` (`AccountId`),
    INDEX `idx_char` (`CharName`),
    INDEX `idx_mail` (`MailId`),
    INDEX `idx_status` (`IsRead`, `IsClaimed`, `IsDeleted`),
    INDEX `idx_expire` (`ExpireTime`),
    FOREIGN KEY (`MailId`) REFERENCES `game_mails`(`Id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='玩家邮件接收表';

-- 邮件模板表
CREATE TABLE IF NOT EXISTS `mail_templates` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Name` VARCHAR(50) NOT NULL COMMENT '模板名称',
    `Title` VARCHAR(100) NOT NULL COMMENT '邮件标题',
    `Content` TEXT NOT NULL COMMENT '邮件内容，支持变量如{playerName}',
    `Attachments` TEXT NULL COMMENT '默认附件JSON',
    `Gold` INT DEFAULT 0,
    `GameGold` INT DEFAULT 0,
    `MailType` ENUM('system', 'reward', 'gm', 'activity', 'compensation') NOT NULL DEFAULT 'system',
    `IsEnabled` TINYINT(1) DEFAULT 1,
    `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdateTime` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    INDEX `idx_name` (`Name`),
    INDEX `idx_enabled` (`IsEnabled`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='邮件模板表';

-- 插入默认模板
INSERT INTO `mail_templates` (`Name`, `Title`, `Content`, `MailType`) VALUES
('welcome', '欢迎来到传奇世界', '亲爱的{playerName}：\n\n欢迎来到传奇世界！祝您游戏愉快！\n\n——运营团队', 'system'),
('maintenance', '维护补偿', '亲爱的玩家：\n\n感谢您的耐心等待，现发放维护补偿，请查收！\n\n——运营团队', 'compensation'),
('activity_reward', '活动奖励', '恭喜您在活动中获得奖励，请查收附件！', 'activity'),
('gm_gift', 'GM礼物', '这是来自GM的礼物，请查收！', 'gm');

-- ============================================
-- 示例数据
-- ============================================
INSERT INTO `game_mails` (`SenderId`, `SenderName`, `ReceiverType`, `Title`, `Content`, `MailType`, `Status`, `SendTime`) VALUES
('SYSTEM', '系统', 'all', '欢迎来到新版本', '亲爱的玩家：\n\n新版本已上线，新增了商城、活动等功能，快来体验吧！\n\n——运营团队', 'system', 'sent', NOW());
