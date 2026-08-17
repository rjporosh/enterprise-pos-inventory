@echo off
setlocal EnableExtensions EnableDelayedExpansion

REM ============================================================
REM Enterprise POS Inventory - Clean and Build
REM Windows version
REM ============================================================

title Enterprise POS Inventory - Clean and Build

REM ------------------------------------------------------------
REM Always use the directory where this script is located
REM ------------------------------------------------------------

cd /d "%~dp0"

set "ROOT_DIR=%CD%"
set "SOLUTION="

echo.
echo ============================================================
echo  Enterprise POS Inventory - Clean and Build
echo ============================================================
echo.
echo Repository: %ROOT_DIR%
echo.

REM ------------------------------------------------------------
REM Check .NET SDK
REM ------------------------------------------------------------

where dotnet >nul 2>&1

if errorlevel 1 (
    echo ERROR: dotnet command was not found.
    echo Please install the .NET SDK and try again.
    echo.
    pause
    exit /b 1
)

echo Using .NET:
dotnet --version
echo.

REM ------------------------------------------------------------
REM Find solution
REM ------------------------------------------------------------

for /f "delims=" %%F in ('dir /b /s "*.slnx" 2^>nul') do (
    if not defined SOLUTION set "SOLUTION=%%F"
)

REM If no .slnx was found, look for .sln
if not defined SOLUTION (
    for /f "delims=" %%F in ('dir /b /s "*.sln" 2^>nul') do (
        if not defined SOLUTION set "SOLUTION=%%F"
    )
)

if not defined SOLUTION (
    echo ERROR: No .sln or .slnx file was found.
    echo.
    pause
    exit /b 1
)

echo Solution:
echo   %SOLUTION%
echo.

REM ------------------------------------------------------------
REM Delete all bin directories
REM ------------------------------------------------------------

echo ============================================================
echo  Removing bin directories
echo ============================================================
echo.

set /a BIN_COUNT=0

for /d /r "%ROOT_DIR%" %%D in (bin) do (
    if exist "%%D" (
        echo Removing: %%D
        rmdir /s /q "%%D"
        set /a BIN_COUNT+=1
    )
)

echo.
echo Removed %BIN_COUNT% bin director^(s^).
echo.

REM ------------------------------------------------------------
REM Delete all obj directories
REM ------------------------------------------------------------

echo ============================================================
echo  Removing obj directories
echo ============================================================
echo.

set /a OBJ_COUNT=0

for /d /r "%ROOT_DIR%" %%D in (obj) do (
    if exist "%%D" (
        echo Removing: %%D
        rmdir /s /q "%%D"
        set /a OBJ_COUNT+=1
    )
)

echo.
echo Removed %OBJ_COUNT% obj director^(s^).
echo.

REM ------------------------------------------------------------
REM Restore
REM ------------------------------------------------------------

echo ============================================================
echo  Running dotnet restore
echo ============================================================
echo.

dotnet restore "%SOLUTION%"

if errorlevel 1 (
    echo.
    echo ERROR: dotnet restore failed.
    echo.
    pause
    exit /b 1
)

echo.
echo Restore completed successfully.
echo.

REM ------------------------------------------------------------
REM Build
REM ------------------------------------------------------------

echo ============================================================
echo  Running dotnet build
echo ============================================================
echo.

dotnet build "%SOLUTION%" --no-restore

if errorlevel 1 (
    echo.
    echo ============================================================
    echo  BUILD FAILED
    echo ============================================================
    echo.
    pause
    exit /b 1
)

echo.
echo ============================================================
echo  BUILD SUCCESSFUL
echo ============================================================
echo.
echo bin directories removed : %BIN_COUNT%
echo obj directories removed : %OBJ_COUNT%
echo.
echo The solution was restored and built successfully.
echo.

pause
exit /b 0
