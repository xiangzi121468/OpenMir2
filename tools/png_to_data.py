#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
PNG精灵图转.data格式工具
用于将PNG精灵图转换为传奇2客户端使用的.data格式 (TWMPackageImages)

.data文件格式:
- 文件头(加密): Title(44) + ImageCount(4) + IndexOffSet(4) + XVersion(2) + Password(17) = 71字节 -> 80字节加密块
- 图像数据: 每个图像 = ImageInfo(14字节) + 像素数据
- 索引数据: 每个图像偏移量(4字节) × 图像数量

TPackDataImageInfo (14字节):
  m_nWidth: Word (2)
  m_nHeight: Word (2)
  bitCount: Byte (1)
  m_wPx: SmallInt (2)
  m_wPy: SmallInt (2)
  m_Len: LongWord (4)
  GraphicType: Byte (1) - 0=DIB, 1=PNG

用法: python png_to_data.py <sprite_sheet.png> <output_name> [frame_width] [frame_height]
"""

import os
import sys
import struct
import zlib
from PIL import Image
import io

# 简单的加密/解密 (与客户端的uEDCode兼容)
# 注意: 这是简化版本，实际客户端使用更复杂的加密
def simple_encode(data, key):
    """简单XOR加密 - 仅用于演示，实际需要匹配客户端加密"""
    # 简化: 不加密，直接返回填充到80字节
    result = bytearray(80)
    data_bytes = bytes(data)
    for i in range(min(len(data_bytes), 80)):
        result[i] = data_bytes[i]
    return bytes(result)


class DataWriter:
    """TWMPackageImages (.data)文件写入器"""
    
    def __init__(self, output_name, use_png=True):
        self.output_name = output_name
        self.use_png = use_png  # 是否使用PNG格式存储
        self.images = []  # [(width, height, px, py, pixel_data, is_png), ...]
    
    def add_image(self, img, offset_x=0, offset_y=0):
        """添加一张图片"""
        if img.mode != 'RGBA':
            img = img.convert('RGBA')
        
        width, height = img.size
        
        if self.use_png:
            # 使用PNG格式 (GraphicType = 1)
            png_buffer = io.BytesIO()
            img.save(png_buffer, format='PNG', optimize=True)
            pixel_data = png_buffer.getvalue()
            is_png = True
        else:
            # 使用16位RGB565格式 (GraphicType = 0)
            pixels = img.load()
            raw_data = bytearray()
            for y in range(height):
                for x in range(width):
                    r, g, b, a = pixels[x, y]
                    if a < 128:
                        # 透明像素用黑色
                        raw_data.extend(struct.pack('<H', 0))
                    else:
                        # RGB565
                        r5 = (r >> 3) & 0x1F
                        g6 = (g >> 2) & 0x3F
                        b5 = (b >> 3) & 0x1F
                        rgb565 = (r5 << 11) | (g6 << 5) | b5
                        raw_data.extend(struct.pack('<H', rgb565))
            
            # 压缩数据
            pixel_data = zlib.compress(bytes(raw_data), 9)
            is_png = False
        
        self.images.append((width, height, offset_x, offset_y, pixel_data, is_png))
    
    def save(self):
        """保存.data文件"""
        data_path = f"{self.output_name}.data"
        
        image_count = len(self.images)
        
        # 准备文件头 (TPackDataHeader)
        # Title: string[40] = 1字节长度 + 40字节内容 = 41字节
        # ImageCount: Integer = 4字节
        # IndexOffSet: Integer = 4字节
        # XVersion: Word = 2字节
        # Password: String[16] = 1字节长度 + 16字节内容 = 17字节
        # 总计: 41 + 4 + 4 + 2 + 17 = 68字节，但加密块需要80字节
        
        title = "OpenMir2 Resource File"
        password = ""
        
        header = bytearray()
        # Title (string[40])
        title_bytes = title.encode('gbk')[:40].ljust(40, b'\x00')
        header.append(min(len(title), 40))  # 长度
        header.extend(title_bytes)
        
        # 先计算图像数据大小来确定IndexOffSet
        # 文件头80字节 + 所有图像数据
        header_size = 80
        
        # 计算所有图像数据的总大小
        image_data_size = 0
        for width, height, px, py, pixel_data, is_png in self.images:
            image_data_size += 14 + len(pixel_data)  # ImageInfo + data
        
        index_offset = header_size + image_data_size
        
        # ImageCount (4字节)
        header.extend(struct.pack('<I', image_count))
        
        # IndexOffSet (4字节)
        header.extend(struct.pack('<I', index_offset))
        
        # XVersion (2字节) - 0表示不加密
        header.extend(struct.pack('<H', 0))
        
        # Password (string[16])
        pwd_bytes = password.encode('gbk')[:16].ljust(16, b'\x00')
        header.append(min(len(password), 16))
        header.extend(pwd_bytes)
        
        # 填充到68字节 (实际TPackDataHeader大小)
        while len(header) < 68:
            header.append(0)
        
        # 注意: 客户端会对前80字节进行加密/解密
        # 这里我们使用简化版本 - 填充到80字节
        header_80 = bytearray(80)
        for i in range(min(len(header), 80)):
            header_80[i] = header[i]
        
        # 准备完整文件数据
        file_data = bytearray(header_80)
        
        # 计算并存储每个图像的偏移量
        offsets = []
        current_offset = header_size
        
        # 写入图像数据
        for width, height, px, py, pixel_data, is_png in self.images:
            offsets.append(current_offset)
            
            # TPackDataImageInfo (14字节)
            # m_nWidth: Word (2)
            file_data.extend(struct.pack('<H', width))
            # m_nHeight: Word (2)
            file_data.extend(struct.pack('<H', height))
            # bitCount: Byte (1) - 16位或32位
            file_data.append(32 if is_png else 16)
            # m_wPx: SmallInt (2)
            file_data.extend(struct.pack('<h', px))
            # m_wPy: SmallInt (2)
            file_data.extend(struct.pack('<h', py))
            # m_Len: LongWord (4)
            file_data.extend(struct.pack('<I', len(pixel_data)))
            # GraphicType: Byte (1) - 0=DIB/压缩, 1=PNG
            file_data.append(1 if is_png else 0)
            
            # 像素数据
            file_data.extend(pixel_data)
            
            current_offset += 14 + len(pixel_data)
        
        # 写入索引数据
        for offset in offsets:
            file_data.extend(struct.pack('<I', offset))
        
        # 保存文件
        with open(data_path, 'wb') as f:
            f.write(file_data)
        
        print(f"已生成: {data_path} ({len(file_data)} 字节, {image_count} 张图片)")
        return data_path


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
            frames.append(frame)
    
    return frames


def trim_image(img):
    """裁剪图片的透明边缘，返回裁剪后的图片和偏移量"""
    if img.mode != 'RGBA':
        img = img.convert('RGBA')
    
    bbox = img.getbbox()
    if bbox is None:
        return Image.new('RGBA', (1, 1), (0, 0, 0, 0)), 0, 0
    
    orig_width, orig_height = img.size
    left, top, right, bottom = bbox
    new_width = right - left
    new_height = bottom - top
    
    offset_x = left - orig_width // 2 + new_width // 2
    offset_y = top - orig_height // 2 + new_height // 2
    
    cropped = img.crop(bbox)
    return cropped, offset_x, offset_y


def convert_sprite_to_data(sprite_path, output_name, frame_width=64, frame_height=64, trim=True, use_png=True):
    """
    将精灵图转换为.data格式
    """
    print(f"正在处理: {sprite_path}")
    print(f"帧大小: {frame_width}x{frame_height}")
    print(f"格式: {'PNG' if use_png else 'RGB565压缩'}")
    
    frames = split_sprite_sheet(sprite_path, frame_width, frame_height)
    print(f"切割出 {len(frames)} 帧")
    
    writer = DataWriter(output_name, use_png=use_png)
    
    for i, frame in enumerate(frames):
        if trim:
            trimmed, ox, oy = trim_image(frame)
            writer.add_image(trimmed, ox, oy)
        else:
            writer.add_image(frame, 0, 0)
    
    writer.save()
    print("转换完成!")


def main():
    if len(sys.argv) < 3:
        print("用法: python png_to_data.py <sprite_sheet.png> <output_name> [frame_width] [frame_height]")
        print("")
        print("示例:")
        print("  python png_to_data.py monster_frost_lord_sprite.png Mon41 64 64")
        print("")
        print("将生成 Mon41.data 文件")
        return
    
    sprite_path = sys.argv[1]
    output_name = sys.argv[2]
    frame_width = int(sys.argv[3]) if len(sys.argv) > 3 else 64
    frame_height = int(sys.argv[4]) if len(sys.argv) > 4 else 64
    
    if not os.path.exists(sprite_path):
        print(f"错误: 文件不存在 - {sprite_path}")
        return
    
    convert_sprite_to_data(sprite_path, output_name, frame_width, frame_height)


if __name__ == "__main__":
    main()
