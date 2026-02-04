-- ========================================
-- 全部武器特效配置
-- ========================================
-- 
-- 特效类型说明:
--   1-9: 基础发光特效 (10帧)
--   10-19: 高级特效 (14-18帧)
--   100-249: 自定义特效 (20帧, 索引30000+)
--
-- 特效分类:
--   1-3: 白色/银色光芒 (普通武器)
--   4-6: 金色光芒 (高级武器)
--   7-9: 火焰/红色 (战士顶级)
--   10-12: 蓝色/冰霜 (法师顶级)
--   13-15: 绿色/自然 (道士顶级)
--   100-109: 冰霜系自定义
--   110-119: 火焰系自定义
--   120-129: 雷电系自定义
--   130-139: 暗影系自定义
--   140-149: 神圣系自定义
-- ========================================

-- ========================================
-- 经典顶级武器特效
-- ========================================

-- 屠龙 - 金色龙气光芒
UPDATE stditems SET Reference = '{"effect":7}' WHERE Id = 225;

-- 裁决之杖 - 红色战魂
UPDATE stditems SET Reference = '{"effect":8}' WHERE Id = 223;

-- 血饮 - 血红光芒
UPDATE stditems SET Reference = '{"effect":9}' WHERE Id = 235;

-- 骨玉权杖 - 紫色魔力
UPDATE stditems SET Reference = '{"effect":10}' WHERE Id = 234;

-- 龙纹剑 - 青色龙纹
UPDATE stditems SET Reference = '{"effect":4}' WHERE Id = 230;

-- 嗜魂法杖 - 幽蓝魂火
UPDATE stditems SET Reference = '{"effect":11}' WHERE Id = 236;

-- 怒斩 - 红色怒焰
UPDATE stditems SET Reference = '{"effect":7}' WHERE Id = 238;

-- 逍遥扇 - 绿色道风
UPDATE stditems SET Reference = '{"effect":13}' WHERE Id = 239;

-- 霸者之刃 - 金色霸气
UPDATE stditems SET Reference = '{"effect":6}' WHERE Id = 240;

-- 无极棍 - 绿色自然
UPDATE stditems SET Reference = '{"effect":14}' WHERE Id = 229;

-- 银蛇 - 银色蛇光
UPDATE stditems SET Reference = '{"effect":2}' WHERE Id = 228;

-- ========================================
-- 天之系列武器 (强化版特效)
-- ========================================

-- 天之屠龙 - 金色龙气增强
UPDATE stditems SET Reference = '{"effect":8}' WHERE Id = 249;

-- 天之裁决之杖 - 红色战魂增强
UPDATE stditems SET Reference = '{"effect":9}' WHERE Id = 244;

-- 天之血饮 - 血红增强
UPDATE stditems SET Reference = '{"effect":10}' WHERE Id = 241;

-- 天之骨玉权杖 - 紫色魔力增强
UPDATE stditems SET Reference = '{"effect":11}' WHERE Id = 242;

-- 天之龙纹剑 - 青龙增强
UPDATE stditems SET Reference = '{"effect":5}' WHERE Id = 243;

-- 天之嗜魂法杖 - 幽蓝增强
UPDATE stditems SET Reference = '{"effect":12}' WHERE Id = 248;

-- 天之怒斩 - 怒焰增强
UPDATE stditems SET Reference = '{"effect":8}' WHERE Id = 245;

-- 天之逍遥扇 - 道风增强
UPDATE stditems SET Reference = '{"effect":14}' WHERE Id = 247;

-- 天之龙牙 - 龙牙光芒
UPDATE stditems SET Reference = '{"effect":6}' WHERE Id = 246;

-- ========================================
-- 开天/玄天/镇天 三神器
-- ========================================

-- 开天 - 金色开天斩
UPDATE stditems SET Reference = '{"effect":7}' WHERE Id = 250;

-- 玄天 - 玄青光芒
UPDATE stditems SET Reference = '{"effect":13}' WHERE Id = 251;

-- 镇天 - 紫色镇魂
UPDATE stditems SET Reference = '{"effect":10}' WHERE Id = 252;

-- ========================================
-- 王者系列
-- ========================================

-- 王者之刃 - 王者金光
UPDATE stditems SET Reference = '{"effect":8}' WHERE Id = 258;

-- 王者之剑 - 王者青光
UPDATE stditems SET Reference = '{"effect":14}' WHERE Id = 259;

-- 王者之杖 - 王者紫光
UPDATE stditems SET Reference = '{"effect":11}' WHERE Id = 260;

-- 主宰神剑 - 神圣金光
UPDATE stditems SET Reference = '{"effect":9}' WHERE Id = 261;

-- ========================================
-- 龙系武器
-- ========================================

-- 龙牙 - 龙牙白光
UPDATE stditems SET Reference = '{"effect":3}' WHERE Id = 237;

