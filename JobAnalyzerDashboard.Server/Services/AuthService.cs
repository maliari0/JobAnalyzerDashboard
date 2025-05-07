using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using JobAnalyzerDashboard.Server.Data;
using JobAnalyzerDashboard.Server.Models;
using JobAnalyzerDashboard.Server.Models.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;

namespace JobAnalyzerDashboard.Server.Services
{
    public class AuthService
    {
        private readonly ApplicationDbContext _context;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AuthService> _logger;
        private readonly EmailService _emailService;

        public AuthService(
            ApplicationDbContext context,
            IConfiguration configuration,
            ILogger<AuthService> logger,
            EmailService emailService)
        {
            _context = context;
            _configuration = configuration;
            _logger = logger;
            _emailService = emailService;
        }

        public async Task<AuthResponseDTO> RegisterAsync(RegisterDTO model)
        {
            try
            {
                // E-posta adresi veya kullanıcı adı zaten kullanılıyor mu kontrol et
                var existingUser = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == model.Email || u.Username == model.Username);

                if (existingUser != null)
                {
                    if (existingUser.Email == model.Email)
                    {
                        return new AuthResponseDTO
                        {
                            Success = false,
                            Message = "Bu e-posta adresi zaten kullanılıyor."
                        };
                    }
                    else
                    {
                        return new AuthResponseDTO
                        {
                            Success = false,
                            Message = "Bu kullanıcı adı zaten kullanılıyor."
                        };
                    }
                }

                // Şifre hash'i oluştur
                CreatePasswordHash(model.Password, out string passwordHash, out string passwordSalt);

                // Yeni kullanıcı oluştur
                var user = new User
                {
                    Email = model.Email,
                    Username = model.Username,
                    PasswordHash = passwordHash,
                    PasswordSalt = passwordSalt,
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    CreatedAt = DateTime.UtcNow,
                    EmailConfirmed = false,
                    EmailConfirmationToken = GenerateRandomToken(),
                    EmailConfirmationTokenExpiry = DateTime.UtcNow.AddDays(1)
                };

                // Kullanıcıyı veritabanına ekle
                await _context.Users.AddAsync(user);
                await _context.SaveChangesAsync();

                // Yeni profil oluştur ve kullanıcıyla ilişkilendir
                var profile = new Profile
                {
                    FullName = $"{model.FirstName} {model.LastName}".Trim(),
                    Email = model.Email,
                    // Diğer profil alanları varsayılan değerlerle doldurulacak
                };

                await _context.Profiles.AddAsync(profile);
                await _context.SaveChangesAsync();

                // Kullanıcı ve profil ilişkisini güncelle
                user.ProfileId = profile.Id;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // E-posta doğrulama e-postası gönder
                await SendEmailConfirmationAsync(user);

                // JWT token oluştur
                var token = GenerateJwtToken(user);

                return new AuthResponseDTO
                {
                    Success = true,
                    Message = "Kayıt başarılı. Lütfen e-posta adresinizi doğrulayın.",
                    Token = token,
                    User = new UserDTO
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Username = user.Username,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = user.Role,
                        EmailConfirmed = user.EmailConfirmed,
                        ProfileId = user.ProfileId
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı kaydı sırasında bir hata oluştu");
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Kayıt sırasında bir hata oluştu. Lütfen daha sonra tekrar deneyin."
                };
            }
        }

        public async Task<AuthResponseDTO> LoginAsync(LoginDTO model)
        {
            try
            {
                // Kullanıcıyı e-posta adresine göre bul
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user == null)
                {
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Geçersiz e-posta adresi veya şifre."
                    };
                }

