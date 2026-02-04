-- ========================================
-- 元宝商城系统数据库
-- ========================================

-- 商品分类表
CREATE TABLE IF NOT EXISTS `shop_categories` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Name` VARCHAR(50) NOT NULL COMMENT '分类名称',
    `SortOrder` INT DEFAULT 0 COMMENT '排序顺序',
    `Icon` VARCHAR(100) DEFAULT NULL COMMENT '分类图标',
    `IsEnabled` TINYINT(1) DEFAULT 1 COMMENT '是否启用',
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_sort` (`SortOrder`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='商城分类表';

-- 商品表
CREATE TABLE IF NOT EXISTS `shop_items` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `CategoryId` INT NOT NULL COMMENT '分类ID',
    `ItemName` VARCHAR(50) NOT NULL COMMENT '物品名称(对应stditems)',
    `ItemId` INT DEFAULT 0 COMMENT '物品ID(stditems.Id)',
    `DisplayName` VARCHAR(100) NOT NULL COMMENT '商城显示名称',
    `Description` VARCHAR(500) DEFAULT NULL COMMENT '商品描述',
    `Icon` VARCHAR(100) DEFAULT NULL COMMENT '商品图标',
    `Price` INT NOT NULL COMMENT '元宝价格',
    `OriginalPrice` INT DEFAULT 0 COMMENT '原价(用于显示折扣)',
    `ItemCount` INT DEFAULT 1 COMMENT '购买获得数量',
    `Stock` INT DEFAULT -1 COMMENT '库存(-1为无限)',
    `SoldCount` INT DEFAULT 0 COMMENT '已售数量',
    `LimitPerUser` INT DEFAULT 0 COMMENT '每人限购(0为不限)',
    `LimitPerDay` INT DEFAULT 0 COMMENT '每日限购(0为不限)',
    `RequireLevel` INT DEFAULT 0 COMMENT '购买等级限制',
    `RequireVip` INT DEFAULT 0 COMMENT 'VIP等级限制',
    `IsHot` TINYINT(1) DEFAULT 0 COMMENT '热销标记',
    `IsNew` TINYINT(1) DEFAULT 0 COMMENT '新品标记',
    `IsRecommend` TINYINT(1) DEFAULT 0 COMMENT '推荐标记',
    `SortOrder` INT DEFAULT 0 COMMENT '排序',
    `StartTime` DATETIME DEFAULT NULL COMMENT '上架时间',
    `EndTime` DATETIME DEFAULT NULL COMMENT '下架时间',
    `IsEnabled` TINYINT(1) DEFAULT 1 COMMENT '是否启用',
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `UpdateTime` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP,
    INDEX `idx_category` (`CategoryId`),
    INDEX `idx_item` (`ItemName`),
    INDEX `idx_enabled` (`IsEnabled`, `SortOrder`),
    FOREIGN KEY (`CategoryId`) REFERENCES `shop_categories`(`Id`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='商城商品表';

-- 购买订单表
CREATE TABLE IF NOT EXISTS `shop_orders` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `OrderNo` VARCHAR(32) NOT NULL UNIQUE COMMENT '订单号',
    `AccountId` VARCHAR(50) NOT NULL COMMENT '账号',
    `CharName` VARCHAR(50) NOT NULL COMMENT '角色名',
    `ShopItemId` INT NOT NULL COMMENT '商品ID',
    `ItemName` VARCHAR(50) NOT NULL COMMENT '物品名称',
    `ItemCount` INT NOT NULL COMMENT '购买数量',
    `UnitPrice` INT NOT NULL COMMENT '单价(元宝)',
    `TotalPrice` INT NOT NULL COMMENT '总价(元宝)',
    `Status` TINYINT DEFAULT 0 COMMENT '状态:0待发货,1已发货,2已取消,3退款',
    `DeliverTime` DATETIME DEFAULT NULL COMMENT '发货时间',
    `Remark` VARCHAR(200) DEFAULT NULL COMMENT '备注',
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_account` (`AccountId`),
    INDEX `idx_char` (`CharName`),
    INDEX `idx_status` (`Status`),
    INDEX `idx_time` (`CreateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='商城订单表';

-- 充值记录表
CREATE TABLE IF NOT EXISTS `recharge_records` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `OrderNo` VARCHAR(64) NOT NULL UNIQUE COMMENT '充值订单号',
    `AccountId` VARCHAR(50) NOT NULL COMMENT '账号',
    `CharName` VARCHAR(50) DEFAULT NULL COMMENT '角色名(可空)',
    `Amount` DECIMAL(10,2) NOT NULL COMMENT '充值金额(元)',
    `GameGold` INT NOT NULL COMMENT '获得元宝',
    `BonusGold` INT DEFAULT 0 COMMENT '赠送元宝',
    `PayChannel` VARCHAR(20) NOT NULL COMMENT '支付渠道:alipay,wechat,card',
    `PayOrderNo` VARCHAR(64) DEFAULT NULL COMMENT '第三方支付订单号',
    `Status` TINYINT DEFAULT 0 COMMENT '状态:0待支付,1已支付,2已发放,3失败,4退款',
    `PayTime` DATETIME DEFAULT NULL COMMENT '支付时间',
    `DeliverTime` DATETIME DEFAULT NULL COMMENT '发放时间',
    `ClientIp` VARCHAR(50) DEFAULT NULL COMMENT '客户端IP',
    `Remark` VARCHAR(200) DEFAULT NULL COMMENT '备注',
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_account` (`AccountId`),
    INDEX `idx_status` (`Status`),
    INDEX `idx_channel` (`PayChannel`),
    INDEX `idx_time` (`CreateTime`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='充值记录表';

-- 充值档位配置表
CREATE TABLE IF NOT EXISTS `recharge_packages` (
    `Id` INT AUTO_INCREMENT PRIMARY KEY,
    `Name` VARCHAR(50) NOT NULL COMMENT '档位名称',
    `Amount` DECIMAL(10,2) NOT NULL COMMENT '充值金额(元)',
    `GameGold` INT NOT NULL COMMENT '基础元宝',
    `BonusGold` INT DEFAULT 0 COMMENT '首充赠送',
    `ExtraBonusGold` INT DEFAULT 0 COMMENT '额外赠送(活动)',
    `Description` VARCHAR(200) DEFAULT NULL COMMENT '描述',
    `SortOrder` INT DEFAULT 0 COMMENT '排序',
    `IsEnabled` TINYINT(1) DEFAULT 1 COMMENT '是否启用',
    `IsHot` TINYINT(1) DEFAULT 0 COMMENT '推荐标记',
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_enabled` (`IsEnabled`, `SortOrder`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='充值档位配置';

-- 礼包码表
CREATE TABLE IF NOT EXISTS `gift_codes` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `Code` VARCHAR(32) NOT NULL UNIQUE COMMENT '礼包码',
    `BatchNo` VARCHAR(32) NOT NULL COMMENT '批次号',
    `Name` VARCHAR(50) NOT NULL COMMENT '礼包名称',
    `Description` VARCHAR(200) DEFAULT NULL COMMENT '描述',
    `GameGold` INT DEFAULT 0 COMMENT '赠送元宝',
    `GamePoint` INT DEFAULT 0 COMMENT '赠送点数',
    `Items` TEXT DEFAULT NULL COMMENT '赠送物品(JSON:[{name,count}])',
    `MaxUseCount` INT DEFAULT 1 COMMENT '最大使用次数',
    `UsedCount` INT DEFAULT 0 COMMENT '已使用次数',
    `LimitOnePerAccount` TINYINT(1) DEFAULT 1 COMMENT '每账号限用一次',
    `RequireLevel` INT DEFAULT 0 COMMENT '使用等级限制',
    `StartTime` DATETIME DEFAULT NULL COMMENT '生效时间',
    `EndTime` DATETIME DEFAULT NULL COMMENT '失效时间',
    `IsEnabled` TINYINT(1) DEFAULT 1 COMMENT '是否启用',
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    INDEX `idx_code` (`Code`),
    INDEX `idx_batch` (`BatchNo`),
    INDEX `idx_enabled` (`IsEnabled`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='礼包码表';

-- 礼包码使用记录
CREATE TABLE IF NOT EXISTS `gift_code_records` (
    `Id` BIGINT AUTO_INCREMENT PRIMARY KEY,
    `CodeId` BIGINT NOT NULL COMMENT '礼包码ID',
    `Code` VARCHAR(32) NOT NULL COMMENT '礼包码',
    `AccountId` VARCHAR(50) NOT NULL COMMENT '账号',
    `CharName` VARCHAR(50) NOT NULL COMMENT '角色名',
    `ClientIp` VARCHAR(50) DEFAULT NULL COMMENT '客户端IP',
    `UseTime` DATETIME DEFAULT CURRENT_TIMESTAMP COMMENT '使用时间',
    INDEX `idx_code` (`CodeId`),
    INDEX `idx_account` (`AccountId`),
    INDEX `idx_char` (`CharName`),
    UNIQUE KEY `uk_code_account` (`Code`, `AccountId`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='礼包码使用记录';

-- VIP等级配置表
CREATE TABLE IF NOT EXISTS `vip_levels` (
    `Level` INT PRIMARY KEY COMMENT 'VIP等级',
    `Name` VARCHAR(50) NOT NULL COMMENT '等级名称',
    `RequireRecharge` INT NOT NULL COMMENT '累计充值要求(元宝)',
    `ExpBonus` INT DEFAULT 0 COMMENT '经验加成(%)',
    `DropBonus` INT DEFAULT 0 COMMENT '掉落加成(%)',
    `ShopDiscount` INT DEFAULT 100 COMMENT '商城折扣(%)',
    `DailyGift` VARCHAR(200) DEFAULT NULL COMMENT '每日礼包(JSON)',
    `Privileges` TEXT DEFAULT NULL COMMENT '特权描述',
    `Icon` VARCHAR(100) DEFAULT NULL COMMENT 'VIP图标'
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='VIP等级配置';

-- 用户VIP信息表
CREATE TABLE IF NOT EXISTS `user_vip` (
    `AccountId` VARCHAR(50) PRIMARY KEY COMMENT '账号',
    `VipLevel` INT DEFAULT 0 COMMENT 'VIP等级',
    `TotalRecharge` INT DEFAULT 0 COMMENT '累计充值(元宝)',
    `LastDailyGiftTime` DATE DEFAULT NULL COMMENT '上次领取每日礼包时间',
    `CreateTime` DATETIME DEFAULT CURRENT_TIMESTAMP,
    `UpdateTime` DATETIME DEFAULT CURRENT_TIMESTAMP ON UPDATE CURRENT_TIMESTAMP
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='用户VIP信息';

-- ========================================
-- 初始化数据
-- ========================================

-- 商品分类
INSERT INTO `shop_categories` (`Id`, `Name`, `SortOrder`, `IsEnabled`) VALUES
(1, '热销推荐', 1, 1),
(2, '武器装备', 2, 1),
(3, '消耗道具', 3, 1),
(4, '材料宝石', 4, 1),
(5, '特殊物品', 5, 1),
(6, '限时特惠', 6, 1);

-- 充值档位
INSERT INTO `recharge_packages` (`Name`, `Amount`, `GameGold`, `BonusGold`, `Description`, `SortOrder`, `IsHot`) VALUES
('小额充值', 6.00, 60, 6, '首充额外赠送6元宝', 1, 0),
('月卡推荐', 30.00, 300, 50, '首充额外赠送50元宝', 2, 1),
('季卡超值', 98.00, 980, 200, '首充额外赠送200元宝', 3, 0),
('半年豪华', 198.00, 1980, 500, '首充额外赠送500元宝', 4, 0),
('年度至尊', 328.00, 3280, 1000, '首充额外赠送1000元宝', 5, 1),
('土豪专属', 648.00, 6480, 2500, '首充额外赠送2500元宝', 6, 0);

-- VIP等级配置
INSERT INTO `vip_levels` (`Level`, `Name`, `RequireRecharge`, `ExpBonus`, `DropBonus`, `ShopDiscount`) VALUES
(0, '普通玩家', 0, 0, 0, 100),
(1, 'VIP1', 100, 5, 2, 98),
(2, 'VIP2', 500, 10, 5, 95),
(3, 'VIP3', 1000, 15, 8, 92),
(4, 'VIP4', 3000, 20, 10, 90),
(5, 'VIP5', 5000, 25, 12, 88),
(6, 'VIP6', 10000, 30, 15, 85),
(7, 'VIP7', 20000, 35, 18, 82),
(8, 'VIP8', 50000, 40, 20, 80),
(9, 'VIP9', 100000, 50, 25, 75),
(10, 'VIP10至尊', 200000, 60, 30, 70);

-- 示例商品数据
INSERT INTO `shop_items` (`CategoryId`, `ItemName`, `DisplayName`, `Description`, `Price`, `OriginalPrice`, `ItemCount`, `IsHot`, `IsNew`, `SortOrder`) VALUES
-- 热销推荐
(1, '金创药(大量)', '金创药大礼包', '包含100瓶金创药，战斗必备', 50, 80, 100, 1, 0, 1),
(1, '魔法药(大量)', '魔法药大礼包', '包含100瓶魔法药，法师必备', 50, 80, 100, 1, 0, 2),
(1, '超级金创药', '超级金创药x50', '高级回复药品', 100, 150, 50, 1, 1, 3),

-- 武器装备
(2, '裁决之杖', '裁决之杖', '战士终极武器，攻击+35', 5000, 0, 1, 1, 0, 1),
(2, '龙纹剑', '龙纹剑', '道士极品武器，道术+10', 4500, 0, 1, 0, 0, 2),
(2, '骨玉权杖', '骨玉权杖', '法师顶级法杖，魔法+12', 4800, 0, 1, 0, 0, 3),
(2, '烈焰龙甲(男)', '烈焰龙甲', '龙系顶级战甲，自带火焰翅膀特效', 8000, 10000, 1, 1, 1, 4),
(2, '龙皇神甲(男)', '龙皇神甲', '至尊龙甲，金龙环绕特效', 12000, 15000, 1, 1, 1, 5),

-- 消耗道具
(3, '随机传送卷', '随机传送卷x10', '随机传送到安全区域', 30, 0, 10, 0, 0, 1),
(3, '回城卷', '回城卷x20', '快速返回安全区', 20, 0, 20, 0, 0, 2),
(3, '祝福油', '祝福油x5', '增加武器幸运值', 200, 0, 5, 1, 0, 3),
(3, '强效太阳水', '强效太阳水x30', '快速恢复生命和魔法', 80, 100, 30, 0, 0, 4),

-- 材料宝石
(4, '黑铁矿石', '高纯度黑铁矿x10', '武器升级必备材料', 100, 0, 10, 0, 0, 1),
(4, '金刚石', '金刚石x5', '高级锻造材料', 300, 0, 5, 0, 0, 2),
(4, '记忆项链', '记忆项链', '可设置回城点', 500, 0, 1, 0, 0, 3),

-- 特殊物品
(5, '技能书礼包', '随机技能书礼包', '随机获得一本高级技能书', 1000, 0, 1, 1, 0, 1),
(5, '双倍经验卡', '双倍经验卡(24小时)', '24小时内经验获取翻倍', 200, 0, 1, 1, 0, 2),
(5, '仓库扩展', '仓库扩展+10格', '永久增加10格仓库空间', 500, 0, 1, 0, 0, 3);

-- 示例礼包码
INSERT INTO `gift_codes` (`Code`, `BatchNo`, `Name`, `GameGold`, `Items`, `MaxUseCount`, `EndTime`) VALUES
('WELCOME2024', 'BATCH001', '新手礼包', 100, '[{"name":"金创药(大量)","count":50},{"name":"回城卷","count":10}]', 10000, '2027-12-31 23:59:59'),
('VIP666', 'BATCH002', 'VIP专属礼包', 500, '[{"name":"祝福油","count":3}]', 1000, '2027-12-31 23:59:59'),
('DRAGON888', 'BATCH003', '龙年礼包', 888, '[{"name":"金刚石","count":2}]', 5000, '2027-12-31 23:59:59');
