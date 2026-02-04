#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
更新配饰装备SQL文件中的Looks字段
使其使用AccessoryItems.data资源 (50000-60000范围)
"""

import re
import os

BASE_DIR = os.path.dirname(os.path.dirname(__file__))
SQL_FILE = os.path.join(BASE_DIR, 'sql', 'new_accessories.sql')

# 名称到新Looks值的映射
LOOKS_MAP = {
    # 戒指
    '冰晶戒指': 50050, '霜魄戒指': 50051, '凝霜戒指': 50052,
    '夜行戒指': 50053, '暗影戒指': 50054,
    '亡灵戒指': 50055, '噬魂戒指': 50056, '骸骨戒指': 50057,
    '血魄戒指': 50058, '血蝠戒指': 50059,
    '龙炎戒指': 50060, '龙魂戒指': 50061, '龙威戒指': 50062,
    '黏液戒指': 50063,
    # 靴子
    '寒冰之靴': 50200, '冰魄之靴': 50201, '凝霜之靴': 50202,
    '暗夜之靴': 50203,
    '亡灵之靴': 50204, '噬魂之靴': 50205, '骸骨之靴': 50206,
    '血影之靴': 50207,
    '龙炎之靴': 50208, '龙魂之靴': 50209, '龙威之靴': 50210,
    '黏液之靴': 50211,
    # 腰带
    '寒冰腰带': 50250, '冰魄腰带': 50251, '凝霜腰带': 50252,
    '暗影腰带': 50253,
    '亡灵腰带': 50254, '噬魂腰带': 50255, '骸骨腰带': 50256,
    '血蝠腰带': 50257,
    '龙炎腰带': 50258, '龙魂腰带': 50259, '龙威腰带': 50260,
    '黏液腰带': 50261,
}


def update_looks():
    """更新SQL文件中的Looks值"""
    
    print(f"读取文件: {SQL_FILE}")
    
    with open(SQL_FILE, 'r', encoding='utf-8') as f:
        content = f.read()
    
    updated_count = 0
    
    for name, new_looks in LOOKS_MAP.items():
        # 匹配模式: VALUES (Idx, 'Name', StdMode, Shape, Weight, DuraMax, ?, ?, Looks, ...)
        # stditems表结构中Looks是第9个字段
        pattern = rf"(INSERT INTO `stditems` VALUES \(\d+, '{re.escape(name)}', \d+, \d+, \d+, \d+, \d+, \d+, )(\d+)(,)"
        
        def replacer(match):
            return f"{match.group(1)}{new_looks}{match.group(3)}"
        
        new_content, count = re.subn(pattern, replacer, content)
        if count > 0:
            content = new_content
            updated_count += count
            print(f"  更新: {name} -> Looks={new_looks}")
    
    with open(SQL_FILE, 'w', encoding='utf-8') as f:
        f.write(content)
    
    print(f"\n完成! 共更新 {updated_count} 条记录")


if __name__ == '__main__':
    update_looks()
