#!/usr/bin/env bash
# Render.com için build script

# Hata durumunda script'i durdur
set -e

# .NET SDK'yı kur
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0
export PATH="$HOME/.dotnet:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"

# Angular uygulamasını derle
cd jobanalyzerdashboard.client
chmod +x build-prod.sh
./build-prod.sh
cd ..

# Backend projesini derle
dotnet publish -c Release -o out
