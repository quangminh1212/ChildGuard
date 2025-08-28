@echo off
setlocal EnableExtensions EnableDelayedExpansion

REM UTF-8 console for Vietnamese output
chcp 65001 >NUL 2>&1

REM Change to repo root (folder of this script)
cd /d "%~dp0"

REM Check dotnet
where dotnet >NUL 2>&1
if errorlevel 1 (
  echo [ERROR] Không tìm thấy .NET SDK. Vui lòng cài .NET 8 SDK từ https://dotnet.microsoft.com/download
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

if "%~1"=="" goto :after_parse

:parse_args
if "%~1"=="" goto :after_parse
if /I "%~1"=="--help" goto :help
if /I "%~1"=="-h" goto :help
if /I "%~1"=="--no-build" set "NO_BUILD=1" & shift & goto :parse_args
if /I "%~1"=="--with-agent" set "RUN_AGENT=1" & shift & goto :parse_args
if /I "%~1"=="--with-service" set "RUN_SERVICE=1" & shift & goto :parse_args
if /I "%~1"=="--tests" set "RUN_TESTS=1" & shift & goto :parse_args
if /I "%~1"=="--ui" (
  if "%~2"=="" ( echo [WARN] Thiếu giá trị cho --ui, dùng mặc định: modern ) else ( set "UI=%~2" )
  shift & shift & goto :parse_args
)
if /I "%~1"=="--open" (
  if "%~2"=="" ( echo [WARN] Thiếu giá trị cho --open, bỏ qua ) else ( set "OPEN=%~2" )
  shift & shift & goto :parse_args
)
REM Unknown arg -> ignore
shift
goto :parse_args

:after_parse

REM Optionally restore/build
if "%NO_BUILD%"=="0" (
  echo [INFO] Khôi phục gói và build solution...
  dotnet restore ChildGuard.sln
  if errorlevel 1 ( echo [ERROR] Restore thất bại && pause && exit /b 1 )
  dotnet build ChildGuard.sln -c Debug
  if errorlevel 1 ( echo [ERROR] Build thất bại && pause && exit /b 1 )
) else (
  echo [INFO] Bỏ qua bước build theo yêu cầu --no-build
)

REM Compose UI args
set "UI_ARGS="
if not "%UI%"=="" set "UI_ARGS=!UI_ARGS! --ui %UI%"
if not "%OPEN%"=="" set "UI_ARGS=!UI_ARGS! --open %OPEN%"

REM Start UI (recommended entry point)
echo [INFO] Khởi chạy giao diện (ChildGuard.UI)...
start "ChildGuard.UI" cmd /c "dotnet run --project ChildGuard.UI -- !UI_ARGS!"

REM Optionally start Agent
if "%RUN_AGENT%"=="1" (
  echo [INFO] Khởi chạy Agent (ChildGuard.Agent)...
  start "ChildGuard.Agent" cmd /c "dotnet run --project ChildGuard.Agent"
)

REM Optionally start Service (chạy như console nếu project hỗ trợ)
if "%RUN_SERVICE%"=="1" (
  echo [INFO] Khởi chạy Service (ChildGuard.Service)...
  start "ChildGuard.Service" cmd /c "dotnet run --project ChildGuard.Service"
)

REM Optionally run tests
if "%RUN_TESTS%"=="1" (
  echo [INFO] Chạy test...
  dotnet test ChildGuard.sln -c Debug
)

echo.
echo [DONE] Đã khởi chạy. Bạn có thể đóng cửa sổ này.
echo     - UI args: --ui %UI% %OPEN:%=%%
echo     - Tùy chọn: --with-agent, --with-service, --tests, --no-build

goto :eof

:help
echo.
echo Cách dùng: run.bat [tuỳ chọn]
echo.
echo Tuỳ chọn:
echo   --ui modern^|classic       Chọn giao diện UI (mặc định: modern)
echo   --open dashboard^|monitoring^|protection^|reports^|settings  Mở thẳng section
echo   --with-agent              Khởi chạy thêm ChildGuard.Agent
echo   --with-service            Khởi chạy thêm ChildGuard.Service (console nếu hỗ trợ)
echo   --tests                   Chạy bộ kiểm thử sau khi khởi chạy

echo   --no-build               Bỏ qua bước build

echo   -h, --help               Hiển thị trợ giúp

echo.
echo Ví dụ:
echo   run.bat --ui modern --open dashboard
echo   run.bat --with-agent --with-service

echo.
goto :eof

