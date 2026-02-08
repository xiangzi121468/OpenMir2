@echo off
chcp 65001 >nul

echo ================================================
echo         OpenMir2 Server Stop Script
echo ================================================
echo.
echo Stopping all OpenMir2 servers...
echo.

:: 关闭相关进程
taskkill /fi "WINDOWTITLE eq OpenMir2 - DBSrv*" /f 2>nul
taskkill /fi "WINDOWTITLE eq OpenMir2 - LoginSrv*" /f 2>nul
taskkill /fi "WINDOWTITLE eq OpenMir2 - GameSrv*" /f 2>nul
taskkill /fi "WINDOWTITLE eq OpenMir2 - WebApi*" /f 2>nul

:: 也可以通过进程名关闭
taskkill /im DBSrv.exe /f 2>nul
taskkill /im LoginSrv.exe /f 2>nul
taskkill /im GameSrv.exe /f 2>nul
taskkill /im WebApi.exe /f 2>nul

echo.
echo All servers stopped!
echo.
pause
