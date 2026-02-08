@echo off
chcp 65001 >nul
setlocal enabledelayedexpansion

echo ================================================
echo         OpenMir2 Server Startup Script
echo ================================================
echo.

:: 检查是否已编译
if not exist "publish\GameSrv\GameSrv.dll" (
    echo [WARNING] Server not compiled yet!
    echo.
    echo Please run: dotnet publish -c Release
    echo.
    set /p COMPILE="Compile now? (Y/N): "
    if /i "!COMPILE!"=="Y" (
        echo Compiling...
        dotnet publish src/DBSrv/DBSrv.csproj -c Release -o publish/DBSrv
        dotnet publish src/LoginSrv/LoginSrv.csproj -c Release -o publish/LoginSrv
        dotnet publish src/GameSrv/GameSrv.csproj -c Release -o publish/GameSrv
        dotnet publish src/WebApi/WebApi.csproj -c Release -o publish/WebApi
        echo Compile completed!
    ) else (
        echo Cancelled.
        pause
        exit /b 1
    )
)

echo Starting servers...
echo.

:: 启动 DBSrv
echo [1/4] Starting DBSrv...
start "OpenMir2 - DBSrv" cmd /k "cd /d %~dp0publish\DBSrv && dotnet DBSrv.dll"
timeout /t 5 /nobreak >nul

:: 启动 LoginSrv
echo [2/4] Starting LoginSrv...
start "OpenMir2 - LoginSrv" cmd /k "cd /d %~dp0publish\LoginSrv && dotnet LoginSrv.dll"
timeout /t 3 /nobreak >nul

:: 启动 GameSrv
echo [3/4] Starting GameSrv...
start "OpenMir2 - GameSrv" cmd /k "cd /d %~dp0publish\GameSrv && dotnet GameSrv.dll"
timeout /t 3 /nobreak >nul

:: 启动 WebApi
echo [4/4] Starting WebApi...
start "OpenMir2 - WebApi" cmd /k "cd /d %~dp0publish\WebApi && dotnet WebApi.dll"

echo.
echo ================================================
echo All servers started!
echo ================================================
echo.
echo Servers:
echo   - DBSrv:    Port 6000 (internal)
echo   - LoginSrv: Port 7000 (client login)
echo   - GameSrv:  Port 7100 (client game)
echo   - WebApi:   Port 5000 (http://localhost:5000)
echo.
echo Press any key to exit (servers will keep running)
pause >nul

endlocal
