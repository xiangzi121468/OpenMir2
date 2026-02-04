#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
武器特效生成工具 v2.0 - 优化版
生成符合客户端State.data索引要求的特效图像

特效索引规则:
- 自定义特效 (reserve[3]=100-249): 索引 30000 + (type-100)*20 + frame
- 每个特效20帧动画，帧率200ms
"""

import os
import math
import random
from PIL import Image, ImageDraw, ImageFilter, ImageEnhance

# 特效颜色定义 (更鲜艳的颜色)
EFFECT_COLORS = {
    "frost": {
        "primary": (100, 180, 255),
        "secondary": (200, 240, 255),
        "glow": (150, 220, 255),
    },
    "fire": {
        "primary": (255, 120, 30),
        "secondary": (255, 200, 50),
        "glow": (255, 80, 0),
    },
    "shadow": {
        "primary": (180, 80, 220),
        "secondary": (220, 150, 255),
        "glow": (150, 50, 200),
    },
    "necro": {
        "primary": (80, 220, 80),
        "secondary": (150, 255, 150),
        "glow": (50, 180, 50),
    },
    "dragon": {
        "primary": (255, 200, 50),
        "secondary": (255, 255, 150),
        "glow": (255, 180, 0),
    },
}

# 特效配置 (与weapon_effect_config.py对应)
WEAPON_EFFECT_CONFIGS = {
    100: {"name": "冰霜之刃", "color": "frost", "type": "aura_sparkle", "frames": 20},
    101: {"name": "冰魄", "color": "frost", "type": "particles", "frames": 20},
    102: {"name": "凝霜", "color": "frost", "type": "mist", "frames": 20},
    103: {"name": "暗影匕首", "color": "shadow", "type": "shadow_pulse", "frames": 20},
    105: {"name": "亡灵法杖", "color": "necro", "type": "soul_fire", "frames": 20},
    110: {"name": "地狱火", "color": "fire", "type": "fire_aura", "frames": 20},
    115: {"name": "龙之光环", "color": "dragon", "type": "dragon_glow", "frames": 20},
}


def create_glow_ring(size, color_config, frame, total_frames, intensity=1.0):
    """创建光环效果"""
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size[0] // 2, size[1] // 2
    
    # 脉冲效果
    pulse = math.sin(frame / total_frames * math.pi * 2) * 0.3 + 0.7
    base_radius = min(size) // 3
    
    # 绘制多层光环
    for layer in range(3):
        radius = int(base_radius * pulse * (1 + layer * 0.15))
        alpha = int(180 * intensity * (1 - layer * 0.3) * pulse)
        
        color = color_config["glow"]
        c = (color[0], color[1], color[2], alpha)
        
        # 使用抗锯齿绘制
        for r in range(radius, max(0, radius - 8), -1):
            a = int(alpha * (1 - (radius - r) / 8))
            draw.ellipse([cx - r, cy - r, cx + r, cy + r], 
                        outline=(color[0], color[1], color[2], a), width=1)
    
    return img.filter(ImageFilter.GaussianBlur(radius=2))


def create_sparkles(size, color_config, frame, total_frames, count=20):
    """创建闪烁星星效果"""
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    random.seed(frame * 12345)
    
    for i in range(count):
        # 随机位置
        x = random.randint(5, size[0] - 5)
        y = random.randint(5, size[1] - 5)
        
        # 随机闪烁相位
        phase = (frame + i * 3) % total_frames / total_frames
        brightness = abs(math.sin(phase * math.pi))
        
        if brightness < 0.3:
            continue
        
        color = color_config["secondary"] if random.random() > 0.5 else color_config["primary"]
        alpha = int(255 * brightness * 0.8)
        
        # 星星大小
        star_size = random.randint(2, 5)
        
        # 绘制十字星
        c = (color[0], color[1], color[2], alpha)
        draw.line([(x - star_size, y), (x + star_size, y)], fill=c, width=1)
        draw.line([(x, y - star_size), (x, y + star_size)], fill=c, width=1)
        
        # 中心亮点
        bright_color = (min(255, color[0] + 50), min(255, color[1] + 50), 
                       min(255, color[2] + 50), min(255, alpha + 50))
        draw.point((x, y), fill=bright_color)
    
    return img


def create_particles(size, color_config, frame, total_frames, count=15):
    """创建飘动粒子效果"""
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size[0] // 2, size[1] // 2
    
    for i in range(count):
        # 粒子随时间移动
        angle = (i / count) * math.pi * 2 + frame * 0.2
        dist = 15 + math.sin(frame * 0.3 + i) * 10
        
        x = int(cx + math.cos(angle) * dist)
        y = int(cy + math.sin(angle) * dist - frame % 10)
        
        # 边界检查
        if not (0 < x < size[0] and 0 < y < size[1]):
            continue
        
        color = color_config["primary"]
        alpha = int(200 * (0.5 + 0.5 * math.sin(frame * 0.5 + i)))
        
        # 粒子大小
        p_size = random.randint(2, 4)
        c = (color[0], color[1], color[2], alpha)
        draw.ellipse([x - p_size, y - p_size, x + p_size, y + p_size], fill=c)
    
    return img.filter(ImageFilter.GaussianBlur(radius=1))


def create_fire_aura(size, color_config, frame, total_frames):
    """创建火焰光环效果"""
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size[0] // 2, size[1] // 2
    
    random.seed(frame * 777)
    
    # 火焰粒子
    for _ in range(25):
        x = cx + random.randint(-25, 25)
        base_y = cy + random.randint(-5, 20)
        
        # 火焰向上飘动
        y = base_y - (frame % 8) * 3 + random.randint(-3, 3)
        
        if y < 5:
            y = base_y
        
        # 颜色随高度变化
        height_ratio = 1 - (cy - y) / 40
        if random.random() > 0.5:
            color = color_config["primary"]
        else:
            color = color_config["secondary"]
        
        alpha = int(200 * height_ratio * random.random())
        p_size = random.randint(3, 7)
        
        c = (color[0], color[1], color[2], max(0, alpha))
        draw.ellipse([x - p_size, y - p_size, x + p_size, y + p_size], fill=c)
    
    return img.filter(ImageFilter.GaussianBlur(radius=2))


def create_shadow_pulse(size, color_config, frame, total_frames):
    """创建暗影脉冲效果"""
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size[0] // 2, size[1] // 2
    
    # 脉冲波纹
    for wave in range(3):
        wave_progress = ((frame / total_frames) + wave * 0.33) % 1.0
        radius = int(10 + wave_progress * 30)
        alpha = int(150 * (1 - wave_progress))
        
        color = color_config["glow"]
        c = (color[0], color[1], color[2], alpha)
        
        draw.ellipse([cx - radius, cy - radius, cx + radius, cy + radius], 
                    outline=c, width=2)
    
    # 中心暗影
    glow_alpha = int(100 + 50 * math.sin(frame * 0.5))
    glow_color = (*color_config["primary"], glow_alpha)
    draw.ellipse([cx - 15, cy - 15, cx + 15, cy + 15], fill=glow_color)
    
    return img.filter(ImageFilter.GaussianBlur(radius=2))


def create_soul_fire(size, color_config, frame, total_frames):
    """创建幽魂火焰效果"""
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size[0] // 2, size[1] // 2
    
    random.seed(frame * 999)
    
    # 幽魂粒子
    for _ in range(15):
        angle = random.random() * math.pi * 2
        dist = random.randint(10, 35)
        
        x = int(cx + math.cos(angle) * dist)
        y = int(cy + math.sin(angle) * dist - (frame % 6) * 2)
        
        if y < 5:
            y = int(cy + math.sin(angle) * dist)
        
        color = color_config["secondary"] if random.random() > 0.3 else color_config["primary"]
        alpha = int(180 * random.random())
        
        # 鬼火形状
        p_size = random.randint(3, 6)
        c = (color[0], color[1], color[2], alpha)
        draw.ellipse([x - p_size, y - p_size, x + p_size, y + p_size], fill=c)
    
    return img.filter(ImageFilter.GaussianBlur(radius=1))


def create_dragon_glow(size, color_config, frame, total_frames):
    """创建龙之光芒效果"""
    img = Image.new('RGBA', size, (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    cx, cy = size[0] // 2, size[1] // 2
    
    # 旋转的金色光芒
    for i in range(8):
        angle = (i / 8) * math.pi * 2 + frame * 0.15
        length = 20 + 5 * math.sin(frame * 0.3 + i)
        
        x1 = int(cx + math.cos(angle) * 8)
        y1 = int(cy + math.sin(angle) * 8)
        x2 = int(cx + math.cos(angle) * length)
        y2 = int(cy + math.sin(angle) * length)
        
        alpha = int(180 + 50 * math.sin(frame * 0.5 + i))
        color = color_config["primary"]
        c = (color[0], color[1], color[2], alpha)
        
        draw.line([(x1, y1), (x2, y2)], fill=c, width=2)
    
    # 中心光球
    glow_alpha = int(150 + 50 * math.sin(frame * 0.4))
    for r in range(15, 0, -2):
        a = int(glow_alpha * (1 - r / 15))
        c = (*color_config["glow"], a)
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=c)
    
    # 添加闪烁星星
    sparkles = create_sparkles(size, color_config, frame, total_frames, count=10)
    img = Image.alpha_composite(img, sparkles)
    
    return img.filter(ImageFilter.GaussianBlur(radius=1))


def generate_effect_frames(effect_type, output_dir, size=(80, 100)):
    """生成指定特效类型的所有帧"""
    if effect_type not in WEAPON_EFFECT_CONFIGS:
        print(f"未知特效类型: {effect_type}")
        return
    
    config = WEAPON_EFFECT_CONFIGS[effect_type]
    color_config = EFFECT_COLORS[config["color"]]
    frames = config["frames"]
    effect_style = config["type"]
    
    print(f"\n生成 {config['name']} (类型{effect_type}) 特效...")
    print(f"  风格: {effect_style}, 帧数: {frames}")
    
    os.makedirs(output_dir, exist_ok=True)
    
    all_frames = []
    
    for frame_idx in range(frames):
        # 根据特效风格创建图像
        composite = Image.new('RGBA', size, (0, 0, 0, 0))
        
        if effect_style == "aura_sparkle":
            glow = create_glow_ring(size, color_config, frame_idx, frames)
            sparkles = create_sparkles(size, color_config, frame_idx, frames)
            composite = Image.alpha_composite(glow, sparkles)
        
        elif effect_style == "particles":
            particles = create_particles(size, color_config, frame_idx, frames)
            glow = create_glow_ring(size, color_config, frame_idx, frames, intensity=0.5)
            composite = Image.alpha_composite(glow, particles)
        
        elif effect_style == "mist":
            glow = create_glow_ring(size, color_config, frame_idx, frames, intensity=0.6)
            particles = create_particles(size, color_config, frame_idx, frames, count=8)
            composite = Image.alpha_composite(glow, particles)
        
        elif effect_style == "shadow_pulse":
            composite = create_shadow_pulse(size, color_config, frame_idx, frames)
        
        elif effect_style == "soul_fire":
            soul = create_soul_fire(size, color_config, frame_idx, frames)
            glow = create_glow_ring(size, color_config, frame_idx, frames, intensity=0.4)
            composite = Image.alpha_composite(glow, soul)
        
        elif effect_style == "fire_aura":
            fire = create_fire_aura(size, color_config, frame_idx, frames)
            glow = create_glow_ring(size, color_config, frame_idx, frames, intensity=0.5)
            composite = Image.alpha_composite(glow, fire)
        
        elif effect_style == "dragon_glow":
            composite = create_dragon_glow(size, color_config, frame_idx, frames)
        
        all_frames.append(composite)
        
        # 保存单帧 (使用索引编号)
        index = 30000 + (effect_type - 100) * 20 + frame_idx
        frame_path = os.path.join(output_dir, f"effect_{index:05d}.png")
        composite.save(frame_path, 'PNG')
    
    # 创建精灵图
    sprite_width = size[0] * frames
    sprite_sheet = Image.new('RGBA', (sprite_width, size[1]), (0, 0, 0, 0))
    
    for i, frame in enumerate(all_frames):
        sprite_sheet.paste(frame, (i * size[0], 0))
    
    sprite_path = os.path.join(output_dir, f"effect_type_{effect_type}_sprite.png")
    sprite_sheet.save(sprite_path, 'PNG')
    print(f"  精灵图: {sprite_path}")
    
    return all_frames


def main():
    output_base = os.path.join(os.path.dirname(__file__), "..", "assets", "weapon_effects_v2")
    
    print("=" * 60)
    print("武器特效生成工具 v2.0")
    print("=" * 60)
    
    for effect_type in WEAPON_EFFECT_CONFIGS:
        output_dir = os.path.join(output_base, f"type_{effect_type}")
        generate_effect_frames(effect_type, output_dir)
    
    print("\n" + "=" * 60)
    print("生成完成！")
    print(f"输出目录: {output_base}")
    print("=" * 60)
    
    print("\n客户端集成说明:")
    print("1. 将 effect_*.png 文件打包到 State.data")
    print("2. 索引号对应: 30000 + (特效类型-100)*20 + 帧号")
    print("3. 在数据库 stditems 表设置 reserve[3] 字段为特效类型")
    print("4. 帧率默认200ms，可在 FState.pas 中调整")


if __name__ == "__main__":
    main()
