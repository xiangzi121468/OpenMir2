"""
新增配饰装备图标生成脚本
生成戒指、靴子、腰带的像素风格图标
"""

from PIL import Image, ImageDraw
import os

# 输出目录
OUTPUT_DIR = os.path.join(os.path.dirname(__file__), '..', 'generated_assets', 'accessories')
os.makedirs(OUTPUT_DIR, exist_ok=True)

# 图标尺寸
ICON_SIZE = 32

# 颜色定义
COLORS = {
    # 冰霜系列
    'frost': {
        'primary': (100, 180, 255),
        'secondary': (180, 220, 255),
        'highlight': (220, 240, 255),
        'shadow': (60, 100, 180),
        'glow': (150, 200, 255, 100)
    },
    # 暗影系列
    'shadow': {
        'primary': (60, 40, 80),
        'secondary': (100, 70, 120),
        'highlight': (150, 120, 180),
        'shadow': (30, 20, 50),
        'glow': (120, 80, 160, 100)
    },
    # 亡灵系列
    'undead': {
        'primary': (180, 180, 160),
        'secondary': (140, 140, 120),
        'highlight': (220, 220, 200),
        'shadow': (80, 80, 60),
        'glow': (100, 200, 100, 100)
    },
    # 吸血系列
    'blood': {
        'primary': (180, 30, 30),
        'secondary': (220, 60, 60),
        'highlight': (255, 120, 120),
        'shadow': (100, 20, 20),
        'glow': (255, 50, 50, 100)
    },
    # 龙系列
    'dragon': {
        'primary': (255, 120, 30),
        'secondary': (255, 180, 60),
        'highlight': (255, 220, 150),
        'shadow': (180, 60, 0),
        'glow': (255, 150, 50, 120)
    },
    # 史莱姆系列
    'slime': {
        'primary': (100, 200, 100),
        'secondary': (150, 230, 150),
        'highlight': (200, 255, 200),
        'shadow': (50, 120, 50),
        'glow': (100, 255, 100, 80)
    }
}


def draw_ring(draw, colors, style='normal'):
    """绘制戒指图标"""
    cx, cy = 16, 16
    
    # 戒指环
    for i in range(3):
        offset = i
        draw.ellipse([cx-10+offset, cy-8+offset, cx+10-offset, cy+8-offset], 
                     outline=colors['shadow'] if i == 0 else colors['primary'])
    
    # 宝石底座
    draw.rectangle([cx-4, cy-12, cx+4, cy-6], fill=colors['secondary'])
    
    # 宝石
    if style == 'crystal':
        # 菱形宝石
        draw.polygon([(cx, cy-14), (cx+4, cy-10), (cx, cy-6), (cx-4, cy-10)], 
                     fill=colors['highlight'])
        draw.line([(cx, cy-14), (cx+2, cy-10)], fill=(255, 255, 255))
    elif style == 'skull':
        # 骷髅宝石
        draw.ellipse([cx-3, cy-13, cx+3, cy-8], fill=colors['highlight'])
        draw.point((cx-1, cy-11), fill=colors['shadow'])
        draw.point((cx+1, cy-11), fill=colors['shadow'])
    else:
        # 圆形宝石
        draw.ellipse([cx-3, cy-13, cx+3, cy-7], fill=colors['highlight'])
        draw.arc([cx-3, cy-13, cx+3, cy-7], 200, 340, fill=(255, 255, 255))


