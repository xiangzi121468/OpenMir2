#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
武器特效配置优化版
与客户端 FState.pas 中的 reserve[3] 特效类型对应

特效类型说明 (reserve[3] 值):
- 1: 静态发光 (索引1403)
- 2: 倚天剑特效 (索引1890-1899, 10帧)
- 3: 静态发光2 (索引2427)
- 4: 传奇神剑特效 (索引2530-2537, 8帧)
- 5: 传奇神剑特效2 (索引2550-2559, 10帧)
- 6: 传奇神剑特效3 (索引2560-2569, 10帧)
- 7-9: 高级特效 (索引3480+, 14帧)
- 10-12: 顶级特效 (索引3610+, 18帧)
- 13-15: 更高级特效 (18帧)
- 100-249: 自定义特效 (索引30000+, 20帧)

帧率: 每200ms切换一帧
"""

# ========================================
# 新增武器特效配置
# ========================================

NEW_WEAPON_EFFECTS = {
    # ================================
    # 冰霜系武器 - 使用特效类型100 (自定义20帧)
    # ================================
    900: {  # 冰霜之刃
        "name": "冰霜之刃",
        "reserve3": 100,  # 自定义特效起始
        "effect_index": 30000,  # 30000 + (100-100)*20 = 30000
        "frame_count": 20,
        "frame_time": 200,  # ms
        "color": "blue",
        "description": "蓝色冰霜光环，冰晶粒子环绕",
    },
    901: {  # 冰魄
        "name": "冰魄",
        "reserve3": 101,
        "effect_index": 30020,  # 30000 + (101-100)*20 = 30020
        "frame_count": 20,
        "frame_time": 200,
        "color": "cyan",
        "description": "青色冰霜光芒，雪花飘落",
    },
    902: {  # 凝霜
        "name": "凝霜",
        "reserve3": 102,
        "effect_index": 30040,
        "frame_count": 20,
        "frame_time": 200,
        "color": "cyan",
        "description": "淡蓝色霜气环绕",
    },
    
    # ================================
    # 暗影系武器 - 使用特效类型103-104
    # ================================
    910: {  # 暗影匕首
        "name": "暗影匕首",
        "reserve3": 103,
        "effect_index": 30060,
        "frame_count": 20,
        "frame_time": 150,  # 更快的闪烁
        "color": "purple",
        "description": "紫色暗影气息，若隐若现",
    },
    
    # ================================
    # 亡灵系武器 - 使用特效类型105-106
    # ================================
    920: {  # 亡灵法杖
        "name": "亡灵法杖",
        "reserve3": 105,
        "effect_index": 30100,
        "frame_count": 20,
        "frame_time": 200,
        "color": "green",
        "description": "绿色幽魂环绕，鬼火飘动",
    },
    
    # ================================
    # 火焰系武器 - 使用特效类型7 (高级14帧)
    # ================================
    961: {  # 地狱火
        "name": "地狱火",
        "reserve3": 7,  # 使用现有高级特效
        "effect_index": 3480,  # 3480 + (7-7)*20 = 3480
        "frame_count": 14,
        "frame_time": 200,
        "color": "red",
        "description": "火焰燃烧效果",
    },
    
    # ================================
    # 龙系顶级武器 - 使用特效类型10 (顶级18帧)
    # ================================
    # 龙之套装使用最高级特效
    954: {  # 龙珠
        "name": "龙珠",
        "reserve3": 10,
        "effect_index": 3610,
        "frame_count": 18,
        "frame_time": 200,
        "color": "gold",
        "description": "金色龙气环绕",
    },
}

# ========================================
# 数据库更新SQL生成
# ========================================

def generate_effect_sql():
    """生成更新武器特效的SQL语句"""
    sql_lines = [
        "-- ========================================",
        "-- 武器特效配置更新",
        "-- 更新 stditems 表的 reserve 字段设置特效类型",
        "-- ========================================",
        "",
    ]
    
    for item_id, config in NEW_WEAPON_EFFECTS.items():
        sql = f"-- {config['name']}: 特效类型={config['reserve3']}, 帧数={config['frame_count']}"
        sql_lines.append(sql)
        # reserve字段在某些版本可能是JSON或特定格式
        # 这里假设reserve[3]对应某个字段
        sql_lines.append(f"-- UPDATE stditems SET Reserved = {config['reserve3']} WHERE Id = {item_id};")
        sql_lines.append("")
    
    return "\n".join(sql_lines)


# ========================================
# 特效图像索引计算
# ========================================

def get_effect_image_indices(reserve3_value):
    """根据reserve[3]值计算特效图像索引范围"""
    if reserve3_value == 1:
        return (1403, 1403, 1)  # 静态
    elif reserve3_value == 2:
        return (1890, 1899, 10)
    elif reserve3_value == 3:
        return (2427, 2427, 1)  # 静态
    elif reserve3_value == 4:
        return (2530, 2537, 8)
    elif reserve3_value == 5:
        return (2550, 2559, 10)
    elif reserve3_value == 6:
        return (2560, 2569, 10)
    elif reserve3_value in [7, 8, 9]:
        base = 3480 + (reserve3_value - 7) * 20
        return (base, base + 13, 14)
    elif reserve3_value in [10, 11, 12]:
        base = 3610 + (reserve3_value - 10) * 20
        return (base, base + 17, 18)
    elif reserve3_value in [13, 14, 15]:
        base = 3670 + (reserve3_value - 13) * 20
        return (base, base + 17, 18)
    elif 100 <= reserve3_value <= 249:
        base = 30000 + (reserve3_value - 100) * 20
        return (base, base + 19, 20)
    else:
        return (0, 0, 0)


def print_effect_summary():
    """打印特效配置摘要"""
    print("=" * 70)
    print("武器特效配置摘要")
    print("=" * 70)
    print(f"{'物品ID':<8} {'名称':<12} {'特效类型':<10} {'索引范围':<20} {'帧数':<6}")
    print("-" * 70)
    
    for item_id, config in NEW_WEAPON_EFFECTS.items():
        start, end, frames = get_effect_image_indices(config['reserve3'])
        index_range = f"{start}-{end}" if start != end else str(start)
        print(f"{item_id:<8} {config['name']:<12} {config['reserve3']:<10} {index_range:<20} {frames:<6}")
    
    print("=" * 70)
    print("\n特效类型说明:")
    print("- 1-6: 基础特效 (静态或简单动画)")
    print("- 7-9: 高级特效 (14帧动画)")
    print("- 10-15: 顶级特效 (18帧动画)")
    print("- 100-249: 自定义特效 (20帧动画, 索引30000+)")
    print("\n帧率: 每200ms切换一帧 (可在客户端代码中调整)")


if __name__ == "__main__":
    print_effect_summary()
    print("\n")
    print(generate_effect_sql())
