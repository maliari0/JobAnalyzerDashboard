using Microsoft.AspNetCore.Mvc;
using JobAnalyzerDashboard.Server.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private static readonly Profile _profile = new Profile
        {
            Id = 1,
            FullName = "Ali Yılmaz",
            Email = "ali.yilmaz@example.com",
            Phone = "+90 555 123 4567",
            LinkedInUrl = "https://linkedin.com/in/aliyilmaz",
            GithubUrl = "https://github.com/aliyilmaz",
            PortfolioUrl = "https://aliyilmaz.dev",
            Skills = "C#, ASP.NET Core, Angular, JavaScript, TypeScript, SQL, Git",
            Education = "Bilgisayar Mühendisliği, XYZ Üniversitesi, 2020",
            Experience = "3 yıl Full Stack Developer deneyimi",
            PreferredJobTypes = "Remote, Hybrid",
            PreferredLocations = "İstanbul, Ankara, İzmir",
            MinimumSalary = "40.000 TL",
            ResumeFilePath = "/uploads/resume.pdf"
        };

        private static readonly List<Resume> _resumes = new List<Resume>
        {
            new Resume
            {
                Id = 1,
                FileName = "Ali_Yilmaz_CV.pdf",
                FilePath = "/uploads/resumes/Ali_Yilmaz_CV.pdf",
                FileSize = 1024 * 1024, // 1MB
                FileType = "application/pdf",
                UploadDate = DateTime.Now.AddDays(-30),
                IsDefault = true,
                ProfileId = 1
            }
        };

        private readonly ILogger<ProfileController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(ILogger<ProfileController> logger, IWebHostEnvironment environment)
        {
            _logger = logger;
            _environment = environment;
        }

        [HttpGet]
        public IActionResult Get()
        {
            // Profil bilgilerine özgeçmişleri ekle
            _profile.Resumes = _resumes.Where(r => r.ProfileId == _profile.Id).ToList();

            return Ok(_profile);
        }

        [HttpPut]
        public IActionResult Update(Profile updatedProfile)
        {
            if (updatedProfile == null)
            {
                return BadRequest();
            }

            // Sadece belirli alanları güncelle, ID'yi değiştirme
            _profile.FullName = updatedProfile.FullName;
            _profile.Email = updatedProfile.Email;
            _profile.Phone = updatedProfile.Phone;
            _profile.LinkedInUrl = updatedProfile.LinkedInUrl;
            _profile.GithubUrl = updatedProfile.GithubUrl;
            _profile.PortfolioUrl = updatedProfile.PortfolioUrl;
            _profile.Skills = updatedProfile.Skills;
            _profile.Education = updatedProfile.Education;
            _profile.Experience = updatedProfile.Experience;
            _profile.PreferredJobTypes = updatedProfile.PreferredJobTypes;
            _profile.PreferredLocations = updatedProfile.PreferredLocations;
            _profile.MinimumSalary = updatedProfile.MinimumSalary;

            // N8n entegrasyonu için eklenen alanları güncelle
            _profile.NotionPageId = updatedProfile.NotionPageId;
            _profile.TelegramChatId = updatedProfile.TelegramChatId;
            _profile.PreferredModel = updatedProfile.PreferredModel;
            _profile.TechnologyStack = updatedProfile.TechnologyStack;
            _profile.Position = updatedProfile.Position;
            _profile.PreferredCategories = updatedProfile.PreferredCategories;
            _profile.MinQualityScore = updatedProfile.MinQualityScore;
            _profile.AutoApplyEnabled = updatedProfile.AutoApplyEnabled;

            // Profil bilgilerine özgeçmişleri ekle
            _profile.Resumes = _resumes.Where(r => r.ProfileId == _profile.Id).ToList();

            return Ok(_profile);
        }

        [HttpGet("resumes")]
        public IActionResult GetResumes()
        {
            return Ok(_resumes.Where(r => r.ProfileId == _profile.Id).ToList());
        }

        [HttpGet("resumes/default")]
        public IActionResult GetDefaultResume()
        {
            var defaultResume = _resumes.FirstOrDefault(r => r.ProfileId == _profile.Id && r.IsDefault);

            if (defaultResume == null)
            {
                return NotFound(new { message = "Varsayılan özgeçmiş bulunamadı" });
            }

            return Ok(defaultResume);
        }

        [HttpPost("upload-resume")]
        public IActionResult UploadResume(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                {
                    return BadRequest(new { message = "Dosya seçilmedi" });
                }

                // PDF dosyası kontrolü
                if (file.ContentType != "application/pdf")
                {
                    return BadRequest(new { message = "Sadece PDF dosyaları yüklenebilir" });
                }

                // Dosya boyutu kontrolü (5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest(new { message = "Dosya boyutu 5MB'dan küçük olmalıdır" });
                }

                // Uploads klasörünü oluştur
                string uploadsFolder = Path.Combine(_environment.ContentRootPath, "wwwroot", "uploads", "resumes");
                Directory.CreateDirectory(uploadsFolder);

                // Benzersiz dosya adı oluştur
                string uniqueFileName = $"{_profile.FullName.Replace(" ", "_")}_{DateTime.Now.ToString("yyyyMMddHHmmss")}.pdf";
                string filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Dosyayı kaydet
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    file.CopyTo(fileStream);
                }

                // Yeni özgeçmiş oluştur
                var resume = new Resume
                {
                    Id = _resumes.Count > 0 ? _resumes.Max(r => r.Id) + 1 : 1,
                    FileName = uniqueFileName,
                    FilePath = $"/uploads/resumes/{uniqueFileName}",
                    FileSize = file.Length,
                    FileType = file.ContentType,
                    UploadDate = DateTime.Now,
                    IsDefault = _resumes.Count(r => r.ProfileId == _profile.Id) == 0, // İlk özgeçmiş ise varsayılan yap
                    ProfileId = _profile.Id
                };

                // Dosya erişim izinlerini kontrol et
                _logger.LogInformation("Dosya yolu: {filePath}", filePath);
                _logger.LogInformation("Dosya URL: {fileUrl}", resume.FilePath);

                // Özgeçmişi listeye ekle
                _resumes.Add(resume);

                _logger.LogInformation("Özgeçmiş yüklendi: {fileName}", uniqueFileName);

                return Ok(resume);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Özgeçmiş yüklenirken hata oluştu");
                return StatusCode(500, new { message = "Özgeçmiş yüklenirken bir hata oluştu" });
            }
        }

        [HttpPut("resumes/{id}/set-default")]
        public IActionResult SetDefaultResume(int id)
        {
            var resume = _resumes.FirstOrDefault(r => r.Id == id && r.ProfileId == _profile.Id);

            if (resume == null)
            {
                return NotFound(new { message = "Özgeçmiş bulunamadı" });
            }

            // Tüm özgeçmişlerin varsayılan durumunu kaldır
            foreach (var r in _resumes.Where(r => r.ProfileId == _profile.Id))
            {
                r.IsDefault = false;
            }

            // Seçilen özgeçmişi varsayılan yap
            resume.IsDefault = true;

            _logger.LogInformation("Varsayılan özgeçmiş ayarlandı: {fileName}", resume.FileName);

            return Ok(resume);
        }

        [HttpDelete("resumes/{id}")]
        public IActionResult DeleteResume(int id)
        {
            var resume = _resumes.FirstOrDefault(r => r.Id == id && r.ProfileId == _profile.Id);

            if (resume == null)
            {
                return NotFound(new { message = "Özgeçmiş bulunamadı" });
            }

            // Dosyayı sil
            try
            {
                string filePath = Path.Combine(_environment.ContentRootPath, "wwwroot", resume.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Özgeçmiş dosyası silinirken hata oluştu: {fileName}", resume.FileName);
                // Dosya silinmese bile devam et
            }

            // Özgeçmişi listeden kaldır
            _resumes.Remove(resume);

            // Eğer silinen özgeçmiş varsayılan ise ve başka özgeçmişler varsa, ilk özgeçmişi varsayılan yap
            if (resume.IsDefault && _resumes.Any(r => r.ProfileId == _profile.Id))
            {
                _resumes.First(r => r.ProfileId == _profile.Id).IsDefault = true;
            }

            _logger.LogInformation("Özgeçmiş silindi: {fileName}", resume.FileName);

            return Ok(new { message = "Özgeçmiş başarıyla silindi" });
        }
    }
}
