#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
PNG图像打包成WIL格式工具
用于将目录下的PNG图像文件打包成传奇2客户端使用的WIL/WIX格式

WIL文件格式:
- 文件头 (44字节):
  - 4字节: 标识 "ILIB" 或版本号
  - 2字节: 图像数量
  - 2字节: 颜色位深度 (8/16位)
  - 2字节: 调色板标志
  - 后面是调色板数据(如果是8位色)
- 图像数据:
  - 2字节: 宽度
  - 2字节: 高度
  - 2字节: X偏移
  - 2字节: Y偏移
  - 像素数据 (RGB565格式)

WIX索引文件格式:
- 4字节: 图像数量 (int32)
- 每张图像: 4字节偏移量 (int32)

用法: python pack_to_wil.py --input <input_dir> --output <output_name> [--start-index <index>]
"""

import os
import sys
import struct
import argparse
import re
from PIL import Image


def rgb_to_rgb565(r, g, b):
    """将RGB颜色转换为RGB565格式"""
    r5 = (r >> 3) & 0x1F
    g6 = (g >> 2) & 0x3F
    b5 = (b >> 3) & 0x1F
    return (r5 << 11) | (g6 << 5) | b5


def is_transparent_color(r, g, b):
    """检查是否为透明色 (RGB(255,0,255) 粉色)"""
    return r == 255 and g == 0 and b == 255


def convert_image_to_rgb565(img):
    """
    将PIL图像转换为RGB565格式的字节数据
    返回: (width, height, pixel_data)
    """
    if img.mode != 'RGBA':
        img = img.convert('RGBA')
    
    width, height = img.size
    pixels = img.load()
    
    pixel_data = bytearray()
    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]
            
            # 检查透明度: alpha < 128 或 粉色透明色
            if a < 128 or is_transparent_color(r, g, b):
                # 透明像素使用0
                pixel_data.extend(struct.pack('<H', 0))
            else:
                # RGB565格式
                rgb565 = rgb_to_rgb565(r, g, b)
                pixel_data.extend(struct.pack('<H', rgb565))
    
    return width, height, bytes(pixel_data)


def extract_number_from_filename(filename):
    """从文件名中提取数字用于排序"""
    match = re.search(r'(\d+)', filename)
    if match:
        return int(match.group(1))
    return 0


def get_png_files(input_dir):
    """获取目录下所有PNG文件并按数字顺序排序"""
    png_files = []
    for filename in os.listdir(input_dir):
        if filename.lower().endswith('.png'):
            filepath = os.path.join(input_dir, filename)
            if os.path.isfile(filepath):
                png_files.append(filepath)
    
    # 按文件名中的数字排序
    png_files.sort(key=lambda x: extract_number_from_filename(os.path.basename(x)))
    return png_files


def pack_to_wil(input_dir, output_name, start_index=0):
    """
    将目录下的PNG文件打包成WIL格式
    
    参数:
        input_dir: 输入目录路径
        output_name: 输出文件名（不含扩展名）
        start_index: 起始索引号（用于命名）
    """
    if not os.path.isdir(input_dir):
        print(f"错误: 目录不存在 - {input_dir}")
        return False
    
    # 获取所有PNG文件
    png_files = get_png_files(input_dir)
    if not png_files:
        print(f"错误: 在目录 {input_dir} 中未找到PNG文件")
        return False
    
    print(f"找到 {len(png_files)} 个PNG文件")
    
    # 准备图像数据
    images = []
    for i, png_path in enumerate(png_files):
        try:
            img = Image.open(png_path)
            width, height, pixel_data = convert_image_to_rgb565(img)
            # X偏移和Y偏移默认为0，可以根据需要调整
            images.append((width, height, 0, 0, pixel_data))
            print(f"  处理: {os.path.basename(png_path)} ({width}x{height})")
        except Exception as e:
            print(f"警告: 无法处理 {png_path}: {e}")
            continue
    
    if not images:
        print("错误: 没有成功处理的图像")
        return False
    
    image_count = len(images)
    print(f"\n开始打包 {image_count} 张图像...")
    
    # 准备WIL文件数据
    wil_data = bytearray()
    
    # WIL文件头 (44字节)
    # 4字节: 标识 "ILIB"
    wil_data.extend(b'ILIB')
    
    # 2字节: 图像数量
    wil_data.extend(struct.pack('<H', image_count))
    
    # 2字节: 颜色位深度 (16位)
    wil_data.extend(struct.pack('<H', 16))
    
    # 2字节: 调色板标志 (16位色不需要调色板，设为0)
    wil_data.extend(struct.pack('<H', 0))
    
    # 剩余36字节填充0 (44 - 4 - 2 - 2 - 2 = 34字节，但通常填充到44字节)
    # 根据格式说明，文件头总共44字节
    wil_data.extend(b'\x00' * (44 - len(wil_data)))
    
    # 写入图像数据并记录偏移量
    offsets = []
    for width, height, offset_x, offset_y, pixel_data in images:
        # 记录当前偏移量（相对于文件开始）
        offsets.append(len(wil_data))
        
        # 2字节: 宽度
        wil_data.extend(struct.pack('<H', width))
        
        # 2字节: 高度
        wil_data.extend(struct.pack('<H', height))
        
        # 2字节: X偏移 (signed short)
        wil_data.extend(struct.pack('<h', offset_x))
        
        # 2字节: Y偏移 (signed short)
        wil_data.extend(struct.pack('<h', offset_y))
        
        # 像素数据 (RGB565格式)
        wil_data.extend(pixel_data)
    
    # 保存WIL文件
    wil_path = f"{output_name}.wil"
    with open(wil_path, 'wb') as f:
        f.write(wil_data)
    print(f"已生成: {wil_path} ({len(wil_data)} 字节)")
    
    # 准备WIX索引文件数据
    wix_data = bytearray()
    
    # 4字节: 图像数量 (int32)
    wix_data.extend(struct.pack('<I', image_count))
    
    # 每张图像: 4字节偏移量 (int32)
    for offset in offsets:
        wix_data.extend(struct.pack('<I', offset))
    
    # 保存WIX文件
    wix_path = f"{output_name}.wix"
    with open(wix_path, 'wb') as f:
        f.write(wix_data)
    print(f"已生成: {wix_path} ({len(wix_data)} 字节)")
    
    print(f"\n打包完成! 共 {image_count} 张图像")
    return True


def main():
    parser = argparse.ArgumentParser(
        description='将PNG图像打包成传奇2客户端使用的WIL格式文件',
        formatter_class=argparse.RawDescriptionHelpFormatter,
        epilog="""
使用示例:
  python pack_to_wil.py --input assets/weapon_effects_v2/type_100 --output Data/State --start-index 30000

注意:
  - 支持透明色 (RGB(255,0,255) 粉色作为透明色)
  - 图像按文件名中的数字顺序排序
  - 输出文件: <output_name>.wil 和 <output_name>.wix
        """
    )
    
    parser.add_argument(
        '--input', '-i',
        required=True,
        help='输入目录路径（包含PNG文件的目录）'
    )
    
    parser.add_argument(
        '--output', '-o',
        required=True,
        help='输出文件名（不含扩展名，将生成 .wil 和 .wix 文件）'
    )
    
    parser.add_argument(
        '--start-index',
        type=int,
        default=0,
        help='起始索引号（用于命名，默认: 0）'
    )
    
    args = parser.parse_args()
    
    # 确保输出目录存在
    output_dir = os.path.dirname(args.output)
    if output_dir and not os.path.exists(output_dir):
        os.makedirs(output_dir, exist_ok=True)
    
    success = pack_to_wil(args.input, args.output, args.start_index)
    
    if success:
        sys.exit(0)
    else:
        sys.exit(1)


if __name__ == "__main__":
    main()
