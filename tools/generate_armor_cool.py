"""
生成炫酷衣服精灵图 - 带光环翅膀特效
参考截图中的火焰/冰霜风格
"""

import os
import math
import random
from PIL import Image, ImageDraw, ImageFilter, ImageEnhance

# 帧配置
FRAME_WIDTH = 80
FRAME_HEIGHT = 100
HUMANFRAME = 600
DIRECTIONS = 8
COLS = 25

# 动作帧数
ACTIONS = {
    'stand': 4,      # 站立
    'walk': 6,       # 行走
    'run': 6,        # 跑步
    'attack1': 6,    # 攻击1
    'attack2': 6,    # 攻击2
    'magic': 6,      # 施法
    'hit': 3,        # 受击
    'die': 4,        # 死亡
    'special': 10,   # 特殊动作
}

# 衣服配置 - 带特效风格
ARMOR_CONFIGS = {
    # 冰霜系 - 蓝色冰晶效果
    50: {
        'name': '寒冰战甲',
        'base_color': (40, 80, 160),
        'effect_color': (100, 180, 255),
        'glow_color': (200, 230, 255),
        'style': 'heavy',
        'effect_type': 'ice_wings',
    },
    51: {
        'name': '冰魄法袍',
        'base_color': (50, 100, 180),
        'effect_color': (120, 200, 255),
        'glow_color': (220, 240, 255),
        'style': 'robe',
        'effect_type': 'ice_aura',
    },
    52: {
        'name': '凝霜道袍',
        'base_color': (60, 120, 170),
        'effect_color': (140, 210, 255),
        'glow_color': (230, 245, 255),
        'style': 'robe',
        'effect_type': 'frost_particles',
    },
    # 暗影系 - 紫黑色暗影效果
    53: {
        'name': '暗影轻甲',
        'base_color': (60, 30, 80),
        'effect_color': (140, 80, 200),
        'glow_color': (180, 120, 255),
        'style': 'light',
        'effect_type': 'shadow_cloak',
    },
    54: {
        'name': '夜行者之衣',
        'base_color': (40, 20, 60),
        'effect_color': (100, 60, 160),
        'glow_color': (150, 100, 220),
        'style': 'light',
        'effect_type': 'shadow_mist',
    },
    # 亡灵系 - 绿色幽魂效果
    55: {
        'name': '亡灵骨甲',
        'base_color': (60, 80, 50),
        'effect_color': (100, 200, 80),
        'glow_color': (150, 255, 120),
        'style': 'heavy',
        'effect_type': 'soul_flames',
    },
    56: {
        'name': '噬魂法衣',
        'base_color': (50, 70, 40),
        'effect_color': (80, 180, 60),
        'glow_color': (130, 240, 100),
        'style': 'robe',
        'effect_type': 'ghost_aura',
    },
    57: {
        'name': '幽魂道袍',
        'base_color': (45, 65, 45),
        'effect_color': (90, 190, 70),
        'glow_color': (140, 250, 110),
        'style': 'robe',
        'effect_type': 'spirit_wisps',
    },
    # 龙系 - 橙红色火焰效果
    58: {
        'name': '烈焰龙甲',
        'base_color': (160, 50, 30),
        'effect_color': (255, 120, 40),
        'glow_color': (255, 200, 100),
        'style': 'heavy',
        'effect_type': 'fire_wings',
    },
    59: {
        'name': '龙魂法袍',
        'base_color': (140, 40, 20),
        'effect_color': (240, 100, 30),
        'glow_color': (255, 180, 80),
        'style': 'robe',
        'effect_type': 'fire_aura',
    },
    60: {
        'name': '龙灵道袍',
        'base_color': (130, 50, 25),
        'effect_color': (230, 110, 35),
        'glow_color': (255, 190, 90),
        'style': 'robe',
        'effect_type': 'dragon_flames',
    },
    61: {
        'name': '龙皇神甲',
        'base_color': (180, 140, 40),
        'effect_color': (255, 200, 80),
        'glow_color': (255, 240, 150),
        'style': 'heavy',
        'effect_type': 'golden_dragon',
    },
    # 血魔系 - 深红血色效果
    62: {
        'name': '血魔战甲',
        'base_color': (120, 20, 30),
        'effect_color': (200, 40, 60),
        'glow_color': (255, 80, 100),
        'style': 'heavy',
        'effect_type': 'blood_aura',
    },
    # 史莱姆系 - 透明果冻效果
    63: {
        'name': '史莱姆软甲',
        'base_color': (60, 180, 120),
        'effect_color': (100, 230, 160),
        'glow_color': (150, 255, 200),
        'style': 'light',
        'effect_type': 'slime_glow',
    },
}

