"""
生成衣服装备界面特效动画
类似截图中的火焰翅膀/冰霜光环效果
用于装备栏显示
"""

import os
import math
import random
from PIL import Image, ImageDraw, ImageFilter, ImageEnhance

# 特效帧配置
EFFECT_WIDTH = 120   # 装备栏特效尺寸
EFFECT_HEIGHT = 160
EFFECT_FRAMES = 20   # 动画帧数

# 衣服特效配置
ARMOR_EFFECTS = {
    # 冰霜系 - 蓝色冰翅膀
    50: {
        'name': '寒冰战甲',
        'type': 'ice_wings',
        'primary': (100, 180, 255),
        'secondary': (200, 230, 255),
        'glow': (220, 240, 255),
    },
    51: {
        'name': '冰魄法袍', 
        'type': 'ice_aura',
        'primary': (120, 200, 255),
        'secondary': (180, 220, 255),
        'glow': (230, 245, 255),
    },
    52: {
        'name': '凝霜道袍',
        'type': 'frost_spiral',
        'primary': (140, 210, 255),
        'secondary': (190, 230, 255),
        'glow': (240, 250, 255),
    },
    # 暗影系 - 紫色暗影
    53: {
        'name': '暗影轻甲',
        'type': 'shadow_wings',
        'primary': (140, 80, 200),
        'secondary': (100, 50, 160),
        'glow': (180, 120, 255),
    },
    54: {
        'name': '夜行者之衣',
        'type': 'dark_mist',
        'primary': (100, 60, 160),
        'secondary': (70, 40, 120),
        'glow': (150, 100, 220),
    },
    # 亡灵系 - 绿色幽魂
    55: {
        'name': '亡灵骨甲',
        'type': 'soul_fire',
        'primary': (100, 200, 80),
        'secondary': (60, 150, 50),
        'glow': (150, 255, 120),
    },
    56: {
        'name': '噬魂法衣',
        'type': 'ghost_aura',
        'primary': (80, 180, 60),
        'secondary': (50, 140, 40),
        'glow': (130, 240, 100),
    },
    57: {
        'name': '幽魂道袍',
        'type': 'spirit_wisps',
        'primary': (90, 190, 70),
        'secondary': (60, 150, 50),
        'glow': (140, 250, 110),
    },
    # 龙系 - 火焰翅膀
    58: {
        'name': '烈焰龙甲',
        'type': 'fire_wings',
        'primary': (255, 120, 40),
        'secondary': (255, 80, 20),
        'glow': (255, 200, 100),
    },
    59: {
        'name': '龙魂法袍',
        'type': 'fire_spiral',
        'primary': (240, 100, 30),
        'secondary': (200, 60, 20),
        'glow': (255, 180, 80),
    },
    60: {
        'name': '龙灵道袍',
        'type': 'dragon_aura',
        'primary': (230, 110, 35),
        'secondary': (190, 70, 25),
        'glow': (255, 190, 90),
    },
    61: {
        'name': '龙皇神甲',
        'type': 'golden_dragon',
        'primary': (255, 200, 80),
        'secondary': (255, 170, 40),
        'glow': (255, 240, 150),
    },
    # 血魔系
    62: {
        'name': '血魔战甲',
        'type': 'blood_wings',
        'primary': (200, 40, 60),
        'secondary': (150, 20, 40),
        'glow': (255, 80, 100),
    },
    # 史莱姆系
    63: {
        'name': '史莱姆软甲',
        'type': 'slime_bubble',
        'primary': (100, 230, 160),
        'secondary': (60, 200, 130),
        'glow': (150, 255, 200),
    },
}

def draw_wings_effect(draw, cx, cy, frame, config, wing_type='fire'):
    """绘制翅膀特效"""
    primary = config['primary']
    secondary = config['secondary']
    glow = config['glow']
    
    phase = frame / EFFECT_FRAMES * math.pi * 2
    
    # 翅膀扇动动画
    wing_angle = 30 + math.sin(phase) * 15
    wing_spread = 40 + math.sin(phase) * 10
    
    for side in [-1, 1]:  # 左右翅膀
        # 多层翅膀羽毛
        for layer in range(5):
            layer_offset = layer * 5
            alpha = 200 - layer * 35
            
            # 主翅膀
            for feather in range(8):
                feather_angle = wing_angle + feather * 8 * side
                rad = math.radians(feather_angle * side)
                
                length = (wing_spread - layer_offset) * (1 - feather * 0.08)
                fx = cx + side * 15 + math.cos(rad) * length * side
                fy = cy - 20 - math.sin(rad) * length * 0.7
                
                # 羽毛颜色渐变
                if feather < 4:
                    color = (*primary, alpha)
                else:
                    color = (*secondary, alpha)
                
                # 绘制羽毛
                draw.polygon([
                    (cx + side * 10, cy - 15),
                    (fx - side * 3, fy - 5),
                    (fx, fy),
                    (fx + side * 3, fy + 3),
                ], fill=color)
        
        # 翅膀发光边缘
        for i in range(12):
            angle = wing_angle + i * 6 * side + math.sin(phase + i * 0.5) * 5
            rad = math.radians(angle * side)
            length = wing_spread + 5
            px = cx + side * 15 + math.cos(rad) * length * side
            py = cy - 20 - math.sin(rad) * length * 0.7
            
            glow_size = 3 + math.sin(phase + i) * 1.5
            glow_alpha = int(150 + math.sin(phase + i * 0.7) * 50)
            draw.ellipse([
                px - glow_size, py - glow_size,
                px + glow_size, py + glow_size
            ], fill=(*glow, glow_alpha))

