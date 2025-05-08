using JobAnalyzerDashboard.Server.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace JobAnalyzerDashboard.Server.Data
{
    public static class DbInitializer
    {
        public static async Task Initialize(ApplicationDbContext context)
        {
            // Veritabanını oluştur (eğer yoksa)
            await context.Database.EnsureCreatedAsync();

            if (!await context.Users.AnyAsync(u => u.Username == "admin"))
            {
                CreatePasswordHash("admin123", out string passwordHash, out string passwordSalt);

                var adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@admin.com",
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    FirstName = "Admin",
                    LastName = "User",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    EmailConfirmed = true, // E-posta doğrulaması gerekmez
                    Role = "Admin"
                };

                await context.Users.AddAsync(adminUser);
                await context.SaveChangesAsync();

                var adminProfile = new Profile
                {
                    FullName = "Admin User",
                    Email = "admin@jobanalyzerdashboard.com"
                };

                await context.Profiles.AddAsync(adminProfile);
                await context.SaveChangesAsync();

                adminUser.ProfileId = adminProfile.Id;
                context.Users.Update(adminUser);
                await context.SaveChangesAsync();
            }
        }

        private static void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = Convert.ToBase64String(hmac.Key);
                passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }
        }
    }
}
