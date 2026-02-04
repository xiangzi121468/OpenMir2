-- ========================================
-- 新增怪物配套武器和装备
-- 与新增怪物掉落配置对应
-- ========================================

-- 物品ID从900开始，避免与现有物品冲突

-- ========================================
-- 冰霜系武器和装备 (冰霜领主掉落)
-- ========================================

-- 冰霜之刃 - 战士武器
INSERT INTO `stditems` VALUES (900, '冰霜之刃', 5, 38, 55, 0, 0, 0, 900, 35000, 0, 3, 0, 2, 10, 32, 0, 0, 0, 0, 0, 40, 80000, 5, 2, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 冰魄 - 法师武器
INSERT INTO `stditems` VALUES (901, '冰魄', 5, 39, 25, 0, 0, 0, 901, 28000, 0, 3, 0, 2, 6, 14, 6, 18, 0, 0, 0, 38, 75000, 5, 0, 0, 0, 2, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 凝霜 - 道士武器
INSERT INTO `stditems` VALUES (902, '凝霜', 5, 40, 30, 0, 0, 0, 902, 26000, 0, 2, 0, 2, 8, 16, 0, 0, 4, 12, 0, 38, 70000, 5, 0, 0, 0, 0, 2, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 寒冰护甲(男) - 战士盔甲
INSERT INTO `stditems` VALUES (903, '寒冰护甲(男)', 10, 38, 30, 0, 1, 0, 903, 20000, 8, 15, 4, 8, 0, 0, 0, 0, 0, 0, 0, 40, 60000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 寒冰护甲(女) - 战士盔甲
INSERT INTO `stditems` VALUES (904, '寒冰护甲(女)', 11, 38, 28, 0, 1, 0, 904, 20000, 7, 14, 4, 9, 0, 0, 0, 0, 0, 0, 0, 40, 60000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 霜寒戒指
INSERT INTO `stditems` VALUES (905, '霜寒戒指', 22, 38, 2, 0, 0, 0, 905, 8000, 0, 2, 1, 3, 2, 5, 1, 3, 1, 3, 0, 35, 40000, 5, 0, 0, 0, 1, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 冰魄项链
INSERT INTO `stditems` VALUES (906, '冰魄项链', 19, 38, 3, 0, 0, 0, 906, 10000, 0, 3, 2, 4, 0, 0, 2, 5, 2, 4, 0, 35, 45000, 5, 0, 0, 1, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 冰晶手镯
INSERT INTO `stditems` VALUES (907, '冰晶手镯', 24, 38, 2, 0, 0, 0, 907, 8000, 0, 2, 1, 3, 1, 3, 1, 4, 1, 3, 0, 35, 38000, 5, 0, 0, 0, 0, 1, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- ========================================
-- 暗影系武器和装备 (暗影刺客掉落)
-- ========================================

-- 暗影匕首 - 刺客型武器
INSERT INTO `stditems` VALUES (910, '暗影匕首', 5, 41, 15, 0, 0, 0, 910, 18000, 0, 1, 0, 0, 8, 18, 0, 0, 2, 6, 0, 28, 35000, 5, 3, 0, 2, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 隐身戒指 - 特殊效果
INSERT INTO `stditems` VALUES (911, '隐身戒指', 22, 41, 1, 0, 0, 0, 911, 6000, 0, 1, 0, 2, 1, 4, 0, 0, 0, 3, 0, 25, 30000, 5, 1, 1, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 夜行护甲(男)
INSERT INTO `stditems` VALUES (912, '夜行护甲(男)', 10, 41, 20, 0, 1, 0, 912, 15000, 5, 10, 3, 7, 0, 0, 0, 0, 0, 0, 0, 30, 40000, 5, 1, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 夜行护甲(女)
INSERT INTO `stditems` VALUES (913, '夜行护甲(女)', 11, 41, 18, 0, 1, 0, 913, 15000, 4, 9, 3, 8, 0, 0, 0, 0, 0, 0, 0, 30, 40000, 5, 1, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 幽灵项链
INSERT INTO `stditems` VALUES (914, '幽灵项链', 19, 41, 2, 0, 0, 0, 914, 8000, 0, 1, 1, 3, 1, 3, 0, 0, 1, 4, 0, 28, 28000, 5, 0, 1, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 暗影手镯
INSERT INTO `stditems` VALUES (915, '暗影手镯', 24, 41, 1, 0, 0, 0, 915, 6000, 0, 1, 0, 2, 2, 4, 0, 0, 0, 2, 0, 26, 25000, 5, 1, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- ========================================
-- 亡灵系武器和装备 (死灵召唤师掉落)
-- ========================================

-- 亡灵法杖 - 法师/道士武器
INSERT INTO `stditems` VALUES (920, '亡灵法杖', 6, 42, 28, 0, 0, 0, 920, 25000, 0, 2, 0, 3, 5, 12, 4, 14, 3, 10, 0, 38, 65000, 5, 0, 0, 0, 0, 2, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 骷髅头盔
INSERT INTO `stditems` VALUES (921, '骷髅头盔', 15, 42, 8, 0, 0, 0, 921, 12000, 3, 6, 2, 5, 0, 0, 0, 0, 0, 0, 0, 32, 35000, 5, 0, 0, 0, 0, 0, 1, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 骷髅戒指
INSERT INTO `stditems` VALUES (922, '骷髅戒指', 22, 42, 2, 0, 0, 0, 922, 7000, 0, 1, 0, 2, 1, 4, 1, 3, 1, 4, 0, 30, 30000, 5, 0, 0, 0, 0, 0, 1, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 亡灵护甲(男)
INSERT INTO `stditems` VALUES (923, '亡灵护甲(男)', 10, 42, 25, 0, 1, 0, 923, 18000, 6, 12, 4, 8, 0, 0, 0, 0, 0, 0, 0, 36, 50000, 5, 0, 0, 0, 0, 0, 1, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 亡灵护甲(女)
INSERT INTO `stditems` VALUES (924, '亡灵护甲(女)', 11, 42, 23, 0, 1, 0, 924, 18000, 5, 11, 4, 9, 0, 0, 0, 0, 0, 0, 0, 36, 50000, 5, 0, 0, 0, 0, 0, 1, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 幽魂项链
INSERT INTO `stditems` VALUES (925, '幽魂项链', 19, 42, 3, 0, 0, 0, 925, 9000, 0, 2, 1, 4, 0, 0, 1, 4, 2, 5, 0, 34, 38000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 噬魂手镯
INSERT INTO `stditems` VALUES (926, '噬魂手镯', 24, 42, 2, 0, 0, 0, 926, 7000, 0, 1, 0, 3, 0, 2, 2, 4, 1, 3, 0, 32, 32000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 骷髅召唤书 - 技能书
INSERT INTO `stditems` VALUES (927, '骷髅召唤书', 4, 1, 2, 0, 0, 0, 927, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 2, 20, 50000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- ========================================
-- 史莱姆系材料 (分裂史莱姆掉落)
-- ========================================

-- 史莱姆凝胶 - 合成材料
INSERT INTO `stditems` VALUES (930, '史莱姆凝胶', 2, 1, 1, 0, 0, 0, 930, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 500, 99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 粘液结晶 - 稀有材料
INSERT INTO `stditems` VALUES (931, '粘液结晶', 2, 2, 1, 0, 0, 0, 931, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 2000, 99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- ========================================
-- 吸血系武器和装备 (吸血蝙蝠掉落)
-- ========================================

-- 吸血戒指 - 攻击吸血效果
INSERT INTO `stditems` VALUES (940, '吸血戒指', 22, 43, 2, 0, 0, 0, 940, 7000, 0, 1, 0, 1, 3, 6, 0, 0, 0, 0, 0, 30, 35000, 5, 0, 0, 0, 0, 0, 0, 1, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 血魄项链
INSERT INTO `stditems` VALUES (941, '血魄项链', 19, 43, 3, 0, 0, 0, 941, 9000, 0, 2, 0, 2, 2, 4, 0, 0, 0, 0, 0, 28, 32000, 5, 0, 0, 0, 0, 0, 0, 1, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 暗夜护甲(男)
INSERT INTO `stditems` VALUES (942, '暗夜护甲(男)', 10, 43, 22, 0, 1, 0, 942, 16000, 5, 11, 2, 6, 0, 0, 0, 0, 0, 0, 0, 32, 42000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 暗夜护甲(女)
INSERT INTO `stditems` VALUES (943, '暗夜护甲(女)', 11, 43, 20, 0, 1, 0, 943, 16000, 4, 10, 2, 7, 0, 0, 0, 0, 0, 0, 0, 32, 42000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 蝙蝠翼披风
INSERT INTO `stditems` VALUES (944, '蝙蝠翼披风', 10, 44, 15, 0, 1, 0, 944, 14000, 3, 8, 3, 6, 0, 0, 0, 0, 0, 0, 0, 28, 38000, 5, 1, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 蝙蝠之牙 - 材料
INSERT INTO `stditems` VALUES (945, '蝙蝠之牙', 2, 3, 1, 0, 0, 0, 945, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 800, 99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 血色晶石 - 材料
INSERT INTO `stditems` VALUES (946, '血色晶石', 2, 4, 1, 0, 0, 0, 946, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 1500, 99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- ========================================
-- 龙系顶级武器和装备 (烈焰魔龙掉落)
-- ========================================

-- 烈焰战甲(男) - 顶级战士盔甲
INSERT INTO `stditems` VALUES (950, '烈焰战甲(男)', 10, 45, 45, 0, 1, 0, 950, 30000, 12, 22, 6, 12, 0, 0, 0, 0, 0, 0, 0, 50, 150000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 烈焰战甲(女)
INSERT INTO `stditems` VALUES (951, '烈焰战甲(女)', 11, 45, 42, 0, 1, 0, 951, 30000, 11, 21, 6, 13, 0, 0, 0, 0, 0, 0, 0, 50, 150000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 龙鳞护甲(男)
INSERT INTO `stditems` VALUES (952, '龙鳞护甲(男)', 10, 46, 50, 0, 1, 0, 952, 35000, 14, 25, 8, 15, 0, 0, 0, 0, 0, 0, 0, 55, 200000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 龙鳞护甲(女)
INSERT INTO `stditems` VALUES (953, '龙鳞护甲(女)', 11, 46, 48, 0, 1, 0, 953, 35000, 13, 24, 8, 16, 0, 0, 0, 0, 0, 0, 0, 55, 200000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 龙珠 - 顶级项链
INSERT INTO `stditems` VALUES (954, '龙珠', 19, 46, 5, 0, 0, 0, 954, 15000, 0, 5, 3, 8, 3, 8, 3, 8, 3, 8, 0, 52, 180000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 龙之戒指
INSERT INTO `stditems` VALUES (955, '龙之戒指', 22, 46, 3, 0, 0, 0, 955, 12000, 0, 3, 2, 5, 4, 10, 3, 8, 3, 8, 0, 50, 120000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 龙之项链
INSERT INTO `stditems` VALUES (956, '龙之项链', 19, 47, 4, 0, 0, 0, 956, 14000, 0, 4, 2, 6, 3, 8, 3, 8, 3, 8, 0, 50, 130000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 龙之手镯
INSERT INTO `stditems` VALUES (957, '龙之手镯', 24, 46, 3, 0, 0, 0, 957, 12000, 0, 3, 1, 5, 3, 7, 3, 7, 3, 7, 0, 50, 110000, 5, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 火焰戒指
INSERT INTO `stditems` VALUES (958, '火焰戒指', 22, 47, 2, 0, 0, 0, 958, 8000, 0, 2, 1, 3, 3, 7, 2, 5, 0, 0, 0, 42, 55000, 5, 0, 0, 0, 1, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 烈焰项链
INSERT INTO `stditems` VALUES (959, '烈焰项链', 19, 48, 3, 0, 0, 0, 959, 10000, 0, 3, 1, 4, 2, 5, 3, 6, 0, 0, 0, 42, 60000, 5, 0, 0, 0, 1, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 炎魔手镯
INSERT INTO `stditems` VALUES (960, '炎魔手镯', 24, 47, 2, 0, 0, 0, 960, 8000, 0, 2, 0, 3, 2, 5, 2, 6, 0, 0, 0, 40, 50000, 5, 0, 0, 0, 1, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
-- 地狱火 - 法师武器
INSERT INTO `stditems` VALUES (961, '地狱火', 6, 45, 30, 0, 0, 0, 961, 28000, 0, 3, 0, 4, 6, 14, 8, 22, 0, 0, 0, 45, 100000, 5, 0, 0, 0, 2, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- 龙系材料
INSERT INTO `stditems` VALUES (965, '龙鳞', 2, 5, 2, 0, 0, 0, 965, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 5000, 99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `stditems` VALUES (966, '龙之心', 2, 6, 3, 0, 0, 0, 966, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 10000, 99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);
INSERT INTO `stditems` VALUES (967, '火焰精华', 2, 7, 1, 0, 0, 0, 967, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0, 1, 3000, 99, 0, 0, 0, 0, 0, 0, 0, 0, 0, 100, 251, 0, 0, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL, NULL);

-- ========================================
-- 字段说明:
-- StdMode: 物品类型 (5=单手剑, 6=双手武器, 10=男衣服, 11=女衣服, 15=头盔, 19=项链, 22=戒指, 24=手镯, 2=材料, 4=技能书)
-- Shape: 外观形状ID (对应客户端资源)
-- Weight: 重量
-- ImgIndex: 图像索引
-- DuraMax: 最大耐久
-- Ac/AcMax: 物理防御
-- Mac/MacMax: 魔法防御
-- Dc/DcMax: 攻击力
-- Mc/McMax: 魔法攻击力
-- Sc/ScMax: 道术攻击力
-- Need: 需求类型 (0=无, 1=等级, 2=攻击, 3=魔法, 4=道术, 5=转生)
-- NeedLevel: 需求等级
-- Price: 价格
-- Stock: 商店库存
-- Atkspd: 攻击速度加成
-- AgilIty: 敏捷加成
-- Accurate: 准确加成
-- Mgavoid: 魔法躲避
-- Strong: 强度
-- Undead: 对亡灵伤害加成
-- ========================================

-- ========================================
-- 武器特效配置 (通过Reserved字段)
-- 特效类型 (reserve[3] 值):
--   1: 静态发光
--   2: 倚天剑特效 (10帧)
--   7-9: 高级特效 (14帧)
--   10-12: 顶级特效 (18帧)
--   100-249: 自定义特效 (20帧, 索引30000+)
-- ========================================

-- 冰霜之刃: 特效类型100, 蓝色冰霜光环
-- UPDATE stditems SET AniCount = 100 WHERE Id = 900;

-- 冰魄: 特效类型101, 青色冰霜光芒
-- UPDATE stditems SET AniCount = 101 WHERE Id = 901;

-- 凝霜: 特效类型102, 淡蓝色霜气
-- UPDATE stditems SET AniCount = 102 WHERE Id = 902;

-- 暗影匕首: 特效类型103, 紫色暗影
-- UPDATE stditems SET AniCount = 103 WHERE Id = 910;

-- 亡灵法杖: 特效类型105, 绿色幽魂
-- UPDATE stditems SET AniCount = 105 WHERE Id = 920;

-- 地狱火: 特效类型7, 火焰燃烧
-- UPDATE stditems SET AniCount = 7 WHERE Id = 961;

-- 龙珠: 特效类型10, 金色龙气
-- UPDATE stditems SET AniCount = 10 WHERE Id = 954;
