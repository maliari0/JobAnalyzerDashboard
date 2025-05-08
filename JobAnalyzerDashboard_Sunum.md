---
title: "JobAnalyzerDashboard Projesi Sunumu"
author: "Muhammed Ali ARI"
date: "Mayıs 2025"
subject: "Web Uygulaması Geliştirme"
keywords: [n8n, LLM, ASP.NET Core, Angular, PostgreSQL, Fullstack]
lang: "tr"
colorlinks: true
toc-title: "İçindekiler"
toc: true
toc-depth: 3
fontsize: 12pt
---

# JobAnalyzerDashboard Projesi Sunumu

## Proje Hakkında

JobAnalyzerDashboard, iş ilanlarını analiz eden ve başvuru sürecini otomatikleştiren kapsamlı bir web uygulamasıdır. Proje, modern teknolojiler kullanılarak geliştirilmiş olup, kullanıcıların iş arama ve başvuru süreçlerini kolaylaştırmayı amaçlamaktadır.

## Teknoloji Yığını

### Backend
- **ASP.NET Core 8.0**: Modern, yüksek performanslı web API geliştirme çerçevesi
- **Entity Framework Core**: ORM (Object-Relational Mapping) aracı
- **PostgreSQL**: Veritabanı yönetim sistemi
- **JWT Authentication**: Güvenli kimlik doğrulama sistemi
- **Google OAuth 2.0**: E-posta gönderimi için OAuth entegrasyonu

### Frontend
- **Angular 19**: Modern, komponent tabanlı frontend çerçevesi
- **TypeScript**: Tip güvenliği sağlayan JavaScript üst kümesi
- **Bootstrap**: Duyarlı tasarım için CSS çerçevesi
- **NgSelect**: Gelişmiş seçim bileşenleri

### Deployment
- **Render.com**: Tam stack uygulama hosting platformu
- **CI/CD Pipeline**: GitHub entegrasyonu ile otomatik deployment
- **Build Scripts**: Özel bash scriptleri ile build ve deployment otomasyonu

## Proje Mimarisi

JobAnalyzerDashboard, modern bir mimari yaklaşımla geliştirilmiştir:

### Backend Mimarisi

1. **Çok Katmanlı Mimari**:
   - **Sunum Katmanı**: ASP.NET Core Web API Controllers
   - **İş Mantığı Katmanı**: Service sınıfları ve business logic
   - **Veri Erişim Katmanı**: Repository pattern ve Entity Framework Core
   - **Veritabanı Katmanı**: PostgreSQL veritabanı

2. **API Endpoints**:
   - `/api/job`: İş ilanları yönetimi
   - `/api/application`: Başvuru işlemleri
   - `/api/profile`: Kullanıcı profil yönetimi
   - `/api/auth`: Kimlik doğrulama ve yetkilendirme
   - `/api/admin`: Admin işlemleri
   - `/api/webhook`: n8n entegrasyonu için webhook'lar

3. **Dependency Injection**:
   - Servis kayıtları ve bağımlılık enjeksiyonu
   - Scoped, Transient ve Singleton servisler
   - Interface tabanlı gevşek bağlantılı mimari

4. **Middleware Pipeline**:
   - Exception handling middleware
   - JWT authentication middleware
   - CORS policy middleware
   - Request logging middleware

### Frontend Mimarisi

1. **Komponent Yapısı**:
   - Modüler Angular komponentleri
   - Lazy loading modülleri
   - Shared komponentler ve direktifler

2. **State Yönetimi**:
   - Service tabanlı state yönetimi
   - RxJS Observable pattern
   - Reactive Forms

3. **HTTP İletişimi**:
   - HttpClient ile API entegrasyonu
   - Interceptor'lar ile token yönetimi
   - Error handling ve retry mekanizmaları

4. **Routing ve Navigation**:
   - Route guards ile yetkilendirme
   - Parametreli routing
   - Child routes

### Entegrasyon Mimarisi

1. **Mikroservis Entegrasyonu**:
   - n8n otomasyon platformu ile webhook tabanlı entegrasyon
   - REST API üzerinden veri alışverişi
   - JSON formatında veri transferi

2. **Güvenlik Katmanı**:
   - JWT tabanlı kimlik doğrulama
   - Role dayalı yetkilendirme (Admin, User)
   - Google OAuth 2.0 entegrasyonu
   - Şifre hashleme ve tuzlama

