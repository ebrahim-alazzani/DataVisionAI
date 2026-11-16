@echo off
setlocal
title DataVision Studio - Release Publisher

echo =======================================================================
echo   DataVision Studio - Self-Contained Release Publisher (win-x64)
echo   Developed by: Ebrahim Al-Azzani
echo =======================================================================
echo.
echo Cleaning output directory...
if exist "%~dp0publish" rmdir /s /q "%~dp0publish"

echo Publishing standalone executable (no .NET runtime required on client machine)...
dotnet publish "%~dp0src\DataVision.Desktop\DataVision.Desktop.csproj" -c Release -r win-x64 --self-contained true -p:WindowsPackageType=None -o "%~dp0publish"

if %ERRORLEVEL% EQU 0 (
    echo.
    echo =======================================================================
    echo   BUILD SUCCESSFUL!
    echo   Standalone executable created at: publish\DataVision.Desktop.exe
    echo =======================================================================
) else (
    echo.
    echo Publish failed with error code %ERRORLEVEL%.
)

pause
endlocal
