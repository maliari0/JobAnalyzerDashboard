using Microsoft.AspNetCore.Mvc;
using JobAnalyzerDashboard.Server.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Security.Cryptography;
using System.Text;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("create-admin")]
        public async Task<IActionResult> CreateAdmin()
        {
            try
            {
                // Admin kullanıcısını kontrol et
                var existingAdmin = await _context.Users.FirstOrDefaultAsync(u => u.Username == "admin");
                
                if (existingAdmin != null)
                {
                    // Admin kullanıcısını sil
                    _context.Users.Remove(existingAdmin);
                    await _context.SaveChangesAsync();
                }

                // Yeni admin kullanıcısı oluştur
                var adminUser = new Models.User
                {
                    Username = "admin",
                    Email = "admin@admin.com",
                    FirstName = "Admin",
                    LastName = "User",
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    EmailConfirmed = true,
                    Role = "Admin"
                };

                // Şifre hash'i oluştur
                CreatePasswordHash("admin123", out string passwordHash, out string passwordSalt);
                adminUser.PasswordHash = passwordHash;
                adminUser.PasswordSalt = passwordSalt;

                // Admin kullanıcısını veritabanına ekle
                await _context.Users.AddAsync(adminUser);
                await _context.SaveChangesAsync();

                // Admin kullanıcısı için profil oluştur
                var adminProfile = new Models.Profile
                {
                    FullName = "Admin User",
                    Email = "admin@admin.com"
                };

                await _context.Profiles.AddAsync(adminProfile);
                await _context.SaveChangesAsync();

                // Admin kullanıcısı ve profil ilişkisini güncelle
                adminUser.ProfileId = adminProfile.Id;
                _context.Users.Update(adminUser);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Admin kullanıcısı başarıyla oluşturuldu." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = $"Hata: {ex.Message}" });
            }
        }

        private void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = Convert.ToBase64String(hmac.Key);
                passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }
        }
    }
}
