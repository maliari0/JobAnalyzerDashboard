# JobAnalyzerDashboard

İş ilanlarını analiz eden ve başvuru sürecini otomatikleştiren bir dashboard uygulaması.

## Özellikler

- İş ilanlarını görüntüleme ve filtreleme
- İş detaylarını inceleme
- Otomatik başvuru yapma
- Başvuru geçmişini takip etme
- Profil yönetimi
- Özgeçmiş yükleme ve yönetme
- Google OAuth entegrasyonu ile e-posta gönderme
- Çoklu kullanıcı desteği (Kayıt ve Giriş)
- Admin paneli ve yönetim özellikleri
- Yapay zeka ile otomatik başvuru e-postası oluşturma
- PostgreSQL veritabanı entegrasyonu
- Türkiye illeri için çoklu seçim desteği
- Telefon numarası doğrulama ve ülke seçimi

## Kurulum

1. Projeyi klonlayın:

   ```bash
   git clone https://github.com/maliari0/JobAnalyzerDashboard.git
   ```

2. `appsettings.example.json` dosyasını `appsettings.json` olarak kopyalayın:

   ```bash
   cp JobAnalyzerDashboard.Server/appsettings.example.json JobAnalyzerDashboard.Server/appsettings.json
   ```

3. `appsettings.json` dosyasını kendi bilgilerinizle güncelleyin veya User Secrets kullanın:

   a. `appsettings.json` dosyasını güncellemek için:
      - PostgreSQL veritabanı bağlantı bilgileri
      - Google OAuth Client ID ve Client Secret
      - Redirect URI

   b. User Secrets kullanmak için (önerilen):

      ```bash
      cd JobAnalyzerDashboard.Server
      dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=your-postgres-host;Database=your-database;Username=your-username;Password=your-password;Port=5432;SSL Mode=Require;Trust Server Certificate=true;Timeout=30;Command Timeout=30;"
      dotnet user-secrets set "Authentication:Google:ClientId" "your-google-client-id"
      dotnet user-secrets set "Authentication:Google:ClientSecret" "your-google-client-secret"
      dotnet user-secrets set "Authentication:Google:RedirectUri" "https://your-domain/api/auth/google/callback"
      ```

4. Projeyi derleyin:

   ```bash
   dotnet build
   ```

5. Projeyi çalıştırın:

   ```bash
   dotnet run --project JobAnalyzerDashboard.Server
   ```

## Geliştirme

### Veritabanı Migrasyonları

Veritabanı migrasyonları oluşturmak için:

```bash
dotnet ef migrations add MigrationName --project JobAnalyzerDashboard.Server
```

Migrasyonları uygulamak için:

```bash
dotnet ef database update --project JobAnalyzerDashboard.Server
```

### OAuth Yapılandırması

Google OAuth kullanmak için:

1. [Google Cloud Console](https://console.cloud.google.com/)'da bir proje oluşturun
2. OAuth 2.0 Client ID ve Client Secret oluşturun
3. Redirect URI'yi `https://your-domain/api/profile/google/callback` olarak ayarlayın
4. Gerekli API'leri etkinleştirin:
   - Gmail API
   - Google+ API
   - People API

## Yeni Özellikler ve Güncellemeler

### Çoklu Kullanıcı Desteği

- Kullanıcı kaydı ve giriş sistemi
- E-posta doğrulama
- Şifre sıfırlama
- Kullanıcı profil yönetimi

### Admin Paneli

- Kullanıcı yönetimi
- İş ilanı yönetimi
- Başvuru yönetimi
- Sistem ayarları

### Otomatik Başvuru Sistemi

- n8n entegrasyonu ile otomatik başvuru e-postası oluşturma
- Google Gemini AI ile kişiselleştirilmiş başvuru metinleri
- Otomatik CV ekleme

### Veritabanı Geliştirmeleri

- PostgreSQL veritabanı entegrasyonu
- Verimli veri depolama ve sorgulama
- İlişkisel veri modeli

### Kullanıcı Arayüzü İyileştirmeleri

- Duyarlı tasarım
- Kullanıcı dostu arayüz
- Türkiye illeri için çoklu seçim
- Telefon numarası doğrulama ve ülke seçimi

## Lisans

Bu proje [MIT Lisansı](LICENSE) altında lisanslanmıştır.