def draw_character_body(draw, cx, cy, direction, frame, config):
    """绘制角色身体"""
    style = config['style']
    base = config['base_color']
    effect = config['effect_color']
    
    # 根据方向调整角度
    angle_offset = direction * 45
    
    # 身体摆动
    sway = math.sin(frame * 0.5) * 2
    
    if style == 'heavy':
        # 重甲 - 宽大肩膀
        # 头部
        draw.ellipse([cx-8, cy-45+sway, cx+8, cy-30+sway], fill=base, outline=effect)
        # 肩甲
        draw.polygon([
            (cx-18, cy-28+sway), (cx-25, cy-20+sway), 
            (cx-20, cy-10+sway), (cx-10, cy-15+sway)
        ], fill=effect, outline=config['glow_color'])
        draw.polygon([
            (cx+18, cy-28+sway), (cx+25, cy-20+sway), 
            (cx+20, cy-10+sway), (cx+10, cy-15+sway)
        ], fill=effect, outline=config['glow_color'])
        # 身体
        draw.polygon([
            (cx-15, cy-25+sway), (cx+15, cy-25+sway),
            (cx+12, cy+10+sway), (cx-12, cy+10+sway)
        ], fill=base, outline=effect)
        # 腰带
        draw.rectangle([cx-14, cy+5+sway, cx+14, cy+12+sway], fill=effect, outline=config['glow_color'])
        # 裙甲
        draw.polygon([
            (cx-14, cy+12+sway), (cx+14, cy+12+sway),
            (cx+16, cy+35+sway), (cx-16, cy+35+sway)
        ], fill=base, outline=effect)
        
    elif style == 'robe':
        # 法袍 - 飘逸长袍
        # 头部
        draw.ellipse([cx-7, cy-42+sway, cx+7, cy-28+sway], fill=base, outline=effect)
        # 兜帽
        draw.arc([cx-12, cy-48+sway, cx+12, cy-25+sway], 180, 0, fill=effect, width=2)
        # 上身
        draw.polygon([
            (cx-10, cy-25+sway), (cx+10, cy-25+sway),
            (cx+8, cy+5+sway), (cx-8, cy+5+sway)
        ], fill=base, outline=effect)
        # 腰带
        draw.rectangle([cx-10, cy+3+sway, cx+10, cy+8+sway], fill=effect, outline=config['glow_color'])
        # 长袍
        robe_sway = math.sin(frame * 0.3 + direction) * 3
        draw.polygon([
            (cx-10, cy+8+sway), (cx+10, cy+8+sway),
            (cx+18+robe_sway, cy+40+sway), (cx-18-robe_sway, cy+40+sway)
        ], fill=base, outline=effect)
        
    else:  # light
        # 轻甲 - 紧身
        # 头部
        draw.ellipse([cx-7, cy-43+sway, cx+7, cy-30+sway], fill=base, outline=effect)
        # 上身
        draw.polygon([
            (cx-10, cy-28+sway), (cx+10, cy-28+sway),
            (cx+8, cy+5+sway), (cx-8, cy+5+sway)
        ], fill=base, outline=effect)
        # 腰带
        draw.rectangle([cx-9, cy+3+sway, cx+9, cy+8+sway], fill=effect, outline=config['glow_color'])
        # 下身
        draw.polygon([
            (cx-9, cy+8+sway), (cx+9, cy+8+sway),
            (cx+10, cy+35+sway), (cx-10, cy+35+sway)
        ], fill=base, outline=effect)

