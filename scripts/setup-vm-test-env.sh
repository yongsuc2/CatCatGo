#!/usr/bin/env bash
set -euo pipefail

DOTNET_CHANNEL="8.0"
DOTNET_INSTALL_DIR="${DOTNET_ROOT:-$HOME/.dotnet}"
SERVER_DIR="$(cd "$(dirname "$0")/../Server" && pwd)"

print_step() { printf "\n=== %s ===\n" "$1"; }

print_step ".NET SDK 확인"
if command -v dotnet &>/dev/null && dotnet --version 2>/dev/null | grep -q "^8\."; then
    echo ".NET 8 SDK 이미 설치됨: $(dotnet --version)"
else
    echo ".NET $DOTNET_CHANNEL SDK 설치 중..."
    curl -fsSL https://dot.net/v1/dotnet-install.sh -o /tmp/dotnet-install.sh
    chmod +x /tmp/dotnet-install.sh
    /tmp/dotnet-install.sh --channel "$DOTNET_CHANNEL" --install-dir "$DOTNET_INSTALL_DIR"
    export DOTNET_ROOT="$DOTNET_INSTALL_DIR"
    export PATH="$DOTNET_ROOT:$PATH"
    echo ".NET SDK 설치 완료: $(dotnet --version)"
fi

if ! echo "$PATH" | grep -q "$DOTNET_INSTALL_DIR"; then
    export DOTNET_ROOT="$DOTNET_INSTALL_DIR"
    export PATH="$DOTNET_ROOT:$PATH"
fi

SHELL_RC="$HOME/.bashrc"
if [ -f "$HOME/.zshrc" ] && [ "$SHELL" = "/bin/zsh" ]; then
    SHELL_RC="$HOME/.zshrc"
fi

if ! grep -q "DOTNET_ROOT" "$SHELL_RC" 2>/dev/null; then
    {
        echo ""
        echo "export DOTNET_ROOT=\"$DOTNET_INSTALL_DIR\""
        echo "export PATH=\"\$DOTNET_ROOT:\$PATH\""
    } >> "$SHELL_RC"
    echo "PATH 설정을 $SHELL_RC 에 추가함"
fi

print_step "NuGet 패키지 복원"
dotnet restore "$SERVER_DIR/CatCatGo.Server.sln"

print_step "솔루션 빌드"
dotnet build "$SERVER_DIR/CatCatGo.Server.sln" --no-restore

print_step "테스트 실행"
dotnet test "$SERVER_DIR/CatCatGo.Server.sln" --no-build --verbosity normal

print_step "완료"
echo "서버 테스트 환경 설정 완료"
echo ""
echo "사용법:"
echo "  cd $SERVER_DIR && dotnet test"
echo "  cd $SERVER_DIR && dotnet test --filter ClassName=AuthServiceTests"
echo "  cd $SERVER_DIR && dotnet test --verbosity detailed"
