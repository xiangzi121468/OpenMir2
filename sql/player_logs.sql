-- ============================================
-- 玩家行为日志系统
-- 记录关键操作用于运营分析和问题排查
-- ============================================

-- 玩家登录日志
CREATE TABLE IF NOT EXISTS `player_login_logs` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `AccountId` VARCHAR(50) NOT NULL COMMENT '账号ID',
    `CharName` VARCHAR(50) NULL COMMENT '角色名',
    `LoginType` ENUM('login', 'logout', 'kick', 'disconnect') NOT NULL COMMENT '类型',
    `ClientIp` VARCHAR(50) NULL COMMENT '客户端IP',
    `DeviceId` VARCHAR(100) NULL COMMENT '设备ID',
    `OnlineSeconds` INT DEFAULT 0 COMMENT '在线时长（秒）',
    `MapName` VARCHAR(50) NULL COMMENT '地图',
    `PosX` INT DEFAULT 0,
    `PosY` INT DEFAULT 0,
    `Level` INT DEFAULT 0 COMMENT '等级',
    `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_account` (`AccountId`),
    INDEX `idx_char` (`CharName`),
    INDEX `idx_time` (`CreateTime`),
    INDEX `idx_ip` (`ClientIp`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='玩家登录日志';

-- 物品变动日志
CREATE TABLE IF NOT EXISTS `player_item_logs` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `AccountId` VARCHAR(50) NOT NULL,
    `CharName` VARCHAR(50) NOT NULL,
    `ItemId` INT NOT NULL COMMENT '物品ID',
    `ItemName` VARCHAR(50) NOT NULL COMMENT '物品名称',
    `ItemCount` INT NOT NULL COMMENT '数量',
    `ActionType` ENUM('pickup', 'drop', 'use', 'buy', 'sell', 'trade_give', 'trade_receive', 'mail', 'gm', 'npc', 'quest', 'craft', 'enhance', 'decompose', 'market_list', 'market_buy', 'market_cancel') NOT NULL COMMENT '操作类型',
    `ActionDetail` VARCHAR(200) NULL COMMENT '详情',
    `TargetName` VARCHAR(50) NULL COMMENT '目标玩家',
    `Gold` INT DEFAULT 0 COMMENT '涉及金币',
    `GameGold` INT DEFAULT 0 COMMENT '涉及元宝',
    `MapName` VARCHAR(50) NULL,
    `PosX` INT DEFAULT 0,
    `PosY` INT DEFAULT 0,
    `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_account` (`AccountId`),
    INDEX `idx_char` (`CharName`),
    INDEX `idx_item` (`ItemId`),
    INDEX `idx_action` (`ActionType`),
    INDEX `idx_time` (`CreateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='物品变动日志';

-- 货币变动日志
CREATE TABLE IF NOT EXISTS `player_currency_logs` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `AccountId` VARCHAR(50) NOT NULL,
    `CharName` VARCHAR(50) NOT NULL,
    `CurrencyType` ENUM('gold', 'gamegold', 'gamepoint') NOT NULL COMMENT '货币类型',
    `Amount` BIGINT NOT NULL COMMENT '变动数量（正负）',
    `BeforeAmount` BIGINT NOT NULL COMMENT '变动前数量',
    `AfterAmount` BIGINT NOT NULL COMMENT '变动后数量',
    `ActionType` ENUM('recharge', 'shop_buy', 'trade', 'sell_item', 'pickup', 'drop', 'gm', 'mail', 'quest', 'npc', 'market', 'repair', 'enhance') NOT NULL,
    `ActionDetail` VARCHAR(200) NULL,
    `OrderNo` VARCHAR(50) NULL COMMENT '关联订单号',
    `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_account` (`AccountId`),
    INDEX `idx_char` (`CharName`),
    INDEX `idx_type` (`CurrencyType`),
    INDEX `idx_action` (`ActionType`),
    INDEX `idx_time` (`CreateTime`),
    INDEX `idx_order` (`OrderNo`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='货币变动日志';

-- 交易日志
CREATE TABLE IF NOT EXISTS `player_trade_logs` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `TradeNo` VARCHAR(50) NOT NULL COMMENT '交易流水号',
    `TradeType` ENUM('player', 'market', 'shop', 'npc') NOT NULL COMMENT '交易类型',
    `Player1Account` VARCHAR(50) NOT NULL,
    `Player1Name` VARCHAR(50) NOT NULL,
    `Player1Items` TEXT NULL COMMENT '玩家1给出物品JSON',
    `Player1Gold` INT DEFAULT 0,
    `Player1GameGold` INT DEFAULT 0,
    `Player2Account` VARCHAR(50) NULL,
    `Player2Name` VARCHAR(50) NULL,
    `Player2Items` TEXT NULL COMMENT '玩家2给出物品JSON',
    `Player2Gold` INT DEFAULT 0,
    `Player2GameGold` INT DEFAULT 0,
    `Status` ENUM('success', 'cancel', 'failed') DEFAULT 'success',
    `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_trade_no` (`TradeNo`),
    INDEX `idx_player1` (`Player1Name`),
    INDEX `idx_player2` (`Player2Name`),
    INDEX `idx_time` (`CreateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='交易日志';

-- 聊天日志
CREATE TABLE IF NOT EXISTS `player_chat_logs` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `AccountId` VARCHAR(50) NOT NULL,
    `CharName` VARCHAR(50) NOT NULL,
    `ChatType` ENUM('normal', 'private', 'guild', 'team', 'world', 'system') NOT NULL,
    `TargetName` VARCHAR(50) NULL COMMENT '私聊目标',
    `Content` TEXT NOT NULL COMMENT '聊天内容',
    `ClientIp` VARCHAR(50) NULL,
    `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_char` (`CharName`),
    INDEX `idx_type` (`ChatType`),
    INDEX `idx_time` (`CreateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='聊天日志';

-- 战斗日志（简化版，只记录关键战斗）
CREATE TABLE IF NOT EXISTS `player_battle_logs` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `AccountId` VARCHAR(50) NOT NULL,
    `CharName` VARCHAR(50) NOT NULL,
    `BattleType` ENUM('pk_kill', 'pk_death', 'boss_kill', 'monster_kill') NOT NULL,
    `TargetType` ENUM('player', 'monster', 'boss') NOT NULL,
    `TargetName` VARCHAR(50) NOT NULL,
    `MapName` VARCHAR(50) NOT NULL,
    `PosX` INT DEFAULT 0,
    `PosY` INT DEFAULT 0,
    `Exp` INT DEFAULT 0 COMMENT '获得经验',
    `DropItems` TEXT NULL COMMENT '掉落物品JSON',
    `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_char` (`CharName`),
    INDEX `idx_type` (`BattleType`),
    INDEX `idx_target` (`TargetName`),
    INDEX `idx_time` (`CreateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='战斗日志';

-- GM操作日志
CREATE TABLE IF NOT EXISTS `gm_action_logs` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `GmAccount` VARCHAR(50) NOT NULL COMMENT 'GM账号',
    `GmName` VARCHAR(50) NOT NULL COMMENT 'GM角色名',
    `Command` VARCHAR(100) NOT NULL COMMENT '命令',
    `Params` TEXT NULL COMMENT '参数',
    `TargetName` VARCHAR(50) NULL COMMENT '目标玩家',
    `Result` VARCHAR(200) NULL COMMENT '执行结果',
    `ClientIp` VARCHAR(50) NULL,
    `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_gm` (`GmAccount`),
    INDEX `idx_target` (`TargetName`),
    INDEX `idx_command` (`Command`),
    INDEX `idx_time` (`CreateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='GM操作日志';

-- 日志表自动清理存储过程
DELIMITER //
CREATE PROCEDURE IF NOT EXISTS `cleanup_old_logs`(IN days_to_keep INT)
BEGIN
    DECLARE cutoff_date DATETIME;
    SET cutoff_date = DATE_SUB(NOW(), INTERVAL days_to_keep DAY);
    
    DELETE FROM player_login_logs WHERE CreateTime < cutoff_date;
    DELETE FROM player_item_logs WHERE CreateTime < cutoff_date;
    DELETE FROM player_currency_logs WHERE CreateTime < cutoff_date;
    DELETE FROM player_trade_logs WHERE CreateTime < cutoff_date;
    DELETE FROM player_chat_logs WHERE CreateTime < cutoff_date;
    DELETE FROM player_battle_logs WHERE CreateTime < cutoff_date;
    
    -- GM日志保留更长时间
    DELETE FROM gm_action_logs WHERE CreateTime < DATE_SUB(NOW(), INTERVAL days_to_keep * 2 DAY);
END //
DELIMITER ;

-- 定时清理事件（每天凌晨3点执行，保留90天日志）
-- CREATE EVENT IF NOT EXISTS `event_cleanup_logs`
-- ON SCHEDULE EVERY 1 DAY STARTS CONCAT(CURDATE(), ' 03:00:00')
-- DO CALL cleanup_old_logs(90);
