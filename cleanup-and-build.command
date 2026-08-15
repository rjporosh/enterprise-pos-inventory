#!/bin/bash

# ============================================================
# Enterprise POS Inventory - Clean and Build
# macOS version
# ============================================================

set -o pipefail

# Always use the directory where this script is located
ROOT_DIR="$(cd "$(dirname "$0")" && pwd)"

cd "$ROOT_DIR" || exit 1

echo ""
echo "============================================================"
echo " Enterprise POS Inventory - Clean and Build"
echo "============================================================"
echo ""
echo "Repository: $ROOT_DIR"
echo ""

# ------------------------------------------------------------
# Check .NET SDK
# ------------------------------------------------------------

if ! command -v dotnet >/dev/null 2>&1; then
    echo "ERROR: dotnet command was not found."
    echo "Please install the .NET SDK and try again."
    echo ""
    read -r -p "Press Enter to exit..."
    exit 1
fi

echo "Using .NET:"
dotnet --version
echo ""

# ------------------------------------------------------------
# Find solution
# ------------------------------------------------------------

SOLUTION=""

# Prefer .slnx if present
while IFS= read -r file; do
    SOLUTION="$file"
    break
done < <(find "$ROOT_DIR" -maxdepth 2 -type f -name "*.slnx" -print)

# Otherwise use .sln
if [ -z "$SOLUTION" ]; then
    while IFS= read -r file; do
        SOLUTION="$file"
        break
    done < <(find "$ROOT_DIR" -maxdepth 2 -type f -name "*.sln" -print)
fi

if [ -z "$SOLUTION" ]; then
    echo "ERROR: No .sln or .slnx file found within the repository root."
    echo ""
    read -r -p "Press Enter to exit..."
    exit 1
fi

echo "Solution:"
echo "  $SOLUTION"
echo ""

# ------------------------------------------------------------
# Delete all bin directories
# ------------------------------------------------------------

echo "============================================================"
echo " Removing bin directories"
echo "============================================================"
echo ""

BIN_COUNT=0

while IFS= read -r -d '' dir; do
    echo "Removing: $dir"
    rm -rf "$dir"
    BIN_COUNT=$((BIN_COUNT + 1))
done < <(find "$ROOT_DIR" -type d -name "bin" -print0)

echo ""
echo "Removed $BIN_COUNT bin director(s)."
echo ""

# ------------------------------------------------------------
# Delete all obj directories
# ------------------------------------------------------------

echo "============================================================"
echo " Removing obj directories"
echo "============================================================"
echo ""

OBJ_COUNT=0

while IFS= read -r -d '' dir; do
    echo "Removing: $dir"
    rm -rf "$dir"
    OBJ_COUNT=$((OBJ_COUNT + 1))
done < <(find "$ROOT_DIR" -type d -name "obj" -print0)

echo ""
echo "Removed $OBJ_COUNT obj director(s)."
echo ""

# ------------------------------------------------------------
# Restore
# ------------------------------------------------------------

echo "============================================================"
echo " Running dotnet restore"
echo "============================================================"
echo ""

dotnet restore "$SOLUTION"

if [ $? -ne 0 ]; then
    echo ""
    echo "ERROR: dotnet restore failed."
    echo ""
    read -r -p "Press Enter to exit..."
    exit 1
fi

echo ""
echo "Restore completed successfully."
echo ""

# ------------------------------------------------------------
# Build
# ------------------------------------------------------------

echo "============================================================"
echo " Running dotnet build"
echo "============================================================"
echo ""

dotnet build "$SOLUTION" --no-restore

if [ $? -ne 0 ]; then
    echo ""
    echo "============================================================"
    echo " BUILD FAILED"
    echo "============================================================"
    echo ""
    read -r -p "Press Enter to exit..."
    exit 1
fi

echo ""
echo "============================================================"
echo " BUILD SUCCESSFUL"
echo "============================================================"
echo ""
echo "bin directories removed : $BIN_COUNT"
echo "obj directories removed : $OBJ_COUNT"
echo ""
echo "The solution was restored and built successfully."
echo ""

read -r -p "Press Enter to exit..."