## Temel Özellikler

### 1. İş İlanı Yönetimi
- İş ilanlarını görüntüleme ve filtreleme
- Detaylı iş bilgilerini inceleme
- Kalite puanı ve eylem önerileri
- İş kategorileri ve etiketleri

### 2. Başvuru Sistemi
- Manuel başvuru yapma
- Otomatik başvuru oluşturma
- Başvuru durumu takibi
- Başvuru geçmişi görüntüleme

### 3. Profil Yönetimi
- Kişisel bilgi düzenleme
- Çoklu özgeçmiş yükleme
- Varsayılan özgeçmiş belirleme
- Tercih edilen iş türleri ve konumlar

### 4. Çoklu Kullanıcı Desteği
- Kullanıcı kaydı ve giriş sistemi
- E-posta doğrulama
- Şifre sıfırlama
- Kullanıcı profil yönetimi

### 5. Admin Paneli
- Kullanıcı yönetimi
- İş ilanı yönetimi
- Başvuru yönetimi
- Sistem ayarları

## n8n Entegrasyonu ve Otomasyon

JobAnalyzerDashboard'un en önemli özelliklerinden biri, n8n otomasyon platformu ile entegrasyonudur. Bu entegrasyon sayesinde:

### n8n Nedir?
n8n, açık kaynaklı bir iş akışı otomasyon aracıdır. API'ler, veritabanları ve diğer hizmetler arasında veri akışını otomatikleştirmeye olanak tanır. Görsel bir arayüz üzerinden, kod yazmadan karmaşık iş akışları oluşturulabilir.

### Entegrasyon Faydaları
1. **İş Akışı Otomasyonu**: Tekrarlayan görevlerin otomatikleştirilmesi
2. **Sistem Entegrasyonu**: Farklı sistemler arasında veri akışı
3. **Webhook Tabanlı İletişim**: Gerçek zamanlı veri alışverişi
4. **Esnek Yapılandırma**: Özelleştirilebilir iş akışları
5. **Yapay Zeka Entegrasyonu**: LLM modelleri ile içerik üretimi

### Otomatik Başvuru Süreci
1. Kullanıcı "Otomatik Oluştur" butonuna tıklar
2. Sistem n8n webhook'una istek gönderir
3. n8n, iş ilanı ve kullanıcı verilerini işler
4. LLM (Büyük Dil Modeli) ile kişiselleştirilmiş başvuru e-postası oluşturulur
5. Oluşturulan e-posta içeriği kullanıcıya gösterilir
6. Kullanıcı içeriği onaylar ve gönderir
7. E-posta, kullanıcının Google hesabı üzerinden gönderilir (OAuth entegrasyonu)
8. Başvuru durumu güncellenir

### N8n Test Sayfası
Projede, n8n entegrasyonunu test etmek ve geliştirmek için özel bir test sayfası bulunmaktadır:

1. **Webhook Testi**: n8n webhook'larına manuel olarak veri gönderme
2. **Örnek Veri Tanımlama**: Test için örnek iş ilanı ve kullanıcı verisi oluşturma
3. **Yanıt İzleme**: n8n'den gelen yanıtları görüntüleme ve analiz etme
4. **Hata Ayıklama**: Webhook iletişiminde oluşan hataları tespit etme

Bu test sayfası, mail tarama sistemi henüz mükemmel çalışmadığı durumlarda, geliştirme ve test amaçlı olarak kullanılabilmektedir. Kullanıcılar, gerçek veriler yerine test verileri ile sistemin işleyişini kontrol edebilir.

### n8n Otomasyon İş Akışları

Projede iki temel n8n iş akışı bulunmaktadır:

#### 1. Algilama3 İş Akışı

Bu iş akışı, e-posta ve webhook üzerinden gelen iş ilanlarını analiz eder ve işler:

**Temel Bileşenler:**
- **Email Trigger (IMAP)**: E-posta kutusunu dinleyerek yeni iş ilanlarını algılar
- **Webhook**: Web API üzerinden gelen iş ilanı verilerini alır
- **AI Agent**: Google Gemini 2.0 Flash modeli kullanarak iş ilanlarını analiz eder
- **Structured Output Parser**: AI çıktısını yapılandırılmış JSON formatına dönüştürür

