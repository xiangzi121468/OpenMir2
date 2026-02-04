#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
配饰装备图标打包工具
将生成的配饰图标(戒指、靴子、腰带)打包成客户端.data格式

客户端资源文件:
- StateItem.data: 物品包裹中的图标 (32x32)
- DnItems.data: 地上掉落物品图标 (48x48 或更大)
"""

import os
import struct
import io
import shutil
from PIL import Image

# 路径配置
BASE_DIR = os.path.dirname(os.path.dirname(__file__))
INPUT_DIR = os.path.join(BASE_DIR, 'generated_assets', 'accessories')
OUTPUT_DIR = os.path.join(BASE_DIR, 'MirClient', 'Data')
ASSETS_DIR = os.path.join(BASE_DIR, 'assets', 'accessories')

# 确保输出目录存在
os.makedirs(OUTPUT_DIR, exist_ok=True)
os.makedirs(ASSETS_DIR, exist_ok=True)

# 配饰图标索引映射 (与SQL中的Shape对应)
# 索引规划:
# - 戒指: 50-70
# - 靴子: 200-220
# - 腰带: 250-270 (避免与靴子冲突)

ACCESSORY_INDEX_MAP = {
    # === 戒指 (50-70) ===
    # 冰霜系列
    '冰晶戒指': 50,
    '霜魄戒指': 51,
    '凝霜戒指': 52,
    # 暗影系列
    '夜行戒指': 53,
    '暗影戒指': 54,
    # 死灵系列
    '亡灵戒指': 55,
    '噬魂戒指': 56,
    '骸骨戒指': 57,
    # 吸血系列
    '血魄戒指': 58,
    '血蝠戒指': 59,
    # 龙系列
    '龙炎戒指': 60,
    '龙魂戒指': 61,
    '龙威戒指': 62,
    # 史莱姆系列
    '黏液戒指': 63,
    
    # === 靴子 (200-220) ===
    # 冰霜系列
    '寒冰之靴': 200,
    '冰魄之靴': 201,
    '凝霜之靴': 202,
    # 暗影系列
    '暗夜之靴': 203,
    # 死灵系列
    '亡灵之靴': 204,
    '噬魂之靴': 205,
    '骸骨之靴': 206,
    # 吸血系列
    '血影之靴': 207,
    # 龙系列
    '龙炎之靴': 208,
    '龙魂之靴': 209,
    '龙威之靴': 210,
    # 史莱姆系列
    '黏液之靴': 211,
    
    # === 腰带 (250-270) ===
    # 冰霜系列
    '寒冰腰带': 250,
    '冰魄腰带': 251,
    '凝霜腰带': 252,
    # 暗影系列
    '暗影腰带': 253,
    # 死灵系列
    '亡灵腰带': 254,
    '噬魂腰带': 255,
    '骸骨腰带': 256,
    # 吸血系列
    '血蝠腰带': 257,
    # 龙系列
    '龙炎腰带': 258,
    '龙魂腰带': 259,
    '龙威腰带': 260,
    # 史莱姆系列
    '黏液腰带': 261,
}


def create_pascal_short_string(s, max_len):
    """创建Pascal短字符串格式"""
    encoded = s.encode('ascii', errors='replace')[:max_len]
    length = len(encoded)
    padding = max_len - length
    return bytes([length]) + encoded + bytes(padding)


def pack_accessories_to_data():
    """将配饰图标打包成.data文件"""
    
    print("=" * 60)
    print("配饰装备图标打包工具")
    print("=" * 60)
    
    # 检查输入目录
    if not os.path.isdir(INPUT_DIR):
        print(f"错误: 输入目录不存在: {INPUT_DIR}")
        print("请先运行 generate_accessories.py 生成图标")
        return False
    
    # 收集所有PNG文件
    png_files = {}
    for f in os.listdir(INPUT_DIR):
        if f.endswith('.png') and not f.startswith('accessories_'):
            name = os.path.splitext(f)[0]
            if name in ACCESSORY_INDEX_MAP:
                index = ACCESSORY_INDEX_MAP[name]
                png_files[index] = os.path.join(INPUT_DIR, f)
    
    if not png_files:
        print("错误: 没有找到有效的配饰图标")
        return False
    
    print(f"\n找到 {len(png_files)} 个配饰图标")
    
    # 计算索引范围
    min_index = min(png_files.keys())
    max_index = max(png_files.keys())
    total_images = max_index + 1
    
    print(f"索引范围: {min_index} - {max_index}")
    print(f"总槽位数: {total_images}")
    
    # 文件头大小
    header_size = 41 + 4 + 4 + 2 + 17  # 68字节
    index_size = total_images * 4
    
    # 构建文件头
    header = bytearray()
    header.extend(create_pascal_short_string('AccessoryItems', 40))
    header.extend(struct.pack('<I', total_images))
    header.extend(struct.pack('<I', header_size))
    header.extend(struct.pack('<H', 1))
    header.extend(create_pascal_short_string('', 16))
    
    # 索引数组
    index_array = [0] * total_images
    
    # 图像数据
    image_data = bytearray()
    current_offset = header_size + index_size
    
    print("\n处理图标:")
    print("-" * 40)
    
    for index in sorted(png_files.keys()):
        img_path = png_files[index]
        name = os.path.splitext(os.path.basename(img_path))[0]
        
        try:
            img = Image.open(img_path)
            width, height = img.size
            
            # 转换为PNG格式存储
            png_buffer = io.BytesIO()
            img.save(png_buffer, format='PNG')
            png_data = png_buffer.getvalue()
            
            # 图像信息头 (14字节)
            img_info = struct.pack('<HHBHHI B',
                width,          # m_nWidth
                height,         # m_nHeight
                32,             # bitCount
                0,              # m_wPx
                0,              # m_wPy
                len(png_data),  # m_Len
                1               # GraphicType (PNG)
            )
            
            # 记录索引
            index_array[index] = current_offset
            
            # 添加图像数据
            image_data.extend(img_info)
            image_data.extend(png_data)
            
            current_offset += len(img_info) + len(png_data)
            
            print(f"  [{index:3d}] {name} ({width}x{height})")
            
        except Exception as e:
            print(f"  [{index:3d}] 错误: {e}")
    
    # 生成StateItem扩展文件
    output_file = os.path.join(OUTPUT_DIR, 'AccessoryItems.data')
    
    with open(output_file, 'wb') as f:
        f.write(header)
        for offset in index_array:
            f.write(struct.pack('<I', offset))
        f.write(image_data)
    
    file_size = os.path.getsize(output_file)
    print("-" * 40)
    print(f"\n已生成: {output_file}")
    print(f"文件大小: {file_size:,} 字节")
    
    # 复制到assets目录备份
    backup_file = os.path.join(ASSETS_DIR, 'AccessoryItems.data')
    shutil.copy(output_file, backup_file)
    print(f"备份到: {backup_file}")
    
    # 生成索引映射文件
    generate_index_file()
    
    return True


def generate_index_file():
    """生成索引映射文件，供客户端代码参考"""
    
    index_file = os.path.join(ASSETS_DIR, 'accessory_index.txt')
    
    with open(index_file, 'w', encoding='utf-8') as f:
        f.write("// 配饰装备图标索引映射\n")
        f.write("// 格式: 索引,名称,类型\n")
        f.write("// 用于客户端代码中加载正确的图标\n\n")
        
        # 按类型分组
        rings = []
        boots = []
        belts = []
        
        for name, index in sorted(ACCESSORY_INDEX_MAP.items(), key=lambda x: x[1]):
            if '戒指' in name:
                rings.append((index, name))
            elif '靴' in name:
                boots.append((index, name))
            elif '腰带' in name:
                belts.append((index, name))
        
        f.write("// === 戒指 (Ring) ===\n")
        for index, name in rings:
            f.write(f"{index},{name},ring\n")
        
        f.write("\n// === 靴子 (Boots) ===\n")
        for index, name in boots:
            f.write(f"{index},{name},boots\n")
        
        f.write("\n// === 腰带 (Belt) ===\n")
        for index, name in belts:
            f.write(f"{index},{name},belt\n")
    
    print(f"索引文件: {index_file}")


def generate_client_code_snippet():
    """生成客户端代码片段，用于加载配饰图标"""
    
    code_file = os.path.join(ASSETS_DIR, 'client_code_snippet.pas')
    
    with open(code_file, 'w', encoding='utf-8') as f:
        f.write('''// 配饰装备图标加载代码片段
// 添加到 MShare.pas

const
  ACCESSORYIMAGESFILE = 'Data\AccessoryItems.data';

var
  g_WAccessoryImages: TWMImages;

// 在初始化时加载
procedure InitAccessoryImages;
begin
  g_WAccessoryImages := TWMImages.Create;
  g_WAccessoryImages.FileName := ACCESSORYIMAGESFILE;
  g_WAccessoryImages.Initialize;
end;

// 获取配饰图标
function GetAccessoryImage(nIndex: Integer): TDirectDrawSurface;
begin
  if nIndex < g_WAccessoryImages.ImageCount then
    Result := g_WAccessoryImages.Images[nIndex]
  else
    Result := nil;
end;

// 配饰索引常量
const
  // 戒指 (Ring)
  IDX_RING_ICE_CRYSTAL = 50;    // 冰晶戒指
  IDX_RING_FROST_SOUL = 51;     // 霜魄戒指
  IDX_RING_FROST_CONDENSE = 52; // 凝霜戒指
  IDX_RING_NIGHT_WALK = 53;     // 夜行戒指
  IDX_RING_SHADOW = 54;         // 暗影戒指
  IDX_RING_UNDEAD = 55;         // 亡灵戒指
  IDX_RING_SOUL_DEVOUR = 56;    // 噬魂戒指
  IDX_RING_SKELETON = 57;       // 骸骨戒指
  IDX_RING_BLOOD_SOUL = 58;     // 血魄戒指
  IDX_RING_BLOOD_BAT = 59;      // 血蝠戒指
  IDX_RING_DRAGON_FLAME = 60;   // 龙炎戒指
  IDX_RING_DRAGON_SOUL = 61;    // 龙魂戒指
  IDX_RING_DRAGON_MIGHT = 62;   // 龙威戒指
  IDX_RING_SLIME = 63;          // 黏液戒指
  
  // 靴子 (Boots)
  IDX_BOOTS_ICE = 200;          // 寒冰之靴
  IDX_BOOTS_FROST_SOUL = 201;   // 冰魄之靴
  IDX_BOOTS_FROST = 202;        // 凝霜之靴
  IDX_BOOTS_DARK_NIGHT = 203;   // 暗夜之靴
  IDX_BOOTS_UNDEAD = 204;       // 亡灵之靴
  IDX_BOOTS_SOUL_DEVOUR = 205;  // 噬魂之靴
  IDX_BOOTS_SKELETON = 206;     // 骸骨之靴
  IDX_BOOTS_BLOOD_SHADOW = 207; // 血影之靴
  IDX_BOOTS_DRAGON_FLAME = 208; // 龙炎之靴
  IDX_BOOTS_DRAGON_SOUL = 209;  // 龙魂之靴
  IDX_BOOTS_DRAGON_MIGHT = 210; // 龙威之靴
  IDX_BOOTS_SLIME = 211;        // 黏液之靴
  
  // 腰带 (Belt)
  IDX_BELT_ICE = 250;           // 寒冰腰带
  IDX_BELT_FROST_SOUL = 251;    // 冰魄腰带
  IDX_BELT_FROST = 252;         // 凝霜腰带
  IDX_BELT_SHADOW = 253;        // 暗影腰带
  IDX_BELT_UNDEAD = 254;        // 亡灵腰带
  IDX_BELT_SOUL_DEVOUR = 255;   // 噬魂腰带
  IDX_BELT_SKELETON = 256;      // 骸骨腰带
  IDX_BELT_BLOOD_BAT = 257;     // 血蝠腰带
  IDX_BELT_DRAGON_FLAME = 258;  // 龙炎腰带
  IDX_BELT_DRAGON_SOUL = 259;   // 龙魂腰带
  IDX_BELT_DRAGON_MIGHT = 260;  // 龙威腰带
  IDX_BELT_SLIME = 261;         // 黏液腰带
''')
    
    print(f"客户端代码: {code_file}")


def main():
    """主函数"""
    success = pack_accessories_to_data()
    
    if success:
        generate_client_code_snippet()
        
        print("\n" + "=" * 60)
        print("打包完成!")
        print("=" * 60)
        print("\n使用说明:")
        print("1. 将 AccessoryItems.data 放入客户端 Data 目录")
        print("2. 参考 client_code_snippet.pas 修改客户端代码")
        print("3. 更新物品表中的 Shape 字段对应图标索引")
    else:
        print("\n打包失败，请检查错误信息")
    
    return 0 if success else 1


if __name__ == '__main__':
    exit(main())
