-- Admin kullanıcısını ve ilişkili verileri silme
DO $$
DECLARE
    user_id INT;
    profile_id INT;
BEGIN
    -- Admin kullanıcısını bul
    SELECT "Id", "ProfileId" INTO user_id, profile_id
    FROM "Users"
    WHERE "Username" = 'admin';
    
    IF user_id IS NULL THEN
        RAISE NOTICE 'Admin kullanıcısı bulunamadı.';
        RETURN;
    END IF;
    
    RAISE NOTICE 'Admin kullanıcısı bulundu. ID: %, ProfileId: %', user_id, profile_id;
    
    -- İlişkili verileri sil
    IF profile_id IS NOT NULL THEN
        -- OAuthTokens tablosundan ilgili kayıtları sil
        DELETE FROM "OAuthTokens" WHERE "ProfileId" = profile_id;
        RAISE NOTICE 'OAuth token kayıtları silindi.';
        
        -- Resumes tablosundan ilgili kayıtları sil
        DELETE FROM "Resumes" WHERE "ProfileId" = profile_id;
        RAISE NOTICE 'Özgeçmiş kayıtları silindi.';
        
        -- Profiles tablosundan profili sil
        DELETE FROM "Profiles" WHERE "Id" = profile_id;
        RAISE NOTICE 'Profil kaydı silindi.';
    END IF;
    
    -- Users tablosundan kullanıcıyı sil
    DELETE FROM "Users" WHERE "Id" = user_id;
    RAISE NOTICE 'Kullanıcı kaydı silindi.';
    
    RAISE NOTICE 'Admin kullanıcısı ve ilişkili tüm veriler başarıyla silindi.';
END $$;