**İş Akışı Süreci:**
1. E-posta veya webhook üzerinden gelen iş ilanı verisi alınır
2. AI Agent, iş ilanını analiz ederek aşağıdaki bilgileri çıkarır:
   ```json
   {
     "is_job_posting": "evet veya hayır",
     "quality_score": 1-5 arası bir sayı,
     "action_suggestion": "sakla veya ilgisiz veya bildir",
     "category": "frontend / backend / mobile / devops / data science / diğer",
     "tags": ["remote", "junior", "b2b", "..."],
     "title": "...",
     "description": "...",
     "employment_type": "Remote / Tam Zamanlı / Yarı Zamanlı",
     "salary": "...",
     "company_name": "...",
     "company_website": "...",
     "contact_email": "...",
     "url": "..."
   }
   ```
3. İş ilanı kalitesine göre farklı işlemler yapılır:
   - **Kaliteli İlanlar (4-5 puan)**: Doğrudan veritabanına kaydedilir
   - **Orta Kaliteli İlanlar (3 puan)**: Telegram üzerinden onay istenir
   - **Düşük Kaliteli İlanlar (1-2 puan)**: Log dosyasına kaydedilir

**Kod Parçası (AI Agent Prompt):**
```
Sen profesyonel bir iş ilanı analistisin.

Aşağıda sana bir iş ilanı verilecek. Lütfen her alanı değerlendirerek puan ver:

Her dolu alan için 1 puan ver:
- title
- description
- salary
- employment_type
- company_website
- contact_email
- url

Toplam puanı hesapla (0–7 arası)
Ardından quality_score = ROUND(toplam / 7 * 5)
```

**Entegrasyon Noktaları:**
- `/api/webhook`: İş ilanlarını almak için webhook endpoint
- `/api/job`: İşlenen iş ilanlarını kaydetmek için API endpoint

#### 2. auto-apply-Render İş Akışı

Bu iş akışı, kullanıcıların iş ilanlarına otomatik başvuru yapmasını sağlar:

**Temel Bileşenler:**
- **Webhook**: Başvuru isteğini alır
- **Get User**: Kullanıcı profil bilgilerini API'den çeker
- **Get CV**: Kullanıcının varsayılan CV'sini API'den çeker
- **AI Agent**: Google Gemini 2.0 Flash modeli kullanarak başvuru e-postası oluşturur
- **HTTP POST**: Oluşturulan e-posta içeriğini API'ye gönderir
- **Telegram**: Başvuru bilgilerini Telegram üzerinden bildirir

**İş Akışı Süreci:**
1. Kullanıcı, web arayüzünden "Otomatik Oluştur" butonuna tıklar
2. Webhook, başvuru isteğini alır (jobId, userId, applicationId)
3. API'den kullanıcı profil bilgileri ve CV alınır
4. AI Agent, kullanıcı profili ve iş ilanı bilgilerine göre kişiselleştirilmiş bir başvuru e-postası oluşturur
5. Oluşturulan e-posta içeriği, API'ye gönderilir ve veritabanına kaydedilir
6. Başvuru bilgileri Telegram üzerinden bildirilir

**Kod Parçası (AI Agent Prompt):**
```
Aşağıda, bir kullanıcı profili ve bir iş ilanı yer almaktadır. Bu bilgilere göre kısa, samimi ve ciddi bir başvuru e-postası yazmanı istiyorum.

🔹 Kurallar:
- "Merhaba" ile başla
- Tek paragraflık metin yaz
- CV'yi iletmek istediğini belirt
- Sonuna adını yaz

📌 Kullanıcı Bilgisi:
Ad: {{ $node["Get User"].json.fullName }}
Eğitim: {{ $('Get User').item.json.education }}
Tecrübe: {{ $('Get User').item.json.experience }}
Tercih Edilen Model: {{ $('Get User').item.json.preferredJobTypes }}
Yetenekler: {{ $('Get User').item.json.skills }}
Tercih Edilen Lokasyon: {{ $('Get User').item.json.preferredLocations }}
```

**Entegrasyon Noktaları:**
- `/api/profile/n8n`: Kullanıcı profil bilgilerini almak için API endpoint
- `/api/profile/resumes/default/n8n`: Kullanıcının varsayılan CV'sini almak için API endpoint
- `/api/job/n8n-save-email`: Oluşturulan e-posta içeriğini kaydetmek için API endpoint

### LLM Entegrasyonu Detayları

Her iki iş akışında da Google Gemini 2.0 Flash modeli kullanılmaktadır. Bu model, düşük gecikme süresi ve yüksek performans sunarak hızlı yanıt üretimi sağlar.

