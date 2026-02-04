"""
打包所有衣服资源和特效到.data格式
"""

import os
import struct
import io
from PIL import Image

# 配置
HUMANFRAME = 600
EFFECT_FRAMES = 20
FRAME_WIDTH = 80
FRAME_HEIGHT = 100
EFFECT_WIDTH = 120
EFFECT_HEIGHT = 160
COLS = 25

# 衣服Shape列表
ARMOR_SHAPES = [50, 51, 52, 53, 54, 55, 56, 57, 58, 59, 60, 61, 62, 63]

def create_pascal_short_string(s, max_len=40):
    """创建Pascal短字符串"""
    encoded = s.encode('ascii', errors='replace')[:max_len]
    return bytes([len(encoded)]) + encoded.ljust(max_len, b'\x00')

def extract_frames(spritesheet_path, frame_width, frame_height, total_frames, cols):
    """从精灵图提取帧"""
    img = Image.open(spritesheet_path).convert('RGBA')
    frames = []
    
    for i in range(total_frames):
        x = (i % cols) * frame_width
        y = (i // cols) * frame_height
        frame = img.crop((x, y, x + frame_width, y + frame_height))
        frames.append(frame)
    
    return frames

def extract_effect_frames(spritesheet_path, frame_width, frame_height, total_frames):
    """从特效精灵图提取帧 (水平排列)"""
    img = Image.open(spritesheet_path).convert('RGBA')
    frames = []
    
    for i in range(total_frames):
        x = i * frame_width
        frame = img.crop((x, 0, x + frame_width, frame_height))
        frames.append(frame)
    
    return frames

def pack_to_data(frames_dict, output_path, title, frame_width, frame_height):
    """打包帧到.data格式"""
    print(f"打包到 {output_path}")
    
    sorted_keys = sorted(frames_dict.keys())
    total_images = sum(len(frames) for frames in frames_dict.values())
    print(f"  总图像数: {total_images}")
    
    with open(output_path, 'wb') as f:
        # 头部 (68字节)
        f.write(create_pascal_short_string(title, 40))
        f.write(struct.pack('<I', total_images))
        index_offset_pos = f.tell()
        f.write(struct.pack('<I', 0))
        f.write(struct.pack('<H', 1))
        f.write(b'\x00' * 17)
        
        # 准备图像数据
        image_data_list = []
        for key in sorted_keys:
            frames = frames_dict[key]
            for frame in frames:
                png_buffer = io.BytesIO()
                frame.save(png_buffer, format='PNG', optimize=True)
                png_data = png_buffer.getvalue()
                
                image_data_list.append({
                    'width': frame.width,
                    'height': frame.height,
                    'x_offset': -frame.width // 2,
                    'y_offset': -frame.height + 10,
                    'data': png_data,
                })
        
        # 写入索引占位
        index_start = f.tell()
        for _ in range(total_images):
            f.write(struct.pack('<I', 0))
        
        # 写入图像数据
        offsets = []
        for img_info in image_data_list:
            offsets.append(f.tell())
            f.write(struct.pack('<H', img_info['width']))
            f.write(struct.pack('<H', img_info['height']))
            f.write(struct.pack('<B', 32))
            f.write(struct.pack('<h', img_info['x_offset']))
            f.write(struct.pack('<h', img_info['y_offset']))
            f.write(struct.pack('<I', len(img_info['data'])))
            f.write(struct.pack('<B', 1))  # PNG
            f.write(img_info['data'])
        
        # 回写索引
        f.seek(index_start)
        for offset in offsets:
            f.write(struct.pack('<I', offset))
        
        # 回写IndexOffSet
        f.seek(index_offset_pos)
        f.write(struct.pack('<I', index_start))
        
        f.seek(0, 2)
        file_size = f.tell()
    
    print(f"  文件大小: {file_size / 1024 / 1024:.2f} MB")
    return True

def main():
    base_dir = os.path.dirname(os.path.abspath(__file__))
    cool_sprites_dir = os.path.join(base_dir, "..", "assets", "armor_sprites_cool")
    effects_dir = os.path.join(base_dir, "..", "assets", "armor_effects")
    output_dir = os.path.join(base_dir, "..", "MirClient", "Data")
    
    os.makedirs(output_dir, exist_ok=True)
    
    print("=" * 60)
    print("衣服资源打包工具")
    print("=" * 60)
    
    # ===== 打包衣服精灵图 =====
    print("\n[1/2] 打包衣服精灵图...")
    armor_frames = {}
    
    for shape_id in ARMOR_SHAPES:
        # 查找精灵图
        pattern = f"armor_{shape_id}_"
        found = False
        
        for filename in os.listdir(cool_sprites_dir):
            if filename.startswith(pattern) and filename.endswith('.png'):
                filepath = os.path.join(cool_sprites_dir, filename)
                print(f"  加载: {filename}")
                frames = extract_frames(filepath, FRAME_WIDTH, FRAME_HEIGHT, HUMANFRAME, COLS)
                armor_frames[shape_id] = frames
                found = True
                break
        
        if not found:
            print(f"  警告: 未找到 Shape {shape_id}")
    
    if armor_frames:
        output_path = os.path.join(output_dir, "NewHum.data")
        pack_to_data(armor_frames, output_path, "NewHumanArmor", FRAME_WIDTH, FRAME_HEIGHT)
    
    # ===== 打包衣服特效 =====
    print("\n[2/2] 打包衣服特效...")
    effect_frames = {}
    
    for shape_id in ARMOR_SHAPES:
        pattern = f"armor_effect_{shape_id}_"
        found = False
        
        for filename in os.listdir(effects_dir):
            if filename.startswith(pattern) and filename.endswith('.png'):
                filepath = os.path.join(effects_dir, filename)
                print(f"  加载: {filename}")
                frames = extract_effect_frames(filepath, EFFECT_WIDTH, EFFECT_HEIGHT, EFFECT_FRAMES)
                effect_frames[shape_id] = frames
                found = True
                break
        
        if not found:
            print(f"  警告: 未找到 Shape {shape_id} 特效")
    
    if effect_frames:
        output_path = os.path.join(output_dir, "ArmorEffect.data")
        pack_to_data(effect_frames, output_path, "ArmorEffect", EFFECT_WIDTH, EFFECT_HEIGHT)
    
    print("\n" + "=" * 60)
    print("打包完成!")
    print("=" * 60)
    print(f"\n输出文件:")
    print(f"  - {os.path.join(output_dir, 'NewHum.data')}")
    print(f"  - {os.path.join(output_dir, 'ArmorEffect.data')}")

if __name__ == "__main__":
    main()
