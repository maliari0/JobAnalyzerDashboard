# Veritabanı Bağlantı Sorunu Çözümü

Render.com'da veritabanı bağlantısı için External Database URL'yi kullanmanız gerekiyor. Bu URL, internet üzerinden erişilebilir olan URL'dir.

## Veritabanı Bağlantı Dizesini Güncelleme

Render.com'da environment variable'ları güncellemek için:

1. Render Dashboard'da servisinize gidin
2. "Environment" sekmesine tıklayın
3. "ConnectionStrings__DefaultConnection" değişkenini bulun
4. Değerini aşağıdaki gibi güncelleyin:

```
postgresql://n8n_db_7rcs_user:tTT6Uaxnv5FrGFKbqiLz1LAGgwSGQRIv@dpg-d034egali9vc73edf1og-a.oregon-postgres.render.com/n8n_db_7rcs
```

5. "Save Changes" butonuna tıklayın
6. Servisinizi yeniden başlatın

## Npgsql Bağlantı Dizesi Formatı

Eğer yukarıdaki URL formatı çalışmazsa, Npgsql'in beklediği standart bağlantı dizesi formatını kullanabilirsiniz:

```
Host=dpg-d034egali9vc73edf1og-a.oregon-postgres.render.com;Database=n8n_db_7rcs;Username=n8n_db_7rcs_user;Password=tTT6Uaxnv5FrGFKbqiLz1LAGgwSGQRIv;Port=5432;SSL Mode=Require;Trust Server Certificate=true;
```

## Veritabanı Şeması Kontrolü

Eğer veritabanı bağlantısı başarılı olmasına rağmen veriler görünmüyorsa, veritabanı şemasını kontrol edin:

1. Veritabanınızda gerekli tablolar oluşturulmuş mu?
2. Tablolarda veri var mı?

Eğer tablolar oluşturulmamışsa, Entity Framework migrations'ı çalıştırmanız gerekebilir:

```bash
dotnet ef database update
```

## Veritabanı Erişim İzinleri

Render.com'daki PostgreSQL veritabanınızın erişim izinlerini kontrol edin:

1. Render Dashboard'da PostgreSQL servisinize gidin
2. "Access Control" sekmesine tıklayın
3. Web servisinizin IP adresinin izin verilen IP'ler listesinde olduğundan emin olun

## Sorun Giderme

Eğer hala sorun yaşıyorsanız, aşağıdaki adımları deneyin:

1. Render.com log'larını kontrol edin
2. Veritabanı bağlantı dizesini doğrulayın
3. Veritabanı kullanıcısının gerekli izinlere sahip olduğundan emin olun
4. Veritabanı sunucusunun çalıştığını ve erişilebilir olduğunu kontrol edin
