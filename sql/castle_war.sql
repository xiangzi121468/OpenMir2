-- ============================================
-- 沙城争夺/攻城战系统
-- Castle War / Sabak Siege System
-- ============================================

-- 城堡信息表
CREATE TABLE IF NOT EXISTS `castles` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `CastleName` VARCHAR(50) NOT NULL COMMENT '城堡名称',
    `MapName` VARCHAR(50) NOT NULL COMMENT '城堡地图',
    `PalaceMapName` VARCHAR(50) NULL COMMENT '皇宫地图',
    `CenterX` INT NOT NULL COMMENT '中心X坐标',
    `CenterY` INT NOT NULL COMMENT '中心Y坐标',
    `OwnerGuildId` INT NULL COMMENT '占领行会ID',
    `OwnerGuildName` VARCHAR(50) NULL COMMENT '占领行会名称',
    `OwnerName` VARCHAR(50) NULL COMMENT '城主角色名',
    `OccupyTime` DATETIME NULL COMMENT '占领时间',
    `TaxRate` INT DEFAULT 10 COMMENT '税率(0-50%)',
    `TotalTax` BIGINT DEFAULT 0 COMMENT '累计税收',
    `DefenseLevel` INT DEFAULT 1 COMMENT '城防等级(1-10)',
    `GateHP` INT DEFAULT 100000 COMMENT '城门HP',
    `GateMaxHP` INT DEFAULT 100000 COMMENT '城门最大HP',
    `LeftTowerHP` INT DEFAULT 50000 COMMENT '左箭塔HP',
    `RightTowerHP` INT DEFAULT 50000 COMMENT '右箭塔HP',
    `TowerMaxHP` INT DEFAULT 50000 COMMENT '箭塔最大HP',
    `GuardCount` INT DEFAULT 10 COMMENT '守卫数量',
    `IsUnderAttack` TINYINT(1) DEFAULT 0 COMMENT '是否正在攻城',
    `WarStartTime` DATETIME NULL COMMENT '本次攻城开始时间',
    `WarEndTime` DATETIME NULL COMMENT '本次攻城结束时间',
    `LastWarTime` DATETIME NULL COMMENT '上次攻城时间',
    `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `UpdateTime` DATETIME NULL ON UPDATE CURRENT_TIMESTAMP,
    UNIQUE KEY `uk_name` (`CastleName`),
    INDEX `idx_owner` (`OwnerGuildId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='城堡信息';

-- 攻城战报名表
CREATE TABLE IF NOT EXISTS `castle_war_applications` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `CastleId` INT NOT NULL COMMENT '城堡ID',
    `WarDate` DATE NOT NULL COMMENT '攻城日期',
    `GuildId` INT NOT NULL COMMENT '行会ID',
    `GuildName` VARCHAR(50) NOT NULL COMMENT '行会名称',
    `GuildMaster` VARCHAR(50) NOT NULL COMMENT '会长名称',
    `ApplyTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    `ApplyFee` BIGINT DEFAULT 0 COMMENT '报名费',
    `Status` ENUM('pending', 'approved', 'rejected', 'cancelled') DEFAULT 'approved' COMMENT '状态',
    `MemberCount` INT DEFAULT 0 COMMENT '报名时行会人数',
    INDEX `idx_castle_date` (`CastleId`, `WarDate`),
    INDEX `idx_guild` (`GuildId`),
    UNIQUE KEY `uk_castle_date_guild` (`CastleId`, `WarDate`, `GuildId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='攻城战报名';

