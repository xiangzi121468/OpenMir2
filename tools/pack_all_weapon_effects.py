#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
打包所有武器特效到 ShineEffect.data
索引映射: GetWStateImg(30000+x) -> ShineEffect.data[x]
"""

import os
import struct
import io
from PIL import Image

PROJECT_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# 特效配置
EFFECT_CONFIGS = {
    100: {"name": "冰霜之刃", "frames": 20, "shine_start": 0},      # 30000-30019 -> 0-19
    101: {"name": "冰魄", "frames": 20, "shine_start": 20},         # 30020-30039 -> 20-39
    102: {"name": "凝霜", "frames": 20, "shine_start": 40},         # 30040-30059 -> 40-59
    103: {"name": "暗影匕首", "frames": 20, "shine_start": 60},     # 30060-30079 -> 60-79
    105: {"name": "亡灵法杖", "frames": 20, "shine_start": 100},    # 30100-30119 -> 100-119
    110: {"name": "地狱火", "frames": 20, "shine_start": 200},      # 30200-30219 -> 200-219
    115: {"name": "龙之光环", "frames": 20, "shine_start": 300},    # 30300-30319 -> 300-319
}


def create_pascal_short_string(s, max_len):
    """创建Pascal短字符串格式"""
    encoded = s.encode('ascii', errors='replace')[:max_len]
    length = len(encoded)
    padding = max_len - length
    return bytes([length]) + encoded + bytes(padding)


def pack_all_effects():
    """打包所有特效到ShineEffect.data"""
    
    effects_dir = os.path.join(PROJECT_ROOT, "assets", "weapon_effects_v2")
    output_file = os.path.join(PROJECT_ROOT, "MirClient", "Data", "NewShineEffect.data")
    
    # 确定最大索引
    max_index = 0
    for effect_type, config in EFFECT_CONFIGS.items():
        end_index = config["shine_start"] + config["frames"]
        if end_index > max_index:
            max_index = end_index
    
    # 预留一些空间
    total_images = max_index + 100
    
    print(f"总图像槽位: {total_images}")
    
    # 文件头 (68字节)
    header_size = 41 + 4 + 4 + 2 + 17
    index_size = total_images * 4
    
    header = bytearray()
    header.extend(create_pascal_short_string("ShineEffect", 40))
    header.extend(struct.pack('<I', total_images))
    header.extend(struct.pack('<I', header_size))
    header.extend(struct.pack('<H', 1))
    header.extend(create_pascal_short_string('', 16))
    
    # 索引数组
    index_array = [0] * total_images
    
    # 图像数据
    image_data = bytearray()
    current_offset = header_size + index_size
    
    # 处理每种特效
    for effect_type, config in EFFECT_CONFIGS.items():
        type_dir = os.path.join(effects_dir, f"type_{effect_type}")
        
        if not os.path.exists(type_dir):
            print(f"[跳过] type_{effect_type}/ 目录不存在")
            continue
        
        print(f"\n处理 {config['name']} (type_{effect_type})...")
        
        base_index = 30000 + (effect_type - 100) * 20
        shine_start = config["shine_start"]
        
        for frame in range(config["frames"]):
            img_file = os.path.join(type_dir, f"effect_{base_index + frame:05d}.png")
            shine_index = shine_start + frame
            
            if not os.path.exists(img_file):
                print(f"  [缺失] effect_{base_index + frame:05d}.png")
                continue
            
            try:
                img = Image.open(img_file)
                width, height = img.size
                
                # 转换为PNG数据
                png_buffer = io.BytesIO()
                img.save(png_buffer, format='PNG')
                png_data = png_buffer.getvalue()
                
                # 图像信息头 (14字节)
                img_info = struct.pack('<HHBHHI B',
                    width, height, 32, 0, 0, len(png_data), 1
                )
                
                # 记录索引
                index_array[shine_index] = current_offset
                
                # 添加图像数据
                image_data.extend(img_info)
                image_data.extend(png_data)
                
                current_offset += len(img_info) + len(png_data)
                
            except Exception as e:
                print(f"  [错误] {img_file}: {e}")
        
        print(f"  完成: ShineEffect索引 {shine_start}-{shine_start + config['frames'] - 1}")
    
    # 写入文件
    os.makedirs(os.path.dirname(output_file), exist_ok=True)
    
    with open(output_file, 'wb') as f:
        f.write(header)
        for offset in index_array:
            f.write(struct.pack('<I', offset))
        f.write(image_data)
    
    print(f"\n已生成: {output_file}")
    print(f"文件大小: {os.path.getsize(output_file):,} 字节")
    
    # 生成索引映射说明
    print("\n索引映射说明:")
    print("=" * 60)
    print(f"{'特效类型':<10} {'武器名称':<12} {'GetWStateImg索引':<18} {'ShineEffect索引':<15}")
    print("-" * 60)
    
    for effect_type, config in EFFECT_CONFIGS.items():
        base = 30000 + (effect_type - 100) * 20
        shine_start = config["shine_start"]
        print(f"{effect_type:<10} {config['name']:<12} {base}-{base+19:<18} {shine_start}-{shine_start+19:<15}")
    
    return output_file


if __name__ == "__main__":
    pack_all_effects()
