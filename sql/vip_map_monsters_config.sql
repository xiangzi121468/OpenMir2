-- ============================================
-- VIP地图怪物和掉落配置
-- 将自定义新怪物和新装备配置到VIP地图
-- ============================================

-- 清空现有VIP地图怪物配置
TRUNCATE TABLE `vip_map_monsters`;

-- ============================================
-- 贵族VIP大厅 (VIPHALL1) - VIP1等级 (充值30元)
-- 定位：入门级VIP地图，适合35-45级玩家
-- ============================================

-- 小怪：吸血蝙蝠群 (连续刷新，供玩家练级)
INSERT INTO `vip_map_monsters` (`vip_map_id`, `monster_name`, `description`, `spawn_count`, `spawn_interval`, `spawn_x`, `spawn_y`, `spawn_range`, `is_boss`, `drop_description`) VALUES
(1, '吸血蝙蝠', '吸血小怪，攻击吸取30%生命', 30, 5, 150, 150, 30, 0, '吸血戒指、吸血护符'),
(1, '小史莱姆', '分裂怪物，数量多', 20, 10, 180, 120, 25, 0, '史莱姆凝胶');

-- 精英：大史莱姆 (30分钟刷新)
INSERT INTO `vip_map_monsters` (`vip_map_id`, `monster_name`, `description`, `spawn_count`, `spawn_interval`, `spawn_x`, `spawn_y`, `spawn_range`, `is_boss`, `drop_description`) VALUES
(1, '大史莱姆', '分裂史莱姆，死亡分裂成小史莱姆', 3, 30, 160, 160, 20, 0, '史莱姆凝胶、粘液结晶');

-- BOSS：暗影刺客 (60分钟刷新)
INSERT INTO `vip_map_monsters` (`vip_map_id`, `monster_name`, `description`, `spawn_count`, `spawn_interval`, `spawn_x`, `spawn_y`, `spawn_range`, `is_boss`, `drop_description`) VALUES
(1, '暗影刺客', '【VIP BOSS】隐身突袭，首击3倍暴击', 1, 60, 180, 180, 5, 1, '暗影匕首、隐身戒指、暗影轻甲、夜行者之衣');

-- ============================================
-- 黄金VIP殿堂 (VIPHALL2) - VIP2等级 (充值100元)
-- 定位：中级VIP地图，适合40-55级玩家
-- ============================================

-- 小怪：暗影刺客群
INSERT INTO `vip_map_monsters` (`vip_map_id`, `monster_name`, `description`, `spawn_count`, `spawn_interval`, `spawn_x`, `spawn_y`, `spawn_range`, `is_boss`, `drop_description`) VALUES
(2, '暗影刺客', '隐身小怪群', 15, 10, 150, 150, 40, 0, '暗影匕首、隐身戒指'),
(2, '吸血蝙蝠', '高级吸血蝙蝠', 20, 8, 170, 130, 30, 0, '吸血戒指、血魄项链');

-- 精英：骷髅召唤兵 (由死灵召唤师召唤)
INSERT INTO `vip_map_monsters` (`vip_map_id`, `monster_name`, `description`, `spawn_count`, `spawn_interval`, `spawn_x`, `spawn_y`, `spawn_range`, `is_boss`, `drop_description`) VALUES
(2, '骷髅战士', '亡灵战士', 10, 15, 160, 160, 25, 0, '骷髅头盔、骷髅戒指');

-- BOSS：死灵召唤师 (90分钟刷新)
INSERT INTO `vip_map_monsters` (`vip_map_id`, `monster_name`, `description`, `spawn_count`, `spawn_interval`, `spawn_x`, `spawn_y`, `spawn_range`, `is_boss`, `drop_description`) VALUES
(2, '死灵召唤师', '【黄金BOSS】召唤骷髅，低血量狂暴', 1, 90, 180, 180, 5, 1, '亡灵法杖、亡灵骨甲、噬魂法衣、幽魂道袍、骷髅召唤书');

-- ============================================
-- 钻石VIP圣殿 (VIPHALL3) - VIP3等级 (充值500元)
-- 定位：高级VIP地图，适合50级以上玩家
-- ============================================

-- 小怪：死灵召唤师小弟
INSERT INTO `vip_map_monsters` (`vip_map_id`, `monster_name`, `description`, `spawn_count`, `spawn_interval`, `spawn_x`, `spawn_y`, `spawn_range`, `is_boss`, `drop_description`) VALUES
(3, '骷髅法师', '亡灵法师', 15, 10, 150, 150, 40, 0, '亡灵法杖碎片'),
(3, '暗影刺客', '精英暗影刺客', 10, 12, 130, 170, 30, 0, '暗影装备碎片');

-- 精英：死灵召唤师 (作为精英怪)
INSERT INTO `vip_map_monsters` (`vip_map_id`, `monster_name`, `description`, `spawn_count`, `spawn_interval`, `spawn_x`, `spawn_y`, `spawn_range`, `is_boss`, `drop_description`) VALUES
(3, '死灵召唤师', '精英死灵召唤师', 2, 30, 160, 160, 20, 0, '亡灵法杖、骷髅头盔');

