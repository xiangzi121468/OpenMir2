#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
怪物精灵图生成工具
根据动画配置生成符合帧布局的精灵图

这个工具用于生成占位精灵图，实际使用时需要替换为真实的美术资源
"""

import os
from PIL import Image, ImageDraw, ImageFont
from monster_sprite_config import MONSTER_ANIMATION_CONFIGS

# 8个方向的标记
DIRECTION_NAMES = ['下', '左下', '左', '左上', '上', '右上', '右', '右下']

def create_placeholder_frame(width, height, monster_name, action, direction, frame_idx, total_frames):
    """创建一个占位帧（用于测试）"""
    img = Image.new('RGBA', (width, height), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # 根据动作类型选择颜色
    action_colors = {
        'Stand': (100, 150, 255, 200),    # 蓝色 - 站立
        'Walk': (100, 255, 100, 200),     # 绿色 - 行走
        'Attack': (255, 100, 100, 200),   # 红色 - 攻击
        'Critical': (255, 200, 0, 200),   # 橙色 - 暴击
        'Struck': (200, 100, 200, 200),   # 紫色 - 受击
        'Die': (100, 100, 100, 200),      # 灰色 - 死亡
    }
    color = action_colors.get(action, (128, 128, 128, 200))
    
    # 绘制椭圆作为怪物轮廓
    margin = 4
    draw.ellipse([margin, margin, width-margin, height-margin], fill=color, outline=(255, 255, 255, 255))
    
    # 标注信息
    text = f"{action[0]}{direction}"
    try:
        # 尝试使用系统字体
        font = ImageFont.truetype("arial.ttf", 10)
    except:
        font = ImageFont.load_default()
    
    # 获取文本边界框
    bbox = draw.textbbox((0, 0), text, font=font)
    text_width = bbox[2] - bbox[0]
    text_height = bbox[3] - bbox[1]
    
    text_x = (width - text_width) // 2
    text_y = (height - text_height) // 2
    draw.text((text_x, text_y), text, fill=(255, 255, 255, 255), font=font)
    
    return img


def generate_sprite_sheet(monster_name, output_dir):
    """为指定怪物生成精灵图"""
    if monster_name not in MONSTER_ANIMATION_CONFIGS:
        print(f"未找到怪物配置: {monster_name}")
        return None
    
    config = MONSTER_ANIMATION_CONFIGS[monster_name]
    
    frame_width = config["frame_width"]
    frame_height = config["frame_height"]
    total_frames = config["total_frames"]
    directions = config["directions"]
    
    # 计算精灵图尺寸
    # 按照传奇格式: 每行是一个方向的所有帧
    # 行数 = 总帧数，列数 = 1 (或者可以按动作组织)
    
    # 为了更好的组织，我们按动作分组
    # 每个动作的帧数 = (frames + skip) * directions
    
    # 计算每行多少帧（取一个合理的值，比如每行32帧）
    frames_per_row = 32
    rows_needed = (total_frames + frames_per_row - 1) // frames_per_row
    
    sheet_width = frames_per_row * frame_width
    sheet_height = rows_needed * frame_height
    
    print(f"\n生成 {config['name']} 精灵图...")
    print(f"  帧尺寸: {frame_width}x{frame_height}")
    print(f"  总帧数: {total_frames}")
    print(f"  精灵图尺寸: {sheet_width}x{sheet_height}")
    
    # 创建精灵图
    sprite_sheet = Image.new('RGBA', (sheet_width, sheet_height), (0, 0, 0, 0))
    
    frame_index = 0
    
    for action_name, action_info in config["actions"].items():
        start = action_info["start"]
        frames = action_info["frames"]
        skip = action_info["skip"]
        
        for dir_idx in range(directions):
            # 计算当前方向的帧范围
            dir_start = start + dir_idx * (frames + skip)
            
            for f in range(frames):
                global_frame = dir_start + f
                
                # 计算在精灵图中的位置
                row = global_frame // frames_per_row
                col = global_frame % frames_per_row
                
                x = col * frame_width
                y = row * frame_height
                
                # 创建占位帧
                frame_img = create_placeholder_frame(
                    frame_width, frame_height,
                    monster_name, action_name, dir_idx, f, frames
                )
                
                sprite_sheet.paste(frame_img, (x, y))
    
    # 保存精灵图
    os.makedirs(output_dir, exist_ok=True)
    output_path = os.path.join(output_dir, f"{config['file_prefix']}_placeholder.png")
    sprite_sheet.save(output_path, 'PNG')
    print(f"  已保存: {output_path}")
    
    return output_path


def generate_animation_guide(monster_name, output_dir):
    """生成动画帧布局指南"""
    if monster_name not in MONSTER_ANIMATION_CONFIGS:
        return
    
    config = MONSTER_ANIMATION_CONFIGS[monster_name]
    
    guide_path = os.path.join(output_dir, f"{config['file_prefix']}_animation_guide.txt")
    
    with open(guide_path, 'w', encoding='utf-8') as f:
        f.write(f"{'='*60}\n")
        f.write(f"{config['name']} ({monster_name}) 动画帧布局指南\n")
        f.write(f"{'='*60}\n\n")
        f.write(f"文件名: {config['file_prefix']}.data\n")
        f.write(f"帧尺寸: {config['frame_width']}x{config['frame_height']} 像素\n")
        f.write(f"总帧数: {config['total_frames']} 帧\n")
        f.write(f"方向数: {config['directions']} (0=下, 1=左下, 2=左, 3=左上, 4=上, 5=右上, 6=右, 7=右下)\n\n")
        
        f.write("动画动作帧布局:\n")
        f.write("-" * 60 + "\n")
        
        for action_name, action_info in config["actions"].items():
            start = action_info["start"]
            frames = action_info["frames"]
            skip = action_info["skip"]
            ftime = action_info["ftime"]
            
            f.write(f"\n【{action_name}】 每个方向 {frames} 帧, 帧时间 {ftime}ms\n")
            
            for dir_idx in range(config["directions"]):
                dir_start = start + dir_idx * (frames + skip)
                dir_end = dir_start + frames - 1
                f.write(f"  方向{dir_idx} ({DIRECTION_NAMES[dir_idx]:2s}): 帧 {dir_start:3d} - {dir_end:3d}\n")
        
        f.write("\n" + "=" * 60 + "\n")
        f.write("美术制作说明:\n")
        f.write("-" * 60 + "\n")
        f.write("1. 每个动作需要制作8个方向的动画\n")
        f.write("2. 方向顺序: 下→左下→左→左上→上→右上→右→右下\n")
        f.write("3. 帧按照 方向0帧0, 方向0帧1, ..., 方向1帧0, ... 的顺序排列\n")
        f.write("4. 所有帧的尺寸必须一致\n")
        f.write("5. 透明背景使用alpha通道\n")
    
    print(f"  动画指南: {guide_path}")


def main():
    output_dir = os.path.join(os.path.dirname(__file__), "..", "assets", "sprite_templates")
    
    print("=" * 60)
    print("怪物精灵图生成工具")
    print("=" * 60)
    
    for monster_name in MONSTER_ANIMATION_CONFIGS:
        generate_sprite_sheet(monster_name, output_dir)
        generate_animation_guide(monster_name, output_dir)
    
    print("\n" + "=" * 60)
    print("生成完成！")
    print(f"输出目录: {output_dir}")
    print("=" * 60)
    print("\n说明:")
    print("- *_placeholder.png 是占位精灵图，用于测试")
    print("- *_animation_guide.txt 是动画帧布局指南，给美术参考")
    print("- 实际使用时需要用真实美术资源替换占位图")


if __name__ == "__main__":
    main()
