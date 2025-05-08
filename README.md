# JobAnalyzerDashboard

İş ilanlarını analiz eden ve başvuru sürecini otomatikleştiren bir dashboard uygulaması. Bu uygulama, iş arayanların iş ilanlarını takip etmelerini, başvurularını yönetmelerini ve yapay zeka destekli otomatik başvuru e-postaları oluşturmalarını sağlar.

## Özellikler

- İş ilanlarını görüntüleme ve filtreleme
- İş detaylarını inceleme
- Otomatik ve manuel başvuru yapma
- Başvuru geçmişini takip etme
- Profil yönetimi
- Çoklu özgeçmiş yükleme ve yönetme
- Google OAuth entegrasyonu ile kendi e-posta hesabınızdan e-posta gönderme
- Çoklu kullanıcı desteği (Kayıt, Giriş, E-posta doğrulama, Şifre sıfırlama)
- Admin paneli ve yönetim özellikleri
- Google Gemini AI ile otomatik başvuru e-postası oluşturma
- PostgreSQL veritabanı entegrasyonu
- Türkiye illeri için çoklu seçim desteği
- Telefon numarası doğrulama ve ülke seçimi
- n8n otomasyon sistemi entegrasyonu

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
      - SMTP ayarları (e-posta doğrulama için)
      - AppSettings (BaseUrl, FrontendUrl)

   b. User Secrets kullanmak için (önerilen):

      ```bash
      cd JobAnalyzerDashboard.Server
      dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=your-postgres-host;Database=your-database;Username=your-username;Password=your-password;Port=5432;SSL Mode=Require;Trust Server Certificate=true;"
      dotnet user-secrets set "Authentication:Google:ClientId" "your-google-client-id"
      dotnet user-secrets set "Authentication:Google:ClientSecret" "your-google-client-secret"
      dotnet user-secrets set "AppSettings:BaseUrl" "https://your-domain"
      dotnet user-secrets set "AppSettings:FrontendUrl" "https://your-domain"
      dotnet user-secrets set "EmailSettings:SmtpServer" "smtp.gmail.com"
      dotnet user-secrets set "EmailSettings:SmtpPort" "587"
      dotnet user-secrets set "EmailSettings:SmtpUsername" "your-email@gmail.com"
      dotnet user-secrets set "EmailSettings:SmtpPassword" "your-app-password"
      ```

4. Projeyi derleyin:

   ```bash
   dotnet build
   ```

5. Projeyi çalıştırın:

   ```bash
   dotnet run --project JobAnalyzerDashboard.Server
   ```

6. Tarayıcınızda aşağıdaki adresi açın:

   ```text
   https://localhost:5001
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
3. Redirect URI'yi `https://your-domain/api/oauth/google/callback` olarak ayarlayın
4. Gerekli API'leri etkinleştirin:
   - Gmail API
   - People API

### SMTP Yapılandırması

E-posta doğrulama ve şifre sıfırlama için SMTP yapılandırması:

1. Gmail için App Password oluşturun:
   - [Google Hesap Güvenliği](https://myaccount.google.com/security) sayfasına gidin
   - İki adımlı doğrulamayı etkinleştirin
   - [Uygulama Şifreleri](https://myaccount.google.com/apppasswords) sayfasına gidin
   - Yeni bir uygulama şifresi oluşturun
2. Oluşturduğunuz şifreyi `EmailSettings:SmtpPassword` olarak ayarlayın

## Yeni Özellikler ve Güncellemeler

### Çoklu Kullanıcı Desteği

- Kullanıcı kaydı ve giriş sistemi
- E-posta doğrulama
- Şifre sıfırlama
- Kullanıcı profil yönetimi
- Çoklu özgeçmiş yükleme ve yönetme

### Admin Paneli

- Kullanıcı yönetimi
- İş ilanı yönetimi
- Başvuru yönetimi
- Sistem ayarları
- Varsayılan admin hesabı (`admin@admin.com` / `admin123`)

### Otomatik Başvuru Sistemi

- n8n entegrasyonu ile otomatik başvuru e-postası oluşturma
- Google Gemini AI ile kişiselleştirilmiş başvuru metinleri
- Otomatik CV ekleme
- Kullanıcının kendi e-posta hesabından gönderme (OAuth)
- Başvuru e-postalarını düzenleme ve onaylama

### Veritabanı Geliştirmeleri

- PostgreSQL veritabanı entegrasyonu
- Verimli veri depolama ve sorgulama
- İlişkisel veri modeli
- Kullanıcı bazlı veri izolasyonu

### Kullanıcı Arayüzü İyileştirmeleri

- Duyarlı tasarım
- Kullanıcı dostu arayüz
- Türkiye illeri için çoklu seçim
- Telefon numarası doğrulama ve ülke seçimi
- Başvuru durumu takibi

### n8n Entegrasyonu

- İş ilanlarını otomatik analiz etme
- Yapay zeka ile iş ilanı kalite puanı hesaplama
- Otomatik başvuru e-postası oluşturma
- Telegram bildirimleri

## Gelecek Planlar

- Mülakat takibi ve Google Calendar entegrasyonu
- Analitik görünümü ve istatistikler
- İş ilanı web scraping
- Bildirim sistemi
- Profil sayfası iyileştirmeleri
- Mobil uygulama

## Lisans

Bu proje [MIT Lisansı](LICENSE) altında lisanslanmıştır.
