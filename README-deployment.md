# JobAnalyzerDashboard Geçici Deployment Kılavuzu

Bu kılavuz, JobAnalyzerDashboard uygulamasını geçici olarak sergilemek için kullanabileceğiniz farklı yöntemleri açıklar.

## Deployment Seçenekleri

Projenizi sergilemek için üç ana seçenek sunuyoruz:

1. **Render.com ile Deployment** - Daha profesyonel ve kalıcı bir çözüm
2. **Vercel + Render Kombinasyonu** - Frontend ve backend için ayrı platformlar
3. **Ngrok ile Hızlı Deployment** - En hızlı ve en basit çözüm

## 1. Render.com ile Deployment (Önerilen)

Render.com, hem frontend hem de backend uygulamalarınızı tek bir platformda barındırmanıza olanak tanır.

### Avantajları:
- Tek platformda tam stack deployment
- PostgreSQL veritabanı entegrasyonu
- Otomatik SSL sertifikaları
- Blueprint ile tek tıklamayla deployment

### Kurulum:
Detaylı adımlar için [deploy-to-render.md](./deploy-to-render.md) dosyasına bakın.

```bash
# Hızlı başlangıç
git add .
git commit -m "Deployment hazırlığı"
# Render.com'da Blueprint kullanarak deploy edin
```

## 2. Vercel + Render Kombinasyonu

Frontend için Vercel, backend için Render.com kullanarak hibrit bir deployment.

### Avantajları:
- Vercel'in Angular uygulamaları için optimize edilmiş altyapısı
- Render.com'un .NET Core uygulamaları için güçlü desteği
- Her iki platformda da ücretsiz tier seçenekleri

### Kurulum:
- Frontend için [vercel.json](./vercel.json) dosyasını kullanın
- Backend için [render.yaml](./render.yaml) dosyasını kullanın
- Detaylı adımlar için [deploy-to-render.md](./deploy-to-render.md) dosyasındaki ilgili bölümlere bakın

## 3. Ngrok ile Hızlı Deployment

Yerel makinenizde çalışan uygulamayı internet üzerinden erişilebilir hale getirin.

### Avantajları:
- Çok hızlı kurulum (dakikalar içinde)
- Yerel geliştirme ortamınızı doğrudan paylaşma
- Herhangi bir bulut hesabı gerektirmez

### Kurulum:
Detaylı adımlar için [ngrok-deployment.md](./ngrok-deployment.md) dosyasına bakın.

```bash
# Uygulamayı başlatın
dotnet run --project JobAnalyzerDashboard.Server

# Yeni bir terminal penceresinde
ngrok http https://localhost:7062
```

## Deployment Sonrası Yapılandırma

Hangi yöntemi seçerseniz seçin, aşağıdaki yapılandırmaları güncellemeniz gerekecektir:

1. **Google OAuth Redirect URI**: Google Cloud Console'da OAuth yapılandırmanızı güncelleyin
2. **n8n Webhook URL'leri**: n8n workflow'larınızdaki webhook URL'lerini yeni deployment URL'lerine göre güncelleyin
3. **CORS Ayarları**: Backend'in frontend'den gelen isteklere izin vermesini sağlayın

## Önerilen Yaklaşım

Projenizi hızlı bir şekilde sergilemek için:

1. İlk olarak **Ngrok** ile hızlı bir deployment yapın (dakikalar içinde hazır)
2. Daha kalıcı bir çözüm için **Render.com** kullanarak tam stack deployment yapın

## Sorun Giderme

- **CORS Hataları**: `appsettings.Production.json` dosyasındaki izin verilen kaynakları kontrol edin
- **Veritabanı Bağlantı Hataları**: Bağlantı dizesinin doğru olduğundan emin olun
- **OAuth Hataları**: Redirect URI'nin Google Cloud Console'da yapılandırıldığından emin olun
- **n8n Entegrasyon Sorunları**: Webhook URL'lerinin güncel olduğunu kontrol edin