-- BOSS：冰霜领主 (120分钟刷新) - 最强BOSS
INSERT INTO `vip_map_monsters` (`vip_map_id`, `monster_name`, `description`, `spawn_count`, `spawn_interval`, `spawn_x`, `spawn_y`, `spawn_range`, `is_boss`, `drop_description`) VALUES
(3, '冰霜领主', '【钻石BOSS】远程冰霜攻击，AOE暴风雪，冻结目标', 1, 120, 180, 180, 5, 1, '冰霜之刃、冰魄、凝霜、寒冰战甲、冰魄法袍、凝霜道袍、霜寒戒指、冰魄项链、冰晶手镯');

-- ============================================
-- 至尊VIP神殿 (VIPHALL4) - VIP4等级 (充值1000元)
-- 新增地图，终极挑战
-- ============================================

-- 先插入VIP地图配置
INSERT INTO `vip_maps` (`map_id`, `map_name`, `description`, `min_vip_level`, `min_recharge`, `entry_fee`, `daily_limit`, `level_limit`, `is_random_entry`, `is_fixed_entry`, `random_x_min`, `random_x_max`, `random_y_min`, `random_y_max`, `fixed_x`, `fixed_y`) VALUES
('VIPHALL4', '至尊VIP神殿', '终极VIP地图，最强BOSS和装备产出地', 4, 1000, 0, 2, 50, 1, 1, 80, 220, 80, 220, 150, 150);

-- 小怪：冰霜小兵
INSERT INTO `vip_map_monsters` (`vip_map_id`, `monster_name`, `description`, `spawn_count`, `spawn_interval`, `spawn_x`, `spawn_y`, `spawn_range`, `is_boss`, `drop_description`) VALUES
(4, '冰霜元素', '冰霜领主的仆从', 20, 8, 150, 150, 50, 0, '冰晶碎片'),
(4, '暗影精英', '精英暗影刺客', 15, 10, 130, 130, 40, 0, '暗影精华');

-- 精英：冰霜领主 (作为精英怪)
INSERT INTO `vip_map_monsters` (`vip_map_id`, `monster_name`, `description`, `spawn_count`, `spawn_interval`, `spawn_x`, `spawn_y`, `spawn_range`, `is_boss`, `drop_description`) VALUES
(4, '冰霜领主', '精英冰霜领主', 1, 60, 180, 120, 10, 0, '冰霜系装备'),
(4, '死灵召唤师', '精英死灵召唤师', 1, 60, 120, 180, 10, 0, '亡灵系装备');

-- 终极BOSS：烈焰魔龙 (180分钟刷新)
INSERT INTO `vip_map_monsters` (`vip_map_id`, `monster_name`, `description`, `spawn_count`, `spawn_interval`, `spawn_x`, `spawn_y`, `spawn_range`, `is_boss`, `drop_description`) VALUES
(4, '烈焰魔龙', '【至尊BOSS】火焰吐息，释放火墙，免疫火焰，全服最强BOSS', 1, 180, 150, 150, 5, 1, '烈焰之刃、火龙战甲、烈焰法袍、炎龙道袍、全套顶级装备');


-- ============================================
-- 怪物掉落配置 (MonItems.txt 格式)
-- 放置于 Envir/MonItems/ 目录
-- ============================================

-- 以下为MonItems配置示例，需要创建对应的txt文件

/*

=== 冰霜领主.txt ===
1/10 冰霜之刃
1/10 冰魄
1/10 凝霜
1/15 寒冰战甲(男)
1/15 寒冰战甲(女)
1/15 冰魄法袍(男)
1/15 冰魄法袍(女)
1/15 凝霜道袍(男)
1/15 凝霜道袍(女)
1/8 霜寒戒指
1/8 冰魄项链
1/8 冰晶手镯
1/3 金条
1/2 祝福油
3/1 强效太阳水

=== 暗影刺客.txt ===
1/20 暗影匕首
1/25 隐身戒指
1/30 暗影轻甲(男)
1/30 暗影轻甲(女)
1/30 夜行者之衣(男)
1/30 夜行者之衣(女)
1/15 幽灵项链
1/15 暗影手镯
1/5 金条
2/1 强效太阳水

=== 死灵召唤师.txt ===
1/12 亡灵法杖
1/20 骷髅头盔
1/15 骷髅戒指
1/20 亡灵骨甲(男)
1/20 亡灵骨甲(女)
1/20 噬魂法衣(男)
1/20 噬魂法衣(女)
1/20 幽魂道袍(男)
1/20 幽魂道袍(女)
1/15 幽魂项链
1/15 噬魂手镯
1/50 骷髅召唤书
1/4 金条
2/1 强效太阳水

=== 大史莱姆.txt ===
1/5 史莱姆凝胶
1/20 粘液结晶
1/1 太阳水

=== 吸血蝙蝠.txt ===
1/30 吸血戒指
1/30 吸血护符
1/20 血魄项链
1/40 血魄手镯
1/40 血红战甲(男)
1/40 血红战甲(女)
1/3 金条
1/1 太阳水

=== 烈焰魔龙.txt ===
1/8 烈焰之刃
1/8 炎龙
1/8 火龙战甲(男)
1/8 火龙战甲(女)
1/10 烈焰法袍(男)
1/10 烈焰法袍(女)
1/10 炎龙道袍(男)
1/10 炎龙道袍(女)
1/5 烈焰戒指
1/5 火龙项链
1/5 炎晶手镯
1/2 金条
5/1 祝福油
3/1 强效太阳水

*/
