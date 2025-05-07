#!/usr/bin/env bash
# Render.com için start script

# Hata durumunda script'i durdur
set -e

# .NET SDK'yı kur (runtime için)
curl -sSL https://dot.net/v1/dotnet-install.sh | bash /dev/stdin --channel 8.0
export PATH="$HOME/.dotnet:$PATH"
export DOTNET_ROOT="$HOME/.dotnet"

# Render.com port ayarı
export ASPNETCORE_URLS="http://+:$PORT"
echo "ASPNETCORE_URLS: $ASPNETCORE_URLS"

# Uygulamayı başlat
cd out && dotnet JobAnalyzerDashboard.Server.dll
