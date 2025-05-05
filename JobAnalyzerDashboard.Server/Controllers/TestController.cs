using Microsoft.AspNetCore.Mvc;
using JobAnalyzerDashboard.Server.Data;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;
using System.Linq;
using System;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Logging;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/test")]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<TestController> _logger;

        public TestController(ApplicationDbContext context, ILogger<TestController> logger)
        {
            _context = context;
            _logger = logger;
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

        [HttpGet("check-admin")]
        public async Task<IActionResult> CheckAdmin()
        {
            try
            {
                _logger.LogInformation("Checking admin user");

                // Admin kullanıcısını kontrol et
                var adminUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == "admin@admin.com" && u.Role == "Admin");

                if (adminUser != null)
                {
                    _logger.LogInformation("Admin user exists: ID={Id}, Username={Username}, Role={Role}, EmailConfirmed={EmailConfirmed}, IsActive={IsActive}",
                        adminUser.Id, adminUser.Username, adminUser.Role, adminUser.EmailConfirmed, adminUser.IsActive);

                    return Ok(new {
                        exists = true,
                        id = adminUser.Id,
                        username = adminUser.Username,
                        email = adminUser.Email,
                        role = adminUser.Role,
                        emailConfirmed = adminUser.EmailConfirmed,
                        isActive = adminUser.IsActive
                    });
                }

                _logger.LogWarning("Admin user does not exist");
                return Ok(new { exists = false });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking admin user");
                return StatusCode(500, new { message = $"Error checking admin user: {ex.Message}" });
            }
        }

        [HttpGet("check-token")]
        public IActionResult CheckToken()
        {
            try
            {
                // Get the token from the Authorization header
                var token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

                if (string.IsNullOrEmpty(token))
                {
                    _logger.LogWarning("No token found in request");
                    return Ok(new { valid = false, message = "No token found in request" });
                }

                _logger.LogInformation("Token found in request: {Token}", token);

                // Parse the token
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jsonToken = handler.ReadToken(token) as System.IdentityModel.Tokens.Jwt.JwtSecurityToken;

                if (jsonToken == null)
                {
                    _logger.LogWarning("Invalid token format");
                    return Ok(new { valid = false, message = "Invalid token format" });
                }

                // Extract claims
                var claims = jsonToken.Claims.Select(c => new { type = c.Type, value = c.Value }).ToList();

                _logger.LogInformation("Token claims: {Claims}", string.Join(", ", claims.Select(c => $"{c.type}={c.value}")));

                return Ok(new {
                    valid = true,
                    claims = claims,
                    expires = jsonToken.ValidTo
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking token");
                return StatusCode(500, new { message = $"Error checking token: {ex.Message}" });
            }
        }
    }
}
