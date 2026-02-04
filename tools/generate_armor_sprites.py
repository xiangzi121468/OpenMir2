"""
衣服精灵图生成工具
生成传奇客户端衣服资源图像

衣服帧结构:
- HUMANFRAME = 600 帧/套
- 8个方向 (0-7: 上、右上、右、右下、下、左下、左、左上)
- 每个方向的动作帧分配:
  * 站立: 4帧 (0-3)
  * 行走: 6帧 (4-9)
  * 跑步: 6帧 (10-15)
  * 攻击1: 6帧 (16-21)
  * 攻击2: 6帧 (22-27)
  * 魔法: 8帧 (28-35)
  * 被击: 3帧 (36-38)
  * 死亡: 4帧 (39-42)
  * ... (其他动作)
  
每方向约75帧, 8方向 = 600帧
"""

import os
import struct
from PIL import Image, ImageDraw

# 衣服配置
ARMOR_CONFIGS = {
    # ID: (名称, 主色调RGB, 副色调RGB, 风格)
    # 冰霜系列 - Shape 50-52
    50: ("寒冰战甲", (100, 180, 255), (200, 230, 255), "heavy"),
    51: ("冰魄法袍", (120, 200, 255), (220, 240, 255), "robe"),
    52: ("凝霜道袍", (140, 210, 255), (210, 235, 255), "robe"),
    
    # 暗影系列 - Shape 53-54
    53: ("暗影轻甲", (80, 60, 120), (140, 100, 180), "light"),
    54: ("夜行者之衣", (60, 40, 100), (120, 80, 160), "light"),
    
    # 亡灵系列 - Shape 55-57
    55: ("亡灵骨甲", (60, 80, 60), (120, 150, 120), "heavy"),
    56: ("噬魂法衣", (80, 100, 80), (140, 180, 140), "robe"),
    57: ("幽魂道袍", (70, 90, 70), (130, 170, 130), "robe"),
    
    # 龙系列 - Shape 58-61
    58: ("烈焰龙甲", (200, 80, 40), (255, 150, 80), "heavy"),
    59: ("龙魂法袍", (180, 60, 30), (240, 120, 60), "robe"),
    60: ("龙灵道袍", (160, 100, 50), (230, 160, 90), "robe"),
    61: ("龙皇神甲", (220, 180, 60), (255, 220, 100), "heavy"),
    
    # 特殊系列 - Shape 62-63
    62: ("血魔战甲", (150, 30, 30), (220, 80, 80), "heavy"),
    63: ("史莱姆软甲", (80, 200, 120), (150, 255, 180), "light"),
}

# 动作帧配置
ACTIONS = {
    "stand": (4, 0),      # 4帧, 起始0
    "walk": (6, 4),       # 6帧, 起始4
    "run": (6, 10),       # 6帧, 起始10
    "attack1": (6, 16),   # 6帧, 起始16
    "attack2": (6, 22),   # 6帧, 起始22
    "magic": (8, 28),     # 8帧, 起始28
    "struck": (3, 36),    # 3帧, 起始36
    "die": (4, 39),       # 4帧, 起始39
    # 保留帧到75
}

DIRECTIONS = 8
FRAMES_PER_DIRECTION = 75
HUMANFRAME = 600

# 图像尺寸
FRAME_WIDTH = 80
FRAME_HEIGHT = 100

