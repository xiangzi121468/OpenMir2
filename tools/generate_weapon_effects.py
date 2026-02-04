#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
武器闪闪特效生成工具
生成传奇2风格的武器发光特效图像

特效类型:
1. 光晕特效 (Glow) - 武器周围的柔和光芒
2. 粒子特效 (Sparkle) - 闪烁的星星点点
3. 流光特效 (Flow) - 沿着武器流动的光线
4. 火焰特效 (Fire) - 火焰燃烧效果
5. 冰霜特效 (Ice) - 冰冷的蓝色光芒
6. 雷电特效 (Lightning) - 电弧闪烁
"""

import os
import math
import random
from PIL import Image, ImageDraw, ImageFilter, ImageEnhance

# 特效颜色定义
EFFECT_COLORS = {
    "gold": [(255, 215, 0), (255, 255, 150), (255, 200, 50)],      # 金色
    "blue": [(100, 150, 255), (150, 200, 255), (50, 100, 200)],    # 蓝色/冰霜
    "red": [(255, 100, 50), (255, 150, 100), (200, 50, 0)],        # 红色/火焰
    "green": [(100, 255, 100), (150, 255, 150), (50, 200, 50)],    # 绿色/毒
    "purple": [(200, 100, 255), (220, 150, 255), (150, 50, 200)],  # 紫色/暗影
    "white": [(255, 255, 255), (240, 240, 255), (220, 220, 240)],  # 白色/圣光
    "cyan": [(100, 255, 255), (150, 255, 255), (50, 200, 200)],    # 青色/雷电
}

# 武器特效配置
WEAPON_EFFECTS = {
    # 冰霜系武器
    "frost_blade": {
        "name": "冰霜之刃",
        "color": "blue",
        "effects": ["glow", "sparkle", "frost_aura"],
        "intensity": 0.8,
        "frame_count": 10,
    },
    "ice_staff": {
        "name": "冰魄法杖",
        "color": "cyan",
        "effects": ["glow", "frost_particles"],
        "intensity": 0.7,
        "frame_count": 8,
    },
    
    # 暗影系武器
    "shadow_dagger": {
        "name": "暗影匕首",
        "color": "purple",
        "effects": ["shadow_aura", "sparkle"],
        "intensity": 0.6,
        "frame_count": 8,
    },
    
    # 亡灵系武器
    "necro_staff": {
        "name": "亡灵法杖",
        "color": "green",
        "effects": ["glow", "soul_particles"],
        "intensity": 0.7,
        "frame_count": 10,
    },
    
    # 火焰系武器
    "fire_blade": {
        "name": "地狱火",
        "color": "red",
        "effects": ["fire_aura", "fire_particles", "glow"],
        "intensity": 0.9,
        "frame_count": 12,
    },
    
    # 龙系顶级武器
    "dragon_blade": {
        "name": "屠龙刀特效",
        "color": "gold",
        "effects": ["glow", "sparkle", "flow", "fire_aura"],
        "intensity": 1.0,
        "frame_count": 14,
    },
}


def create_glow_effect(size, color, intensity, frame):
    """创建光晕特效"""
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size[0] // 2, size[1] // 2
    max_radius = min(size) // 3
    
    # 根据帧数调整光晕大小
    pulse = math.sin(frame * 0.5) * 0.2 + 0.8
    
    for r in range(int(max_radius * pulse), 0, -2):
        alpha = int(255 * (1 - r / (max_radius * pulse)) * intensity * 0.6)
        c = (color[0], color[1], color[2], alpha)
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=c)
    
    return img.filter(ImageFilter.GaussianBlur(radius=3))


def create_sparkle_effect(size, colors, intensity, frame, count=15):
    """创建闪烁粒子特效"""
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    random.seed(frame * 1000)  # 确保每帧有不同但可重复的随机数
    
    for _ in range(count):
        x = random.randint(10, size[0] - 10)
        y = random.randint(10, size[1] - 10)
        
        # 随机选择颜色
        color = random.choice(colors)
        
        # 星星大小随机
        star_size = random.randint(2, 6)
        
        # 闪烁效果
        flicker = random.random() * 0.5 + 0.5
        alpha = int(255 * intensity * flicker)
        
        # 绘制十字星形
        c = (color[0], color[1], color[2], alpha)
        draw.line([(x - star_size, y), (x + star_size, y)], fill=c, width=1)
        draw.line([(x, y - star_size), (x, y + star_size)], fill=c, width=1)
        
        # 中心亮点
        c_bright = (min(255, color[0] + 50), min(255, color[1] + 50), min(255, color[2] + 50), alpha)
        draw.point((x, y), fill=c_bright)
    
    return img


def create_flow_effect(size, color, intensity, frame):
    """创建流光特效"""
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    # 流光沿对角线移动
    cx, cy = size[0] // 2, size[1] // 2
    
    # 计算流光位置
    offset = (frame * 5) % (size[1] + 20) - 10
    
    for i in range(5):
        y = offset + i * 3
        alpha = int(255 * intensity * (1 - abs(i - 2) / 3))
        c = (color[0], color[1], color[2], alpha)
        
        # 绘制水平流光线
        draw.line([(0, y), (size[0], y)], fill=c, width=2)
    
    return img.filter(ImageFilter.GaussianBlur(radius=2))


def create_fire_aura(size, colors, intensity, frame):
    """创建火焰光环特效"""
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size[0] // 2, size[1] // 2
    
    random.seed(frame * 100)
    
    # 绘制多个火焰粒子
    for _ in range(20):
        # 火焰向上飘动
        x = cx + random.randint(-20, 20)
        base_y = cy + random.randint(-10, 30)
        y = base_y - (frame % 10) * 3
        
        if y < 0:
            y = base_y
        
        color = random.choice(colors)
        particle_size = random.randint(3, 8)
        
        alpha = int(255 * intensity * random.random())
        c = (color[0], color[1], color[2], alpha)
        
        draw.ellipse([x - particle_size, y - particle_size, 
                     x + particle_size, y + particle_size], fill=c)
    
    return img.filter(ImageFilter.GaussianBlur(radius=2))


def create_frost_aura(size, colors, intensity, frame):
    """创建冰霜光环特效"""
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size[0] // 2, size[1] // 2
    
    # 冰晶粒子
    random.seed(frame * 200)
    
    for _ in range(15):
        angle = random.random() * math.pi * 2
        radius = random.randint(10, 35)
        
        x = int(cx + math.cos(angle) * radius)
        y = int(cy + math.sin(angle) * radius)
        
        color = random.choice(colors)
        
        # 冰晶形状
        ice_size = random.randint(2, 5)
        alpha = int(255 * intensity * (0.5 + random.random() * 0.5))
        c = (color[0], color[1], color[2], alpha)
        
        # 六角冰晶
        for a in range(6):
            angle_offset = a * math.pi / 3
            x2 = int(x + math.cos(angle_offset) * ice_size)
            y2 = int(y + math.sin(angle_offset) * ice_size)
            draw.line([(x, y), (x2, y2)], fill=c, width=1)
    
    return img.filter(ImageFilter.GaussianBlur(radius=1))


def create_shadow_aura(size, colors, intensity, frame):
    """创建暗影光环特效"""
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size[0] // 2, size[1] // 2
    
    # 暗影波动
    wave = math.sin(frame * 0.3) * 5
    
    for i in range(3):
        radius = 20 + i * 8 + wave
        alpha = int(100 * intensity * (1 - i / 3))
        color = colors[0]
        c = (color[0], color[1], color[2], alpha)
        
        draw.ellipse([cx - radius, cy - radius, cx + radius, cy + radius], 
                    outline=c, width=2)
    
    return img.filter(ImageFilter.GaussianBlur(radius=2))


def create_soul_particles(size, colors, intensity, frame):
    """创建灵魂粒子特效"""
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size[0] // 2, size[1] // 2
    
    random.seed(frame * 300)
    
    # 漂浮的灵魂光点
    for _ in range(10):
        angle = random.random() * math.pi * 2 + frame * 0.1
        radius = random.randint(15, 40)
        
        x = int(cx + math.cos(angle) * radius)
        y = int(cy + math.sin(angle) * radius - (frame % 5) * 2)
        
        color = random.choice(colors)
        alpha = int(200 * intensity * random.random())
        c = (color[0], color[1], color[2], alpha)
        
        # 小光球
        draw.ellipse([x - 3, y - 3, x + 3, y + 3], fill=c)
    
    return img.filter(ImageFilter.GaussianBlur(radius=1))


def create_frost_particles(size, colors, intensity, frame):
    """创建冰霜粒子特效"""
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    random.seed(frame * 400)
    
    # 飘落的冰晶
    for _ in range(12):
        x = random.randint(5, size[0] - 5)
        y = (random.randint(0, size[1]) + frame * 3) % size[1]
        
        color = random.choice(colors)
        alpha = int(200 * intensity * random.random())
        c = (color[0], color[1], color[2], alpha)
        
        # 小雪花
        for angle in range(6):
            a = angle * math.pi / 3
            x2 = int(x + math.cos(a) * 3)
            y2 = int(y + math.sin(a) * 3)
            draw.line([(x, y), (x2, y2)], fill=c, width=1)
    
    return img


def generate_weapon_effect_frames(weapon_key, output_dir):
    """为指定武器生成特效帧序列"""
    if weapon_key not in WEAPON_EFFECTS:
        print(f"未找到武器配置: {weapon_key}")
        return
    
    config = WEAPON_EFFECTS[weapon_key]
    colors = EFFECT_COLORS[config["color"]]
    intensity = config["intensity"]
    frame_count = config["frame_count"]
    
    print(f"\n生成 {config['name']} 特效...")
    print(f"  颜色: {config['color']}")
    print(f"  特效: {', '.join(config['effects'])}")
    print(f"  帧数: {frame_count}")
    
    # 特效图像尺寸
    size = (80, 100)
    
    os.makedirs(output_dir, exist_ok=True)
    
    frames = []
    
    for frame_idx in range(frame_count):
        # 创建合成图像
        composite = Image.new('RGBA', size, (0, 0, 0, 0))
        
        # 叠加各种特效
        for effect_name in config["effects"]:
            effect_img = None
            
            if effect_name == "glow":
                effect_img = create_glow_effect(size, colors[0], intensity, frame_idx)
            elif effect_name == "sparkle":
                effect_img = create_sparkle_effect(size, colors, intensity, frame_idx)
            elif effect_name == "flow":
                effect_img = create_flow_effect(size, colors[0], intensity, frame_idx)
            elif effect_name == "fire_aura":
                effect_img = create_fire_aura(size, colors, intensity, frame_idx)
            elif effect_name == "frost_aura":
                effect_img = create_frost_aura(size, colors, intensity, frame_idx)
            elif effect_name == "shadow_aura":
                effect_img = create_shadow_aura(size, colors, intensity, frame_idx)
            elif effect_name == "soul_particles":
                effect_img = create_soul_particles(size, colors, intensity, frame_idx)
            elif effect_name == "frost_particles":
                effect_img = create_frost_particles(size, colors, intensity, frame_idx)
            elif effect_name == "fire_particles":
                effect_img = create_fire_aura(size, colors, intensity, frame_idx)
            
            if effect_img:
                composite = Image.alpha_composite(composite, effect_img)
        
        frames.append(composite)
        
        # 保存单帧
        frame_path = os.path.join(output_dir, f"{weapon_key}_frame_{frame_idx:02d}.png")
        composite.save(frame_path, 'PNG')
    
    # 创建精灵图 (所有帧横向排列)
    sprite_width = size[0] * frame_count
    sprite_sheet = Image.new('RGBA', (sprite_width, size[1]), (0, 0, 0, 0))
    
    for i, frame in enumerate(frames):
        sprite_sheet.paste(frame, (i * size[0], 0))
    
    sprite_path = os.path.join(output_dir, f"{weapon_key}_sprite.png")
    sprite_sheet.save(sprite_path, 'PNG')
    print(f"  精灵图: {sprite_path}")
    
    # 创建8方向精灵图 (用于游戏内)
    directions = 8
    dir_sprite = Image.new('RGBA', (size[0] * frame_count, size[1] * directions), (0, 0, 0, 0))
    
    for dir_idx in range(directions):
        for frame_idx, frame in enumerate(frames):
            # 不同方向可以稍微旋转或调整
            rotated = frame.rotate(dir_idx * 45, expand=False, fillcolor=(0, 0, 0, 0))
            dir_sprite.paste(rotated, (frame_idx * size[0], dir_idx * size[1]), rotated)
    
    dir_sprite_path = os.path.join(output_dir, f"{weapon_key}_8dir_sprite.png")
    dir_sprite.save(dir_sprite_path, 'PNG')
    print(f"  8方向精灵图: {dir_sprite_path}")
    
    return frames


def main():
    output_dir = os.path.join(os.path.dirname(__file__), "..", "assets", "weapon_effects")
    
    print("=" * 60)
    print("武器闪闪特效生成工具")
    print("=" * 60)
    
    for weapon_key in WEAPON_EFFECTS:
        weapon_output = os.path.join(output_dir, weapon_key)
        generate_weapon_effect_frames(weapon_key, weapon_output)
    
    print("\n" + "=" * 60)
    print("生成完成！")
    print(f"输出目录: {output_dir}")
    print("=" * 60)
    
    print("\n使用说明:")
    print("1. *_sprite.png - 单行精灵图，用于预览")
    print("2. *_8dir_sprite.png - 8方向精灵图，用于游戏内")
    print("3. 将精灵图转换为 .data 格式后放入客户端 Data 目录")
    print("4. 在 MShare.pas 中配置武器特效索引")


if __name__ == "__main__":
    main()
