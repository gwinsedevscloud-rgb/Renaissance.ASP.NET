@echo off
title Renaissance Launcher
cd /d "%~dp0"

powershell.exe -NoProfile -ExecutionPolicy Bypass -File "%~dp0Launcher.ps1"
if errorlevel 1 (
    echo.
    pause
    exit /b 1
)

ping 127.0.0.1 -n 6 >nul
exit /b 0