def create_armor_frame(shape_id, direction, frame_idx, action_name):
    """创建单帧衣服图像"""
    config = ARMOR_CONFIGS.get(shape_id)
    if not config:
        config = ("未知衣服", (128, 128, 128), (180, 180, 180), "light")
    
    name, primary_color, secondary_color, style = config
    
    img = Image.new('RGBA', (FRAME_WIDTH, FRAME_HEIGHT), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # 中心点
    cx, cy = FRAME_WIDTH // 2, FRAME_HEIGHT // 2 + 10
    
    # 根据方向调整角度
    angle_offset = direction * 45
    
    # 动画偏移
    anim_offset = frame_idx % 6
    bounce = abs(3 - anim_offset) if action_name in ["walk", "run"] else 0
    
    # 根据风格绘制不同形状
    if style == "heavy":
        # 重甲 - 方形轮廓
        draw.rectangle([cx-20, cy-35+bounce, cx+20, cy+25], 
                      fill=primary_color, outline=secondary_color, width=2)
        # 肩甲
        draw.ellipse([cx-28, cy-30, cx-15, cy-20], fill=secondary_color)
        draw.ellipse([cx+15, cy-30, cx+28, cy-20], fill=secondary_color)
        # 腰带
        draw.rectangle([cx-22, cy+5, cx+22, cy+10], fill=secondary_color)
    elif style == "robe":
        # 长袍 - 梯形轮廓
        points = [(cx, cy-40+bounce), (cx-25, cy+30), (cx+25, cy+30)]
        draw.polygon(points, fill=primary_color, outline=secondary_color)
        # 领口
        draw.arc([cx-8, cy-42+bounce, cx+8, cy-35+bounce], 0, 180, fill=secondary_color, width=2)
        # 腰带
        draw.line([cx-15, cy-5, cx+15, cy-5], fill=secondary_color, width=2)
    else:  # light
        # 轻甲 - 流线型
        draw.ellipse([cx-18, cy-35+bounce, cx+18, cy+20], 
                    fill=primary_color, outline=secondary_color, width=1)
        # 护肩线条
        draw.line([cx-20, cy-25, cx-25, cy-15], fill=secondary_color, width=2)
        draw.line([cx+20, cy-25, cx+25, cy-15], fill=secondary_color, width=2)
    
    # 添加方向指示线（调试用）
    dir_colors = [(255,0,0), (255,128,0), (255,255,0), (128,255,0),
                  (0,255,0), (0,255,128), (0,255,255), (128,0,255)]
    dir_x = cx + int(15 * [0, 0.7, 1, 0.7, 0, -0.7, -1, -0.7][direction])
    dir_y = cy - 40 + bounce + int(8 * [-1, -0.7, 0, 0.7, 1, 0.7, 0, -0.7][direction])
    draw.ellipse([dir_x-3, dir_y-3, dir_x+3, dir_y+3], fill=dir_colors[direction])
    
    return img

def generate_armor_spritesheet(shape_id, output_dir):
    """生成单套衣服的完整精灵图"""
    config = ARMOR_CONFIGS.get(shape_id)
    if not config:
        print(f"未找到Shape {shape_id}的配置")
        return None
    
    name = config[0]
    print(f"生成 {name} (Shape {shape_id})...")
    
    # 创建600帧图像列表
    frames = []
    
    for direction in range(DIRECTIONS):
        for frame_idx in range(FRAMES_PER_DIRECTION):
            # 确定当前动作
            action_name = "stand"
            for act_name, (count, start) in ACTIONS.items():
                if start <= frame_idx < start + count:
                    action_name = act_name
                    break
            
            img = create_armor_frame(shape_id, direction, frame_idx, action_name)
            frames.append(img)
    
    # 保存为精灵图 (25列 × 24行)
    cols = 25
    rows = (HUMANFRAME + cols - 1) // cols
    
    sheet_width = cols * FRAME_WIDTH
    sheet_height = rows * FRAME_HEIGHT
    
    spritesheet = Image.new('RGBA', (sheet_width, sheet_height), (0, 0, 0, 0))
    
    for i, frame in enumerate(frames):
        x = (i % cols) * FRAME_WIDTH
        y = (i // cols) * FRAME_HEIGHT
        spritesheet.paste(frame, (x, y))
    
    # 保存
    output_path = os.path.join(output_dir, f"armor_{shape_id}_{name}.png")
    spritesheet.save(output_path)
    print(f"  保存: {output_path}")
    
    return frames

def pack_armors_to_data(frames_dict, output_path, title="HumArmor"):
    """将衣服帧打包成.data格式"""
    
    # 收集所有图像
    all_images = []
    for shape_id in sorted(frames_dict.keys()):
        all_images.extend(frames_dict[shape_id])
    
    if not all_images:
        print("没有图像需要打包")
        return
    
    print(f"打包 {len(all_images)} 张图像到 {output_path}")
    
    with open(output_path, 'wb') as f:
        # 写入头部
        title_bytes = title.encode('ascii')[:40].ljust(41, b'\x00')
        f.write(struct.pack('B', min(len(title), 40)))
        f.write(title_bytes[:40])
        
        image_count = len(all_images)
        f.write(struct.pack('<I', image_count))  # ImageCount
        
        # 预留IndexOffSet位置
        index_offset_pos = f.tell()
        f.write(struct.pack('<I', 0))  # IndexOffSet placeholder
        
        f.write(struct.pack('<H', 1))  # XVersion
        f.write(b'\x00' * 17)  # Password
        
        # 收集图像数据
        image_data_list = []
        for img in all_images:
            png_data = img.tobytes()  # RGBA raw
            image_data_list.append((img.width, img.height, png_data))
        
        # 写入索引区位置
        index_start = f.tell()
        
        # 写入图像索引占位
        for _ in range(image_count):
            f.write(struct.pack('<I', 0))
        
        # 写入图像数据并记录偏移
        offsets = []
        for width, height, data in image_data_list:
            offset = f.tell()
            offsets.append(offset)
            
            # 图像信息
            f.write(struct.pack('<H', width))
            f.write(struct.pack('<H', height))
            f.write(struct.pack('<B', 32))  # BitCount
            f.write(struct.pack('<h', -width//2))  # XOffset (居中)
            f.write(struct.pack('<h', -height+10))  # YOffset
            f.write(struct.pack('<I', len(data)))
            f.write(struct.pack('<B', 0))  # GraphicType: 0=DIB
            f.write(data)
        
        # 回写索引
        f.seek(index_start)
        for offset in offsets:
            f.write(struct.pack('<I', offset))
        
        # 回写IndexOffSet
        f.seek(index_offset_pos)
        f.write(struct.pack('<I', index_start))
    
    print(f"完成打包: {output_path}")

def main():
    # 输出目录
    output_dir = os.path.join(os.path.dirname(__file__), "..", "assets", "armor_sprites")
    os.makedirs(output_dir, exist_ok=True)
    
    print("=" * 50)
    print("衣服精灵图生成工具")
    print("=" * 50)
    
    # 生成所有衣服
    all_frames = {}
    for shape_id in ARMOR_CONFIGS:
        frames = generate_armor_spritesheet(shape_id, output_dir)
        if frames:
            all_frames[shape_id] = frames
    
    print("\n" + "=" * 50)
    print(f"完成生成 {len(all_frames)} 套衣服精灵图")
    print(f"输出目录: {output_dir}")
    print("=" * 50)
    
    # 可选：打包成.data (由于数据量大，这里只生成PNG)
    # data_output = os.path.join(os.path.dirname(__file__), "..", "MirClient", "Data", "HumArmor.data")
    # pack_armors_to_data(all_frames, data_output)
    
    print("\n注意: 衣服资源需要600帧/套，完整版本需要美术资源。")
    print("当前生成的是placeholder图像，用于验证数据流。")
    print("\n建议方案:")
    print("1. 使用现有衣服Shape (修改数据库Shape指向现有衣服)")
    print("2. 或者准备专业美术资源后再打包")

if __name__ == "__main__":
    main()