-- 攻城战记录表
CREATE TABLE IF NOT EXISTS `castle_war_records` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `CastleId` INT NOT NULL,
    `CastleName` VARCHAR(50) NOT NULL,
    `WarDate` DATE NOT NULL COMMENT '攻城日期',
    `StartTime` DATETIME NOT NULL COMMENT '开始时间',
    `EndTime` DATETIME NULL COMMENT '结束时间',
    `Duration` INT DEFAULT 0 COMMENT '持续时间（秒）',
    `DefenderGuildId` INT NULL COMMENT '守城行会ID',
    `DefenderGuildName` VARCHAR(50) NULL COMMENT '守城行会名',
    `WinnerGuildId` INT NULL COMMENT '胜利行会ID',
    `WinnerGuildName` VARCHAR(50) NULL COMMENT '胜利行会名',
    `WinnerName` VARCHAR(50) NULL COMMENT '夺城者角色名',
    `AttackerCount` INT DEFAULT 0 COMMENT '攻城方人数',
    `DefenderCount` INT DEFAULT 0 COMMENT '守城方人数',
    `TotalKills` INT DEFAULT 0 COMMENT '总击杀数',
    `GateDestroyed` TINYINT(1) DEFAULT 0 COMMENT '城门是否被破坏',
    `Result` ENUM('defender_win', 'attacker_win', 'draw', 'cancelled') NULL COMMENT '结果',
    `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_castle` (`CastleId`),
    INDEX `idx_date` (`WarDate`),
    INDEX `idx_winner` (`WinnerGuildId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='攻城战记录';

-- 攻城战击杀记录
CREATE TABLE IF NOT EXISTS `castle_war_kills` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `WarRecordId` INT NOT NULL COMMENT '战争记录ID',
    `CastleId` INT NOT NULL,
    `KillerAccount` VARCHAR(50) NOT NULL,
    `KillerName` VARCHAR(50) NOT NULL COMMENT '击杀者',
    `KillerGuildId` INT NULL,
    `KillerGuildName` VARCHAR(50) NULL,
    `VictimAccount` VARCHAR(50) NOT NULL,
    `VictimName` VARCHAR(50) NOT NULL COMMENT '被杀者',
    `VictimGuildId` INT NULL,
    `VictimGuildName` VARCHAR(50) NULL,
    `MapName` VARCHAR(50) NOT NULL,
    `PosX` INT DEFAULT 0,
    `PosY` INT DEFAULT 0,
    `KillTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_war` (`WarRecordId`),
    INDEX `idx_killer` (`KillerName`),
    INDEX `idx_victim` (`VictimName`),
    INDEX `idx_time` (`KillTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='攻城战击杀记录';

-- 攻城战排行（每次战争结束后统计）
CREATE TABLE IF NOT EXISTS `castle_war_rankings` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `WarRecordId` INT NOT NULL,
    `CastleId` INT NOT NULL,
    `WarDate` DATE NOT NULL,
    `AccountId` VARCHAR(50) NOT NULL,
    `CharName` VARCHAR(50) NOT NULL,
    `GuildId` INT NULL,
    `GuildName` VARCHAR(50) NULL,
    `Kills` INT DEFAULT 0 COMMENT '击杀数',
    `Deaths` INT DEFAULT 0 COMMENT '死亡数',
    `DamageDealt` BIGINT DEFAULT 0 COMMENT '造成伤害',
    `DamageTaken` BIGINT DEFAULT 0 COMMENT '承受伤害',
    `HealingDone` BIGINT DEFAULT 0 COMMENT '治疗量',
    `GateDamage` BIGINT DEFAULT 0 COMMENT '对城门伤害',
    `Score` INT DEFAULT 0 COMMENT '综合得分',
    `Rank` INT DEFAULT 0 COMMENT '排名',
    `RewardClaimed` TINYINT(1) DEFAULT 0 COMMENT '是否领取奖励',
    `CreateTime` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_war` (`WarRecordId`),
    INDEX `idx_char` (`CharName`),
    INDEX `idx_guild` (`GuildId`),
    INDEX `idx_score` (`Score` DESC)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='攻城战排行';

-- 城堡配置表
CREATE TABLE IF NOT EXISTS `castle_configs` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `ConfigKey` VARCHAR(50) NOT NULL COMMENT '配置键',
    `ConfigValue` VARCHAR(200) NOT NULL COMMENT '配置值',
    `Description` VARCHAR(200) NULL COMMENT '说明',
    UNIQUE KEY `uk_key` (`ConfigKey`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='攻城战配置';

-- 插入默认配置
INSERT INTO `castle_configs` (`ConfigKey`, `ConfigValue`, `Description`) VALUES
('WAR_DAY', '6', '攻城日（0=周日, 6=周六）'),
('WAR_START_HOUR', '20', '攻城开始时间（小时）'),
('WAR_START_MINUTE', '0', '攻城开始时间（分钟）'),
('WAR_DURATION_MINUTES', '120', '攻城持续时间（分钟）'),
('APPLY_FEE', '1000000', '报名费（金币）'),
('APPLY_DEADLINE_HOURS', '24', '报名截止时间（攻城前N小时）'),
('MIN_GUILD_LEVEL', '3', '最低行会等级要求'),
('MIN_GUILD_MEMBERS', '10', '最低行会人数要求'),
('RESPAWN_DELAY_SECONDS', '30', '攻城期间复活延迟（秒）'),
('GATE_REPAIR_COST', '100000', '城门修复费用'),
('TOWER_REPAIR_COST', '50000', '箭塔修复费用'),
('TAX_COLLECT_INTERVAL', '3600', '税收结算间隔（秒）'),
('MAX_TAX_RATE', '50', '最高税率')
ON DUPLICATE KEY UPDATE ConfigValue=VALUES(ConfigValue);

-- 插入默认城堡（沙巴克）
INSERT INTO `castles` (`CastleName`, `MapName`, `PalaceMapName`, `CenterX`, `CenterY`, `GateHP`, `GateMaxHP`) VALUES
('沙巴克', 'D715', 'D716', 280, 310, 500000, 500000)
ON DUPLICATE KEY UPDATE MapName=VALUES(MapName);

-- 城堡守卫配置
CREATE TABLE IF NOT EXISTS `castle_guards` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `CastleId` INT NOT NULL,
    `GuardName` VARCHAR(50) NOT NULL COMMENT '守卫名称',
    `GuardType` ENUM('gate_guard', 'tower_guard', 'palace_guard', 'patrol') NOT NULL COMMENT '守卫类型',
    `MapName` VARCHAR(50) NOT NULL,
    `PosX` INT NOT NULL,
    `PosY` INT NOT NULL,
    `Level` INT DEFAULT 50,
    `HP` INT DEFAULT 10000,
    `Attack` INT DEFAULT 500,
    `RespawnSeconds` INT DEFAULT 60 COMMENT '复活时间',
    `IsEnabled` TINYINT(1) DEFAULT 1,
    INDEX `idx_castle` (`CastleId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='城堡守卫配置';

-- 插入沙巴克默认守卫
INSERT INTO `castle_guards` (`CastleId`, `GuardName`, `GuardType`, `MapName`, `PosX`, `PosY`, `Level`, `HP`, `Attack`) VALUES
(1, '沙巴克城门卫士', 'gate_guard', 'D715', 280, 350, 60, 50000, 800),
(1, '沙巴克城门卫士', 'gate_guard', 'D715', 290, 350, 60, 50000, 800),
(1, '沙巴克箭塔弓手', 'tower_guard', 'D715', 260, 340, 55, 30000, 600),
(1, '沙巴克箭塔弓手', 'tower_guard', 'D715', 300, 340, 55, 30000, 600),
(1, '沙巴克皇宫护卫', 'palace_guard', 'D716', 15, 15, 70, 80000, 1000),
(1, '沙巴克巡逻兵', 'patrol', 'D715', 280, 300, 50, 20000, 500);
