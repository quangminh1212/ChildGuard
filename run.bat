@echo off
setlocal EnableExtensions EnableDelayedExpansion

REM Pre-scan args for --no-elevate to allow headless/CI environments
set "NO_ELEVATE="
for %%A in (%*) do (
  if /I "%%~A"=="--no-elevate" set "NO_ELEVATE=1"
)

REM Change to repo root (folder of this script)
cd /d "%~dp0"

REM Auto-elevate to Administrator if not already (unless --no-elevate)
if not "%NO_ELEVATE%"=="1" (
  net session >NUL 2>&1
  if %errorlevel% NEQ 0 (
    echo [INFO] Elevating privileges (UAC)...
    powershell -NoProfile -WindowStyle Hidden -Command "Start-Process -FilePath '%~f0' -ArgumentList '%*' -Verb RunAs"
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
set "UI=modern"
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
  dotnet restore "ChildGuard.sln"
  if errorlevel 1 ( echo [ERROR] Restore failed && pause && exit /b 1 )
  dotnet build "ChildGuard.sln" -c Debug
  if errorlevel 1 ( echo [ERROR] Build failed && pause && exit /b 1 )
) else (
  echo [INFO] Skipping build as requested by --no-build
)

REM Compose UI args
set "UI_ARGS="
if not "%UI%"=="" set "UI_ARGS=!UI_ARGS! --ui %UI%"
if not "%OPEN%"=="" set "UI_ARGS=!UI_ARGS! --open %OPEN%"
echo [INFO] UI_ARGS: !UI_ARGS!
REM Prefer running built EXE if available (faster UI startup and reliable window)
set "UI_EXE=ChildGuard.UI\bin\Debug\net8.0-windows\ChildGuard.UI.exe"
if not exist "%UI_EXE%" set "UI_EXE="


REM Diagnostic mode to run UI inline without start, to show logs
if /I "%~1"=="--diagnose" set "DIAGNOSE=1"

REM Start UI (recommended entry point)
echo [INFO] Starting UI (ChildGuard.UI)...
if not "%UI_EXE%"=="" (
  if "%DIAGNOSE%"=="1" (
    echo [DIAG] "%UI_EXE%" !UI_ARGS!
    "%UI_EXE%" !UI_ARGS!
    if errorlevel 1 echo [ERR] UI exited with code %errorlevel% && exit /b %errorlevel%
  ) else (
    start "ChildGuard.UI" "%UI_EXE%" !UI_ARGS!
  )
) else (
  if "%DIAGNOSE%"=="1" (
    echo [DIAG] cmd /c dotnet run --project ChildGuard.UI -- !UI_ARGS!
    cmd /c dotnet run --project ChildGuard.UI -- !UI_ARGS!
    if errorlevel 1 echo [ERR] UI exited with code %errorlevel% && exit /b %errorlevel%
  ) else (
    echo [INFO] logging: dotnet run --project ChildGuard.UI -- !UI_ARGS! >"%UI_LOG%" 2>&1
    start "ChildGuard.UI" cmd /c "dotnet run --project ChildGuard.UI -- !UI_ARGS! 1>""%UI_LOG%"" 2>&1"
  )
)

REM Start Agent/Service with logs (if requested)
if "%RUN_AGENT%"=="1" (
  echo [INFO] Starting Agent (ChildGuard.Agent)...
  if "%DIAGNOSE%"=="1" (
    dotnet run --project ChildGuard.Agent
  ) else (
    echo [INFO] (logging) Agent -> "%LOG_DIR%\agent.log"
    start "ChildGuard.Agent" cmd /c "dotnet run --project ChildGuard.Agent 1>""%LOG_DIR%\agent.log"" 2>&1"
  )
)

if "%RUN_SERVICE%"=="1" (
  echo [INFO] Starting Service (ChildGuard.Service)...
  if "%DIAGNOSE%"=="1" (
    dotnet run --project ChildGuard.Service
  ) else (
    echo [INFO] (logging) Service -> "%LOG_DIR%\service.log"
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

