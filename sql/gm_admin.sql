-- ========================================
-- GM管理后台数据库
-- ========================================

-- 管理员表
CREATE TABLE IF NOT EXISTS `admin_users` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Username` VARCHAR(50) NOT NULL UNIQUE COMMENT '用户名',
    `Password` VARCHAR(128) NOT NULL COMMENT '密码(加密)',
    `Salt` VARCHAR(32) NOT NULL COMMENT '密码盐',
    `Nickname` VARCHAR(50) DEFAULT NULL COMMENT '昵称',
    `Email` VARCHAR(100) DEFAULT NULL COMMENT '邮箱',
    `Phone` VARCHAR(20) DEFAULT NULL COMMENT '手机',
    `Avatar` VARCHAR(200) DEFAULT NULL COMMENT '头像',
    `Role` VARCHAR(20) NOT NULL DEFAULT 'gm' COMMENT '角色:superadmin,admin,gm,viewer',
    `Permissions` TEXT DEFAULT NULL COMMENT '权限列表(JSON)',
    `LastLoginTime` DATETIME DEFAULT NULL COMMENT '最后登录时间',
    `LastLoginIp` VARCHAR(50) DEFAULT NULL COMMENT '最后登录IP',
    `LoginCount` INT DEFAULT 0 COMMENT '登录次数',
    `Status` TINYINT DEFAULT 1 COMMENT '状态:0禁用,1正常',
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `UpdateTime` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX `idx_username` (`Username`),
    INDEX `idx_role` (`Role`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='管理员表';

-- 管理员操作日志
CREATE TABLE IF NOT EXISTS `admin_logs` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `AdminId` INT NOT NULL COMMENT '管理员ID',
    `AdminName` VARCHAR(50) NOT NULL COMMENT '管理员名',
    `Module` VARCHAR(50) NOT NULL COMMENT '模块',
    `Action` VARCHAR(50) NOT NULL COMMENT '操作',
    `Target` VARCHAR(100) DEFAULT NULL COMMENT '操作对象',
    `Content` TEXT DEFAULT NULL COMMENT '操作内容',
    `IpAddress` VARCHAR(50) DEFAULT NULL COMMENT 'IP地址',
    `UserAgent` VARCHAR(500) DEFAULT NULL COMMENT '浏览器信息',
    `Result` TINYINT DEFAULT 1 COMMENT '结果:0失败,1成功',
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_admin` (`AdminId`),
    INDEX `idx_module` (`Module`),
    INDEX `idx_time` (`CreateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='管理员操作日志';

-- 系统公告表
CREATE TABLE IF NOT EXISTS `system_notices` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Title` VARCHAR(200) NOT NULL COMMENT '标题',
    `Content` TEXT NOT NULL COMMENT '内容',
    `Type` VARCHAR(20) DEFAULT 'normal' COMMENT '类型:normal,important,urgent',
    `Target` VARCHAR(20) DEFAULT 'all' COMMENT '目标:all,online,vip',
    `StartTime` DATETIME DEFAULT NULL COMMENT '生效时间',
    `EndTime` DATETIME DEFAULT NULL COMMENT '失效时间',
    `IsEnabled` TINYINT DEFAULT 1 COMMENT '是否启用',
    `IsSent` TINYINT DEFAULT 0 COMMENT '是否已发送',
    `SentTime` DATETIME DEFAULT NULL COMMENT '发送时间',
    `AdminId` INT DEFAULT NULL COMMENT '创建人',
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_enabled` (`IsEnabled`, `StartTime`, `EndTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='系统公告表';

-- 封禁记录表
CREATE TABLE IF NOT EXISTS `ban_records` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `BanType` VARCHAR(20) NOT NULL COMMENT '封禁类型:account,ip,device',
    `BanValue` VARCHAR(100) NOT NULL COMMENT '封禁值',
    `CharName` VARCHAR(50) DEFAULT NULL COMMENT '角色名',
    `Reason` VARCHAR(500) NOT NULL COMMENT '封禁原因',
    `Duration` INT DEFAULT -1 COMMENT '封禁时长(分钟,-1永久)',
    `StartTime` DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '开始时间',
    `EndTime` DATETIME DEFAULT NULL COMMENT '结束时间',
    `AdminId` INT NOT NULL COMMENT '操作管理员',
    `AdminName` VARCHAR(50) NOT NULL COMMENT '管理员名',
    `Status` TINYINT DEFAULT 1 COMMENT '状态:0已解封,1封禁中',
    `UnbanTime` DATETIME DEFAULT NULL COMMENT '解封时间',
    `UnbanAdminId` INT DEFAULT NULL COMMENT '解封管理员',
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_type_value` (`BanType`, `BanValue`),
    INDEX `idx_status` (`Status`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='封禁记录表';

-- 邮件发送记录
CREATE TABLE IF NOT EXISTS `mail_records` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `MailType` VARCHAR(20) NOT NULL COMMENT '类型:single,batch,all',
    `Title` VARCHAR(200) NOT NULL COMMENT '标题',
    `Content` TEXT NOT NULL COMMENT '内容',
    `Attachments` TEXT DEFAULT NULL COMMENT '附件(JSON:[{item,count}])',
    `GameGold` INT DEFAULT 0 COMMENT '附带元宝',
    `Recipients` TEXT DEFAULT NULL COMMENT '收件人列表',
    `RecipientCount` INT DEFAULT 0 COMMENT '收件人数量',
    `SentCount` INT DEFAULT 0 COMMENT '已发送数量',
    `Status` TINYINT DEFAULT 0 COMMENT '状态:0待发送,1发送中,2已完成',
    `AdminId` INT NOT NULL,
    `AdminName` VARCHAR(50) NOT NULL,
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_status` (`Status`),
    INDEX `idx_time` (`CreateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='邮件发送记录';

-- 数据统计快照
CREATE TABLE IF NOT EXISTS `stats_snapshot` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `SnapshotDate` DATE NOT NULL COMMENT '统计日期',
    `TotalAccounts` INT DEFAULT 0 COMMENT '总账号数',
    `TotalCharacters` INT DEFAULT 0 COMMENT '总角色数',
    `NewAccounts` INT DEFAULT 0 COMMENT '新增账号',
    `NewCharacters` INT DEFAULT 0 COMMENT '新增角色',
    `ActiveAccounts` INT DEFAULT 0 COMMENT '活跃账号',
    `OnlinePeak` INT DEFAULT 0 COMMENT '在线峰值',
    `TotalRecharge` DECIMAL(12,2) DEFAULT 0 COMMENT '充值金额',
    `RechargeCount` INT DEFAULT 0 COMMENT '充值笔数',
    `ShopSales` INT DEFAULT 0 COMMENT '商城销售额(元宝)',
    `ShopOrders` INT DEFAULT 0 COMMENT '商城订单数',
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    UNIQUE KEY `uk_date` (`SnapshotDate`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='数据统计快照';

-- ========================================
-- 初始化数据
-- ========================================

-- 默认超级管理员 (密码: admin123)
INSERT INTO `admin_users` (`Username`, `Password`, `Salt`, `Nickname`, `Role`, `Status`) VALUES
('admin', 'e10adc3949ba59abbe56e057f20f883e', 'default_salt', '超级管理员', 'superadmin', 1),
('gm001', 'e10adc3949ba59abbe56e057f20f883e', 'default_salt', 'GM小一', 'gm', 1);

-- 示例公告
INSERT INTO `system_notices` (`Title`, `Content`, `Type`, `Target`, `IsEnabled`, `AdminId`) VALUES
('欢迎来到传奇世界', '亲爱的玩家，欢迎您来到传奇世界！祝您游戏愉快！', 'normal', 'all', 1, 1),
('服务器维护公告', '服务器将于每周三凌晨3:00-5:00进行例行维护，届时将无法登录游戏，请提前做好准备。', 'important', 'all', 1, 1);
