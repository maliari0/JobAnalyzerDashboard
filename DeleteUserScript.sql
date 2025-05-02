-- Kullanıcı ID'sini ve Profil ID'sini bul
DO $$
DECLARE
    user_id INT;
    profile_id INT;
    email_to_delete VARCHAR := 'maliari4558@gmail.com';
BEGIN
    -- Kullanıcıyı bul
    SELECT "Id", "ProfileId" INTO user_id, profile_id
    FROM "Users"
    WHERE "Email" = email_to_delete;
    
    IF user_id IS NULL THEN
        RAISE NOTICE 'Kullanıcı bulunamadı: %', email_to_delete;
        RETURN;
    END IF;
    
    RAISE NOTICE 'Kullanıcı bulundu. ID: %, ProfileId: %', user_id, profile_id;
    
    -- İlişkili verileri sil
    IF profile_id IS NOT NULL THEN
        -- OAuthTokens tablosundan ilgili kayıtları sil
        DELETE FROM "OAuthTokens" WHERE "ProfileId" = profile_id;
        RAISE NOTICE 'OAuth token kayıtları silindi.';
        
        -- Resumes tablosundan ilgili kayıtları sil
        DELETE FROM "Resumes" WHERE "ProfileId" = profile_id;
        RAISE NOTICE 'Özgeçmiş kayıtları silindi.';
        
        -- Applications tablosundan ilgili kayıtları sil
        DELETE FROM "Applications" WHERE "ProfileId" = profile_id;
        RAISE NOTICE 'Başvuru kayıtları silindi.';
        
        -- Profiles tablosundan profili sil
        DELETE FROM "Profiles" WHERE "Id" = profile_id;
        RAISE NOTICE 'Profil kaydı silindi.';
    END IF;
    
    -- Users tablosundan kullanıcıyı sil
    DELETE FROM "Users" WHERE "Id" = user_id;
    RAISE NOTICE 'Kullanıcı kaydı silindi.';
    
    RAISE NOTICE 'Kullanıcı ve ilişkili tüm veriler başarıyla silindi: %', email_to_delete;
END $$;
