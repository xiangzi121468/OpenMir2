#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
传奇2客户端 .data 格式打包工具
将PNG图像打包成客户端使用的 .data 格式

文件格式 (TWMPackageImages):
- TPackDataHeader (文件头, 约65字节):
  - Title: string[40] (41字节: 1字节长度 + 40字节内容)
  - ImageCount: Integer (4字节)
  - IndexOffSet: Integer (4字节)
  - XVersion: Word (2字节)
  - Password: String[16] (17字节: 1字节长度 + 16字节内容)

- 索引数据: 每张图像4字节偏移量

- 图像数据:
  - TPackDataImageInfo (14字节):
    - m_nWidth: Word (2字节)
    - m_nHeight: Word (2字节)
    - bitCount: Byte (1字节)
    - m_wPx: SmallInt (2字节)
    - m_wPy: SmallInt (2字节)
    - m_Len: LongWord (4字节)
    - GraphicType: Byte (1字节, 0=DIB, 1=PNG)
  - 图像数据 (PNG或DIB格式)
"""

import os
import re
import struct
import argparse
from PIL import Image
import io


def create_pascal_short_string(s, max_len):
    """创建Pascal短字符串格式 (1字节长度 + 内容)"""
    encoded = s.encode('ascii', errors='replace')[:max_len]
    length = len(encoded)
    padding = max_len - length
    return bytes([length]) + encoded + bytes(padding)


def rgb_to_rgb565(r, g, b):
    """将RGB转换为RGB565格式"""
    return ((r & 0xF8) << 8) | ((g & 0xFC) << 3) | (b >> 3)


def image_to_rgb565(img):
    """将图像转换为RGB565格式数据"""
    if img.mode != 'RGBA':
        img = img.convert('RGBA')
    
    width, height = img.size
    pixels = img.load()
    data = bytearray()
    
    # 透明色使用 RGB(0,0,0) 或 alpha=0
    for y in range(height):
        for x in range(width):
            r, g, b, a = pixels[x, y]
            
            if a < 128:  # 透明像素
                # 使用黑色作为透明色
                pixel = 0
            else:
                pixel = rgb_to_rgb565(r, g, b)
            
            data.extend(struct.pack('<H', pixel))
    
    return bytes(data)


def pack_images_to_data(input_dir, output_file, start_index=0, title="StateEffect"):
    """将PNG图像打包成.data文件"""
    
    # 收集所有PNG文件并按数字排序
    png_files = []
    for f in os.listdir(input_dir):
        if f.lower().endswith('.png'):
            # 提取文件名中的数字
            match = re.search(r'(\d+)', f)
            if match:
                index = int(match.group(1))
                png_files.append((index, os.path.join(input_dir, f)))
    
    png_files.sort(key=lambda x: x[0])
    
    if not png_files:
        print(f"错误: 在 {input_dir} 中没有找到PNG文件")
        return False
    
    print(f"找到 {len(png_files)} 个PNG文件")
    print(f"索引范围: {png_files[0][0]} - {png_files[-1][0]}")
    
    # 计算最大索引以确定总图像数
    max_index = png_files[-1][0]
    min_index = png_files[0][0] if start_index == 0 else start_index
    
    # 如果指定了起始索引，调整
    if start_index > 0:
        total_images = max_index - start_index + 1
    else:
        total_images = max_index + 1
    
    print(f"总图像槽位: {total_images}")
    
    # 准备数据结构
    header_size = 41 + 4 + 4 + 2 + 17  # TPackDataHeader = 68字节
    index_size = total_images * 4  # 每个索引4字节
    
    # 文件头
    header = bytearray()
    header.extend(create_pascal_short_string(title, 40))
    header.extend(struct.pack('<I', total_images))  # ImageCount
    header.extend(struct.pack('<I', header_size))   # IndexOffSet (索引紧跟文件头)
    header.extend(struct.pack('<H', 1))             # XVersion
    header.extend(create_pascal_short_string('', 16))  # Password (空)
    
    # 索引数组 (初始化为0)
    index_array = [0] * total_images
    
    # 图像数据
    image_data = bytearray()
    current_offset = header_size + index_size  # 图像数据起始位置
    
    # 创建索引到文件的映射
    file_map = {idx: path for idx, path in png_files}
    
    for i in range(total_images):
        actual_index = i + (start_index if start_index > 0 else 0)
        
        if actual_index in file_map:
            img_path = file_map[actual_index]
            
            try:
                img = Image.open(img_path)
                width, height = img.size
                
                # 使用PNG格式存储 (GraphicType=1)
                png_buffer = io.BytesIO()
                img.save(png_buffer, format='PNG')
                png_data = png_buffer.getvalue()
                
                # 图像信息头 (14字节)
                img_info = struct.pack('<HHBHHI B',
                    width,          # m_nWidth
                    height,         # m_nHeight
                    32,             # bitCount (32位RGBA)
                    0,              # m_wPx (X偏移)
                    0,              # m_wPy (Y偏移)
                    len(png_data),  # m_Len
                    1               # GraphicType (1=PNG)
                )
                
                # 记录索引
                index_array[i] = current_offset
                
                # 添加图像数据
                image_data.extend(img_info)
                image_data.extend(png_data)
                
                current_offset += len(img_info) + len(png_data)
                
                print(f"  处理: {os.path.basename(img_path)} ({width}x{height})")
                
            except Exception as e:
                print(f"  警告: 无法处理 {img_path}: {e}")
                index_array[i] = 0
        else:
            # 空槽位
            index_array[i] = 0
    
    # 写入文件
    with open(output_file, 'wb') as f:
        f.write(header)
        
        # 写入索引
        for offset in index_array:
            f.write(struct.pack('<I', offset))
        
        # 写入图像数据
        f.write(image_data)
    
    print(f"\n已生成: {output_file}")
    print(f"文件大小: {os.path.getsize(output_file)} 字节")
    
    return True


def merge_to_existing(existing_file, input_dir, start_index, output_file=None):
    """将新图像合并到现有的.data文件"""
    
    if output_file is None:
        output_file = existing_file
    
    # TODO: 实现合并逻辑
    print("合并功能待实现，请使用独立打包模式")
    return False


def main():
    parser = argparse.ArgumentParser(description='将PNG图像打包成传奇2客户端.data格式')
    parser.add_argument('--input', '-i', required=True, help='输入PNG文件目录')
    parser.add_argument('--output', '-o', required=True, help='输出.data文件路径')
    parser.add_argument('--start-index', '-s', type=int, default=0, help='起始索引号')
    parser.add_argument('--title', '-t', default='StateEffect', help='文件标题')
    parser.add_argument('--merge', '-m', help='合并到现有文件')
    
    args = parser.parse_args()
    
    if not os.path.isdir(args.input):
        print(f"错误: 输入目录不存在: {args.input}")
        return 1
    
    if args.merge:
        success = merge_to_existing(args.merge, args.input, args.start_index, args.output)
    else:
        success = pack_images_to_data(args.input, args.output, args.start_index, args.title)
    
    return 0 if success else 1


if __name__ == '__main__':
    exit(main())
