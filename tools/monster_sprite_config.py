#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
怪物精灵图动画配置
定义每个怪物的动画帧布局，与客户端 Actor.pas 中的 TMonsterAction 对应
"""

# 动画动作定义
# 格式: (起始帧, 帧数, 跳过帧数, 帧时间ms)
# 每个动作有8个方向，总帧数 = 帧数 * 8

MONSTER_ANIMATION_CONFIGS = {
    # ===============================
    # 冰霜领主 (FrostLord) - MA400
    # 总帧数: 320帧
    # ===============================
    "FrostLord": {
        "name": "冰霜领主",
        "file_prefix": "Mon41",
        "directions": 8,
        "actions": {
            "Stand":    {"start": 0,   "frames": 4,  "skip": 6, "ftime": 200},  # 0-31
            "Walk":     {"start": 32,  "frames": 6,  "skip": 4, "ftime": 150},  # 32-79
            "Attack":   {"start": 80,  "frames": 8,  "skip": 2, "ftime": 120},  # 80-143
            "Critical": {"start": 144, "frames": 10, "skip": 0, "ftime": 100},  # 144-223
            "Struck":   {"start": 224, "frames": 2,  "skip": 0, "ftime": 100},  # 224-239
            "Die":      {"start": 240, "frames": 10, "skip": 0, "ftime": 150},  # 240-319
        },
        "total_frames": 320,
        "frame_width": 80,
        "frame_height": 100,
    },
    
    # ===============================
    # 暗影刺客 (ShadowAssassin) - MA401
    # 总帧数: 288帧
    # ===============================
    "ShadowAssassin": {
        "name": "暗影刺客",
        "file_prefix": "Mon42",
        "directions": 8,
        "actions": {
            "Stand":    {"start": 0,   "frames": 4,  "skip": 6, "ftime": 180},  # 0-31
            "Walk":     {"start": 32,  "frames": 6,  "skip": 4, "ftime": 120},  # 32-79
            "Attack":   {"start": 80,  "frames": 6,  "skip": 4, "ftime": 80},   # 80-127
            "Critical": {"start": 128, "frames": 8,  "skip": 2, "ftime": 100},  # 128-191
            "Struck":   {"start": 192, "frames": 2,  "skip": 0, "ftime": 100},  # 192-207
            "Die":      {"start": 208, "frames": 10, "skip": 0, "ftime": 140},  # 208-287
        },
        "total_frames": 288,
        "frame_width": 64,
        "frame_height": 80,
    },
    
    # ===============================
    # 死灵召唤师 (NecroSummoner) - MA402
    # 总帧数: 320帧
    # ===============================
    "NecroSummoner": {
        "name": "死灵召唤师",
        "file_prefix": "Mon43",
        "directions": 8,
        "actions": {
            "Stand":    {"start": 0,   "frames": 4,  "skip": 6, "ftime": 200},  # 0-31
            "Walk":     {"start": 32,  "frames": 6,  "skip": 4, "ftime": 180},  # 32-79
            "Attack":   {"start": 80,  "frames": 8,  "skip": 2, "ftime": 150},  # 80-143
            "Critical": {"start": 144, "frames": 10, "skip": 0, "ftime": 180},  # 144-223
            "Struck":   {"start": 224, "frames": 2,  "skip": 0, "ftime": 100},  # 224-239
            "Die":      {"start": 240, "frames": 10, "skip": 0, "ftime": 150},  # 240-319
        },
        "total_frames": 320,
        "frame_width": 80,
        "frame_height": 100,
    },
    
    # ===============================
    # 分裂史莱姆 (SplitSlime) - MA403
    # 总帧数: 160帧
    # ===============================
    "SplitSlime": {
        "name": "分裂史莱姆",
        "file_prefix": "Mon44",
        "directions": 8,
        "actions": {
            "Stand":    {"start": 0,   "frames": 4,  "skip": 6, "ftime": 250},  # 0-31
            "Walk":     {"start": 32,  "frames": 4,  "skip": 6, "ftime": 200},  # 32-63
            "Attack":   {"start": 64,  "frames": 4,  "skip": 6, "ftime": 150},  # 64-95
            "Critical": {"start": 64,  "frames": 4,  "skip": 6, "ftime": 150},  # 同Attack
            "Struck":   {"start": 96,  "frames": 2,  "skip": 0, "ftime": 100},  # 96-111
            "Die":      {"start": 112, "frames": 6,  "skip": 4, "ftime": 120},  # 112-159
        },
        "total_frames": 160,
        "frame_width": 48,
        "frame_height": 48,
    },
    
    # ===============================
    # 吸血蝙蝠 (VampireBat) - MA404
    # 总帧数: 192帧
    # ===============================
    "VampireBat": {
        "name": "吸血蝙蝠",
        "file_prefix": "Mon45",
        "directions": 8,
        "actions": {
            "Stand":    {"start": 0,   "frames": 4,  "skip": 6, "ftime": 150},  # 0-31
            "Walk":     {"start": 32,  "frames": 6,  "skip": 4, "ftime": 100},  # 32-79
            "Attack":   {"start": 80,  "frames": 6,  "skip": 4, "ftime": 100},  # 80-127
            "Critical": {"start": 80,  "frames": 6,  "skip": 4, "ftime": 100},  # 同Attack
            "Struck":   {"start": 128, "frames": 2,  "skip": 0, "ftime": 100},  # 128-143
            "Die":      {"start": 144, "frames": 6,  "skip": 4, "ftime": 140},  # 144-191
        },
        "total_frames": 192,
        "frame_width": 64,
        "frame_height": 64,
    },
    
    # ===============================
    # 烈焰魔龙 (InfernoWyrm) - MA405
    # 总帧数: 368帧
    # ===============================
    "InfernoWyrm": {
        "name": "烈焰魔龙",
        "file_prefix": "Mon46",
        "directions": 8,
        "actions": {
            "Stand":    {"start": 0,   "frames": 4,  "skip": 6, "ftime": 200},  # 0-31
            "Walk":     {"start": 32,  "frames": 6,  "skip": 4, "ftime": 180},  # 32-79
            "Attack":   {"start": 80,  "frames": 10, "skip": 0, "ftime": 120},  # 80-159
            "Critical": {"start": 160, "frames": 12, "skip": 0, "ftime": 100},  # 160-255
            "Struck":   {"start": 256, "frames": 2,  "skip": 0, "ftime": 100},  # 256-271
            "Die":      {"start": 272, "frames": 12, "skip": 0, "ftime": 150},  # 272-367
        },
        "total_frames": 368,
        "frame_width": 120,
        "frame_height": 120,
    },
}

def get_frame_layout(monster_name):
    """获取怪物的帧布局信息"""
    if monster_name not in MONSTER_ANIMATION_CONFIGS:
        return None
    
    config = MONSTER_ANIMATION_CONFIGS[monster_name]
    layout = []
    
    for action_name, action_info in config["actions"].items():
        start = action_info["start"]
        frames = action_info["frames"]
        skip = action_info["skip"]
        directions = config["directions"]
        
        # 计算每个方向的帧范围
        frames_per_direction = frames + skip
        for dir_idx in range(directions):
            dir_start = start + dir_idx * frames_per_direction
            dir_end = dir_start + frames
            layout.append({
                "action": action_name,
                "direction": dir_idx,
                "start_frame": dir_start,
                "end_frame": dir_end,
                "frame_count": frames
            })
    
    return layout

def print_frame_layout(monster_name):
    """打印怪物的帧布局"""
    if monster_name not in MONSTER_ANIMATION_CONFIGS:
        print(f"未找到怪物配置: {monster_name}")
        return
    
    config = MONSTER_ANIMATION_CONFIGS[monster_name]
    print(f"\n{'='*60}")
    print(f"怪物: {config['name']} ({monster_name})")
    print(f"总帧数: {config['total_frames']}")
    print(f"帧尺寸: {config['frame_width']}x{config['frame_height']}")
    print(f"{'='*60}")
    
    for action_name, action_info in config["actions"].items():
        start = action_info["start"]
        frames = action_info["frames"]
        skip = action_info["skip"]
        ftime = action_info["ftime"]
        total_for_action = (frames + skip) * config["directions"]
        end = start + total_for_action - 1
        
        print(f"  {action_name:10s}: 帧{start:3d}-{end:3d} ({frames}帧/方向, skip={skip}, {ftime}ms)")


if __name__ == "__main__":
    print("怪物动画配置总览")
    print("=" * 60)
    
    for monster_name in MONSTER_ANIMATION_CONFIGS:
        print_frame_layout(monster_name)
