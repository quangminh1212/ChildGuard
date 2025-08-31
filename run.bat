@echo off
setlocal enableextensions enabledelayedexpansion

REM Change to the directory of this script (repo root)
cd /d "%~dp0"

echo ======================================================
echo  ChildGuard - Build and Run (Service + Tray)
echo ======================================================

REM Check .NET SDK availability
where dotnet >nul 2>&1
if errorlevel 1 (
  echo [ERROR] .NET SDK not found. Please install .NET 8 SDK from:
  echo         https://dotnet.microsoft.com/download
  pause
  exit /b 1
)

echo.
echo [1/3] Restoring packages...
dotnet restore
if errorlevel 1 (
  echo [ERROR] dotnet restore failed.
  pause
  exit /b 1
)

echo.
echo [2/3] Building solution (Release)...
dotnet build -c Release
if errorlevel 1 (
  echo [ERROR] dotnet build failed.
  pause
  exit /b 1
)

echo.
echo [3/3] Launching Service and Tray...

REM Launch Service in a new window (title helps you find the console window)
start "ChildGuard.Service" cmd /c dotnet run --project "ChildGuard.Service" -c Release

REM Launch Tray in a new window
start "ChildGuard.Tray" cmd /c dotnet run --project "ChildGuard.Tray" -c Release

echo.
echo Started:
echo   - ChildGuard.Service (console window titled: ChildGuard.Service)
echo   - ChildGuard.Tray    (console window titled: ChildGuard.Tray)
echo.
echo Data folders:
echo   Config and logs under: %APPDATA%\ChildGuard

echo.
echo Done. You can close this window.
endlocal