def draw_boots(draw, colors, style='normal'):
    """绘制靴子图标"""
    # 左靴子
    draw.rectangle([6, 10, 14, 26], fill=colors['primary'])
    draw.rectangle([4, 22, 16, 28], fill=colors['secondary'])
    draw.polygon([(4, 28), (16, 28), (18, 30), (2, 30)], fill=colors['shadow'])
    
    # 右靴子 (略微偏移)
    draw.rectangle([18, 8, 26, 24], fill=colors['primary'])
    draw.rectangle([16, 20, 28, 26], fill=colors['secondary'])
    draw.polygon([(16, 26), (28, 26), (30, 28), (14, 28)], fill=colors['shadow'])
    
    # 装饰
    if style == 'flame':
        # 火焰装饰
        draw.polygon([(8, 10), (10, 6), (12, 10)], fill=colors['highlight'])
        draw.polygon([(20, 8), (22, 4), (24, 8)], fill=colors['highlight'])
    elif style == 'ice':
        # 冰晶装饰
        draw.line([(10, 12), (10, 8)], fill=colors['highlight'], width=2)
        draw.line([(22, 10), (22, 6)], fill=colors['highlight'], width=2)
    elif style == 'wing':
        # 翅膀装饰
        draw.polygon([(14, 14), (18, 10), (18, 18)], fill=colors['highlight'])
    
    # 高光
    draw.line([(7, 12), (7, 20)], fill=colors['highlight'])
    draw.line([(19, 10), (19, 18)], fill=colors['highlight'])


def draw_belt(draw, colors, style='normal'):
    """绘制腰带图标"""
    # 腰带主体
    draw.rectangle([4, 12, 28, 20], fill=colors['primary'])
    draw.rectangle([4, 12, 28, 14], fill=colors['highlight'])
    draw.rectangle([4, 18, 28, 20], fill=colors['shadow'])
    
    # 腰带扣
    draw.rectangle([12, 10, 20, 22], fill=colors['secondary'])
    draw.rectangle([14, 12, 18, 20], fill=colors['shadow'])
    
    # 装饰
    if style == 'gem':
        # 宝石装饰
        draw.ellipse([14, 13, 18, 17], fill=colors['highlight'])
    elif style == 'skull':
        # 骷髅装饰
        draw.ellipse([13, 12, 19, 18], fill=colors['highlight'])
        draw.point((14, 14), fill=colors['shadow'])
        draw.point((17, 14), fill=colors['shadow'])
    elif style == 'dragon':
        # 龙纹装饰
        draw.polygon([(15, 11), (17, 11), (16, 14)], fill=colors['highlight'])
        draw.polygon([(15, 19), (17, 19), (16, 16)], fill=colors['highlight'])
    
    # 金属扣环高光
    draw.arc([12, 10, 20, 22], 220, 320, fill=(255, 255, 255))


def create_accessory_icon(name, item_type, color_scheme, style='normal'):
    """创建配饰图标"""
    img = Image.new('RGBA', (ICON_SIZE, ICON_SIZE), (0, 0, 0, 0))
    draw = ImageDraw.Draw(img)
    
    colors = COLORS[color_scheme]
    
    if item_type == 'ring':
        draw_ring(draw, colors, style)
    elif item_type == 'boots':
        draw_boots(draw, colors, style)
    elif item_type == 'belt':
        draw_belt(draw, colors, style)
    
    # 保存图片
    filename = f"{name}.png"
    filepath = os.path.join(OUTPUT_DIR, filename)
    img.save(filepath)
    print(f"Generated: {filename}")
    return filepath


