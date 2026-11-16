@echo off
setlocal
title DataVision Studio - Launching...

echo =======================================================================
echo   DataVision Studio - Enterprise Business Intelligence Platform
echo   Developed by: Ebrahim Al-Azzani
echo =======================================================================
echo.
echo Starting application using relative directory...
cd /d "%~dp0src\DataVision.Desktop"

dotnet run --no-build -p:Platform=x64

if %ERRORLEVEL% NEQ 0 (
    echo.
    echo Application exited with error code %ERRORLEVEL%.
    pause
)
endlocal