**Model Özellikleri:**
- **Model Adı**: `models/gemini-2.0-flash`
- **Bağlam Penceresi**: 32K token
- **Yanıt Hızı**: Düşük gecikme süresi (200-500ms)
- **Çok Dilli Destek**: Türkçe dahil 40+ dil desteği
- **Yapılandırılmış Çıktı**: JSON formatında yapılandırılmış yanıtlar üretebilme

**Entegrasyon Yöntemi:**
- n8n'in Langchain entegrasyonu kullanılarak AI Agent ve Output Parser düğümleri ile entegre edilmiştir
- Sistem mesajları ve kullanıcı promptları ayrı ayrı yapılandırılarak daha tutarlı sonuçlar elde edilmiştir
- Hata durumlarında otomatik yeniden deneme mekanizması eklenmiştir (maksimum 5 deneme)

## Yapay Zeka (LLM) Entegrasyonu

Projede Google Gemini AI kullanılarak otomatik başvuru e-postaları oluşturulmaktadır:

### LLM Entegrasyonunun Faydaları
1. **Kişiselleştirilmiş İçerik**: Kullanıcı profiline ve iş ilanına özel içerik
2. **Zaman Tasarrufu**: Manuel e-posta yazma süresinden tasarruf
3. **Profesyonel İçerik**: Tutarlı ve profesyonel başvuru metinleri
4. **Çok Dilli Destek**: Farklı dillerde başvuru e-postaları

### LLM Entegrasyon Süreci
1. İş ilanı ve kullanıcı profil verileri n8n'e gönderilir
2. n8n, verileri işleyerek Google Gemini API'sine iletir
3. LLM, verilen bağlama göre kişiselleştirilmiş bir başvuru e-postası oluşturur
4. Oluşturulan içerik, kullanıcıya gösterilmek üzere sisteme geri gönderilir
5. Kullanıcı, içeriği düzenleyebilir ve onaylayabilir

## Veritabanı Yapısı

Proje, PostgreSQL veritabanı kullanmaktadır ve aşağıdaki temel tablolara sahiptir:

### Temel Tablolar

1. **Users**: Kullanıcı hesap bilgileri
   - Id (PK)
   - Email (unique)
   - PasswordHash
   - FirstName, LastName
   - Role (Admin, User)
   - EmailConfirmed
   - ProfileId (FK)

2. **Profiles**: Kullanıcı profil bilgileri
   - Id (PK)
   - FullName
   - Email
   - Phone
   - LinkedInUrl, GithubUrl, PortfolioUrl
   - Skills, Education, Experience
   - PreferredJobTypes, PreferredLocations
   - MinimumSalary
   - UserId (FK)

3. **Resumes**: Kullanıcı özgeçmişleri
   - Id (PK)
   - ProfileId (FK)
   - FileName
   - FilePath
   - UploadDate
   - IsDefault
   - FileSize
   - FileType

4. **Jobs**: İş ilanları
   - Id (PK)
   - Title
   - Company
   - Location
   - Description
   - Requirements
   - Salary
   - PostedDate
   - ExpiryDate
   - QualityScore
   - ActionSuggestion
   - Categories
   - ContactEmail

5. **Applications**: Başvurular
   - Id (PK)
   - JobId (FK)
   - UserId (FK)
   - AppliedDate
   - Status (Applying, Applied, Rejected, Interview)
   - AppliedMethod (Manual, n8n)
   - SentMessage
   - EmailContent
   - IsAutoApplied
   - CvAttached

6. **OAuthTokens**: OAuth token bilgileri
   - Id (PK)
   - UserId (FK)
   - ProfileId (FK)
   - Provider (Google)
   - AccessToken
   - RefreshToken
   - ExpiresAt
   - Scope

### Veritabanı İlişkileri

- **User-Profile**: One-to-One ilişki
- **Profile-Resumes**: One-to-Many ilişki
- **User-Applications**: One-to-Many ilişki
- **Job-Applications**: One-to-Many ilişki
- **Profile-OAuthTokens**: One-to-Many ilişki

### Veritabanı Migrasyonları

Proje, Entity Framework Core Code-First yaklaşımı kullanılarak geliştirilmiştir. Veritabanı şeması, migration'lar aracılığıyla yönetilmektedir:

