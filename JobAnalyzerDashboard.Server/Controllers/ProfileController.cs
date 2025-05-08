using Microsoft.AspNetCore.Mvc;
using JobAnalyzerDashboard.Server.Models;
using JobAnalyzerDashboard.Server.Repositories;
using JobAnalyzerDashboard.Server.Data;
using JobAnalyzerDashboard.Server.Services;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProfileController : ControllerBase
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly IProfileRepository _profileRepository;
        private readonly IWebHostEnvironment _environment;
        private readonly ApplicationDbContext _context;
        private readonly OAuthService _oauthService;

        public ProfileController(
            ILogger<ProfileController> logger,
            IProfileRepository profileRepository,
            IWebHostEnvironment environment,
            ApplicationDbContext context,
            OAuthService oauthService)
        {
            _logger = logger;
            _profileRepository = profileRepository;
            _environment = environment;
            _context = context;
            _oauthService = oauthService;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Kullanıcı ID'sini al
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(new { message = "Oturum açmanız gerekiyor" });
                }

                // Kullanıcıyı bul
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                // Kullanıcının profilini bul
                var profile = user.ProfileId.HasValue
                    ? await _profileRepository.GetProfileWithResumesAsync(user.ProfileId.Value)
                    : null;

                // Profil bulunamadıysa, yeni bir profil oluştur
                if (profile == null)
                {
                    _logger.LogInformation("Kullanıcı {UserId} için profil bulunamadı, yeni bir profil oluşturuluyor", userId);

                    profile = new Profile
                    {
                        FullName = $"{user.FirstName} {user.LastName}".Trim(),
                        Email = user.Email,
                        Phone = "",
                        LinkedInUrl = "",
                        GithubUrl = "",
                        PortfolioUrl = "",
                        Skills = "",
                        Education = "",
                        Experience = "",
                        PreferredJobTypes = "",
                        PreferredLocations = "",
                        MinimumSalary = "",
                        ResumeFilePath = "",
                        NotionPageId = "",
                        TelegramChatId = "",
                        PreferredModel = "",
                        TechnologyStack = "",
                        Position = "",
                        PreferredCategories = "[]",
                        MinQualityScore = 3,
                        AutoApplyEnabled = false,
                        UserId = userId
                    };

                    await _profileRepository.AddAsync(profile);
                    await _profileRepository.SaveChangesAsync();

                    // Kullanıcı ve profil ilişkisini güncelle
                    user.ProfileId = profile.Id;
                    _context.Users.Update(user);
                    await _context.SaveChangesAsync();

                    // Yeni oluşturulan profili tekrar yükle (Resumes koleksiyonu ile birlikte)
                    profile = await _profileRepository.GetProfileWithResumesAsync(profile.Id);
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profil alınırken hata oluştu");
                return StatusCode(500, new { message = "Profil alınırken bir hata oluştu" });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }

        [HttpPut]
        public async Task<IActionResult> Update(Profile profile)
        {
            try
            {
                if (profile == null)
                {
                    return BadRequest(new { message = "Geçersiz profil verisi" });
                }

                // Kullanıcı ID'sini al
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(new { message = "Oturum açmanız gerekiyor" });
                }

                // Kullanıcıyı bul
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                // Kullanıcının profilini bul
                if (!user.ProfileId.HasValue)
                {
                    return NotFound(new { message = "Kullanıcı profili bulunamadı" });
                }

                var existingProfile = await _profileRepository.GetByIdAsync(user.ProfileId.Value);
                if (existingProfile == null)
                {
                    return NotFound(new { message = "Kullanıcı profili bulunamadı" });
                }

                // Profil ID'sinin kullanıcıya ait olduğunu doğrula
                if (existingProfile.UserId != userId)
                {
                    return Unauthorized(new { message = "Bu profili düzenleme yetkiniz yok" });
                }

                // Yeni bir profil nesnesi oluştur
                var newProfile = new Profile
                {
                    Id = existingProfile.Id,
                    FullName = profile.FullName ?? "",
                    Email = profile.Email ?? "",
                    Phone = profile.Phone ?? "",
                    LinkedInUrl = profile.LinkedInUrl ?? "",
                    GithubUrl = profile.GithubUrl ?? "",
                    PortfolioUrl = profile.PortfolioUrl ?? "",
                    Skills = profile.Skills ?? "",
                    Education = profile.Education ?? "",
                    Experience = profile.Experience ?? "",
                    PreferredJobTypes = profile.PreferredJobTypes ?? "",
                    PreferredLocations = profile.PreferredLocations ?? "",
                    MinimumSalary = profile.MinimumSalary ?? "",
                    ResumeFilePath = profile.ResumeFilePath ?? "",
                    NotionPageId = profile.NotionPageId ?? "",
                    TelegramChatId = profile.TelegramChatId ?? "",
                    PreferredModel = profile.PreferredModel ?? "",
                    TechnologyStack = profile.TechnologyStack ?? "",
                    Position = profile.Position ?? "",
                    PreferredCategories = profile.PreferredCategories ?? "[]",
                    MinQualityScore = profile.MinQualityScore,
                    AutoApplyEnabled = profile.AutoApplyEnabled,
                    UserId = userId
                };

                // DbContext'i temizle
                _context.ChangeTracker.Clear();

                // Profili güncelle
                _context.Profiles.Update(newProfile);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Profil güncellendi: {ProfileId}", newProfile.Id);

                // Güncellenmiş profili getir
                var updatedProfile = await _profileRepository.GetProfileWithResumesAsync(newProfile.Id);
                return Ok(updatedProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profil güncellenirken hata oluştu");
                return StatusCode(500, new { message = "Profil güncellenirken bir hata oluştu" });
            }
        }

        [HttpGet("resumes")]
        public async Task<IActionResult> GetResumes()
        {
            try
            {
                // Kullanıcı ID'sini al
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(new { message = "Oturum açmanız gerekiyor" });
                }

                // Kullanıcıyı bul
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                // Kullanıcının profil ID'sini kontrol et
                if (!user.ProfileId.HasValue)
                {
                    return NotFound(new { message = "Kullanıcı profili bulunamadı" });
                }

                // Kullanıcının özgeçmişlerini getir
                var resumes = await _profileRepository.GetResumesAsync(user.ProfileId.Value);
                return Ok(resumes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Özgeçmişler alınırken hata oluştu");
                return StatusCode(500, new { message = "Özgeçmişler alınırken bir hata oluştu" });
            }
        }

        [HttpPost("resumes")]
        public async Task<IActionResult> UploadResume([FromForm] ResumeUploadModel model)
        {
            try
            {
                // Kullanıcı ID'sini al
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(new { message = "Oturum açmanız gerekiyor" });
                }

                // Kullanıcıyı bul
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                // Kullanıcının profil ID'sini kontrol et
                if (!user.ProfileId.HasValue)
                {
                    return NotFound(new { message = "Kullanıcı profili bulunamadı" });
                }

                if (model.File == null || model.File.Length == 0)
                {
                    return BadRequest(new { message = "Dosya yüklenmedi" });
                }

                if (!model.File.ContentType.Equals("application/pdf", StringComparison.OrdinalIgnoreCase))
                {
                    return BadRequest(new { message = "Sadece PDF dosyaları kabul edilmektedir" });
                }

                // Dosya adını güvenli hale getir
                string fileName = Path.GetFileNameWithoutExtension(model.File.FileName);
                fileName = $"{fileName}_{DateTime.UtcNow:yyyyMMddHHmmss}.pdf";

                // Dosya yolu oluştur
                string uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "resumes");
                string filePath = Path.Combine(uploadsFolder, fileName);

                // Klasör yoksa oluştur
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Dosyayı kaydet
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await model.File.CopyToAsync(fileStream);
                }

                // Özgeçmiş kaydını oluştur
                var resume = new Resume
                {
                    FileName = model.File.FileName,
                    FilePath = $"/uploads/resumes/{fileName}",
                    FileSize = model.File.Length,
                    FileType = model.File.ContentType,
                    UploadDate = DateTime.UtcNow,
                    IsDefault = model.IsDefault,
                    ProfileId = user.ProfileId.Value
                };

                // Eğer varsayılan olarak işaretlendiyse, diğer özgeçmişlerin varsayılan durumunu kaldır
                if (model.IsDefault)
                {
                    await _profileRepository.SetDefaultResumeAsync(0, user.ProfileId.Value); // 0 ID'si geçici olarak kullanılıyor
                }

                // Özgeçmişi veritabanına kaydet
                await _profileRepository.AddResumeAsync(resume);
                await _profileRepository.SaveChangesAsync();

                // Eğer varsayılan olarak işaretlendiyse, yeni eklenen özgeçmişi varsayılan yap
                if (model.IsDefault)
                {
                    await _profileRepository.SetDefaultResumeAsync(resume.Id, user.ProfileId.Value);
                }

                _logger.LogInformation("Kullanıcı {UserId} için yeni özgeçmiş yüklendi: {FileName}", userId, resume.FileName);

                return Ok(resume);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Özgeçmiş yüklenirken hata oluştu");
                return StatusCode(500, new { message = "Özgeçmiş yüklenirken bir hata oluştu" });
            }
        }

        [HttpPut("resumes/{id}/set-default")]
        public async Task<IActionResult> SetDefaultResume(int id)
        {
            try
            {
                // Kullanıcı ID'sini al
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(new { message = "Oturum açmanız gerekiyor" });
                }

                // Kullanıcıyı bul
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                // Kullanıcının profil ID'sini kontrol et
                if (!user.ProfileId.HasValue)
                {
                    return NotFound(new { message = "Kullanıcı profili bulunamadı" });
                }

                var resume = await _profileRepository.GetResumeByIdAsync(id);
                if (resume == null)
                {
                    return NotFound(new { message = "Özgeçmiş bulunamadı" });
                }

                // Özgeçmişin kullanıcıya ait olduğunu doğrula
                if (resume.ProfileId != user.ProfileId.Value)
                {
                    return Unauthorized(new { message = "Bu özgeçmişi düzenleme yetkiniz yok" });
                }

                await _profileRepository.SetDefaultResumeAsync(id, user.ProfileId.Value);
                _logger.LogInformation("Kullanıcı {UserId} için varsayılan özgeçmiş güncellendi: {Id}", userId, id);

                return Ok(new { message = "Varsayılan özgeçmiş güncellendi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Varsayılan özgeçmiş güncellenirken hata oluştu: {Id}", id);
                return StatusCode(500, new { message = "Varsayılan özgeçmiş güncellenirken bir hata oluştu" });
            }
        }

        [HttpDelete("resumes/{id}")]
        public async Task<IActionResult> DeleteResume(int id)
        {
            try
            {
                // Kullanıcı ID'sini al
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(new { message = "Oturum açmanız gerekiyor" });
                }

                // Kullanıcıyı bul
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                // Kullanıcının profil ID'sini kontrol et
                if (!user.ProfileId.HasValue)
                {
                    return NotFound(new { message = "Kullanıcı profili bulunamadı" });
                }

                var resume = await _profileRepository.GetResumeByIdAsync(id);
                if (resume == null)
                {
                    return NotFound(new { message = "Özgeçmiş bulunamadı" });
                }

                // Özgeçmişin kullanıcıya ait olduğunu doğrula
                if (resume.ProfileId != user.ProfileId.Value)
                {
                    return Unauthorized(new { message = "Bu özgeçmişi silme yetkiniz yok" });
                }

                // Dosyayı sil
                string filePath = Path.Combine(_environment.WebRootPath, resume.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Veritabanından kaydı sil
                await _profileRepository.DeleteResumeAsync(id, user.ProfileId.Value);
                _logger.LogInformation("Kullanıcı {UserId} için özgeçmiş silindi: {Id}", userId, id);

                return Ok(new { message = "Özgeçmiş başarıyla silindi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Özgeçmiş silinirken hata oluştu: {Id}", id);
                return StatusCode(500, new { message = "Özgeçmiş silinirken bir hata oluştu" });
            }
        }

        [HttpGet("resumes/has-resume")]
        public async Task<IActionResult> HasUploadedResume()
        {
            try
            {
                // Kullanıcı ID'sini al
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(new { message = "Oturum açmanız gerekiyor" });
                }

                // Kullanıcıyı bul
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                // Kullanıcının profil ID'sini kontrol et
                if (!user.ProfileId.HasValue)
                {
                    return Ok(new { hasResume = false });
                }

                // Kullanıcının özgeçmişlerini getir
                var resumes = await _profileRepository.GetResumesAsync(user.ProfileId.Value);
                bool hasResume = resumes != null && resumes.Any();

                _logger.LogInformation("Kullanıcı {UserId} için özgeçmiş kontrolü yapıldı. Sonuç: {HasResume}", userId, hasResume);
                return Ok(new { hasResume = hasResume });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Özgeçmiş kontrolü yapılırken hata oluştu");
                return StatusCode(500, new { message = "Özgeçmiş kontrolü yapılırken bir hata oluştu" });
            }
        }

        [HttpGet("resumes/default")]
        public async Task<IActionResult> GetDefaultResume()
        {
            try
            {
                // Kullanıcı ID'sini al
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(new { message = "Oturum açmanız gerekiyor" });
                }

                // Kullanıcıyı bul
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                // Kullanıcının profil ID'sini kontrol et
                if (!user.ProfileId.HasValue)
                {
                    return NotFound(new { message = "Kullanıcı profili bulunamadı" });
                }

                // Kullanıcının varsayılan özgeçmişini getir
                var defaultResume = await _profileRepository.GetDefaultResumeAsync(user.ProfileId.Value);
                if (defaultResume == null)
                {
                    _logger.LogWarning("Kullanıcı {UserId} için varsayılan özgeçmiş bulunamadı", userId);
                    return NotFound(new { message = "Varsayılan özgeçmiş bulunamadı" });
                }

                // Dosya yolunu oluştur
                string filePath = Path.Combine(_environment.WebRootPath, defaultResume.FilePath.TrimStart('/'));

                // Dosyanın varlığını kontrol et
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("Kullanıcı {UserId} için varsayılan özgeçmiş dosyası bulunamadı: {FilePath}", userId, filePath);
                    return NotFound(new { message = "Varsayılan özgeçmiş dosyası bulunamadı" });
                }

                // Dosyayı oku
                var fileBytes = System.IO.File.ReadAllBytes(filePath);

                // Dosya boyutu çok büyükse, ilk 1000 baytını al
                var truncatedBytes = fileBytes.Length > 1000 ? fileBytes.Take(1000).ToArray() : fileBytes;
                var base64Content = Convert.ToBase64String(truncatedBytes);

                // Özgeçmiş bilgilerini ve dosya içeriğini döndür
                var result = new
                {
                    fileContent = base64Content,
                    fileName = defaultResume.FileName,
                    filePath = defaultResume.FilePath,
                    fileType = defaultResume.FileType,
                    fileSize = defaultResume.FileSize,
                    uploadDate = defaultResume.UploadDate,
                    id = defaultResume.Id,
                    isDefault = defaultResume.IsDefault,
                    isTruncated = fileBytes.Length > 1000
                };

                _logger.LogInformation("Kullanıcı {UserId} için varsayılan özgeçmiş alındı: {Id} - {FileName}", userId, defaultResume.Id, defaultResume.FileName);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Varsayılan özgeçmiş alınırken hata oluştu");
                return StatusCode(500, new { message = "Varsayılan özgeçmiş alınırken bir hata oluştu" });
            }
        }

        // n8n için özel endpoint - kimlik doğrulaması gerektirmez
        [AllowAnonymous]
        [HttpGet("resumes/default/n8n")]
        public async Task<IActionResult> GetDefaultResumeForN8n([FromQuery] int userId = 0)
        {
            try
            {
                _logger.LogInformation("n8n için varsayılan özgeçmiş istendi. UserId: {UserId}", userId);

                // Kullanıcı ID'si belirtilmişse, o kullanıcının özgeçmişini getir
                if (userId > 0)
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null || !user.ProfileId.HasValue)
                    {
                        _logger.LogWarning("n8n için belirtilen kullanıcı bulunamadı. UserId: {UserId}", userId);
                        return NotFound(new { message = $"Kullanıcı bulunamadı (ID: {userId})" });
                    }

                    // Kullanıcının tüm özgeçmişlerini getir
                    var resumes = await _profileRepository.GetResumesAsync(user.ProfileId.Value);
                    if (!resumes.Any())
                    {
                        _logger.LogWarning("n8n için kullanıcının hiç özgeçmişi bulunamadı. UserId: {UserId}", userId);
                        return NotFound(new { message = $"Kullanıcının hiç özgeçmişi bulunamadı (ID: {userId})" });
                    }

                    // Varsayılan özgeçmişi kontrol et
                    var defaultResume = resumes.FirstOrDefault(r => r.IsDefault);

                    // Varsayılan özgeçmiş yoksa, ilk özgeçmişi varsayılan olarak ayarla
                    if (defaultResume == null)
                    {
                        _logger.LogWarning("n8n için kullanıcının varsayılan özgeçmişi bulunamadı, ilk özgeçmiş varsayılan olarak ayarlanıyor. UserId: {UserId}", userId);
                        defaultResume = resumes.First();
                        await _profileRepository.SetDefaultResumeAsync(defaultResume.Id, user.ProfileId.Value);
                    }

                    // Dosya yolunu oluştur
                    string filePath = Path.Combine(_environment.WebRootPath, defaultResume.FilePath.TrimStart('/'));

                    // Dosyanın varlığını kontrol et
                    if (!System.IO.File.Exists(filePath))
                    {
                        _logger.LogWarning("n8n için kullanıcının varsayılan özgeçmiş dosyası bulunamadı. UserId: {UserId}, FilePath: {FilePath}", userId, filePath);
                        return NotFound(new { message = $"Kullanıcının özgeçmiş dosyası bulunamadı (ID: {userId})" });
                    }

                    // Dosyayı oku
                    var fileBytes = System.IO.File.ReadAllBytes(filePath);

                    // Dosya boyutu çok büyükse, ilk 1000 baytını al
                    var truncatedBytes = fileBytes.Length > 1000 ? fileBytes.Take(1000).ToArray() : fileBytes;
                    var base64Content = Convert.ToBase64String(truncatedBytes);

                    // Özgeçmiş bilgilerini ve dosya içeriğini döndür
                    var result = new
                    {
                        fileContent = base64Content,
                        fileName = defaultResume.FileName,
                        filePath = defaultResume.FilePath,
                        fileType = defaultResume.FileType,
                        fileSize = defaultResume.FileSize,
                        uploadDate = defaultResume.UploadDate,
                        id = defaultResume.Id,
                        isDefault = defaultResume.IsDefault,
                        isTruncated = fileBytes.Length > 1000
                    };

                    _logger.LogInformation("n8n için kullanıcının varsayılan özgeçmişi alındı. UserId: {UserId}, ResumeId: {ResumeId}", userId, defaultResume.Id);
                    return Ok(result);
                }

                // Varsayılan olarak ilk kullanıcının profilini kullan (admin)
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@admin.com");
                if (adminUser == null || !adminUser.ProfileId.HasValue)
                {
                    // Admin kullanıcısı yoksa, herhangi bir kullanıcıyı dene
                    var anyUser = await _context.Users.FirstOrDefaultAsync(u => u.ProfileId.HasValue);
                    if (anyUser == null || !anyUser.ProfileId.HasValue)
                    {
                        _logger.LogWarning("n8n için varsayılan özgeçmiş alınırken profil bulunamadı");
                        return NotFound(new { message = "Profil bulunamadı" });
                    }

                    // Kullanıcının varsayılan özgeçmişini getir
                    var defaultResume = await _profileRepository.GetDefaultResumeAsync(anyUser.ProfileId.Value);
                    if (defaultResume == null)
                    {
                        _logger.LogWarning("n8n için varsayılan özgeçmiş bulunamadı");
                        return NotFound(new { message = "Varsayılan özgeçmiş bulunamadı" });
                    }

                    // Dosya yolunu oluştur
                    string filePath = Path.Combine(_environment.WebRootPath, defaultResume.FilePath.TrimStart('/'));

                    // Dosyanın varlığını kontrol et
                    if (!System.IO.File.Exists(filePath))
                    {
                        _logger.LogWarning("n8n için varsayılan özgeçmiş dosyası bulunamadı: {FilePath}", filePath);
                        return NotFound(new { message = "Varsayılan özgeçmiş dosyası bulunamadı" });
                    }

                    // Dosyayı oku
                    var fileBytes = System.IO.File.ReadAllBytes(filePath);

                    // Dosya boyutu çok büyükse, ilk 1000 baytını al
                    var truncatedBytes = fileBytes.Length > 1000 ? fileBytes.Take(1000).ToArray() : fileBytes;
                    var base64Content = Convert.ToBase64String(truncatedBytes);

                    // Özgeçmiş bilgilerini ve dosya içeriğini döndür
                    var result = new
                    {
                        fileContent = base64Content,
                        fileName = defaultResume.FileName,
                        filePath = defaultResume.FilePath,
                        fileType = defaultResume.FileType,
                        fileSize = defaultResume.FileSize,
                        uploadDate = defaultResume.UploadDate,
                        id = defaultResume.Id,
                        isDefault = defaultResume.IsDefault,
                        isTruncated = fileBytes.Length > 1000
                    };

                    _logger.LogInformation("n8n için varsayılan özgeçmiş alındı: {Id} - {FileName}", defaultResume.Id, defaultResume.FileName);
                    return Ok(result);
                }
                else
                {
                    // Admin kullanıcısının varsayılan özgeçmişini getir
                    var defaultResume = await _profileRepository.GetDefaultResumeAsync(adminUser.ProfileId.Value);
                    if (defaultResume == null)
                    {
                        _logger.LogWarning("n8n için admin kullanıcısının varsayılan özgeçmişi bulunamadı");
                        return NotFound(new { message = "Varsayılan özgeçmiş bulunamadı" });
                    }

                    // Dosya yolunu oluştur
                    string filePath = Path.Combine(_environment.WebRootPath, defaultResume.FilePath.TrimStart('/'));

                    // Dosyanın varlığını kontrol et
                    if (!System.IO.File.Exists(filePath))
                    {
                        _logger.LogWarning("n8n için admin kullanıcısının varsayılan özgeçmiş dosyası bulunamadı: {FilePath}", filePath);
                        return NotFound(new { message = "Varsayılan özgeçmiş dosyası bulunamadı" });
                    }

                    // Dosyayı oku
                    var fileBytes = System.IO.File.ReadAllBytes(filePath);

                    // Dosya boyutu çok büyükse, ilk 1000 baytını al
                    var truncatedBytes = fileBytes.Length > 1000 ? fileBytes.Take(1000).ToArray() : fileBytes;
                    var base64Content = Convert.ToBase64String(truncatedBytes);

                    // Özgeçmiş bilgilerini ve dosya içeriğini döndür
                    var result = new
                    {
                        fileContent = base64Content,
                        fileName = defaultResume.FileName,
                        filePath = defaultResume.FilePath,
                        fileType = defaultResume.FileType,
                        fileSize = defaultResume.FileSize,
                        uploadDate = defaultResume.UploadDate,
                        id = defaultResume.Id,
                        isDefault = defaultResume.IsDefault,
                        isTruncated = fileBytes.Length > 1000
                    };

                    _logger.LogInformation("n8n için admin kullanıcısının varsayılan özgeçmişi alındı: {Id} - {FileName}", defaultResume.Id, defaultResume.FileName);
                    return Ok(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "n8n için varsayılan özgeçmiş alınırken hata oluştu. UserId: {UserId}", userId);
                return StatusCode(500, new { message = "Varsayılan özgeçmiş alınırken bir hata oluştu" });
            }
        }

        [HttpGet("resumes/view/{fileName}")]
        public IActionResult ViewResume(string fileName)
        {
            try
            {
                // Kullanıcı ID'sini al
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(new { message = "Oturum açmanız gerekiyor" });
                }

                if (string.IsNullOrEmpty(fileName))
                {
                    return BadRequest(new { message = "Dosya adı belirtilmedi" });
                }

                // Dosya yolunu oluştur
                string filePath = Path.Combine(_environment.WebRootPath, "uploads", "resumes", fileName);

                // Dosyanın varlığını kontrol et
                if (!System.IO.File.Exists(filePath))
                {
                    _logger.LogWarning("Kullanıcı {UserId} için dosya bulunamadı: {FilePath}", userId, filePath);
                    return NotFound(new { message = "Dosya bulunamadı" });
                }

                // Dosyayı oku ve döndür
                var fileBytes = System.IO.File.ReadAllBytes(filePath);

                // MIME türünü belirle
                string contentType = "application/pdf";

                // Dosyayı döndür
                _logger.LogInformation("Kullanıcı {UserId} için dosya görüntüleniyor: {FileName}", userId, fileName);
                return File(fileBytes, contentType, fileName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Dosya görüntülenirken hata oluştu: {FileName}", fileName);
                return StatusCode(500, new { message = "Dosya görüntülenirken bir hata oluştu" });
            }
        }

        // n8n için özel endpoint - kimlik doğrulaması gerektirmez
        [AllowAnonymous]
        [HttpGet("n8n")]
        public async Task<IActionResult> GetProfileForN8n([FromQuery] int userId = 0)
        {
            try
            {
                _logger.LogInformation("n8n için profil bilgisi istendi. UserId: {UserId}", userId);

                // Kullanıcı ID'si belirtilmişse, o kullanıcının profilini getir
                if (userId > 0)
                {
                    var user = await _context.Users.FindAsync(userId);
                    if (user == null || !user.ProfileId.HasValue)
                    {
                        _logger.LogWarning("n8n için belirtilen kullanıcı bulunamadı. UserId: {UserId}", userId);
                        return NotFound(new { message = $"Kullanıcı bulunamadı (ID: {userId})" });
                    }

                    // Kullanıcının profilini getir
                    var userProfile = await _profileRepository.GetByIdAsync(user.ProfileId.Value);
                    if (userProfile == null)
                    {
                        _logger.LogWarning("n8n için kullanıcı profili bulunamadı. UserId: {UserId}", userId);
                        return NotFound(new { message = $"Kullanıcı profili bulunamadı (ID: {userId})" });
                    }

                    _logger.LogInformation("n8n için kullanıcı profili başarıyla alındı. UserId: {UserId}, ProfileId: {ProfileId}", userId, userProfile.Id);
                    return Ok(userProfile);
                }

                // Varsayılan olarak ilk kullanıcının profilini kullan (admin)
                var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@admin.com");
                if (adminUser == null || !adminUser.ProfileId.HasValue)
                {
                    // Admin kullanıcısı yoksa, herhangi bir kullanıcıyı dene
                    var anyUser = await _context.Users.FirstOrDefaultAsync(u => u.ProfileId.HasValue);
                    if (anyUser == null || !anyUser.ProfileId.HasValue)
                    {
                        _logger.LogWarning("n8n için profil alınırken profil bulunamadı");
                        return NotFound(new { message = "Profil bulunamadı" });
                    }

                    // Profili getir
                    var profile = await _profileRepository.GetByIdAsync(anyUser.ProfileId.Value);
                    if (profile == null)
                    {
                        _logger.LogWarning("n8n için profil bulunamadı");
                        return NotFound(new { message = "Profil bulunamadı" });
                    }

                    _logger.LogInformation("n8n için profil alındı: {Id}", profile.Id);
                    return Ok(profile);
                }
                else
                {
                    // Admin kullanıcısının profilini getir
                    var profile = await _profileRepository.GetByIdAsync(adminUser.ProfileId.Value);
                    if (profile == null)
                    {
                        _logger.LogWarning("n8n için admin kullanıcısının profili bulunamadı");
                        return NotFound(new { message = "Profil bulunamadı" });
                    }

                    _logger.LogInformation("n8n için admin kullanıcısının profili alındı: {Id}", profile.Id);
                    return Ok(profile);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "n8n için profil alınırken hata oluştu. UserId: {UserId}", userId);
                return StatusCode(500, new { message = "Profil alınırken bir hata oluştu" });
            }
        }

        [HttpGet("oauth-status")]
        [Produces("application/json")]
        public async Task<IActionResult> GetOAuthStatus()
        {
            try
            {
                // Kullanıcı ID'sini al
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(new { message = "Oturum açmanız gerekiyor" });
                }

                // Kullanıcıyı bul
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                // Kullanıcının profil ID'sini kontrol et
                if (!user.ProfileId.HasValue)
                {
                    return NotFound(new { message = "Kullanıcı profili bulunamadı" });
                }

                _logger.LogInformation("Getting OAuth status for user {UserId} with profile {ProfileId}", userId, user.ProfileId.Value);
                var tokens = await _oauthService.GetOAuthTokensAsync(user.ProfileId.Value);

                var result = tokens.Select(t => new
                {
                    t.Id,
                    t.ProfileId,
                    t.Provider,
                    t.Email,
                    t.ExpiresAt
                }).ToList();

                _logger.LogInformation("Found {Count} OAuth tokens for user {UserId} with profile {ProfileId}", result.Count, userId, user.ProfileId.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OAuth status");
                return StatusCode(500, new { message = "Error getting OAuth status" });
            }
        }

        [AllowAnonymous]
        [HttpGet("authorize-google")]
        public async Task<IActionResult> AuthorizeGoogle([FromQuery] int userId = 0)
        {
            try
            {
                // Kullanıcı ID'si belirtilmemişse, mevcut kullanıcıyı al
                if (userId <= 0)
                {
                    userId = GetCurrentUserId();
                    if (userId <= 0)
                    {
                        return Unauthorized(new { message = "Oturum açmanız gerekiyor veya userId parametresi belirtilmelidir" });
                    }
                }

                // Kullanıcıyı bul
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                // Kullanıcının profil ID'sini kontrol et
                if (!user.ProfileId.HasValue)
                {
                    return NotFound(new { message = "Kullanıcı profili bulunamadı" });
                }

                var authUrl = _oauthService.GetAuthorizationUrl("Google", user.ProfileId.Value);
                _logger.LogInformation("Generated Google authorization URL for user {UserId} with profile {ProfileId}", userId, user.ProfileId.Value);
                return Redirect(authUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Google authorization URL");
                return StatusCode(500, new { message = "Error generating Google authorization URL" });
            }
        }

        [HttpDelete("revoke-oauth/{provider}")]
        [Produces("application/json")]
        public async Task<IActionResult> RevokeAccess(string provider)
        {
            try
            {
                // Kullanıcı ID'sini al
                var userId = GetCurrentUserId();
                if (userId <= 0)
                {
                    return Unauthorized(new { message = "Oturum açmanız gerekiyor" });
                }

                // Kullanıcıyı bul
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                // Kullanıcının profil ID'sini kontrol et
                if (!user.ProfileId.HasValue)
                {
                    return NotFound(new { message = "Kullanıcı profili bulunamadı" });
                }

                _logger.LogInformation("Revoking {Provider} access for user {UserId} with profile {ProfileId}", provider, userId, user.ProfileId.Value);
                var success = await _oauthService.RevokeTokenAsync(user.ProfileId.Value, provider);

                if (success)
                {
                    _logger.LogInformation("{Provider} access revoked successfully for user {UserId} with profile {ProfileId}", provider, userId, user.ProfileId.Value);
                    return Ok(new { success = true, message = $"{provider} access revoked successfully" });
                }
                else
                {
                    _logger.LogWarning("No {Provider} token found for user {UserId} with profile {ProfileId}", provider, userId, user.ProfileId.Value);
                    return NotFound(new { success = false, message = $"No {provider} token found for this profile" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking {Provider} access", provider);
                return StatusCode(500, new { message = $"Error revoking {provider} access" });
            }
        }
    }

    public class ResumeUploadModel
    {
        [Required]
        public IFormFile File { get; set; } = null!;
        public bool IsDefault { get; set; } = false;
    }
}