                // Şifre doğrulaması
                if (!VerifyPasswordHash(model.Password, user.PasswordHash, user.PasswordSalt))
                {
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Geçersiz e-posta adresi veya şifre."
                    };
                }

                // E-posta doğrulaması kontrolü (admin kullanıcısı için atla)
                if (!user.EmailConfirmed && user.Email != "admin@admin.com")
                {
                    return new AuthResponseDTO
                    {
                        Success = false,
                        Message = "Lütfen önce e-posta adresinizi doğrulayın."
                    };
                }

                // Admin kullanıcısı için e-posta doğrulamasını otomatik olarak yap
                if (user.Email == "admin@admin.com" && !user.EmailConfirmed)
                {
                    user.EmailConfirmed = true;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Admin kullanıcısı için e-posta doğrulaması otomatik olarak yapıldı");
                }

                // Son giriş tarihini güncelle
                user.LastLoginAt = DateTime.UtcNow;
                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // JWT token oluştur
                var token = GenerateJwtToken(user);

                return new AuthResponseDTO
                {
                    Success = true,
                    Message = "Giriş başarılı.",
                    Token = token,
                    User = new UserDTO
                    {
                        Id = user.Id,
                        Email = user.Email,
                        Username = user.Username,
                        FirstName = user.FirstName,
                        LastName = user.LastName,
                        Role = user.Role,
                        EmailConfirmed = user.EmailConfirmed,
                        ProfileId = user.ProfileId
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı girişi sırasında bir hata oluştu");
                return new AuthResponseDTO
                {
                    Success = false,
                    Message = "Giriş sırasında bir hata oluştu. Lütfen daha sonra tekrar deneyin."
                };
            }
        }

        public async Task<bool> ConfirmEmailAsync(EmailConfirmationDTO model)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u =>
                    u.Email == model.Email &&
                    u.EmailConfirmationToken == model.Token &&
                    u.EmailConfirmationTokenExpiry > DateTime.UtcNow);

                if (user == null)
                {
                    return false;
                }

                user.EmailConfirmed = true;
                user.EmailConfirmationToken = null;
                user.EmailConfirmationTokenExpiry = null;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta doğrulama sırasında bir hata oluştu");
                return false;
            }
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordDTO model)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

                if (user == null)
                {
                    // Güvenlik nedeniyle kullanıcı bulunamasa bile başarılı döndür
                    return true;
                }

                // Şifre sıfırlama token'ı oluştur
                user.PasswordResetToken = GenerateRandomToken();
                user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1);

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                // Şifre sıfırlama e-postası gönder
                await SendPasswordResetEmailAsync(user);

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre sıfırlama isteği sırasında bir hata oluştu");
                return false;
            }
        }

        public async Task<bool> ResetPasswordAsync(ResetPasswordDTO model)
        {
            try
            {
                var user = await _context.Users.FirstOrDefaultAsync(u =>
                    u.Email == model.Email &&
                    u.PasswordResetToken == model.Token &&
                    u.PasswordResetTokenExpiry > DateTime.UtcNow);

                if (user == null)
                {
                    return false;
                }

                // Yeni şifre hash'i oluştur
                CreatePasswordHash(model.Password, out string passwordHash, out string passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;
                user.PasswordResetToken = null;
                user.PasswordResetTokenExpiry = null;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre sıfırlama sırasında bir hata oluştu");
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDTO model)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return false;
                }

                // Mevcut şifre doğrulaması
                if (!VerifyPasswordHash(model.CurrentPassword, user.PasswordHash, user.PasswordSalt))
                {
                    return false;
                }

                // Yeni şifre hash'i oluştur
                CreatePasswordHash(model.NewPassword, out string passwordHash, out string passwordSalt);

                user.PasswordHash = passwordHash;
                user.PasswordSalt = passwordSalt;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre değiştirme sırasında bir hata oluştu");
                return false;
            }
        }

        public async Task<UserDTO?> GetUserByIdAsync(int userId)
        {
            try
            {
                var user = await _context.Users.FindAsync(userId);

                if (user == null)
                {
                    return null;
                }

                return new UserDTO
                {
                    Id = user.Id,
                    Email = user.Email,
                    Username = user.Username,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Role = user.Role,
                    EmailConfirmed = user.EmailConfirmed,
                    ProfileId = user.ProfileId
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kullanıcı bilgileri alınırken bir hata oluştu");
                return null;
            }
        }

        #region Helper Methods

        private void CreatePasswordHash(string password, out string passwordHash, out string passwordSalt)
        {
            using (var hmac = new HMACSHA512())
            {
                passwordSalt = Convert.ToBase64String(hmac.Key);
                passwordHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
            }
        }

        private bool VerifyPasswordHash(string password, string storedHash, string storedSalt)
        {
            var saltBytes = Convert.FromBase64String(storedSalt);
            using (var hmac = new HMACSHA512(saltBytes))
            {
                var computedHash = Convert.ToBase64String(hmac.ComputeHash(Encoding.UTF8.GetBytes(password)));
                return computedHash == storedHash;
            }
        }

        private string GenerateJwtToken(User user)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JWT Secret is not configured");

            // Ensure the key is at least 32 bytes (256 bits) long
            if (secretKey.Length < 32)
            {
                secretKey = secretKey.PadRight(32, 'X');
                _logger.LogWarning("JWT Secret key was too short and has been padded to 32 characters");
            }

            var key = Encoding.ASCII.GetBytes(secretKey);
            _logger.LogInformation("Generating JWT token for user: {UserId}, {Username}, {Role}", user.Id, user.Username, user.Role);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Role, user.Role),
                new Claim("ProfileId", user.ProfileId?.ToString() ?? "0")
            };

            _logger.LogInformation("JWT claims: {Claims}", string.Join(", ", claims.Select(c => $"{c.Type}={c.Value}")));

            var tokenHandler = new JwtSecurityTokenHandler();
            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(int.Parse(jwtSettings["ExpirationInMinutes"] ?? "60")),
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(key), SecurityAlgorithms.HmacSha256Signature),
                Issuer = jwtSettings["Issuer"],
                Audience = jwtSettings["Audience"]
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            var tokenString = tokenHandler.WriteToken(token);

            _logger.LogInformation("JWT token generated successfully");

            return tokenString;
        }

        private string GenerateRandomToken()
        {
            var randomBytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomBytes);
            }
            return Convert.ToBase64String(randomBytes);
        }

        private async Task SendEmailConfirmationAsync(User user)
        {
            try
            {
                if (string.IsNullOrEmpty(user.EmailConfirmationToken))
                {
                    _logger.LogWarning("E-posta doğrulama token'ı boş olduğu için e-posta gönderilemiyor");
                    return;
                }

                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:52545";
                var confirmationLink = $"{baseUrl}/confirm-email?token={Uri.EscapeDataString(user.EmailConfirmationToken)}&email={Uri.EscapeDataString(user.Email)}";

                _logger.LogInformation("E-posta doğrulama bağlantısı oluşturuldu: {ConfirmationLink}", confirmationLink);

                var subject = "E-posta Adresinizi Doğrulayın - Job Analyzer Dashboard";
                var body = $@"
                    <h2>Merhaba {user.FirstName} {user.LastName},</h2>
                    <p>Job Analyzer Dashboard'a kaydolduğunuz için teşekkür ederiz.</p>
                    <p>Lütfen aşağıdaki bağlantıya tıklayarak e-posta adresinizi doğrulayın:</p>
                    <p><a href='{confirmationLink}'>E-posta Adresimi Doğrula</a></p>
                    <p>Bu bağlantı 24 saat boyunca geçerlidir.</p>
                    <p>Eğer bu işlemi siz yapmadıysanız, lütfen bu e-postayı dikkate almayın.</p>
                    <p>Saygılarımızla,<br>Job Analyzer Dashboard Ekibi</p>
                ";

                _logger.LogInformation("E-posta doğrulama e-postası gönderiliyor: {Email}", user.Email);
                var result = await _emailService.SendEmailAsync(user.Email, subject, body);

                if (result)
                {
                    _logger.LogInformation("E-posta doğrulama e-postası başarıyla gönderildi: {Email}", user.Email);
                }
                else
                {
                    _logger.LogWarning("E-posta doğrulama e-postası gönderilemedi: {Email}", user.Email);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta doğrulama e-postası gönderilirken bir hata oluştu: {ErrorMessage}", ex.Message);

                // İç içe istisnalar varsa onları da logla
                var innerException = ex.InnerException;
                while (innerException != null)
                {
                    _logger.LogError(innerException, "İç istisna: {ErrorMessage}", innerException.Message);
                    innerException = innerException.InnerException;
                }
            }
        }

        private async Task SendPasswordResetEmailAsync(User user)
        {
            try
            {
                if (string.IsNullOrEmpty(user.PasswordResetToken))
                {
                    return;
                }

                var baseUrl = _configuration["AppSettings:BaseUrl"] ?? "https://localhost:52545";
                var resetLink = $"{baseUrl}/reset-password?token={Uri.EscapeDataString(user.PasswordResetToken)}&email={Uri.EscapeDataString(user.Email)}";

                var subject = "Şifre Sıfırlama - Job Analyzer Dashboard";
                var body = $@"
                    <h2>Merhaba {user.FirstName} {user.LastName},</h2>
                    <p>Şifrenizi sıfırlamak için bir istek aldık.</p>
                    <p>Lütfen aşağıdaki bağlantıya tıklayarak şifrenizi sıfırlayın:</p>
                    <p><a href='{resetLink}'>Şifremi Sıfırla</a></p>
                    <p>Bu bağlantı 1 saat boyunca geçerlidir.</p>
                    <p>Eğer bu işlemi siz yapmadıysanız, lütfen bu e-postayı dikkate almayın.</p>
                    <p>Saygılarımızla,<br>Job Analyzer Dashboard Ekibi</p>
                ";

                await _emailService.SendEmailAsync(user.Email, subject, body);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre sıfırlama e-postası gönderilirken bir hata oluştu");
            }
        }

        #endregion
    }
}
