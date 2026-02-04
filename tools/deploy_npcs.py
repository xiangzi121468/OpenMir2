#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
部署NPC脚本和配置
"""

import os
import shutil

# 路径配置
DOC_SCRIPTS = r"d:\aiwork\OpenMir2\doc\scripts"
SERVER_NPC_DEF = r"d:\aiwork\OpenMir2\MirServer\Mir200\Envir\Npc_Def"
NPCS_TXT = r"d:\aiwork\OpenMir2\MirServer\Mir200\Envir\Npcs.txt"

# NPC配置 (名称, 代码, 地图, x, y, 范围, 图标)
NEW_NPCS = [
    ("装备鉴定师", 1001, "0", 340, 340, 0, 14),      # 比奇省
    ("神秘卷轴商人", 1002, "0", 345, 340, 0, 14),    # 比奇省
    ("VIP大厅传送员", 1003, "0", 350, 340, 0, 26),   # 比奇省
    ("沙城皇宫使者", 1004, "D716", 15, 15, 0, 8),    # 沙巴克皇宫
]

# NPC脚本映射 (源文件 -> 目标文件)
NPC_SCRIPTS = {
    "装备鉴定师.txt": "装备鉴定师-1001.txt",
    "神秘卷轴商人.txt": "神秘卷轴商人-1002.txt",
    "VIP大厅传送员.txt": "VIP大厅传送员-1003.txt",
    "沙城皇宫使者.txt": "沙城皇宫使者-1004.txt",
}

def copy_npc_scripts():
    """复制NPC脚本到服务器目录"""
    print("复制NPC脚本...")
    for src_name, dst_name in NPC_SCRIPTS.items():
        src_path = os.path.join(DOC_SCRIPTS, src_name)
        dst_path = os.path.join(SERVER_NPC_DEF, dst_name)
        
        if os.path.exists(src_path):
            shutil.copy2(src_path, dst_path)
            print(f"  OK: {src_name} -> {dst_name}")
        else:
            print(f"  FAIL: {src_name} not found")

def update_npcs_txt():
    """更新Npcs.txt添加新NPC"""
    print("\n更新Npcs.txt...")
    
    # 读取现有内容
    try:
        with open(NPCS_TXT, 'r', encoding='gbk', errors='ignore') as f:
            content = f.read()
    except:
        with open(NPCS_TXT, 'r', encoding='utf-8', errors='ignore') as f:
            content = f.read()
    
    # 检查是否已添加
    if "装备鉴定师" in content:
        print("  NPC已存在，跳过")
        return
    
    # 添加新NPC
    new_lines = "\n; === 新增NPC ===\n"
    for npc in NEW_NPCS:
        name, code, map_name, x, y, scope, icon = npc
        line = f"{name}           {code}     {map_name}  {x}    {y}   {scope}     {icon}\n"
        new_lines += line
        print(f"  + {name}")
    
    # 写入
    try:
        with open(NPCS_TXT, 'a', encoding='gbk') as f:
            f.write(new_lines)
    except:
        with open(NPCS_TXT, 'a', encoding='utf-8') as f:
            f.write(new_lines)
    
    print("  OK: Npcs.txt updated")

def main():
    print("=" * 50)
    print("OpenMir2 NPC部署工具")
    print("=" * 50)
    
    copy_npc_scripts()
    update_npcs_txt()
    
    print("\n" + "=" * 50)
    print("部署完成！")
    print("=" * 50)

if __name__ == "__main__":
    main()
