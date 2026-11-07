#!/usr/bin/env bash
set -euo pipefail
if git rev-parse --is-inside-work-tree >/dev/null 2>&1; then
    CURRENT_BRANCH="$(git rev-parse --abbrev-ref HEAD 2>/dev/null || true)"
    if [[ "$CURRENT_BRANCH" == "main" ]] && git diff --quiet 2>/dev/null; then
        git pull origin main || true
    fi
fi
ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$ROOT_DIR/SlavicGame.csproj"
CONFIGURATION="${CONFIGURATION:-Release}"

if ! command -v dotnet >/dev/null 2>&1; then
    echo "BŁĄD: nie znaleziono polecenia dotnet." >&2
    echo "Zainstaluj .NET SDK 10 i uruchom skrypt ponownie." >&2
    exit 127
fi

DOTNET_VERSION="$(dotnet --version)"
if [[ "${DOTNET_VERSION%%.*}" != "10" ]]; then
    echo "UWAGA: projekt celuje w .NET 10, a znaleziono SDK: $DOTNET_VERSION" >&2
fi

echo "[SlavicGame] compile shaders..."
"$ROOT_DIR/tools/compile-shaders.sh"

if [[ "$(uname -s)" == Linux && ! -f "$ROOT_DIR/native/fidelityfx/libslavic_fsr3_vk.so" ]]; then
    echo "[SlavicGame] setup native FidelityFX FSR 3.1.4 + Frame Generation..."
    "$ROOT_DIR/tools/setup-fidelityfx.sh"
fi

echo "[SlavicGame] restore..."
dotnet restore "$PROJECT"

echo "[SlavicGame] build ($CONFIGURATION)..."
dotnet build "$PROJECT" --configuration "$CONFIGURATION" --no-restore

echo "[SlavicGame] start..."
exec dotnet run \
    --project "$PROJECT" \
    --configuration "$CONFIGURATION" \
    --no-build \
    -- "$@"