def draw_effect_wings(img, draw, cx, cy, frame, config, effect_type):
    """绘制翅膀/光环特效"""
    effect = config['effect_color']
    glow = config['glow_color']
    
    # 动画相位
    phase = frame * 0.4
    
    if 'wings' in effect_type or 'fire' in effect_type:
        # 翅膀效果
        wing_spread = 20 + math.sin(phase) * 5
        wing_height = 30 + math.cos(phase) * 3
        
        # 左翅膀
        for i in range(5):
            alpha = 180 - i * 30
            offset = i * 3
            wing_color = (*effect[:3], alpha)
            draw.polygon([
                (cx-5, cy-20),
                (cx-wing_spread-offset, cy-wing_height+offset),
                (cx-wing_spread-offset-5, cy-10+offset),
                (cx-8, cy-5)
            ], fill=wing_color)
        
        # 右翅膀
        for i in range(5):
            alpha = 180 - i * 30
            offset = i * 3
            wing_color = (*effect[:3], alpha)
            draw.polygon([
                (cx+5, cy-20),
                (cx+wing_spread+offset, cy-wing_height+offset),
                (cx+wing_spread+offset+5, cy-10+offset),
                (cx+8, cy-5)
            ], fill=wing_color)
            
    elif 'aura' in effect_type:
        # 光环效果
        for i in range(8):
            angle = i * 45 + frame * 10
            rad = math.radians(angle)
            radius = 25 + math.sin(phase + i) * 5
            x = cx + math.cos(rad) * radius
            y = cy - 10 + math.sin(rad) * radius * 0.5
            size = 4 + math.sin(phase + i * 0.5) * 2
            alpha = int(150 + math.sin(phase + i) * 50)
            particle_color = (*glow[:3], alpha)
            draw.ellipse([x-size, y-size, x+size, y+size], fill=particle_color)
            
    elif 'particles' in effect_type or 'wisps' in effect_type:
        # 粒子效果
        random.seed(frame)
        for i in range(15):
            px = cx + random.randint(-30, 30)
            py = cy + random.randint(-40, 30) - (frame % 20) * 2
            size = random.randint(1, 3)
            alpha = random.randint(100, 200)
            particle_color = (*glow[:3], alpha)
            draw.ellipse([px-size, py-size, px+size, py+size], fill=particle_color)
            
    elif 'mist' in effect_type or 'cloak' in effect_type:
        # 迷雾效果
        for i in range(6):
            angle = i * 60 + frame * 5
            rad = math.radians(angle)
            rx = 20 + math.sin(phase + i) * 5
            ry = 35
            for j in range(3):
                x = cx + math.cos(rad) * (rx - j * 5)
                y = cy + 10 + math.sin(rad) * (ry - j * 8) * 0.3
                alpha = 100 - j * 25
                mist_color = (*effect[:3], alpha)
                draw.ellipse([x-8+j*2, y-4, x+8-j*2, y+4], fill=mist_color)
                
    elif 'flames' in effect_type:
        # 火焰效果
        for i in range(10):
            fx = cx + random.randint(-15, 15)
            fy_base = cy + 30
            flame_height = 20 + random.randint(0, 15)
            fy = fy_base - flame_height - (frame % 10) * 2
            
            alpha = 150 + random.randint(0, 50)
            flame_color = (*effect[:3], alpha)
            
            # 火焰形状
            draw.polygon([
                (fx, fy_base),
                (fx - 3, fy_base - flame_height * 0.3),
                (fx, fy),
                (fx + 3, fy_base - flame_height * 0.3)
            ], fill=flame_color)
            
    elif 'golden' in effect_type:
        # 金龙特效 - 环绕光环+粒子
        for ring in range(3):
            ring_radius = 25 + ring * 8
            for i in range(12):
                angle = i * 30 + frame * (8 - ring * 2)
                rad = math.radians(angle)
                x = cx + math.cos(rad) * ring_radius
                y = cy - 5 + math.sin(rad) * ring_radius * 0.4
                size = 3 - ring * 0.5
                alpha = int(200 - ring * 40)
                gold_color = (*glow[:3], alpha)
                draw.ellipse([x-size, y-size, x+size, y+size], fill=gold_color)
                
    elif 'blood' in effect_type:
        # 血色特效
        for i in range(8):
            angle = i * 45 + frame * 3
            rad = math.radians(angle)
            distance = 20 + math.sin(phase + i) * 8
            x = cx + math.cos(rad) * distance
            y = cy + math.sin(rad) * distance * 0.5
            
            # 血滴
            drop_len = 5 + math.sin(phase + i * 0.7) * 3
            alpha = int(180 + math.sin(phase + i) * 40)
            blood_color = (*effect[:3], alpha)
            draw.ellipse([x-2, y-2, x+2, y+drop_len], fill=blood_color)
            
    elif 'slime' in effect_type:
        # 史莱姆发光效果
        for i in range(6):
            bx = cx + math.sin(phase + i * 1.2) * 15
            by = cy + 10 + math.cos(phase + i * 0.8) * 10
            size = 5 + math.sin(phase + i * 0.5) * 3
            alpha = int(120 + math.sin(phase + i) * 50)
            slime_color = (*glow[:3], alpha)
            draw.ellipse([bx-size, by-size, bx+size, by+size], fill=slime_color)