-- 龙炎之刃 - 龙炎火光
UPDATE stditems SET Reference = '{"effect":7}' WHERE Id = 253;

-- 天龙圣剑 - 天龙神光
UPDATE stditems SET Reference = '{"effect":9}' WHERE Id = 254;

-- 炎龙刃 - 炎龙火焰
UPDATE stditems SET Reference = '{"effect":8}' WHERE Id = 255;

-- 青龙刺 - 青龙光芒
UPDATE stditems SET Reference = '{"effect":13}' WHERE Id = 256;

-- 雷龙杖 - 雷龙紫电
UPDATE stditems SET Reference = '{"effect":10}' WHERE Id = 257;

-- ========================================
-- 其他高级武器
-- ========================================

-- 开天战廓 - 战廓红光
UPDATE stditems SET Reference = '{"effect":7}' WHERE Id = 262;

-- 乾坤 - 乾坤道光
UPDATE stditems SET Reference = '{"effect":14}' WHERE Id = 263;

-- 玄灵青蟒 - 青蟒魔光
UPDATE stditems SET Reference = '{"effect":11}' WHERE Id = 264;

-- 热血之刃 - 热血红光
UPDATE stditems SET Reference = '{"effect":8}' WHERE Id = 265;

-- 霸天鬼斩 - 鬼斩黑光
UPDATE stditems SET Reference = '{"effect":9}' WHERE Id = 266;

-- 鹰魂神刃 - 鹰魂青光
UPDATE stditems SET Reference = '{"effect":13}' WHERE Id = 267;

-- 冥神之刃 - 冥神紫光
UPDATE stditems SET Reference = '{"effect":10}' WHERE Id = 268;

-- ========================================
-- 新增自定义武器 (ID 900+) - 使用自定义特效
-- ========================================

-- 冰霜之刃 - 蓝色冰霜光环 (自定义20帧)
UPDATE stditems SET Reference = '{"effect":100}' WHERE Id = 900;

-- 冰魄 - 青色冰霜光芒
UPDATE stditems SET Reference = '{"effect":101}' WHERE Id = 901;

-- 凝霜 - 淡蓝色霜气
UPDATE stditems SET Reference = '{"effect":102}' WHERE Id = 902;

-- 暗影匕首 - 紫色暗影
UPDATE stditems SET Reference = '{"effect":103}' WHERE Id = 910;

-- 亡灵法杖 - 绿色幽魂
UPDATE stditems SET Reference = '{"effect":105}' WHERE Id = 920;

-- 地狱火 - 红色地狱火焰
UPDATE stditems SET Reference = '{"effect":110}' WHERE Id = 961;

-- 龙珠 - 金色龙之光环
UPDATE stditems SET Reference = '{"effect":115}' WHERE Id = 954;

-- ========================================
-- 中级武器基础特效
-- ========================================

-- 井中月 - 银色月光
UPDATE stditems SET Reference = '{"effect":1}' WHERE Id = 222;

-- 命运之刃 - 命运光芒
UPDATE stditems SET Reference = '{"effect":2}' WHERE Id = 224;

-- 炼狱 - 炼狱火光
UPDATE stditems SET Reference = '{"effect":3}' WHERE Id = 221;

-- 凌风 - 风之光芒
UPDATE stditems SET Reference = '{"effect":1}' WHERE Id = 216;

-- 八荒 - 八荒之力
UPDATE stditems SET Reference = '{"effect":2}' WHERE Id = 215;

-- 海魂 - 海蓝光芒
UPDATE stditems SET Reference = '{"effect":4}' WHERE Id = 231;

-- 偃月 - 月光
UPDATE stditems SET Reference = '{"effect":1}' WHERE Id = 232;

-- 魔杖 - 魔力光芒
UPDATE stditems SET Reference = '{"effect":5}' WHERE Id = 233;

-- ========================================
-- 统计: 共配置 55 件武器特效
-- ========================================

-- 验证查询
-- SELECT Id, Name, Reference FROM stditems 
-- WHERE Reference IS NOT NULL AND Reference != ''
-- ORDER BY Id;

-- ========================================
-- 特效效果说明
-- ========================================
-- 
-- 基础特效 (1-15):
--   1: 白色微光
--   2: 银色闪烁
--   3: 白色脉冲
--   4: 青色光芒
--   5: 蓝色魔力
--   6: 金色光辉
--   7: 红色火焰
--   8: 橙色战魂
--   9: 血红光芒
--   10: 紫色魔力
--   11: 深蓝幽光
--   12: 蓝紫混合
--   13: 青绿自然
--   14: 翠绿道光
--   15: 深绿森林
--
-- 自定义特效 (100+):
--   100-109: 冰霜系 (蓝色调)
--   110-119: 火焰系 (红色调)
--   120-129: 雷电系 (紫黄调)
--   130-139: 暗影系 (紫黑调)
--   140-149: 神圣系 (金白调)
--
-- ========================================
