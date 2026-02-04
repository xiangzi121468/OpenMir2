#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
批量转换新怪物精灵图为.data格式 (TWMPackageImages)
"""

import os
import sys

sys.path.insert(0, os.path.dirname(os.path.abspath(__file__)))

from png_to_data import convert_sprite_to_data

# 怪物精灵图配置
# (精灵图文件名, 输出名, 帧宽度, 帧高度)
# 输出文件将命名为 Mon41.data, Mon42.data 等
MONSTER_CONFIGS = [
    ("monster_frost_lord_sprite.png", "Mon41", 42, 64),      # 冰霜领主
    ("monster_shadow_assassin_sprite.png", "Mon42", 51, 64), # 暗影刺客
    ("monster_necro_summoner_sprite.png", "Mon43", 42, 64),  # 死灵召唤师
    ("monster_split_slime_sprite.png", "Mon44", 64, 64),     # 分裂史莱姆
    ("monster_vampire_bat_sprite.png", "Mon45", 64, 64),     # 吸血蝙蝠
    ("monster_inferno_wyrm_sprite.png", "Mon46", 42, 48),    # 烈焰魔龙
]

def main():
    sprites_dir = os.path.join(os.path.dirname(__file__), "..", "assets", "monsters")
    output_dir = os.path.join(os.path.dirname(__file__), "..", "assets", "data_output")
    os.makedirs(output_dir, exist_ok=True)
    
    print("=" * 60)
    print("批量转换怪物精灵图为.data格式")
    print("=" * 60)
    
    success_count = 0
    fail_count = 0
    
    for sprite_name, data_name, frame_w, frame_h in MONSTER_CONFIGS:
        sprite_path = os.path.join(sprites_dir, sprite_name)
        output_path = os.path.join(output_dir, data_name)
        
        print(f"\n处理: {sprite_name}")
        print(f"  → {data_name}.data")
        
        if not os.path.exists(sprite_path):
            print(f"  [错误] 文件不存在: {sprite_path}")
            fail_count += 1
            continue
        
        try:
            convert_sprite_to_data(sprite_path, output_path, frame_w, frame_h, trim=True, use_png=True)
            success_count += 1
        except Exception as e:
            print(f"  [错误] 转换失败: {e}")
            import traceback
            traceback.print_exc()
            fail_count += 1
    
    print("\n" + "=" * 60)
    print(f"转换完成: 成功 {success_count}, 失败 {fail_count}")
    print(f"输出目录: {output_dir}")
    print("=" * 60)
    
    if success_count > 0:
        print("\n使用说明:")
        print("1. 将生成的 .data 文件复制到客户端 Data 目录")
        print("2. 在客户端代码 MShare.pas 中添加资源加载:")
        print("   g_WMon41Img := CreateImages('Data\\Mon41.data', g_GameDevice);")
        print("3. 在 GetMonImg 函数中添加 case 分支")
        print("4. 更新服务端怪物的 Appr 值 (400, 410, 420...)")


if __name__ == "__main__":
    main()
