# JWT Secret Key Sorunu Çözümü

Render.com'da JWT token oluşturulurken bir hata alınıyor. Hata mesajı:

```
IDX10720: Unable to create KeyedHashAlgorithm for algorithm 'http://www.w3.org/2001/04/xmldsig-more#hmac-sha256', the key size must be greater than: '256' bits, key has '200' bits.
```

Bu hata, JWT imzalamak için kullanılan gizli anahtarın (secret key) yeterince uzun olmadığını gösteriyor. JWT için kullanılan anahtar en az 256 bit (32 byte) uzunluğunda olmalıdır.

## Çözüm

İki değişiklik yapıldı:

1. `render.yaml` dosyasında JWT Secret değeri güncellendi:
   - Rastgele oluşturulan değer yerine sabit ve yeterince uzun bir değer kullanıldı

2. `AuthService.cs` dosyasında `GenerateJwtToken` metodu güncellendi:
   - Secret key'in uzunluğunu kontrol eden ve gerekirse uzatan kod eklendi
   - Bu sayede kısa bir secret key kullanılsa bile otomatik olarak uzatılacak

## Render.com'da Güncelleme

Render.com'da environment variable'ları güncellemek için:

1. Render Dashboard'da servisinize gidin
2. "Environment" sekmesine tıklayın
3. "JwtSettings__Secret" değişkenini bulun
4. Değerini en az 32 karakter uzunluğunda bir değerle güncelleyin:

```
YourSuperSecretKeyThatIsAtLeast32CharactersLongForSecurityReasons123456789
```

5. "Save Changes" butonuna tıklayın
6. Servisinizi yeniden başlatın

## Güvenlik Notu

Production ortamında JWT Secret değerini güçlü ve rastgele bir değer olarak ayarlamanız önemlidir. Bu değer, JWT token'larının güvenliğini sağlar ve yetkisiz erişimleri önler.

Örnek güçlü bir secret key oluşturmak için:

```csharp
using System;
using System.Security.Cryptography;

public class SecretGenerator
{
    public static string GenerateSecretKey(int length = 64)
    {
        var randomBytes = new byte[length];
        using (var rng = RandomNumberGenerator.Create())
        {
            rng.GetBytes(randomBytes);
        }
        return Convert.ToBase64String(randomBytes);
    }
}
```
