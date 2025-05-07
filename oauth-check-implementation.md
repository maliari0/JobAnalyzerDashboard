# OAuth Bağlantı Kontrolü ve Yönlendirme Eklenmesi

Kullanıcının OAuth bağlantısı olmadan başvuru yapmasını önlemek ve gerektiğinde OAuth bağlantısı için yönlendirmek için yapılan değişiklikler.

## Yapılan Değişiklikler

### 1. Manuel Başvuru İçin OAuth Kontrolü

`applyToJob()` metoduna OAuth kontrolü eklendi:

```typescript
applyToJob(): void {
  if (!this.job || this.applyingInProgress) {
    return;
  }

  // Önce OAuth durumunu kontrol et
  this.profileService.getOAuthStatus().subscribe({
    next: (tokens) => {
      // Google OAuth token'ı var mı kontrol et
      const googleToken = tokens.find(t => t.provider === 'Google');

      if (!googleToken) {
        // OAuth token'ı yoksa, kullanıcıya bildir ve yetkilendirme sayfasına yönlendir
        if (confirm('Manuel başvuru yapmak için Google hesabınızı yetkilendirmeniz gerekiyor. Yetkilendirme sayfasına yönlendirilmek istiyor musunuz?')) {
          const currentUser = this.authService.currentUserValue;
          this.profileService.authorizeGoogle(currentUser?.id);
        }
        return;
      }

      // OAuth token'ı varsa devam et
      this.applyingInProgress = true;
      
      // Başvuru işlemine devam et...
```

### 2. Otomatik Başvuru İçin OAuth Kontrolü

`autoApply()` metoduna OAuth kontrolü eklendi:

```typescript
autoApply(): void {
  if (!this.job || this.applyingInProgress) {
    return;
  }

  // Önce OAuth durumunu kontrol et
  this.profileService.getOAuthStatus().subscribe({
    next: (tokens) => {
      // Google OAuth token'ı var mı kontrol et
      const googleToken = tokens.find(t => t.provider === 'Google');

      if (!googleToken) {
        // OAuth token'ı yoksa, kullanıcıya bildir ve yetkilendirme sayfasına yönlendir
        if (confirm('Otomatik başvuru yapmak için Google hesabınızı yetkilendirmeniz gerekiyor. Yetkilendirme sayfasına yönlendirilmek istiyor musunuz?')) {
          const currentUser = this.authService.currentUserValue;
          this.profileService.authorizeGoogle(currentUser?.id);
        }
        return;
      }

      // OAuth token'ı varsa devam et
      this.applyingInProgress = true;
      
      // Otomatik başvuru işlemine devam et...
```

## Nasıl Çalışır?

1. Kullanıcı "Manuel Başvur" veya "Otomatik Oluştur" butonuna tıkladığında, önce OAuth durumu kontrol edilir.
2. Eğer kullanıcının Google OAuth token'ı yoksa, bir onay mesajı gösterilir.
3. Kullanıcı onaylarsa, Google OAuth yetkilendirme sayfasına yönlendirilir.
4. Yetkilendirme tamamlandıktan sonra, kullanıcı profil sayfasına yönlendirilir.
5. Kullanıcı tekrar iş ilanı sayfasına gidip başvuru yapabilir.

## Güvenlik ve Kullanıcı Deneyimi

Bu değişiklikler, kullanıcıların başvuru yapmadan önce Google hesaplarını yetkilendirmelerini sağlayarak:

1. Başvuru sürecinde hata oluşmasını önler
2. Kullanıcıya neden yetkilendirme gerektiğini açıklar
3. Yetkilendirme işlemi için kolay bir yol sunar
4. Kullanıcı deneyimini iyileştirir
