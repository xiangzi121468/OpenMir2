-- ============================================
-- 完整技能书系统
-- OpenMir2 Skill Books System
-- ============================================

-- StdMode = 4 表示技能书
-- Shape = 技能ID (对应magics表的MagicId)
-- NeedLevel = 学习等级要求
-- Price = 价格

-- ============================================
-- 战士技能书 (Job = 0)
-- ============================================

-- 基本剑术 (7级) - 名称需与magics表匹配
INSERT INTO `stditems` VALUES (1001, '基本剑术', 4, 1, 1, 0, 0, 0, 1001, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 7, 500, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 攻杀剑术 (19级)
INSERT INTO `stditems` VALUES (1002, '攻杀剑术', 4, 7, 1, 0, 0, 0, 1002, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 19, 2000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 刺杀剑术 (25级)
INSERT INTO `stditems` VALUES (1003, '刺杀剑术', 4, 12, 1, 0, 0, 0, 1003, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 25, 5000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 半月弯刀 (28级)
INSERT INTO `stditems` VALUES (1004, '半月弯刀', 4, 25, 1, 0, 0, 0, 1004, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 28, 8000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 野蛮冲撞 (30级)
INSERT INTO `stditems` VALUES (1005, '野蛮冲撞', 4, 27, 1, 0, 0, 0, 1005, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 30, 15000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 烈火剑法 (35级)
INSERT INTO `stditems` VALUES (1006, '烈火剑法', 4, 26, 1, 0, 0, 0, 1006, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 35, 50000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 狮子吼 (38级)
INSERT INTO `stditems` VALUES (1007, '狮子吼', 4, 41, 1, 0, 0, 0, 1007, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 38, 80000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 开天斩 (37级)
INSERT INTO `stditems` VALUES (1008, '开天斩', 4, 43, 1, 0, 0, 0, 1008, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 37, 100000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 龙影剑法 (38级)
INSERT INTO `stditems` VALUES (1009, '龙影剑法', 4, 42, 1, 0, 0, 0, 1009, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 38, 120000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 逐日剑法 (46级)
INSERT INTO `stditems` VALUES (1010, '逐日剑法', 4, 74, 1, 0, 0, 0, 1010, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 46, 200000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 护体神盾 (39级)
INSERT INTO `stditems` VALUES (1011, '护体神盾', 4, 75, 1, 0, 0, 0, 1011, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 39, 150000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- ============================================
-- 法师技能书 (Job = 1)
-- ============================================

-- 火球术 (7级)
INSERT INTO `stditems` VALUES (1020, '火球术', 4, 1, 1, 0, 0, 0, 1020, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 7, 500, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 抗拒火环 (12级)
INSERT INTO `stditems` VALUES (1021, '抗拒火环', 4, 8, 1, 0, 0, 0, 1021, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 12, 1000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 诱惑之光 (13级)
INSERT INTO `stditems` VALUES (1022, '诱惑之光', 4, 9, 1, 0, 0, 0, 1022, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 13, 1500, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 地狱火 (16级)
INSERT INTO `stditems` VALUES (1023, '地狱火', 4, 10, 1, 0, 0, 0, 1023, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 16, 3000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 雷电术 (17级)
INSERT INTO `stditems` VALUES (1024, '雷电术', 4, 11, 1, 0, 0, 0, 1024, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 17, 5000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 瞬息移动 (19级)
INSERT INTO `stditems` VALUES (1025, '瞬息移动', 4, 21, 1, 0, 0, 0, 1025, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 19, 8000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 火墙 (24级)
INSERT INTO `stditems` VALUES (1026, '火墙', 4, 22, 1, 0, 0, 0, 1026, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 24, 15000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 爆裂火焰 (22级)
INSERT INTO `stditems` VALUES (1027, '爆裂火焰', 4, 23, 1, 0, 0, 0, 1027, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 22, 12000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 地狱雷光 (30级)
INSERT INTO `stditems` VALUES (1028, '地狱雷光', 4, 24, 1, 0, 0, 0, 1028, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 30, 30000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 魔法盾 (31级)
INSERT INTO `stditems` VALUES (1029, '魔法盾', 4, 31, 1, 0, 0, 0, 1029, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 31, 50000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 圣言术 (32级)
INSERT INTO `stditems` VALUES (1030, '圣言术', 4, 32, 1, 0, 0, 0, 1030, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 32, 60000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 冰咆哮 (35级)
INSERT INTO `stditems` VALUES (1031, '冰咆哮', 4, 33, 1, 0, 0, 0, 1031, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 35, 80000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 寒冰掌 (36级)
INSERT INTO `stditems` VALUES (1032, '寒冰掌', 4, 44, 1, 0, 0, 0, 1032, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 36, 100000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 灭天火 (38级)
INSERT INTO `stditems` VALUES (1033, '灭天火', 4, 45, 1, 0, 0, 0, 1033, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 38, 150000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 地狱烈焰 (46级)
INSERT INTO `stditems` VALUES (1034, '地狱烈焰', 4, 47, 1, 0, 0, 0, 1034, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 46, 200000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 流星火雨 (46级)
INSERT INTO `stditems` VALUES (1035, '流星火雨', 4, 58, 1, 0, 0, 0, 1035, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 46, 250000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 四级魔法盾 (40级)
INSERT INTO `stditems` VALUES (1036, '四级魔法盾', 4, 66, 1, 0, 0, 0, 1036, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 40, 180000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- ============================================
-- 道士技能书 (Job = 2)
-- ============================================

-- 治愈术 (7级)
INSERT INTO `stditems` VALUES (1040, '治愈术', 4, 2, 1, 0, 0, 0, 1040, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 7, 500, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 精神力战法 (9级)
INSERT INTO `stditems` VALUES (1041, '精神力战法', 4, 3, 1, 0, 0, 0, 1041, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 9, 800, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 施毒术 (14级)
INSERT INTO `stditems` VALUES (1042, '施毒术', 4, 4, 1, 0, 0, 0, 1042, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 14, 2000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 灵魂火符 (18级)
INSERT INTO `stditems` VALUES (1043, '灵魂火符', 4, 5, 1, 0, 0, 0, 1043, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 18, 5000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 召唤骷髅 (19级)
INSERT INTO `stditems` VALUES (1044, '召唤骷髅', 4, 6, 1, 0, 0, 0, 1044, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 19, 8000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 隐身术 (20级)
INSERT INTO `stditems` VALUES (1045, '隐身术', 4, 13, 1, 0, 0, 0, 1045, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 20, 10000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 集体隐身术 (21级)
INSERT INTO `stditems` VALUES (1046, '集体隐身术', 4, 14, 1, 0, 0, 0, 1046, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 21, 12000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 幽灵盾 (22级)
INSERT INTO `stditems` VALUES (1047, '幽灵盾', 4, 15, 1, 0, 0, 0, 1047, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 22, 15000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 神圣战甲术 (25级)
INSERT INTO `stditems` VALUES (1048, '神圣战甲术', 4, 16, 1, 0, 0, 0, 1048, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 25, 20000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 困魔咒 (28级)
INSERT INTO `stditems` VALUES (1049, '困魔咒', 4, 17, 1, 0, 0, 0, 1049, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 28, 30000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 群体治疗术 (33级)
INSERT INTO `stditems` VALUES (1050, '群体治疗术', 4, 29, 1, 0, 0, 0, 1050, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 33, 50000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 召唤神兽 (35级)
INSERT INTO `stditems` VALUES (1051, '召唤神兽', 4, 30, 1, 0, 0, 0, 1051, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 35, 80000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 心灵启示 (26级)
INSERT INTO `stditems` VALUES (1052, '心灵启示', 4, 28, 1, 0, 0, 0, 1052, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 26, 25000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 气功波 (36级)
INSERT INTO `stditems` VALUES (1053, '气功波', 4, 48, 1, 0, 0, 0, 1053, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 36, 100000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 无极真气 (38级)
INSERT INTO `stditems` VALUES (1054, '无极真气', 4, 50, 1, 0, 0, 0, 1054, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 38, 120000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 噬血术 (46级)
INSERT INTO `stditems` VALUES (1055, '噬血术', 4, 59, 1, 0, 0, 0, 1055, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 46, 180000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 召唤月灵 (45级)
INSERT INTO `stditems` VALUES (1056, '召唤月灵', 4, 72, 1, 0, 0, 0, 1056, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 45, 200000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 四级召唤神兽 (50级)
INSERT INTO `stditems` VALUES (1057, '四级召唤神兽', 4, 71, 1, 0, 0, 0, 1057, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 50, 250000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- ============================================
-- 新增特殊技能书 (VIP地图BOSS掉落)
-- ============================================

-- 冰霜领主掉落
INSERT INTO `stditems` VALUES (1060, '冰霜护甲', 4, 100, 1, 0, 0, 0, 1060, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 45, 300000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `stditems` VALUES (1061, '冰霜冲击', 4, 101, 1, 0, 0, 0, 1061, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 48, 350000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 死灵召唤师掉落 (已有骷髅召唤书 ID=927)
INSERT INTO `stditems` VALUES (1062, '亡灵召唤术', 4, 102, 1, 0, 0, 0, 1062, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 50, 400000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `stditems` VALUES (1063, '亡灵诅咒', 4, 103, 1, 0, 0, 0, 1063, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 48, 350000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 烈焰魔龙掉落
INSERT INTO `stditems` VALUES (1064, '龙炎术', 4, 104, 1, 0, 0, 0, 1064, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 52, 500000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `stditems` VALUES (1065, '龙魂附体', 4, 105, 1, 0, 0, 0, 1065, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 55, 600000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `stditems` VALUES (1066, '龙之怒吼', 4, 106, 1, 0, 0, 0, 1066, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 55, 600000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 暗影刺客掉落
INSERT INTO `stditems` VALUES (1067, '暗影突袭', 4, 107, 1, 0, 0, 0, 1067, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 40, 200000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `stditems` VALUES (1068, '致命一击', 4, 108, 1, 0, 0, 0, 1068, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 42, 250000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- ============================================
-- 新增魔法技能定义 (需要添加到magics表)
-- ============================================

-- 新增技能定义 (ID从100开始)
INSERT INTO `magics` VALUES (100, 100, '冰霜护甲', 4, 80, 20, 0, 0, 30, 0, 0, 1, 45, 200, 48, 300, 50, 500, 60, '提升魔法防御，有概率冻结攻击者');
INSERT INTO `magics` VALUES (101, 101, '冰霜冲击', 0, 81, 25, 15, 25, 15, 10, 15, 0, 48, 200, 50, 300, 52, 500, 30, '向前冲击并冻结范围内敌人');
INSERT INTO `magics` VALUES (102, 102, '亡灵召唤术', 4, 82, 30, 0, 0, 40, 0, 0, 2, 50, 200, 52, 300, 55, 500, 180, '召唤强力亡灵战士');
INSERT INTO `magics` VALUES (103, 103, '亡灵诅咒', 2, 83, 20, 5, 10, 25, 5, 10, 2, 48, 200, 50, 300, 52, 500, 60, '诅咒敌人，持续掉血');
INSERT INTO `magics` VALUES (104, 104, '龙炎术', 14, 84, 40, 30, 50, 25, 20, 30, 1, 52, 200, 54, 300, 56, 500, 60, '释放龙炎，造成大范围火焰伤害');
INSERT INTO `magics` VALUES (105, 105, '龙魂附体', 4, 85, 50, 0, 0, 60, 0, 0, 0, 55, 200, 57, 300, 60, 500, 120, '附体龙魂，大幅提升攻击力');
INSERT INTO `magics` VALUES (106, 106, '龙之怒吼', 7, 86, 35, 20, 40, 30, 15, 25, 2, 55, 200, 57, 300, 60, 500, 90, '龙之怒吼，震慑范围内敌人');
INSERT INTO `magics` VALUES (107, 107, '暗影突袭', 0, 87, 15, 0, 0, 10, 0, 0, 0, 40, 200, 42, 300, 45, 500, 20, '快速突进并攻击，有概率暴击');
INSERT INTO `magics` VALUES (108, 108, '致命一击', 0, 88, 20, 0, 0, 15, 0, 0, 0, 42, 200, 44, 300, 46, 500, 30, '积蓄力量，下次攻击造成三倍伤害');

-- ============================================
-- 技能书掉落配置示例 (MonItems格式)
-- ============================================

/*
=== 冰霜领主.txt ===
1/30 冰霜护甲
1/35 冰霜冲击

=== 死灵召唤师.txt ===
1/50 骷髅召唤书
1/30 亡灵召唤术
1/35 亡灵诅咒

=== 烈焰魔龙.txt ===
1/25 龙炎术
1/30 龙魂附体
1/30 龙之怒吼

=== 暗影刺客.txt ===
1/20 暗影突袭
1/25 致命一击
*/

-- ============================================
-- 技能书统计
-- ============================================
-- 战士技能书: 11本 (基础剑术到护体神盾)
-- 法师技能书: 17本 (火球术到四级魔法盾)  
-- 道士技能书: 18本 (治愈术到四级召唤神兽)
-- 特殊技能书: 9本 (VIP BOSS掉落)
-- 总计: 55本技能书
-- ============================================
