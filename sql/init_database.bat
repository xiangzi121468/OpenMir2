@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ================================================
echo OpenMir2 Database Initialization Script
echo ================================================
echo.

:: 设置默认值
set DB_HOST=localhost
set DB_PORT=3306
set DB_USER=root
set DB_PASS=

:: 解析命令行参数
if not "%1"=="" set DB_HOST=%1
if not "%2"=="" set DB_USER=%2
if not "%3"=="" set DB_PASS=%3
if not "%4"=="" set DB_PORT=%4

:: 提示输入密码
if "%DB_PASS%"=="" (
    set /p DB_PASS="Enter MySQL password for %DB_USER%@%DB_HOST%: "
)

set MYSQL_CMD=mysql -h%DB_HOST% -P%DB_PORT% -u%DB_USER% -p%DB_PASS%

echo.
echo Database Host: %DB_HOST%:%DB_PORT%
echo Database User: %DB_USER%
echo.

:: 创建数据库
echo [1/5] Creating databases...
%MYSQL_CMD% -e "CREATE DATABASE IF NOT EXISTS mir2_account CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;"
%MYSQL_CMD% -e "CREATE DATABASE IF NOT EXISTS mir2_data CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;"
%MYSQL_CMD% -e "CREATE DATABASE IF NOT EXISTS mir2_db CHARACTER SET utf8mb4 COLLATE utf8mb4_general_ci;"
if errorlevel 1 (
    echo ERROR: Failed to create databases
    exit /b 1
)
echo OK: Databases created

:: 阶段1: 核心数据库
echo.
echo [2/5] Initializing core databases...
echo   - mir2_account.sql
%MYSQL_CMD% mir2_account < mir2_account.sql
if errorlevel 1 echo   WARNING: mir2_account.sql may have errors

echo   - mir2_data.sql
%MYSQL_CMD% mir2_data < mir2_data.sql
if errorlevel 1 echo   WARNING: mir2_data.sql may have errors

echo   - mir2_db.sql (large file, please wait...)
%MYSQL_CMD% --max_allowed_packet=512M mir2_db < mir2_db.sql
if errorlevel 1 echo   WARNING: mir2_db.sql may have errors
echo OK: Core databases initialized

:: 阶段2: 索引优化
echo.
echo [3/5] Creating indexes...
echo   - characters_indexes.sql
%MYSQL_CMD% mir2_db < characters_indexes.sql
echo   - fix_gamegold_field.sql
%MYSQL_CMD% mir2_db < fix_gamegold_field.sql
echo OK: Indexes created

:: 阶段3: 功能模块
echo.
echo [4/5] Initializing feature modules...
for %%f in (shop_system.sql mail_system.sql notice_system.sql activity_system.sql castle_war.sql appraisal_system.sql vip_map_system.sql market_system.sql player_logs.sql gm_admin.sql) do (
    echo   - %%f
    %MYSQL_CMD% mir2_db < %%f
)
echo OK: Feature modules initialized

:: 阶段4-5: 游戏内容
echo.
echo [5/5] Loading game content...
for %%f in (skill_books.sql new_weapons.sql new_armors.sql new_accessories.sql new_monsters.sql) do (
    echo   - %%f
    %MYSQL_CMD% mir2_data < %%f
)

echo   - vip_map_monsters_config.sql
%MYSQL_CMD% mir2_db < vip_map_monsters_config.sql

echo   - all_weapon_effects.sql
%MYSQL_CMD% mir2_data < all_weapon_effects.sql

echo   - update_accessory_looks.sql
%MYSQL_CMD% mir2_data < update_accessory_looks.sql

echo OK: Game content loaded

:: 验证
echo.
echo ================================================
echo Verification
echo ================================================
%MYSQL_CMD% -e "SELECT 'mir2_account' AS db, COUNT(*) AS tables FROM information_schema.tables WHERE table_schema = 'mir2_account';"
%MYSQL_CMD% -e "SELECT 'mir2_data' AS db, COUNT(*) AS tables FROM information_schema.tables WHERE table_schema = 'mir2_data';"
%MYSQL_CMD% -e "SELECT 'mir2_db' AS db, COUNT(*) AS tables FROM information_schema.tables WHERE table_schema = 'mir2_db';"

echo.
echo ================================================
echo Database initialization completed!
echo ================================================

endlocal
