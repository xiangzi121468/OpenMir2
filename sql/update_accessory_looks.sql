-- ============================================
-- 更新配饰装备的Looks字段
-- 使其使用AccessoryItems.data资源
-- 客户端索引范围: 50000-60000
-- ============================================

-- 说明:
-- 客户端通过 Looks 字段加载物品图标
-- 50000-60000 范围对应 AccessoryItems.data
-- 实际索引 = Looks - 50000

-- ============================================
-- 戒指系列 (索引 50-63, Looks = 50050-50063)
-- ============================================

UPDATE `stditems` SET `Looks` = 50050 WHERE `Name` = '冰晶戒指';
UPDATE `stditems` SET `Looks` = 50051 WHERE `Name` = '霜魄戒指';
UPDATE `stditems` SET `Looks` = 50052 WHERE `Name` = '凝霜戒指';
UPDATE `stditems` SET `Looks` = 50053 WHERE `Name` = '夜行戒指';
UPDATE `stditems` SET `Looks` = 50054 WHERE `Name` = '暗影戒指';
UPDATE `stditems` SET `Looks` = 50055 WHERE `Name` = '亡灵戒指';
UPDATE `stditems` SET `Looks` = 50056 WHERE `Name` = '噬魂戒指';
UPDATE `stditems` SET `Looks` = 50057 WHERE `Name` = '骸骨戒指';
UPDATE `stditems` SET `Looks` = 50058 WHERE `Name` = '血魄戒指';
UPDATE `stditems` SET `Looks` = 50059 WHERE `Name` = '血蝠戒指';
UPDATE `stditems` SET `Looks` = 50060 WHERE `Name` = '龙炎戒指';
UPDATE `stditems` SET `Looks` = 50061 WHERE `Name` = '龙魂戒指';
UPDATE `stditems` SET `Looks` = 50062 WHERE `Name` = '龙威戒指';
UPDATE `stditems` SET `Looks` = 50063 WHERE `Name` = '黏液戒指';

-- ============================================
-- 靴子系列 (索引 200-211, Looks = 50200-50211)
-- ============================================

UPDATE `stditems` SET `Looks` = 50200 WHERE `Name` = '寒冰之靴';
UPDATE `stditems` SET `Looks` = 50201 WHERE `Name` = '冰魄之靴';
UPDATE `stditems` SET `Looks` = 50202 WHERE `Name` = '凝霜之靴';
UPDATE `stditems` SET `Looks` = 50203 WHERE `Name` = '暗夜之靴';
UPDATE `stditems` SET `Looks` = 50204 WHERE `Name` = '亡灵之靴';
UPDATE `stditems` SET `Looks` = 50205 WHERE `Name` = '噬魂之靴';
UPDATE `stditems` SET `Looks` = 50206 WHERE `Name` = '骸骨之靴';
UPDATE `stditems` SET `Looks` = 50207 WHERE `Name` = '血影之靴';
UPDATE `stditems` SET `Looks` = 50208 WHERE `Name` = '龙炎之靴';
UPDATE `stditems` SET `Looks` = 50209 WHERE `Name` = '龙魂之靴';
UPDATE `stditems` SET `Looks` = 50210 WHERE `Name` = '龙威之靴';
UPDATE `stditems` SET `Looks` = 50211 WHERE `Name` = '黏液之靴';

-- ============================================
-- 腰带系列 (索引 250-261, Looks = 50250-50261)
-- ============================================

UPDATE `stditems` SET `Looks` = 50250 WHERE `Name` = '寒冰腰带';
UPDATE `stditems` SET `Looks` = 50251 WHERE `Name` = '冰魄腰带';
UPDATE `stditems` SET `Looks` = 50252 WHERE `Name` = '凝霜腰带';
UPDATE `stditems` SET `Looks` = 50253 WHERE `Name` = '暗影腰带';
UPDATE `stditems` SET `Looks` = 50254 WHERE `Name` = '亡灵腰带';
UPDATE `stditems` SET `Looks` = 50255 WHERE `Name` = '噬魂腰带';
UPDATE `stditems` SET `Looks` = 50256 WHERE `Name` = '骸骨腰带';
UPDATE `stditems` SET `Looks` = 50257 WHERE `Name` = '血蝠腰带';
UPDATE `stditems` SET `Looks` = 50258 WHERE `Name` = '龙炎腰带';
UPDATE `stditems` SET `Looks` = 50259 WHERE `Name` = '龙魂腰带';
UPDATE `stditems` SET `Looks` = 50260 WHERE `Name` = '龙威腰带';
UPDATE `stditems` SET `Looks` = 50261 WHERE `Name` = '黏液腰带';

-- ============================================
-- 验证更新结果
-- ============================================
-- SELECT `Idx`, `Name`, `StdMode`, `Shape`, `Looks` 
-- FROM `stditems` 
-- WHERE `Looks` >= 50000 AND `Looks` < 60000
-- ORDER BY `Looks`;
