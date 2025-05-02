using System;
using System.Threading.Tasks;
using Npgsql;

namespace DeleteUserApp
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Veritabanı bağlantı bilgileri
            string connectionString = "Host=dpg-d034egali9vc73edf1og-a.oregon-postgres.render.com;Database=n8n_db_7rcs;Username=n8n_db_7rcs_user;Password=tTT6Uaxnv5FrGFKbqiLz1LAGgwSGQRIv;Port=5432;SSL Mode=Require;Trust Server Certificate=true;Timeout=30;Command Timeout=30;";
            string emailToDelete = "maliari4558@gmail.com";

            try
            {
                // Veritabanına bağlan
                using (var connection = new NpgsqlConnection(connectionString))
                {
                    await connection.OpenAsync();
                    Console.WriteLine("Veritabanına bağlandı.");

                    // Kullanıcıyı bul
                    int? userId = null;
                    int? profileId = null;

                    using (var command = new NpgsqlCommand("SELECT \"Id\", \"ProfileId\" FROM \"Users\" WHERE \"Email\" = @Email", connection))
                    {
                        command.Parameters.AddWithValue("Email", emailToDelete);

                        using (var reader = await command.ExecuteReaderAsync())
                        {
                            if (await reader.ReadAsync())
                            {
                                userId = reader.GetInt32(0);
                                profileId = !reader.IsDBNull(1) ? reader.GetInt32(1) : (int?)null;
                                Console.WriteLine($"Kullanıcı bulundu. ID: {userId}, ProfileId: {profileId}");
                            }
                            else
                            {
                                Console.WriteLine($"'{emailToDelete}' e-posta adresine sahip kullanıcı bulunamadı.");
                                return;
                            }
                        }
                    }

                    // İlişkili verileri sil
                    if (profileId.HasValue)
                    {
                        // OAuthTokens tablosundan ilgili kayıtları sil
                        using (var command = new NpgsqlCommand("DELETE FROM \"OAuthTokens\" WHERE \"ProfileId\" = @ProfileId", connection))
                        {
                            command.Parameters.AddWithValue("ProfileId", profileId.Value);
                            int affectedRows = await command.ExecuteNonQueryAsync();
                            Console.WriteLine($"{affectedRows} OAuth token kaydı silindi.");
                        }

                        // Resumes tablosundan ilgili kayıtları sil
                        using (var command = new NpgsqlCommand("DELETE FROM \"Resumes\" WHERE \"ProfileId\" = @ProfileId", connection))
                        {
                            command.Parameters.AddWithValue("ProfileId", profileId.Value);
                            int affectedRows = await command.ExecuteNonQueryAsync();
                            Console.WriteLine($"{affectedRows} özgeçmiş kaydı silindi.");
                        }

                        // Applications tablosundan ilgili kayıtları sil
                        using (var command = new NpgsqlCommand("DELETE FROM \"Applications\" WHERE \"ProfileId\" = @ProfileId", connection))
                        {
                            command.Parameters.AddWithValue("ProfileId", profileId.Value);
                            int affectedRows = await command.ExecuteNonQueryAsync();
                            Console.WriteLine($"{affectedRows} başvuru kaydı silindi.");
                        }

                        // Profiles tablosundan profili sil
                        using (var command = new NpgsqlCommand("DELETE FROM \"Profiles\" WHERE \"Id\" = @ProfileId", connection))
                        {
                            command.Parameters.AddWithValue("ProfileId", profileId.Value);
                            int affectedRows = await command.ExecuteNonQueryAsync();
                            Console.WriteLine($"{affectedRows} profil kaydı silindi.");
                        }
                    }

                    // Users tablosundan kullanıcıyı sil
                    using (var command = new NpgsqlCommand("DELETE FROM \"Users\" WHERE \"Id\" = @UserId", connection))
                    {
                        command.Parameters.AddWithValue("UserId", userId.Value);
                        int affectedRows = await command.ExecuteNonQueryAsync();
                        Console.WriteLine($"{affectedRows} kullanıcı kaydı silindi.");
                    }

                    Console.WriteLine($"'{emailToDelete}' e-posta adresine sahip kullanıcı ve ilişkili tüm veriler başarıyla silindi.");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Hata oluştu: {ex.Message}");
                if (ex.InnerException != null)
                {
                    Console.WriteLine($"İç hata: {ex.InnerException.Message}");
                }
            }

            Console.WriteLine("İşlem tamamlandı. Çıkmak için bir tuşa basın...");
            Console.ReadKey();
        }
    }
}