def draw_aura_effect(draw, cx, cy, frame, config):
    """绘制光环特效"""
    primary = config['primary']
    glow = config['glow']
    
    phase = frame / EFFECT_FRAMES * math.pi * 2
    
    # 多层光环
    for ring in range(4):
        ring_radius = 30 + ring * 12
        particles = 16 - ring * 2
        
        for i in range(particles):
            angle = i * (360 / particles) + frame * (10 - ring * 2)
            rad = math.radians(angle)
            
            # 椭圆轨道
            x = cx + math.cos(rad) * ring_radius
            y = cy + math.sin(rad) * ring_radius * 0.5
            
            # 粒子大小和透明度脉动
            size = 4 + math.sin(phase + i * 0.5 + ring) * 2
            alpha = int(180 - ring * 35 + math.sin(phase + i) * 30)
            
            color = (*primary, alpha) if ring < 2 else (*glow, alpha)
            draw.ellipse([x - size, y - size, x + size, y + size], fill=color)
    
    # 中心光晕
    for r in range(20, 5, -3):
        alpha = int(100 * (1 - r / 20))
        draw.ellipse([cx - r, cy - r, cx + r, cy + r], fill=(*glow, alpha))

def draw_fire_effect(draw, cx, cy, frame, config):
    """绘制火焰特效"""
    primary = config['primary']
    secondary = config['secondary']
    glow = config['glow']
    
    random.seed(frame * 123)
    
    # 火焰粒子
    for i in range(30):
        # 火焰上升轨迹
        base_x = cx + random.randint(-25, 25)
        rise = (frame * 3 + random.randint(0, 40)) % 80
        base_y = cy + 50 - rise
        
        # 火焰摆动
        sway = math.sin((frame + i) * 0.3) * (10 - rise * 0.1)
        fx = base_x + sway
        fy = base_y
        
        # 大小随高度减小
        size = max(1, 6 - rise * 0.06)
        
        # 颜色：底部黄色，顶部红色
        if rise < 30:
            color = (*glow, 200)
        elif rise < 50:
            color = (*primary, 180)
        else:
            color = (*secondary, 120)
        
        draw.ellipse([fx - size, fy - size, fx + size, fy + size], fill=color)
    
    # 火焰核心
    for i in range(10):
        fx = cx + random.randint(-10, 10)
        fy = cy + 30 + random.randint(-5, 5)
        size = random.randint(3, 6)
        draw.ellipse([fx - size, fy - size, fx + size, fy + size], fill=(*glow, 220))

def draw_spiral_effect(draw, cx, cy, frame, config):
    """绘制螺旋特效"""
    primary = config['primary']
    glow = config['glow']
    
    phase = frame / EFFECT_FRAMES * math.pi * 2
    
    # 双螺旋
    for spiral in range(2):
        offset = spiral * math.pi
        
        for i in range(30):
            t = i / 30
            angle = t * 720 + frame * 15 + offset * 180
            rad = math.radians(angle)
            
            # 螺旋半径
            radius = 10 + t * 35
            
            x = cx + math.cos(rad) * radius
            y = cy + math.sin(rad) * radius * 0.5 - t * 40
            
            # 大小和透明度
            size = 2 + (1 - t) * 3
            alpha = int(200 * (1 - t * 0.5))
            
            color = (*primary, alpha) if t < 0.5 else (*glow, alpha)
            draw.ellipse([x - size, y - size, x + size, y + size], fill=color)

