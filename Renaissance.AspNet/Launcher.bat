@echo off
title MedReach Launcher
cd /d "%~dp0"

echo.
echo  Starting MedReach (API + Web + Field)...
echo.

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Launcher.ps1"
if errorlevel 1 (
    echo.
    echo  Something went wrong. See messages above.
    pause
    exit /b 1
)

echo.
pause
exit /b 0
