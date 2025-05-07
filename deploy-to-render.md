# JobAnalyzerDashboard Render.com Deployment Kılavuzu

Bu kılavuz, JobAnalyzerDashboard uygulamasını Render.com platformunda nasıl deploy edeceğinizi adım adım açıklar.

## Ön Koşullar

1. [Render.com](https://render.com) hesabı
2. [GitHub](https://github.com) hesabı (opsiyonel, ancak önerilir)
3. Google OAuth kimlik bilgileri (mevcut olanları kullanabilirsiniz)

## 1. Hazırlık Adımları

### 1.1. Projeyi GitHub'a Yükleyin (Önerilen)

```bash
git init
git add .
git commit -m "Initial commit for deployment"
git remote add origin https://github.com/KULLANICI_ADINIZ/JobAnalyzerDashboard.git
git push -u origin main
```

### 1.2. Render.com Blueprint Yapılandırması

Projede oluşturduğumuz `render.yaml` dosyası, Render.com'un Blueprint özelliğini kullanarak tek tıklamayla tüm altyapıyı oluşturmanıza olanak tanır.

## 2. Backend Deployment

### 2.1. Render.com'da Web Service Oluşturma

1. [Render Dashboard](https://dashboard.render.com)'a giriş yapın
2. "New +" butonuna tıklayın ve "Web Service" seçin
3. GitHub/GitLab reponuzu bağlayın veya "Public Git repository" seçeneğini kullanın
4. Repository URL'sini girin
5. Aşağıdaki ayarları yapılandırın:
   - **Name**: jobanalyzerdashboard-api
   - **Runtime**: .NET
   - **Build Command**: `dotnet publish -c Release -o out`
   - **Start Command**: `cd out && dotnet JobAnalyzerDashboard.Server.dll`

### 2.2. Environment Variables

Aşağıdaki environment variable'ları ekleyin:

- `ASPNETCORE_ENVIRONMENT`: Production
- `ConnectionStrings__DefaultConnection`: PostgreSQL bağlantı dizesi (Render.com'un sağladığı)
- `JwtSettings__Secret`: Güçlü bir rastgele dize (en az 32 karakter)
- `JwtSettings__Issuer`: JobAnalyzerDashboard
- `JwtSettings__Audience`: JobAnalyzerDashboardUsers
- `JwtSettings__ExpirationInMinutes`: 60
- `Authentication__Google__ClientId`: Google OAuth Client ID
- `Authentication__Google__ClientSecret`: Google OAuth Client Secret
- `Authentication__Google__RedirectUri`: https://jobanalyzerdashboard-api.onrender.com/api/auth/google/callback
- `AppSettings__BaseUrl`: https://jobanalyzerdashboard-api.onrender.com

## 3. Frontend Deployment

### 3.1. Angular Uygulamasını Production için Hazırlama

1. `proxy.conf.js` dosyasını güncelleyin (production API URL'si için)
2. Angular uygulamasını build edin:

```bash
cd jobanalyzerdashboard.client
npm install
ng build --configuration production
```

### 3.2. Vercel ile Frontend Deployment

1. [Vercel](https://vercel.com)'e kaydolun
2. "New Project" butonuna tıklayın
3. GitHub reponuzu bağlayın
4. Aşağıdaki ayarları yapılandırın:
   - **Framework Preset**: Angular
   - **Build Command**: `cd jobanalyzerdashboard.client && npm install && npm run build`
   - **Output Directory**: `jobanalyzerdashboard.client/dist/jobanalyzerdashboard.client/browser`
5. "Deploy" butonuna tıklayın

## 4. Veritabanı Yapılandırması

Render.com, PostgreSQL veritabanı hizmeti sunar:

1. Render Dashboard'da "New +" butonuna tıklayın ve "PostgreSQL" seçin
2. Aşağıdaki ayarları yapılandırın:
   - **Name**: jobanalyzerdashboard-db
   - **Database**: jobanalyzerdashboard
   - **User**: jobanalyzerdashboard_user
3. "Create Database" butonuna tıklayın
4. Oluşturulan bağlantı dizesini backend servisinin environment variable'larına ekleyin

## 5. Son Adımlar

1. Backend servisinin başarıyla deploy edildiğini doğrulayın
2. Frontend uygulamasının backend API'sine bağlanabildiğini doğrulayın
3. Google OAuth yönlendirme URI'sinin doğru yapılandırıldığını kontrol edin
4. n8n entegrasyonunun yeni deployment URL'lerini kullanacak şekilde güncellendiğinden emin olun

## Sorun Giderme

- **CORS Hataları**: `appsettings.Production.json` dosyasındaki izin verilen kaynakları kontrol edin
- **Veritabanı Bağlantı Hataları**: Bağlantı dizesinin doğru olduğundan emin olun
- **OAuth Hataları**: Redirect URI'nin Google Cloud Console'da yapılandırıldığından emin olun