def draw_mist_effect(draw, cx, cy, frame, config):
    """绘制迷雾特效"""
    primary = config['primary']
    secondary = config['secondary']
    
    phase = frame / EFFECT_FRAMES * math.pi * 2
    
    # 漂浮的雾气
    for layer in range(5):
        for i in range(8):
            angle = i * 45 + layer * 20 + frame * (3 - layer * 0.5)
            rad = math.radians(angle)
            
            distance = 25 + layer * 10 + math.sin(phase + i) * 8
            x = cx + math.cos(rad) * distance
            y = cy + math.sin(rad) * distance * 0.4 + layer * 5
            
            # 雾气椭圆
            w = 15 - layer * 2
            h = 8 - layer
            alpha = 120 - layer * 20
            
            color = (*primary, alpha) if layer < 3 else (*secondary, alpha)
            draw.ellipse([x - w, y - h, x + w, y + h], fill=color)

def draw_bubble_effect(draw, cx, cy, frame, config):
    """绘制气泡特效"""
    primary = config['primary']
    glow = config['glow']
    
    random.seed(42)  # 固定随机种子保证一致性
    
    # 生成气泡
    for i in range(20):
        # 气泡参数
        start_x = cx + random.randint(-40, 40)
        speed = random.uniform(1.5, 3)
        size_base = random.randint(3, 8)
        phase_offset = random.uniform(0, math.pi * 2)
        
        # 上升位置
        rise = (frame * speed + random.randint(0, 60)) % 100
        y = cy + 60 - rise
        
        # 水平摆动
        x = start_x + math.sin((frame * 0.2 + phase_offset)) * 8
        
        # 大小脉动
        size = size_base + math.sin(frame * 0.3 + phase_offset) * 2
        
        # 透明度
        alpha = int(180 - rise * 1.2)
        if alpha < 30:
            continue
        
        # 气泡
        color = (*primary, alpha)
        draw.ellipse([x - size, y - size, x + size, y + size], fill=color)
        
        # 高光
        highlight_size = size * 0.3
        draw.ellipse([
            x - size * 0.5, y - size * 0.5,
            x - size * 0.5 + highlight_size, y - size * 0.5 + highlight_size
        ], fill=(*glow, min(255, alpha + 50)))

def generate_effect_frame(config, frame):
    """生成单帧特效"""
    img = Image.new('RGBA', (EFFECT_WIDTH, EFFECT_HEIGHT), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img, 'RGBA')
    
    cx = EFFECT_WIDTH // 2
    cy = EFFECT_HEIGHT // 2
    
    effect_type = config['type']
    
    if 'wings' in effect_type:
        draw_wings_effect(draw, cx, cy, frame, config)
    elif 'aura' in effect_type or 'dragon' in effect_type:
        draw_aura_effect(draw, cx, cy, frame, config)
    elif 'fire' in effect_type or 'soul' in effect_type:
        draw_fire_effect(draw, cx, cy, frame, config)
    elif 'spiral' in effect_type:
        draw_spiral_effect(draw, cx, cy, frame, config)
    elif 'mist' in effect_type or 'wisps' in effect_type or 'ghost' in effect_type:
        draw_mist_effect(draw, cx, cy, frame, config)
    elif 'bubble' in effect_type or 'slime' in effect_type:
        draw_bubble_effect(draw, cx, cy, frame, config)
    else:
        draw_aura_effect(draw, cx, cy, frame, config)
    
    # 添加发光模糊
    img = img.filter(ImageFilter.GaussianBlur(radius=1))
    
    return img

def generate_effect_spritesheet(shape_id, config, output_dir):
    """生成特效精灵图"""
    name = config['name']
    print(f"生成 {name} 特效 (Shape {shape_id})...")
    
    # 水平排列所有帧
    sheet_width = EFFECT_WIDTH * EFFECT_FRAMES
    sheet_height = EFFECT_HEIGHT
    
    spritesheet = Image.new('RGBA', (sheet_width, sheet_height), (0, 0, 0, 0))
    
    for frame in range(EFFECT_FRAMES):
        frame_img = generate_effect_frame(config, frame)
        spritesheet.paste(frame_img, (frame * EFFECT_WIDTH, 0))
    
    output_path = os.path.join(output_dir, f"armor_effect_{shape_id}_{name}.png")
    spritesheet.save(output_path, 'PNG')
    print(f"  保存: {output_path}")
    
    return True

def main():
    base_dir = os.path.dirname(os.path.abspath(__file__))
    output_dir = os.path.join(base_dir, "..", "assets", "armor_effects")
    os.makedirs(output_dir, exist_ok=True)
    
    print("=" * 60)
    print("衣服装备特效生成器")
    print("=" * 60)
    
    count = 0
    for shape_id, config in ARMOR_EFFECTS.items():
        if generate_effect_spritesheet(shape_id, config, output_dir):
            count += 1
    
    print("\n" + "=" * 60)
    print(f"完成! 生成了 {count} 套衣服特效")
    print(f"输出目录: {output_dir}")
    print("=" * 60)

if __name__ == "__main__":
    main()
