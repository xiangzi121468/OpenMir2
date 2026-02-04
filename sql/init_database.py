#!/usr/bin/env python3
# -*- coding: utf-8 -*-
"""
OpenMir2 Database Initialization Script
"""

import os
import sys
import argparse
import subprocess
from pathlib import Path

# SQL脚本执行顺序
INIT_SCRIPTS = {
    # 阶段1: 核心数据库
    "phase1_core": [
        ("mir2_account", "mir2_account.sql"),
        ("mir2_data", "mir2_data.sql"),
        ("mir2_db", "mir2_db.sql"),
    ],
    # 阶段2: 索引优化
    "phase2_indexes": [
        ("mir2_db", "characters_indexes.sql"),
        ("mir2_db", "fix_gamegold_field.sql"),
    ],
    # 阶段3: 功能模块
    "phase3_modules": [
        ("mir2_db", "shop_system.sql"),
        ("mir2_db", "mail_system.sql"),
        ("mir2_db", "notice_system.sql"),
        ("mir2_db", "activity_system.sql"),
        ("mir2_db", "castle_war.sql"),
        ("mir2_db", "appraisal_system.sql"),
        ("mir2_db", "vip_map_system.sql"),
        ("mir2_db", "market_system.sql"),
        ("mir2_db", "player_logs.sql"),
        ("mir2_db", "gm_admin.sql"),
    ],
    # 阶段4: 游戏内容
    "phase4_content": [
        ("mir2_data", "skill_books.sql"),
        ("mir2_data", "new_weapons.sql"),
        ("mir2_data", "new_armors.sql"),
        ("mir2_data", "new_accessories.sql"),
        ("mir2_data", "new_monsters.sql"),
    ],
    # 阶段5: 配置数据
    "phase5_config": [
        ("mir2_db", "vip_map_monsters_config.sql"),
        ("mir2_data", "all_weapon_effects.sql"),
        ("mir2_data", "update_accessory_looks.sql"),
    ],
}

def run_mysql(host, port, user, password, database, sql_file, script_dir):
    """执行MySQL脚本"""
    sql_path = script_dir / sql_file
    if not sql_path.exists():
        print(f"    WARNING: {sql_file} not found, skipping")
        return False
    
    cmd = [
        "mysql",
        f"-h{host}",
        f"-P{port}",
        f"-u{user}",
        f"-p{password}",
        "--max_allowed_packet=512M",
        database
    ]
    
    try:
        with open(sql_path, 'r', encoding='utf-8', errors='ignore') as f:
            result = subprocess.run(
                cmd,
                stdin=f,
                capture_output=True,
                text=True
            )
            if result.returncode != 0:
                print(f"    WARNING: {sql_file} returned errors")
                return False
        return True
    except Exception as e:
        print(f"    ERROR: {sql_file} - {e}")
        return False

def create_databases(host, port, user, password):
    """创建数据库"""
    databases = [
        "CREATE DATABASE IF NOT EXISTS mir2_account CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;",
        "CREATE DATABASE IF NOT EXISTS mir2_data CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;",
        "CREATE DATABASE IF NOT EXISTS mir2_db CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;",
    ]
    
    cmd_base = ["mysql", f"-h{host}", f"-P{port}", f"-u{user}", f"-p{password}", "-e"]
    
    for sql in databases:
        try:
            subprocess.run(cmd_base + [sql], capture_output=True, check=True)
        except subprocess.CalledProcessError as e:
            print(f"ERROR: Failed to create database: {e}")
            return False
    
    return True

def verify_installation(host, port, user, password):
    """验证安装"""
    cmd = [
        "mysql", f"-h{host}", f"-P{port}", f"-u{user}", f"-p{password}", "-e",
        """
        SELECT 'mir2_account' AS db, COUNT(*) AS tables FROM information_schema.tables WHERE table_schema = 'mir2_account'
        UNION ALL
        SELECT 'mir2_data' AS db, COUNT(*) AS tables FROM information_schema.tables WHERE table_schema = 'mir2_data'
        UNION ALL
        SELECT 'mir2_db' AS db, COUNT(*) AS tables FROM information_schema.tables WHERE table_schema = 'mir2_db';
        """
    ]
    
    try:
        result = subprocess.run(cmd, capture_output=True, text=True)
        print(result.stdout)
    except Exception as e:
        print(f"Verification error: {e}")

def main():
    parser = argparse.ArgumentParser(description="OpenMir2 Database Initialization")
    parser.add_argument("--host", default="localhost", help="MySQL host")
    parser.add_argument("--port", default="3306", help="MySQL port")
    parser.add_argument("--user", default="root", help="MySQL user")
    parser.add_argument("--password", default="", help="MySQL password")
    parser.add_argument("--phase", default="all", help="Phase to run (1-5 or all)")
    args = parser.parse_args()
    
    # 获取脚本目录
    script_dir = Path(__file__).parent.absolute()
    
    print("=" * 50)
    print("OpenMir2 Database Initialization Script")
    print("=" * 50)
    print(f"\nDatabase Host: {args.host}:{args.port}")
    print(f"Database User: {args.user}")
    print("")
    
    # 创建数据库
    print("[1/6] Creating databases...")
    if not create_databases(args.host, args.port, args.user, args.password):
        print("ERROR: Failed to create databases")
        sys.exit(1)
    print("OK: Databases created\n")
    
    # 执行各阶段脚本
    phases = ["phase1_core", "phase2_indexes", "phase3_modules", "phase4_content", "phase5_config"]
    phase_names = ["Core databases", "Indexes", "Feature modules", "Game content", "Configuration"]
    
    for i, (phase, name) in enumerate(zip(phases, phase_names), 2):
        print(f"[{i}/6] Initializing {name}...")
        for db, script in INIT_SCRIPTS[phase]:
            print(f"  - {script}")
            run_mysql(args.host, args.port, args.user, args.password, db, script, script_dir)
        print(f"OK: {name} initialized\n")
    
    # 验证
    print("=" * 50)
    print("Verification")
    print("=" * 50)
    verify_installation(args.host, args.port, args.user, args.password)
    
    print("\n" + "=" * 50)
    print("Database initialization completed!")
    print("=" * 50)

if __name__ == "__main__":
    main()
