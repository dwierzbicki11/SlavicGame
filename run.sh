#!/usr/bin/env bash
set -euo pipefail

ROOT_DIR="$(cd "$(dirname "${BASH_SOURCE[0]}")" && pwd)"
PROJECT="$ROOT_DIR/SlavicGame.csproj"
CONFIGURATION="${CONFIGURATION:-Release}"

if ! command -v dotnet >/dev/null 2>&1; then
    echo "BŁĄD: nie znaleziono polecenia dotnet." >&2
    echo "Zainstaluj .NET SDK 11 i uruchom skrypt ponownie." >&2
    exit 127
fi

DOTNET_VERSION="$(dotnet --version)"
if [[ "${DOTNET_VERSION%%.*}" != "11" ]]; then
    echo "UWAGA: projekt celuje w .NET 11, a znaleziono SDK: $DOTNET_VERSION" >&2
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
