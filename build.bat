@echo off
setlocal

REM ============================================================
REM  BasicThemer2 - Single-target one-key build script
REM  Builds one .NET Framework 4.0 executable that runs on
REM  Windows Vista through Windows 11.
REM ============================================================

set "PROJECT_DIR=%~dp0BasicThemer2"
set "PROJECT_FILE=%PROJECT_DIR%\BasicThemer2.csproj"

REM Auto-detect MSBuild
set "MSBUILD="
for /f "usebackq delims=" %%i in (`"%ProgramFiles(x86)%\Microsoft Visual Studio\Installer\vswhere.exe" -latest -requires Microsoft.Component.MSBuild -find MSBuild\**\Bin\MSBuild.exe 2^>nul`) do set "MSBUILD=%%i"
if "%MSBUILD%"=="" (
    echo [ERROR] MSBuild not found. Please install Visual Studio 2019 or later.
    exit /b 1
)

echo ============================================================
echo  MSBuild: %MSBUILD%
echo  Project: %PROJECT_FILE%
echo ============================================================
echo.

set "CONFIG=Release"
if not "%1"=="" set "CONFIG=%1"

set "FAILED=0"

REM ---------- Build ----------
echo [1/1] Build .NET Framework 4.0 (%CONFIG%)...
"%MSBUILD%" "%PROJECT_FILE%" /t:Rebuild /p:Configuration=%CONFIG% /verbosity:minimal
if errorlevel 1 (
    echo.
    echo [FAILED] build error!
    set "FAILED=1"
) else (
    echo [OK] -^> bin\%CONFIG%\BasicThemer2.exe
)
echo.

if "%FAILED%"=="1" (
    echo ============================================================
    echo  Some targets failed. See the log above.
    echo ============================================================
    echo.
    pause
    exit /b 1
)

echo ============================================================
echo  All builds completed!
echo ============================================================
echo.
pause
exit /b 0