```csharp
// ApplicationDbContext.cs
public class ApplicationDbContext : DbContext
{
    public DbSet<User> Users { get; set; }
    public DbSet<Profile> Profiles { get; set; }
    public DbSet<Resume> Resumes { get; set; }
    public DbSet<Job> Jobs { get; set; }
    public DbSet<Application> Applications { get; set; }
    public DbSet<OAuthToken> OAuthTokens { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // İlişki tanımlamaları
        modelBuilder.Entity<User>()
            .HasOne(u => u.Profile)
            .WithOne(p => p.User)
            .HasForeignKey<Profile>(p => p.UserId);

        // Diğer ilişki ve konfigürasyonlar...

        // Seed data
        SeedData(modelBuilder);
    }
}
```

## Deployment ve Hosting

JobAnalyzerDashboard, Render.com platformunda deploy edilmiştir:

### Deployment Özellikleri
1. **Tek Platform**: Frontend ve backend tek bir URL üzerinden sunulmaktadır
2. **PostgreSQL Entegrasyonu**: Render.com üzerinde PostgreSQL veritabanı
3. **Otomatik SSL**: HTTPS desteği
4. **CI/CD Pipeline**: Otomatik derleme ve deployment

### Erişim Bilgileri
- **URL**: https://jobanalyzerdashboard.onrender.com
- **n8n Webhook**: https://n8n-service-a2yz.onrender.com/webhook/apply-auto

## Kullanıcı Arayüzü

Uygulama, kullanıcı dostu ve modern bir arayüze sahiptir:

### Ana Sayfalar
1. **İş İlanları**: Tüm iş ilanlarını görüntüleme ve filtreleme
2. **İş Detayları**: İş ilanı detaylarını görüntüleme ve başvuru yapma
3. **Profil**: Kullanıcı profilini düzenleme ve özgeçmiş yönetimi
4. **Başvuru Geçmişi**: Yapılan başvuruları görüntüleme ve takip etme
5. **Admin Paneli**: Sistem yönetimi (sadece admin kullanıcıları için)

### Özel Özellikler
1. **Türkiye İlleri Seçimi**: Çoklu il seçimi desteği
2. **Telefon Doğrulama**: Ülke kodu seçimi ile telefon doğrulama
3. **Özgeçmiş Yönetimi**: Çoklu özgeçmiş yükleme ve varsayılan belirleme
4. **Otomatik E-posta Oluşturma**: LLM ile kişiselleştirilmiş başvuru e-postaları

## Güvenlik Özellikleri

Uygulama, modern güvenlik standartlarına uygun olarak geliştirilmiştir:

1. **JWT Kimlik Doğrulama**: Güvenli token tabanlı kimlik doğrulama
2. **Role Dayalı Yetkilendirme**: Kullanıcı rollerine göre erişim kontrolü
3. **Google OAuth 2.0**: Güvenli e-posta gönderimi için OAuth entegrasyonu
4. **Şifre Hashleme**: Güvenli şifre depolama
5. **HTTPS**: Güvenli veri iletimi

## Gelecek Geliştirmeler

Projenin gelecek versiyonlarında planlanan geliştirmeler:

1. **Mülakat Takibi (Google Calendar)**: Mülakat süreçlerinin takibi ve Google Calendar entegrasyonu ile otomatik hatırlatmalar
2. **Analiz Görünümü**: İş ilanları ve başvurular için detaylı analitik paneli, grafikler ve raporlar
3. **Web Sitesi Tarama Sistemi**: Popüler iş sitelerinden otomatik iş ilanı toplama ve analiz etme
4. **Gelişmiş Bildirim Sistemi**: E-posta, push bildirimleri ve SMS entegrasyonu
5. **Profil Sayfası Geliştirmeleri**: Daha kapsamlı profil bilgileri, beceri eşleştirme ve öneri sistemleri

## Sonuç

JobAnalyzerDashboard, modern web teknolojileri, yapay zeka ve otomasyon araçlarını bir araya getirerek iş arama ve başvuru süreçlerini kolaylaştıran kapsamlı bir çözüm sunmaktadır. Proje, sürekli geliştirilmeye açık olup, kullanıcı geri bildirimleri doğrultusunda yeni özellikler eklenmeye devam edilecektir.

---

*Bu doküman, JobAnalyzerDashboard projesinin teknik sunumu için hazırlanmıştır.*

*Geliştirici: Muhammed Ali ARI*
