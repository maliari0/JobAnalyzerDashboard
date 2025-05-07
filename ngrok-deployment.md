# JobAnalyzerDashboard Ngrok ile Geçici Deployment Kılavuzu

Bu kılavuz, JobAnalyzerDashboard uygulamasını ngrok kullanarak hızlı bir şekilde internet üzerinden erişilebilir hale getirmenizi sağlar.

## Ön Koşullar

1. [ngrok](https://ngrok.com/) hesabı ve kurulumu
2. Çalışan bir JobAnalyzerDashboard uygulaması

## 1. Ngrok Kurulumu ve Yapılandırması

### 1.1. Ngrok Kurulumu

1. [ngrok.com](https://ngrok.com/) adresinden kaydolun
2. Ngrok'u indirin ve kurun
3. Authtoken'ınızı yapılandırın:

```bash
ngrok config add-authtoken YOUR_AUTH_TOKEN
```

### 1.2. Kalıcı Subdomain Oluşturma (Önerilen)

Ücretsiz ngrok hesabı ile geçici URL'ler alırsınız, ancak her başlatmada değişir. Daha kalıcı bir çözüm için:

1. [ngrok.com](https://ngrok.com/) adresinde oturum açın
2. Ücretli bir plan satın alın (Basic plan yeterlidir)
3. Kalıcı bir subdomain oluşturun (örn. jobanalyzerdashboard.ngrok.io)

## 2. Uygulamayı Çalıştırma

### 2.1. Backend ve Frontend'i Başlatma

```bash
cd JobAnalyzerDashboard
dotnet run --project JobAnalyzerDashboard.Server
```

Bu komut hem backend hem de frontend'i başlatacaktır.

### 2.2. Ngrok ile Tünelleme

Yeni bir terminal penceresinde:

```bash
# Kalıcı subdomain ile (önerilen)
ngrok http https://localhost:7062 --subdomain=jobanalyzerdashboard

# veya rastgele subdomain ile (ücretsiz plan)
ngrok http https://localhost:7062
```

## 3. Yapılandırma Ayarları

### 3.1. Google OAuth için Redirect URI Güncelleme

Google Cloud Console'da OAuth yapılandırmanızı güncelleyin:

1. [Google Cloud Console](https://console.cloud.google.com/) adresine gidin
2. OAuth 2.0 Client ID'nizi seçin
3. Authorized redirect URIs'ye şunu ekleyin:
   - `https://YOUR_NGROK_SUBDOMAIN.ngrok.io/api/auth/google/callback`

### 3.2. n8n Entegrasyonu için Webhook URL Güncelleme

n8n workflow'unuzu yeni ngrok URL'sine göre güncelleyin:

1. n8n arayüzüne gidin
2. Webhook node'larını açın
3. URL'leri `https://YOUR_NGROK_SUBDOMAIN.ngrok.io/api/...` olarak güncelleyin

## 4. Uygulama Erişimi

Ngrok başarıyla çalıştığında, terminal çıktısında görünen URL'yi kullanarak uygulamanıza internet üzerinden erişebilirsiniz:

```
Forwarding https://YOUR_NGROK_SUBDOMAIN.ngrok.io -> https://localhost:7062
```

## 5. Önemli Notlar

1. **Ücretsiz Plan Sınırlamaları**: Ücretsiz ngrok planı, oturumunuz sona erdiğinde URL'nin değişmesine neden olur
2. **Güvenlik**: Ngrok, geliştirme ve geçici demo amaçlıdır, uzun vadeli production kullanımı için uygun değildir
3. **Bağlantı Süresi**: Ücretsiz planda, ngrok tünelleri belirli bir süre sonra kapanabilir
4. **CORS**: Ngrok URL'si değiştiğinde CORS ayarlarını güncellemeniz gerekebilir

## 6. Sorun Giderme

- **CORS Hataları**: Program.cs dosyasında AllowedOrigins listesine ngrok URL'nizi ekleyin
- **OAuth Hataları**: Google Cloud Console'da redirect URI'nin doğru yapılandırıldığından emin olun
- **Bağlantı Sorunları**: ngrok tünelinin aktif olduğunu ve doğru porta yönlendirildiğini kontrol edin
