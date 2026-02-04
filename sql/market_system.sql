-- =============================================
-- 寄售行系统数据库表
-- =============================================

-- 寄售物品表
CREATE TABLE IF NOT EXISTS `market_items` (
    `id` BIGINT PRIMARY KEY AUTO_INCREMENT COMMENT '记录ID',
    `seller_account` VARCHAR(50) NOT NULL COMMENT '卖家账号',
    `seller_name` VARCHAR(50) NOT NULL COMMENT '卖家角色名',
    `item_name` VARCHAR(100) NOT NULL COMMENT '物品名称',
    `item_type` INT NOT NULL DEFAULT 0 COMMENT '物品类型(1武器2衣服3首饰4头盔5药品6材料7其他)',
    `item_idx` INT NOT NULL DEFAULT 0 COMMENT '物品索引',
    `item_data` TEXT NULL COMMENT '物品数据(JSON序列化)',
    `price` BIGINT NOT NULL DEFAULT 0 COMMENT '售价(元宝)',
    `status` TINYINT NOT NULL DEFAULT 0 COMMENT '状态(0在售1已售2已取消3已过期)',
    `buyer_account` VARCHAR(50) NULL COMMENT '买家账号',
    `buyer_name` VARCHAR(50) NULL COMMENT '买家角色名',
    `create_time` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '上架时间',
    `expire_time` DATETIME NULL COMMENT '过期时间',
    `buy_time` DATETIME NULL COMMENT '购买时间',
    `cancel_time` DATETIME NULL COMMENT '取消时间',
    INDEX `idx_seller` (`seller_account`),
    INDEX `idx_status` (`status`),
    INDEX `idx_item_type` (`item_type`),
    INDEX `idx_item_name` (`item_name`),
    INDEX `idx_create_time` (`create_time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='寄售物品表';

-- 交易记录表
CREATE TABLE IF NOT EXISTS `market_transactions` (
    `id` BIGINT PRIMARY KEY AUTO_INCREMENT COMMENT '记录ID',
    `market_item_id` BIGINT NOT NULL COMMENT '寄售物品ID',
    `seller_account` VARCHAR(50) NOT NULL COMMENT '卖家账号',
    `seller_name` VARCHAR(50) NOT NULL COMMENT '卖家角色名',
    `buyer_account` VARCHAR(50) NOT NULL COMMENT '买家账号',
    `buyer_name` VARCHAR(50) NULL COMMENT '买家角色名',
    `item_name` VARCHAR(100) NOT NULL COMMENT '物品名称',
    `price` BIGINT NOT NULL COMMENT '成交价格',
    `fee` BIGINT NOT NULL DEFAULT 0 COMMENT '手续费',
    `create_time` DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP COMMENT '交易时间',
    INDEX `idx_seller` (`seller_account`),
    INDEX `idx_buyer` (`buyer_account`),
    INDEX `idx_time` (`create_time`)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COMMENT='交易记录表';
