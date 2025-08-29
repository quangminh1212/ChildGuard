@echo off
setlocal EnableExtensions EnableDelayedExpansion

REM Pre-scan args for --no-elevate to allow headless/CI environments
set "NO_ELEVATE="
if not "%~1"=="" (
  for %%A in (%*) do (
    if /I "%%~A"=="--no-elevate" set "NO_ELEVATE=1"
  )
)

REM Change to repo root (folder of this script)
cd /d "%~dp0"

REM Auto-elevate to Administrator if not already (unless --no-elevate)
set "_HASARGS=0"
if not "%~1"=="" set "_HASARGS=1"
if not "%NO_ELEVATE%"=="1" (
  net session >NUL 2>&1
  if errorlevel 1 (
    echo [INFO] Elevating privileges ^(UAC^)...
    if "%_HASARGS%"=="1" (
      powershell -NoProfile -WindowStyle Hidden -Command "Start-Process -FilePath '%~f0' -ArgumentList '%*' -Verb RunAs"
    ) else (
      powershell -NoProfile -WindowStyle Hidden -Command "Start-Process -FilePath '%~f0' -Verb RunAs"
    )
    exit /b
  )
)

REM Check dotnet
where dotnet >NUL 2>&1
if errorlevel 1 (
  echo [ERROR] .NET SDK not found. Install .NET 8 SDK: https://dotnet.microsoft.com/download
  pause
  exit /b 1
)

REM Defaults
set "NO_BUILD=0"
set "RUN_AGENT=0"
set "RUN_SERVICE=0"
set "RUN_TESTS=0"
set "UI=windows"
set "OPEN="
set "DIAGNOSE=0"

if "%~1"=="" goto :after_parse

:parse_args
if "%~1"=="" goto :after_parse
if /I "%~1"=="--help" goto :help
if /I "%~1"=="-h" goto :help
if /I "%~1"=="--no-build" set "NO_BUILD=1" & shift & goto :parse_args
if /I "%~1"=="--with-agent" set "RUN_AGENT=1" & shift & goto :parse_args
if /I "%~1"=="--with-service" set "RUN_SERVICE=1" & shift & goto :parse_args
if /I "%~1"=="--tests" set "RUN_TESTS=1" & shift & goto :parse_args
if /I "%~1"=="--diagnose" set "DIAGNOSE=1" & shift & goto :parse_args
if /I "%~1"=="--ui" (
  if "%~2"=="" ( echo [WARN] Missing value for --ui, using default: modern ) else ( set "UI=%~2" )
  shift & shift & goto :parse_args
)
if /I "%~1"=="--open" (
  if "%~2"=="" ( echo [WARN] Missing value for --open, ignored ) else ( set "OPEN=%~2" )
  shift & shift & goto :parse_args
)

REM Prepare logs dir early (after args parsed)
set "LOG_DIR=%cd%\logs"
if not exist "%LOG_DIR%" mkdir "%LOG_DIR%" >NUL 2>&1
set "UI_LOG=%LOG_DIR%\ui.log"
set "AGENT_LOG=%LOG_DIR%\agent.log"
set "SERVICE_LOG=%LOG_DIR%\service.log"

echo [INFO] Logs:
echo   UI     : "%UI_LOG%"
echo   Agent  : "%AGENT_LOG%"
echo   Service: "%SERVICE_LOG%"


REM Unknown arg -> ignore
shift
goto :parse_args

:after_parse

REM Optionally restore/build
if "%NO_BUILD%"=="0" (
  echo [INFO] Restoring packages and building solution...
  REM Ensure UI is not running to avoid file locks when copying DLLs
  tasklist /FI "IMAGENAME eq ChildGuard.UI.exe" | find /I "ChildGuard.UI.exe" >NUL
  if not errorlevel 1 (
    echo [INFO] Stopping running UI to avoid build file locks...
    taskkill /IM ChildGuard.UI.exe /F >NUL 2>&1
    powershell -NoProfile -WindowStyle Hidden -Command "Start-Sleep -Seconds 1" >NUL 2>&1
  )
  dotnet restore "ChildGuard.sln"
  if errorlevel 1 ( echo [ERROR] Restore failed && pause && exit /b 1 )
  dotnet build "ChildGuard.sln" -c Debug
  if errorlevel 1 ( echo [ERROR] Build failed && pause && exit /b 1 )
) else (
  echo [INFO] Skipping build as requested by --no-build
)

REM Compose UI args
set "UI_ARGS="

