-- OpenMir2 数据库验证脚本
-- 用于检查数据库表是否创建成功

-- ============================================
-- 1. 数据库统计
-- ============================================
SELECT '=== 数据库表统计 ===' AS info;

SELECT 
    table_schema AS '数据库',
    COUNT(*) AS '表数量'
FROM information_schema.tables 
WHERE table_schema IN ('mir2_account', 'mir2_data', 'mir2_db')
GROUP BY table_schema;

-- ============================================
-- 2. 核心表检查
-- ============================================
SELECT '=== 核心表检查 ===' AS info;

SELECT 
    'mir2_account.account' AS '表名',
    IF(EXISTS(SELECT 1 FROM information_schema.tables WHERE table_schema='mir2_account' AND table_name='account'), 'OK', 'MISSING') AS '状态';

SELECT 
    'mir2_data.stditems' AS '表名',
    IF(EXISTS(SELECT 1 FROM information_schema.tables WHERE table_schema='mir2_data' AND table_name='stditems'), 'OK', 'MISSING') AS '状态';

SELECT 
    'mir2_data.magics' AS '表名',
    IF(EXISTS(SELECT 1 FROM information_schema.tables WHERE table_schema='mir2_data' AND table_name='magics'), 'OK', 'MISSING') AS '状态';

SELECT 
    'mir2_data.monsters' AS '表名',
    IF(EXISTS(SELECT 1 FROM information_schema.tables WHERE table_schema='mir2_data' AND table_name='monsters'), 'OK', 'MISSING') AS '状态';

SELECT 
    'mir2_db.characters' AS '表名',
    IF(EXISTS(SELECT 1 FROM information_schema.tables WHERE table_schema='mir2_db' AND table_name='characters'), 'OK', 'MISSING') AS '状态';

-- ============================================
-- 3. 功能模块表检查
-- ============================================
SELECT '=== 功能模块表检查 ===' AS info;

SELECT 
    table_name AS '表名',
    'OK' AS '状态'
FROM information_schema.tables 
WHERE table_schema = 'mir2_db' 
AND table_name IN (
    'shop_categories', 'shop_items', 'shop_orders',
    'recharge_records', 'recharge_packages',
    'game_mails', 'player_mails', 'mail_templates',
    'game_notices', 'notice_send_logs',
    'game_activities', 'activity_logs', 'activity_rewards',
    'castles', 'castle_war_records', 'castle_war_kills',
    'appraisal_attributes', 'appraisal_logs',
    'vip_maps', 'vip_map_monsters',
    'market_items', 'market_transactions',
    'player_login_logs', 'player_item_logs',
    'admin_users', 'admin_logs'
)
ORDER BY table_name;

-- ============================================
-- 4. 数据统计
-- ============================================
SELECT '=== 数据统计 ===' AS info;

SELECT 'stditems (物品)' AS '表', COUNT(*) AS '记录数' FROM mir2_data.stditems;
SELECT 'magics (技能)' AS '表', COUNT(*) AS '记录数' FROM mir2_data.magics;
SELECT 'monsters (怪物)' AS '表', COUNT(*) AS '记录数' FROM mir2_data.monsters;

-- ============================================
-- 5. 索引检查
-- ============================================
SELECT '=== 索引检查 ===' AS info;

SELECT 
    table_name AS '表',
    index_name AS '索引',
    column_name AS '列'
FROM information_schema.statistics 
WHERE table_schema = 'mir2_db' 
AND table_name = 'characters'
AND index_name != 'PRIMARY'
ORDER BY index_name;

-- ============================================
-- 验证完成
-- ============================================
SELECT '=== 验证完成 ===' AS info;
