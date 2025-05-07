# Google OAuth Bağlantı Sorunu Çözümü

"Google Hesabını Bağla" butonuna tıkladığınızda API sayfasına yönlendirilmeniz ve "Error generating Google authorization URL" hatası almanız, Google OAuth yapılandırmasıyla ilgili bir sorun olduğunu gösteriyor.

## Sorunun Nedeni

Render.com'da deployment yapıldığında, Google OAuth yapılandırması için gerekli environment variable'lar doğru şekilde ayarlanmamış. Özellikle:

1. `Authentication__Google__ClientId`
2. `Authentication__Google__ClientSecret`
3. `Authentication__Google__RedirectUri`
4. `Authentication__Google__Scopes`
5. `AppSettings__BaseUrl`

Bu değişkenler, Google OAuth işlemlerinin doğru çalışması için gereklidir.

## Çözüm

`render.yaml` dosyasında Google OAuth yapılandırmasını güncelledim:

```yaml
- key: Authentication__Google__ClientId
  value: "602631296713-kueq8lm3tsc3ms8ff87de29f4r7bjjjd.apps.googleusercontent.com"
- key: Authentication__Google__ClientSecret
  value: "GOCSPX-717SkyWhuTcanQmmpmlj1BCtK44"
- key: Authentication__Google__RedirectUri
  value: "https://jobanalyzerdashboard.onrender.com/api/auth/google/callback"
- key: Authentication__Google__Scopes
  value: "https://www.googleapis.com/auth/gmail.send https://www.googleapis.com/auth/userinfo.email https://www.googleapis.com/auth/userinfo.profile"
- key: AppSettings__BaseUrl
  value: "https://jobanalyzerdashboard.onrender.com"
```

## Google Cloud Console Yapılandırması

Ayrıca, Google Cloud Console'da OAuth yapılandırmanızı güncellemeniz gerekiyor:

1. [Google Cloud Console](https://console.cloud.google.com/) adresine gidin
2. OAuth 2.0 Client ID'nizi seçin
3. Authorized redirect URIs'ye şunu ekleyin:
   - `https://jobanalyzerdashboard.onrender.com/api/auth/google/callback`
4. "Save" butonuna tıklayın

## Deployment Adımları

Bu değişiklikleri GitHub reponuza push edin:

```bash
git add render.yaml google-oauth-fix.md
git commit -m "Google OAuth yapılandırması güncellendi"
git push
```

Ardından Render.com'da yeni bir deployment başlatın veya mevcut servisinizin Environment Variables bölümünden Google OAuth değişkenlerini manuel olarak güncelleyin.

## Sorun Giderme

Eğer hala sorun yaşıyorsanız, aşağıdaki adımları deneyin:

1. Render.com log'larını kontrol edin
2. Google OAuth yapılandırmasını doğrulayın
3. Google Cloud Console'da Authorized redirect URIs'nin doğru yapılandırıldığından emin olun
4. Tarayıcı konsolunda hata mesajlarını kontrol edin