REM Always stop any existing UI instance before launching to avoid conflicts/file locks
 tasklist /FI "IMAGENAME eq ChildGuard.UI.exe" | find /I "ChildGuard.UI.exe" >NUL
 if not errorlevel 1 (
   echo [INFO] Stopping existing ChildGuard.UI.exe...
   taskkill /IM ChildGuard.UI.exe /F >NUL 2>&1
   powershell -NoProfile -WindowStyle Hidden -Command "Start-Sleep -Milliseconds 500" >NUL 2>&1
 )

if not "%UI%"=="" set "UI_ARGS=!UI_ARGS! --ui %UI%"
if not "%OPEN%"=="" set "UI_ARGS=!UI_ARGS! --open %OPEN%"
if "%DIAGNOSE%"=="1" set "UI_ARGS=!UI_ARGS! --debug-ui"
echo [INFO] UI_ARGS: !UI_ARGS!
REM Prefer running built EXE if available (faster UI startup and reliable window)
set "UI_EXE=ChildGuard.UI\bin\Debug\net8.0-windows\ChildGuard.UI.exe"
if not exist "%UI_EXE%" set "UI_EXE="


REM Diagnostic mode to run UI inline without start, to show logs
if /I "%~1"=="--diagnose" set "DIAGNOSE=1"

REM Start UI (no nested parentheses; use labels to avoid cmd parser issues)
echo [INFO] Starting UI (ChildGuard.UI)...
if exist "%UI_EXE%" goto run_ui_exe
goto run_ui_dotnet

:run_ui_exe
if /I "%DIAGNOSE%"=="1" (
  setlocal DisableDelayedExpansion
  set "ARGS=%UI_ARGS%"
  echo [DIAG] "%UI_EXE%" %ARGS%
  "%UI_EXE%" %ARGS%
  endlocal
) else (
  powershell -NoProfile -WindowStyle Hidden -Command "Start-Process -FilePath '%UI_EXE%' -ArgumentList '%UI_ARGS%' -WorkingDirectory '%cd%'"
)
goto after_ui

:run_ui_dotnet
if /I "%DIAGNOSE%"=="1" (
  echo [DIAG] cmd /c dotnet run --project ChildGuard.UI -- !UI_ARGS!
  cmd /c dotnet run --project ChildGuard.UI -- !UI_ARGS!
  if errorlevel 1 echo [ERR] UI exited with code %errorlevel% && exit /b %errorlevel%
) else (
    echo [INFO] logging: dotnet run --project ChildGuard.UI -- !UI_ARGS! >"%UI_LOG%" 2>&1
    powershell -NoProfile -WindowStyle Hidden -Command "Start-Process -FilePath 'dotnet' -ArgumentList 'run --project ChildGuard.UI -- %UI_ARGS%' -WorkingDirectory '%cd%' -RedirectStandardOutput '%UI_LOG%' -RedirectStandardError '%UI_LOG%'"
  )
)
goto after_ui

:after_ui


REM Start Agent/Service with logs (if requested)
if "%RUN_AGENT%"=="1" (
  echo [INFO] Starting Agent (ChildGuard.Agent)...
  if "%DIAGNOSE%"=="1" (
    dotnet run --project ChildGuard.Agent
  ) else (
    echo [INFO] ^(logging^) Agent -> "%LOG_DIR%\agent.log"
    start "ChildGuard.Agent" cmd /c "dotnet run --project ChildGuard.Agent 1>""%LOG_DIR%\agent.log"" 2>&1"
  )
)

if "%RUN_SERVICE%"=="1" (
  echo [INFO] Starting Service (ChildGuard.Service)...
  if "%DIAGNOSE%"=="1" (
    dotnet run --project ChildGuard.Service
  ) else (
    echo [INFO] ^(logging^) Service -> "%LOG_DIR%\service.log"
    start "ChildGuard.Service" cmd /c "dotnet run --project ChildGuard.Service 1>""%LOG_DIR%\service.log"" 2>&1"
  )
)

REM Optionally run tests
if "%RUN_TESTS%"=="1" (
  echo [INFO] Running tests...
  dotnet test "ChildGuard.sln" -c Debug
)

echo.
echo [DONE] Launched. You can close this window.
echo     - UI args: --ui %UI% %OPEN:%=%%
echo     - Options: --with-agent, --with-service, --tests, --no-build

goto :eof

:help
echo.
echo Usage: run.bat [options]
echo.
echo Options:
echo   --ui modern^|classic       Choose UI variant (default: modern)
echo   --open dashboard^|monitoring^|protection^|reports^|settings  Open specific section
echo   --with-agent              Launch ChildGuard.Agent
echo   --with-service            Launch ChildGuard.Service (console if supported)
echo   --tests                   Run test suite after launch

echo   --no-build               Skip build step

echo   -h, --help               Show help

echo.
echo Examples:
echo   run.bat --ui modern --open dashboard
echo   run.bat --with-agent --with-service

echo.
goto :eof

