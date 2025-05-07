#!/usr/bin/env bash
# Angular build çıktılarını ASP.NET Core wwwroot klasörüne kopyala

# Hata durumunda script'i durdur
set -e

echo "Angular build çıktıları kopyalanıyor..."

# wwwroot klasörünü oluştur
mkdir -p out/wwwroot

# Angular build çıktılarını kopyala
cp -r jobanalyzerdashboard.client/dist/jobanalyzerdashboard.client/browser/* out/wwwroot/

echo "Angular build çıktıları başarıyla kopyalandı!"
ls -la out/wwwroot/