def add_glow_effect(img, glow_color, intensity=1.5):
    """添加整体发光效果"""
    # 创建发光层
    glow_layer = img.copy()
    glow_layer = glow_layer.filter(ImageFilter.GaussianBlur(radius=3))
    
    # 增强亮度
    enhancer = ImageEnhance.Brightness(glow_layer)
    glow_layer = enhancer.enhance(intensity)
    
    # 合成
    result = Image.alpha_composite(glow_layer, img)
    return result

def generate_single_frame(config, direction, frame_idx, action_frame):
    """生成单帧图像"""
    img = Image.new('RGBA', (FRAME_WIDTH, FRAME_HEIGHT), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img, 'RGBA')
    
    cx = FRAME_WIDTH // 2
    cy = FRAME_HEIGHT // 2 + 5
    
    # 绘制特效（底层）
    draw_effect_wings(img, draw, cx, cy, action_frame, config, config['effect_type'])
    
    # 绘制角色身体
    draw_character_body(draw, cx, cy, direction, action_frame, config)
    
    # 添加发光
    img = add_glow_effect(img, config['glow_color'], 1.3)
    
    return img

def generate_armor_spritesheet(shape_id, config, output_dir):
    """生成完整的衣服精灵图 (600帧)"""
    name = config['name']
    print(f"生成 {name} (Shape {shape_id})...")
    
    # 计算精灵图尺寸
    rows = (HUMANFRAME + COLS - 1) // COLS
    sheet_width = COLS * FRAME_WIDTH
    sheet_height = rows * FRAME_HEIGHT
    
    spritesheet = Image.new('RGBA', (sheet_width, sheet_height), (0, 0, 0, 0))
    
    frame_idx = 0
    for direction in range(DIRECTIONS):
        for action_name, frame_count in ACTIONS.items():
            for action_frame in range(frame_count):
                if frame_idx >= HUMANFRAME:
                    break
                    
                # 生成单帧
                frame_img = generate_single_frame(config, direction, frame_idx, action_frame)
                
                # 放置到精灵图
                x = (frame_idx % COLS) * FRAME_WIDTH
                y = (frame_idx // COLS) * FRAME_HEIGHT
                spritesheet.paste(frame_img, (x, y))
                
                frame_idx += 1
        
        # 填充剩余帧
        while frame_idx % (HUMANFRAME // DIRECTIONS) != 0 and frame_idx < HUMANFRAME:
            frame_img = generate_single_frame(config, direction, frame_idx, 0)
            x = (frame_idx % COLS) * FRAME_WIDTH
            y = (frame_idx // COLS) * FRAME_HEIGHT
            spritesheet.paste(frame_img, (x, y))
            frame_idx += 1
    
    # 填充到600帧
    while frame_idx < HUMANFRAME:
        frame_img = generate_single_frame(config, 0, frame_idx, frame_idx % 4)
        x = (frame_idx % COLS) * FRAME_WIDTH
        y = (frame_idx // COLS) * FRAME_HEIGHT
        spritesheet.paste(frame_img, (x, y))
        frame_idx += 1
    
    # 保存
    output_path = os.path.join(output_dir, f"armor_{shape_id}_{name}.png")
    spritesheet.save(output_path, 'PNG')
    print(f"  保存: {output_path}")
    
    return True

def main():
    base_dir = os.path.dirname(os.path.abspath(__file__))
    output_dir = os.path.join(base_dir, "..", "assets", "armor_sprites_cool")
    os.makedirs(output_dir, exist_ok=True)
    
    print("=" * 60)
    print("炫酷衣服精灵图生成器")
    print("=" * 60)
    
    count = 0
    for shape_id, config in ARMOR_CONFIGS.items():
        if generate_armor_spritesheet(shape_id, config, output_dir):
            count += 1
    
    print("\n" + "=" * 60)
    print(f"完成! 生成了 {count} 套衣服资源")
    print(f"输出目录: {output_dir}")
    print("=" * 60)

if __name__ == "__main__":
    main()
