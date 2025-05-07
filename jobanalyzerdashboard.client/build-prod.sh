#!/bin/bash
# Angular production build script

# Hata durumunda script'i durdur
set -e

# Node.js ve npm'in kurulu olduğundan emin ol
echo "Node.js ve npm versiyonları kontrol ediliyor..."
node --version
npm --version

# Bağımlılıkları yükle
echo "Bağımlılıklar yükleniyor..."
npm install

# Production build
echo "Production build başlatılıyor..."
npm run build -- --configuration production

echo "Build tamamlandı!"
