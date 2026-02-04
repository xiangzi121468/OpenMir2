#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
武器特效一键集成脚本
自动打包所有特效图像并生成集成报告
"""

import os
import sys
import shutil
import subprocess

# 项目根目录
PROJECT_ROOT = os.path.dirname(os.path.dirname(os.path.abspath(__file__)))

# 特效配置
EFFECT_CONFIGS = {
    100: {"name": "冰霜之刃", "weapon_id": 900, "frames": 20},
    101: {"name": "冰魄", "weapon_id": 901, "frames": 20},
    102: {"name": "凝霜", "weapon_id": 902, "frames": 20},
    103: {"name": "暗影匕首", "weapon_id": 910, "frames": 20},
    105: {"name": "亡灵法杖", "weapon_id": 920, "frames": 20},
    110: {"name": "地狱火", "weapon_id": 961, "frames": 20},
    115: {"name": "龙之光环", "weapon_id": 954, "frames": 20},
}


def check_effect_files():
    """检查特效文件是否存在"""
    print("=" * 60)
    print("检查特效图像文件...")
    print("=" * 60)
    
    effects_dir = os.path.join(PROJECT_ROOT, "assets", "weapon_effects_v2")
    all_ok = True
    
    for effect_type, config in EFFECT_CONFIGS.items():
        type_dir = os.path.join(effects_dir, f"type_{effect_type}")
        
        if not os.path.exists(type_dir):
            print(f"[缺失] type_{effect_type}/ - {config['name']}")
            all_ok = False
            continue
        
        # 检查帧文件
        missing_frames = []
        start_index = 30000 + (effect_type - 100) * 20
        for i in range(config['frames']):
            frame_file = os.path.join(type_dir, f"effect_{start_index + i:05d}.png")
            if not os.path.exists(frame_file):
                missing_frames.append(start_index + i)
        
        if missing_frames:
            print(f"[部分] type_{effect_type}/ - {config['name']}: 缺少 {len(missing_frames)} 帧")
            all_ok = False
        else:
            print(f"[完整] type_{effect_type}/ - {config['name']}: {config['frames']} 帧")
    
    return all_ok


def copy_to_client():
    """复制特效文件到客户端目录结构"""
    print("\n" + "=" * 60)
    print("准备客户端资源目录...")
    print("=" * 60)
    
    # 创建客户端资源目录
    client_data_dir = os.path.join(PROJECT_ROOT, "MirClient", "Data")
    effect_output_dir = os.path.join(client_data_dir, "WeaponEffects")
    
    os.makedirs(effect_output_dir, exist_ok=True)
    print(f"创建目录: {effect_output_dir}")
    
    # 复制特效文件
    effects_dir = os.path.join(PROJECT_ROOT, "assets", "weapon_effects_v2")
    
    for effect_type, config in EFFECT_CONFIGS.items():
        src_dir = os.path.join(effects_dir, f"type_{effect_type}")
        if os.path.exists(src_dir):
            # 复制精灵图
            sprite_file = os.path.join(src_dir, f"effect_type_{effect_type}_sprite.png")
            if os.path.exists(sprite_file):
                dst_file = os.path.join(effect_output_dir, f"type_{effect_type}_sprite.png")
                shutil.copy2(sprite_file, dst_file)
                print(f"复制: type_{effect_type}_sprite.png")
    
    return effect_output_dir


def generate_integration_report():
    """生成集成报告"""
    print("\n" + "=" * 60)
    print("生成集成报告...")
    print("=" * 60)
    
    report_path = os.path.join(PROJECT_ROOT, "doc", "weapon_effect_status.md")
    
    report = """# 武器特效集成状态报告

## 特效文件状态

| 特效类型 | 武器名称 | 武器ID | 索引范围 | 帧数 | 状态 |
|---------|---------|-------|---------|-----|------|
"""
    
    effects_dir = os.path.join(PROJECT_ROOT, "assets", "weapon_effects_v2")
    
    for effect_type, config in EFFECT_CONFIGS.items():
        type_dir = os.path.join(effects_dir, f"type_{effect_type}")
        start_index = 30000 + (effect_type - 100) * 20
        end_index = start_index + config['frames'] - 1
        
        if os.path.exists(type_dir):
            status = "✅ 已生成"
        else:
            status = "❌ 缺失"
        
        report += f"| {effect_type} | {config['name']} | {config['weapon_id']} | {start_index}-{end_index} | {config['frames']} | {status} |\n"
    
    report += """
## 集成步骤

### 1. 打包特效图像到 State.data

使用打包工具:
```bash
cd tools
python pack_to_wil.py --input ../assets/weapon_effects_v2/type_100 --output ../MirClient/Data/State --start-index 30000
```

### 2. 执行数据库更新

```sql
source sql/weapon_effects_update.sql
```

### 3. 修改服务端代码 (如需要)

确保服务端正确传递 `reserve[3]` 字段给客户端。

### 4. 测试

1. 启动服务端和客户端
2. 使用GM命令获取测试武器
3. 装备后在人物界面检查特效

## 文件清单

- `assets/weapon_effects_v2/` - 特效PNG图像
- `tools/pack_to_wil.py` - WIL打包工具
- `tools/weapon_effect_config.py` - 特效配置
- `tools/generate_weapon_effects_v2.py` - 特效生成工具
- `sql/weapon_effects_update.sql` - 数据库更新脚本
- `sql/new_weapons.sql` - 新武器定义
- `doc/weapon_effect_integration.md` - 集成指南
"""
    
    with open(report_path, 'w', encoding='utf-8') as f:
        f.write(report)
    
    print(f"报告已生成: {report_path}")
    return report_path


def main():
    print("=" * 60)
    print("武器特效集成工具")
    print("=" * 60)
    
    # 1. 检查特效文件
    files_ok = check_effect_files()
    
    # 2. 复制到客户端目录
    output_dir = copy_to_client()
    
    # 3. 生成报告
    report_path = generate_integration_report()
    
    # 4. 打印总结
    print("\n" + "=" * 60)
    print("集成准备完成!")
    print("=" * 60)
    
    print("\n下一步操作:")
    print("1. 运行打包命令将PNG转换为WIL格式:")
    print("   python tools/pack_to_wil.py --help")
    print("\n2. 执行数据库更新:")
    print("   mysql -u root -p mir2_db < sql/weapon_effects_update.sql")
    print("\n3. 查看集成报告:")
    print(f"   {report_path}")
    
    if not files_ok:
        print("\n[警告] 部分特效文件缺失，请先运行生成工具:")
        print("   python tools/generate_weapon_effects_v2.py")


if __name__ == "__main__":
    main()
