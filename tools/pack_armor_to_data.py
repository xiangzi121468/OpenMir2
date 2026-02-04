"""
衣服精灵图打包工具
将生成的衣服PNG精灵图打包成客户端.data格式
"""

import os
import struct
from PIL import Image

# 配置
FRAME_WIDTH = 80
FRAME_HEIGHT = 100
HUMANFRAME = 600
COLS = 25  # 精灵图列数

# 衣服Shape映射
ARMOR_SHAPES = {
    50: "寒冰战甲",
    51: "冰魄法袍", 
    52: "凝霜道袍",
    53: "暗影轻甲",
    54: "夜行者之衣",
    55: "亡灵骨甲",
    56: "噬魂法衣",
    57: "幽魂道袍",
    58: "烈焰龙甲",
    59: "龙魂法袍",
    60: "龙灵道袍",
    61: "龙皇神甲",
    62: "血魔战甲",
    63: "史莱姆软甲",
}

def create_pascal_short_string(s, max_len=40):
    """创建Pascal短字符串格式"""
    encoded = s.encode('ascii', errors='replace')[:max_len]
    length = len(encoded)
    return bytes([length]) + encoded.ljust(max_len, b'\x00')

def extract_frames_from_spritesheet(spritesheet_path):
    """从精灵图提取单帧"""
    img = Image.open(spritesheet_path).convert('RGBA')
    
    frames = []
    for i in range(HUMANFRAME):
        x = (i % COLS) * FRAME_WIDTH
        y = (i // COLS) * FRAME_HEIGHT
        
        frame = img.crop((x, y, x + FRAME_WIDTH, y + FRAME_HEIGHT))
        frames.append(frame)
    
    return frames

def pack_to_data(frames_dict, output_path, title="NewHum"):
    """
    将衣服帧打包成.data格式
    
    frames_dict: {shape_id: [frame_images]}
    """
    
    print(f"开始打包衣服资源到 {output_path}")
    
    # 按Shape ID排序
    sorted_shapes = sorted(frames_dict.keys())
    
    # 计算总图像数
    total_images = sum(len(frames) for frames in frames_dict.values())
    print(f"总图像数: {total_images}")
    
    with open(output_path, 'wb') as f:
        # ===== 写入头部 (68字节) =====
        # Title: 41字节 (1字节长度 + 40字节内容)
        f.write(create_pascal_short_string(title, 40))
        
        # ImageCount: 4字节
        f.write(struct.pack('<I', total_images))
        
        # IndexOffSet: 4字节 (稍后填充)
        index_offset_pos = f.tell()
        f.write(struct.pack('<I', 0))
        
        # XVersion: 2字节
        f.write(struct.pack('<H', 1))
        
        # Password: 17字节
        f.write(b'\x00' * 17)
        
        # ===== 准备图像数据 =====
        image_data_list = []
        
        for shape_id in sorted_shapes:
            frames = frames_dict[shape_id]
            print(f"  处理 Shape {shape_id}: {len(frames)} 帧")
            
            for frame in frames:
                # 转换为PNG格式
                import io
                png_buffer = io.BytesIO()
                frame.save(png_buffer, format='PNG')
                png_data = png_buffer.getvalue()
                
                image_data_list.append({
                    'width': frame.width,
                    'height': frame.height,
                    'x_offset': -frame.width // 2,
                    'y_offset': -frame.height + 10,
                    'data': png_data,
                    'is_png': True
                })
        
        # ===== 写入索引区 =====
        index_start = f.tell()
        
        # 先写入占位索引
        for _ in range(total_images):
            f.write(struct.pack('<I', 0))
        
        # ===== 写入图像数据并记录偏移 =====
        offsets = []
        
        for img_info in image_data_list:
            offset = f.tell()
            offsets.append(offset)
            
            # TPackDataImageInfo
            f.write(struct.pack('<H', img_info['width']))     # Width
            f.write(struct.pack('<H', img_info['height']))    # Height
            f.write(struct.pack('<B', 32))                    # BitCount (32位RGBA)
            f.write(struct.pack('<h', img_info['x_offset']))  # XOffset
            f.write(struct.pack('<h', img_info['y_offset']))  # YOffset
            f.write(struct.pack('<I', len(img_info['data']))) # DataLength
            f.write(struct.pack('<B', 1 if img_info['is_png'] else 0))  # GraphicType: 1=PNG
            
            # 图像数据
            f.write(img_info['data'])
        
        # ===== 回写索引 =====
        f.seek(index_start)
        for offset in offsets:
            f.write(struct.pack('<I', offset))
        
        # ===== 回写IndexOffSet =====
        f.seek(index_offset_pos)
        f.write(struct.pack('<I', index_start))
        
        # 获取最终文件大小
        f.seek(0, 2)
        file_size = f.tell()
    
    print(f"打包完成: {output_path}")
    print(f"文件大小: {file_size / 1024:.1f} KB")
    return True

def main():
    # 路径配置
    base_dir = os.path.dirname(os.path.abspath(__file__))
    sprites_dir = os.path.join(base_dir, "..", "assets", "armor_sprites")
    output_dir = os.path.join(base_dir, "..", "MirClient", "Data")
    
    os.makedirs(output_dir, exist_ok=True)
    
    print("=" * 60)
    print("衣服资源打包工具")
    print("=" * 60)
    
    # 收集所有衣服帧
    all_frames = {}
    
    for shape_id, name in ARMOR_SHAPES.items():
        # 查找精灵图文件
        pattern = f"armor_{shape_id}_"
        
        for filename in os.listdir(sprites_dir):
            if filename.startswith(pattern) and filename.endswith('.png'):
                filepath = os.path.join(sprites_dir, filename)
                print(f"加载: {filename}")
                
                frames = extract_frames_from_spritesheet(filepath)
                all_frames[shape_id] = frames
                break
        else:
            print(f"警告: 未找到 Shape {shape_id} ({name}) 的精灵图")
    
    if not all_frames:
        print("错误: 没有找到任何衣服精灵图")
        return
    
    print(f"\n找到 {len(all_frames)} 套衣服资源")
    
    # 打包成.data
    output_path = os.path.join(output_dir, "NewHum.data")
    pack_to_data(all_frames, output_path, "NewHumanArmor")
    
    print("\n" + "=" * 60)
    print("打包完成!")
    print("=" * 60)
    print(f"\n输出文件: {output_path}")
    print("\n下一步:")
    print("1. 将 NewHum.data 部署到客户端 Data 目录")
    print("2. 修改客户端 MShare.pas 添加新衣服资源加载")
    print("3. 或者将帧合并到现有 Hum.data / Hum2.data")

if __name__ == "__main__":
    main()
