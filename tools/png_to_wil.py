#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
PNG精灵图转WIL格式工具
用于将PNG精灵图转换为传奇2客户端使用的WIL/WIX格式

WIL文件格式:
- 文件头: Title(44字节) + ImageCount(4) + ColorCount(4) + PaletteSize(4) + VerFlag(4) = 60字节
- 图像数据: 每个图像 = ImageInfo(12字节) + 像素数据

WIX索引文件格式:
- 文件头: Title(44字节) + IndexCount(4) + VerFlag(4) = 52字节
- 索引数据: 每个图像偏移量(4字节) × 图像数量

用法: python png_to_wil.py <sprite_sheet.png> <output_name> [frame_width] [frame_height]
"""

import os
import sys
import struct
from PIL import Image

class WILWriter:
    """WIL/WIX文件写入器"""
    
    def __init__(self, output_name, bit_count=16):
        self.output_name = output_name
        self.bit_count = bit_count  # 16位色
        self.images = []  # [(width, height, px, py, pixel_data), ...]
    
    def add_image(self, img, offset_x=0, offset_y=0):
        """添加一张图片"""
        if img.mode != 'RGBA':
            img = img.convert('RGBA')
        
        width, height = img.size
        pixels = img.load()
        
        # 转换为16位565格式 (或带透明的格式)
        pixel_data = bytearray()
        for y in range(height):
            for x in range(width):
                r, g, b, a = pixels[x, y]
                if a < 128:  # 透明像素
                    # 使用黑色(0,0,0)作为透明色
                    pixel_data.extend(struct.pack('<H', 0))
                else:
                    # RGB565格式
                    r5 = (r >> 3) & 0x1F
                    g6 = (g >> 2) & 0x3F
                    b5 = (b >> 3) & 0x1F
                    rgb565 = (r5 << 11) | (g6 << 5) | b5
                    pixel_data.extend(struct.pack('<H', rgb565))
        
        self.images.append((width, height, offset_x, offset_y, bytes(pixel_data)))
    
    def save(self):
        """保存WIL和WIX文件"""
        wil_path = f"{self.output_name}.wil"
        wix_path = f"{self.output_name}.wix"
        
        # 计算文件结构
        image_count = len(self.images)
        color_count = 65536  # 16位色
        palette_size = 0  # 16位色不需要调色板
        
        # WIL文件头 (60字节，但实际只写56字节，VerFlag=0时)
        # Title: 44字节 (1字节长度 + 43字节字符串，pascal风格)
        title = "WEMADE Entertainment inc."
        
        # 准备WIL数据
        wil_data = bytearray()
        
        # 写入文件头 (版本1格式，无VerFlag)
        # Title: 1字节长度 + 43字节内容 = 44字节
        title_bytes = title.encode('ascii')[:43].ljust(43, b'\x00')
        wil_data.append(len(title))  # 长度字节
        wil_data.extend(title_bytes)
        
        # ImageCount, ColorCount, PaletteSize (各4字节)
        wil_data.extend(struct.pack('<I', image_count))
        wil_data.extend(struct.pack('<I', color_count))
        wil_data.extend(struct.pack('<I', palette_size))
        
        header_size = len(wil_data)  # 应该是56字节
        
        # 计算每个图像的偏移量并写入图像数据
        offsets = []
        for width, height, px, py, pixel_data in self.images:
            offsets.append(len(wil_data))
            
            # 写入图像信息 (12字节: width, height, px, py, 各2字节)
            wil_data.extend(struct.pack('<H', width))   # m_nWidth
            wil_data.extend(struct.pack('<H', height))  # m_nHeight
            wil_data.extend(struct.pack('<h', px))      # m_wPx (signed)
            wil_data.extend(struct.pack('<h', py))      # m_wPy (signed)
            # 版本1不需要 ImageVersion 和 nSize
            
            # 写入像素数据
            wil_data.extend(pixel_data)
        
        # 保存WIL文件
        with open(wil_path, 'wb') as f:
            f.write(wil_data)
        
        # 准备WIX索引数据
        wix_data = bytearray()
        
        # WIX文件头 (版本1格式，48字节)
        wix_title = "WEMADE Entertainment inc."
        wix_title_bytes = wix_title.encode('ascii')[:43].ljust(43, b'\x00')
        wix_data.append(len(wix_title))
        wix_data.extend(wix_title_bytes)
        wix_data.extend(struct.pack('<I', image_count))  # IndexCount
        
        # 写入索引
        for offset in offsets:
            wix_data.extend(struct.pack('<I', offset))
        
        # 保存WIX文件
        with open(wix_path, 'wb') as f:
            f.write(wix_data)
        
        print(f"已生成: {wil_path} ({len(wil_data)} 字节, {image_count} 张图片)")
        print(f"已生成: {wix_path} ({len(wix_data)} 字节)")
        return wil_path, wix_path


def split_sprite_sheet(sprite_path, frame_width, frame_height):
    """将精灵图切割成单独的帧"""
    img = Image.open(sprite_path)
    img_width, img_height = img.size
    
    frames = []
    cols = img_width // frame_width
    rows = img_height // frame_height
    
    for row in range(rows):
        for col in range(cols):
            left = col * frame_width
            top = row * frame_height
            right = left + frame_width
            bottom = top + frame_height
            
            frame = img.crop((left, top, right, bottom))
            
            # 检查是否为空帧（全透明）
            if frame.mode == 'RGBA':
                pixels = frame.getdata()
                if all(p[3] == 0 for p in pixels):
                    # 空帧，跳过或添加占位
                    pass
            
            frames.append(frame)
    
    return frames


def trim_image(img):
    """裁剪图片的透明边缘，返回裁剪后的图片和偏移量"""
    if img.mode != 'RGBA':
        img = img.convert('RGBA')
    
    # 获取非透明区域的边界
    bbox = img.getbbox()
    if bbox is None:
        # 完全透明的图片
        return Image.new('RGBA', (1, 1), (0, 0, 0, 0)), 0, 0
    
    # 计算偏移量（相对于原图中心）
    orig_width, orig_height = img.size
    left, top, right, bottom = bbox
    
    # 偏移量 = 裁剪后图片中心相对于原图中心的位置
    new_width = right - left
    new_height = bottom - top
    
    # 传奇使用的是左上角偏移
    offset_x = left - orig_width // 2 + new_width // 2
    offset_y = top - orig_height // 2 + new_height // 2
    
    cropped = img.crop(bbox)
    return cropped, offset_x, offset_y


def convert_sprite_to_wil(sprite_path, output_name, frame_width=64, frame_height=64, trim=True):
    """
    将精灵图转换为WIL格式
    
    参数:
        sprite_path: PNG精灵图路径
        output_name: 输出文件名（不含扩展名）
        frame_width: 每帧宽度
        frame_height: 每帧高度
        trim: 是否裁剪透明边缘
    """
    print(f"正在处理: {sprite_path}")
    print(f"帧大小: {frame_width}x{frame_height}")
    
    # 切割精灵图
    frames = split_sprite_sheet(sprite_path, frame_width, frame_height)
    print(f"切割出 {len(frames)} 帧")
    
    # 创建WIL写入器
    writer = WILWriter(output_name)
    
    # 添加每一帧
    for i, frame in enumerate(frames):
        if trim:
            trimmed, ox, oy = trim_image(frame)
            writer.add_image(trimmed, ox, oy)
        else:
            writer.add_image(frame, 0, 0)
    
    # 保存文件
    writer.save()
    print("转换完成!")


def main():
    if len(sys.argv) < 3:
        print("用法: python png_to_wil.py <sprite_sheet.png> <output_name> [frame_width] [frame_height]")
        print("")
        print("示例:")
        print("  python png_to_wil.py monster_frost_lord_sprite.png Mon41 64 64")
        print("")
        print("参数:")
        print("  sprite_sheet.png  - PNG精灵图文件")
        print("  output_name       - 输出文件名（将生成 output_name.wil 和 output_name.wix）")
        print("  frame_width       - 每帧宽度（默认64）")
        print("  frame_height      - 每帧高度（默认64）")
        return
    
    sprite_path = sys.argv[1]
    output_name = sys.argv[2]
    frame_width = int(sys.argv[3]) if len(sys.argv) > 3 else 64
    frame_height = int(sys.argv[4]) if len(sys.argv) > 4 else 64
    
    if not os.path.exists(sprite_path):
        print(f"错误: 文件不存在 - {sprite_path}")
        return
    
    convert_sprite_to_wil(sprite_path, output_name, frame_width, frame_height)


if __name__ == "__main__":
    main()
