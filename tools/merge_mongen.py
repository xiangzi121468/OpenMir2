#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
合并新怪物配置到MonGen.txt
"""

import os
from datetime import datetime

BASE_DIR = os.path.dirname(os.path.dirname(__file__))
MONGEN_FILE = os.path.join(BASE_DIR, 'MirServer', 'Mir200', 'Envir', 'MonGen.txt')

# 新增怪物配置（使用Tab分隔）
NEW_CONFIG = """
; ============================================
; 新增怪物普通地图刷新配置
; OpenMir2 New Monster Spawn Configuration
; 添加时间: {timestamp}
; ============================================

; 新手区 - 大史莱姆/小史莱姆 (等级: 1-15)
0	280	300	20	小史莱姆	10	5
0	300	280	20	小史莱姆	10	5
0	320	320	15	大史莱姆	5	8
0	290	340	15	迷你史莱姆	15	3

; 中级区 - 吸血蝙蝠 (等级: 20-35)
D714	100	100	30	吸血蝙蝠	8	8
D714	150	150	25	吸血蝙蝠	6	8
D714	200	120	20	吸血蝙蝠	5	10

; 高级区 - 暗影刺客 (等级: 35-45)
D715	180	180	25	暗影刺客	5	12
D715	220	200	20	暗影刺客	4	15
D715	150	220	20	暗影刺客	4	15

; BOSS区 - 冰霜领主 (60分钟刷新, 等级: 40+)
D715	250	250	5	冰霜领主	1	60

; BOSS区 - 死灵召唤师 (60分钟刷新, 等级: 42+)
D716	150	150	5	死灵召唤师	1	60

; 终极BOSS - 烈焰魔龙 (120分钟刷新, 等级: 48+)
D717	80	80	3	烈焰魔龙	1	120

; 混合怪物区
D715	100	250	30	大史莱姆	3	10
D715	130	230	25	吸血蝙蝠	4	10
D716	200	200	25	暗影刺客	3	15
D716	180	220	20	大史莱姆	2	10
"""

def merge_config():
    print(f"读取文件: {MONGEN_FILE}")
    
    # 读取原文件（使用GBK编码，忽略错误）
    with open(MONGEN_FILE, 'rb') as f:
        raw_content = f.read()
    
    # 尝试解码
    try:
        content = raw_content.decode('gbk', errors='ignore')
    except:
        content = raw_content.decode('utf-8', errors='ignore')
    
    # 检查是否已经合并过
    if '新增怪物普通地图刷新配置' in content or 'New Monster Spawn Configuration' in content:
        # 删除之前的配置
        lines = content.split('\n')
        new_lines = []
        skip = False
        for line in lines:
            if '新增怪物普通地图刷新配置' in line or 'New Monster Spawn Configuration' in line:
                skip = True
                # 回退删除空行和分隔线
                while new_lines and (new_lines[-1].strip() == '' or new_lines[-1].startswith(';')):
                    if new_lines[-1].startswith('; =='):
                        new_lines.pop()
                        break
                    new_lines.pop()
                continue
            if not skip:
                new_lines.append(line)
        content = '\n'.join(new_lines)
        print("检测到已有配置，已清理")
    
    # 追加新配置
    timestamp = datetime.now().strftime('%Y-%m-%d %H:%M:%S')
    new_content = content.rstrip() + '\n' + NEW_CONFIG.format(timestamp=timestamp)
    
    # 写回文件（使用GBK编码）
    try:
        with open(MONGEN_FILE, 'w', encoding='gbk') as f:
            f.write(new_content)
    except:
        with open(MONGEN_FILE, 'w', encoding='utf-8') as f:
            f.write(new_content)
    
    print("合并完成！")
    print(f"\n新增怪物刷新配置:")
    print("- 小史莱姆 x20 (新手区)")
    print("- 大史莱姆 x10 (新手区)")
    print("- 迷你史莱姆 x15 (新手区)")
    print("- 吸血蝙蝠 x19 (D714)")
    print("- 暗影刺客 x16 (D715/D716)")
    print("- 冰霜领主 x1 (D715, 60分钟)")
    print("- 死灵召唤师 x1 (D716, 60分钟)")
    print("- 烈焰魔龙 x1 (D717, 120分钟)")

if __name__ == '__main__':
    merge_config()
