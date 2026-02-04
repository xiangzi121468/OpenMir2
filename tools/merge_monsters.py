#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
合并新怪物刷新配置到MonGen.txt
"""

import os

MONGEN_TXT = r"d:\aiwork\OpenMir2\MirServer\Mir200\Envir\MonGen.txt"

# 新怪物配置（GBK编码）
NEW_MONSTERS_CONFIG = """
; ============================================
; 新增怪物刷新配置 (2026-02-04)
; ============================================

; 新手区 - 史莱姆系列 (等级1-15)
0         280   300   20   小史莱姆      10   5
0         300   280   20   小史莱姆      10   5
0         320   320   15   大史莱姆      5    8
0         290   340   15   迷你史莱姆    15   3

; 中级区 - 吸血蝙蝠 (等级20-35)
D714      100   100   30   吸血蝙蝠      8    8
D714      150   150   25   吸血蝙蝠      6    8
D714      200   120   20   吸血蝙蝠      5    10

; 高级区 - 暗影刺客 (等级35-45)
D715      180   180   25   暗影刺客      5    12
D715      220   200   20   暗影刺客      4    15
D715      150   220   20   暗影刺客      4    15

; BOSS - 冰霜领主 (等级40+)
D715      250   250   5    冰霜领主      1    60

; BOSS - 死灵召唤师 (等级42+)
D716      150   150   5    死灵召唤师    1    60

; 终极BOSS - 烈焰魔龙 (等级48+)
D717      80    80    3    烈焰魔龙      1    120

; 混合刷新区
D715      100   250   30   大史莱姆      3    10
D715      130   230   25   吸血蝙蝠      4    10
D716      200   200   25   暗影刺客      3    15
D716      180   220   20   大史莱姆      2    10

; ============================================
"""

def main():
    print("=" * 50)
    print("OpenMir2 Monster Config Merge Tool")
    print("=" * 50)
    
    # 读取现有配置
    try:
        with open(MONGEN_TXT, 'r', encoding='gbk', errors='ignore') as f:
            content = f.read()
    except:
        with open(MONGEN_TXT, 'r', encoding='utf-8', errors='ignore') as f:
            content = f.read()
    
    # 检查是否已添加
    if "小史莱姆" in content or "新增怪物刷新配置" in content:
        print("New monster config already exists, skipping")
        return
    
    # 追加新配置
    try:
        with open(MONGEN_TXT, 'a', encoding='gbk') as f:
            f.write(NEW_MONSTERS_CONFIG)
    except:
        with open(MONGEN_TXT, 'a', encoding='utf-8') as f:
            f.write(NEW_MONSTERS_CONFIG)
    
    print("OK: Monster config merged to MonGen.txt")
    print("=" * 50)

if __name__ == "__main__":
    main()