def generate_all_accessories():
    """生成所有配饰图标"""
    accessories = [
        # 冰霜领主系列
        ('冰晶戒指', 'ring', 'frost', 'crystal'),
        ('霜魄戒指', 'ring', 'frost', 'crystal'),
        ('凝霜戒指', 'ring', 'frost', 'crystal'),
        ('寒冰之靴', 'boots', 'frost', 'ice'),
        ('冰魄之靴', 'boots', 'frost', 'ice'),
        ('凝霜之靴', 'boots', 'frost', 'ice'),
        ('寒冰腰带', 'belt', 'frost', 'gem'),
        ('冰魄腰带', 'belt', 'frost', 'gem'),
        ('凝霜腰带', 'belt', 'frost', 'gem'),
        
        # 暗影刺客系列
        ('夜行戒指', 'ring', 'shadow', 'normal'),
        ('暗影戒指', 'ring', 'shadow', 'normal'),
        ('暗夜之靴', 'boots', 'shadow', 'wing'),
        ('暗影腰带', 'belt', 'shadow', 'gem'),
        
        # 死灵召唤师系列
        ('亡灵戒指', 'ring', 'undead', 'skull'),
        ('噬魂戒指', 'ring', 'undead', 'skull'),
        ('骸骨戒指', 'ring', 'undead', 'skull'),
        ('亡灵之靴', 'boots', 'undead', 'normal'),
        ('噬魂之靴', 'boots', 'undead', 'normal'),
        ('骸骨之靴', 'boots', 'undead', 'normal'),
        ('亡灵腰带', 'belt', 'undead', 'skull'),
        ('噬魂腰带', 'belt', 'undead', 'skull'),
        ('骸骨腰带', 'belt', 'undead', 'skull'),
        
        # 吸血蝙蝠系列
        ('血魄戒指', 'ring', 'blood', 'crystal'),
        ('血蝠戒指', 'ring', 'blood', 'normal'),
        ('血影之靴', 'boots', 'blood', 'normal'),
        ('血蝠腰带', 'belt', 'blood', 'gem'),
        
        # 烈焰魔龙系列
        ('龙炎戒指', 'ring', 'dragon', 'crystal'),
        ('龙魂戒指', 'ring', 'dragon', 'crystal'),
        ('龙威戒指', 'ring', 'dragon', 'crystal'),
        ('龙炎之靴', 'boots', 'dragon', 'flame'),
        ('龙魂之靴', 'boots', 'dragon', 'flame'),
        ('龙威之靴', 'boots', 'dragon', 'flame'),
        ('龙炎腰带', 'belt', 'dragon', 'dragon'),
        ('龙魂腰带', 'belt', 'dragon', 'dragon'),
        ('龙威腰带', 'belt', 'dragon', 'dragon'),
        
        # 大史莱姆系列
        ('黏液戒指', 'ring', 'slime', 'normal'),
        ('黏液之靴', 'boots', 'slime', 'normal'),
        ('黏液腰带', 'belt', 'slime', 'gem'),
    ]
    
    print(f"开始生成 {len(accessories)} 个配饰图标...")
    print(f"输出目录: {OUTPUT_DIR}")
    print("-" * 50)
    
    for name, item_type, color_scheme, style in accessories:
        create_accessory_icon(name, item_type, color_scheme, style)
    
    print("-" * 50)
    print(f"完成! 共生成 {len(accessories)} 个配饰图标")
    
    # 生成精灵表
    generate_sprite_sheet(accessories)


def generate_sprite_sheet(accessories):
    """生成精灵表 (用于客户端资源打包)"""
    cols = 8
    rows = (len(accessories) + cols - 1) // cols
    
    sheet_width = cols * ICON_SIZE
    sheet_height = rows * ICON_SIZE
    
    sheet = Image.new('RGBA', (sheet_width, sheet_height), (0, 0, 0, 0))
    
    for i, (name, _, _, _) in enumerate(accessories):
        filepath = os.path.join(OUTPUT_DIR, f"{name}.png")
        if os.path.exists(filepath):
            icon = Image.open(filepath)
            x = (i % cols) * ICON_SIZE
            y = (i // cols) * ICON_SIZE
            sheet.paste(icon, (x, y))
    
    sheet_path = os.path.join(OUTPUT_DIR, 'accessories_spritesheet.png')
    sheet.save(sheet_path)
    print(f"\n精灵表已生成: accessories_spritesheet.png ({sheet_width}x{sheet_height})")
    
    # 生成索引文件
    index_path = os.path.join(OUTPUT_DIR, 'accessories_index.txt')
    with open(index_path, 'w', encoding='utf-8') as f:
        f.write("# 配饰图标索引\n")
        f.write("# 格式: 索引,名称,类型,X,Y\n")
        for i, (name, item_type, _, _) in enumerate(accessories):
            x = (i % cols) * ICON_SIZE
            y = (i // cols) * ICON_SIZE
            f.write(f"{i},{name},{item_type},{x},{y}\n")
    print(f"索引文件已生成: accessories_index.txt")


if __name__ == '__main__':
    generate_all_accessories()
