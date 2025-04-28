using Microsoft.AspNetCore.Mvc;
using JobAnalyzerDashboard.Server.Models;
using JobAnalyzerDashboard.Server.Repositories;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProfileController : ControllerBase
    {
        private readonly ILogger<ProfileController> _logger;
        private readonly IProfileRepository _profileRepository;
        private readonly IWebHostEnvironment _environment;

        public ProfileController(
            ILogger<ProfileController> logger,
            IProfileRepository profileRepository,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _profileRepository = profileRepository;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            try
            {
                // Varsayılan profil ID'si 1
                var profile = await _profileRepository.GetProfileWithResumesAsync(1);
                if (profile == null)
                {
                    return NotFound(new { message = "Profil bulunamadı" });
                }

                return Ok(profile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Profil alınırken hata oluştu");
                return StatusCode(500, new { message = "Profil alınırken bir hata oluştu" });
            }
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

                // Varsayılan profil ID'si 1
                var existingProfile = await _profileRepository.GetByIdAsync(1);
                if (existingProfile == null)
                {
                    return NotFound(new { message = "Profil bulunamadı" });
                }

                profile.Id = 1; // ID'yi zorla 1 yap
                await _profileRepository.UpdateAsync(profile);
                await _profileRepository.SaveChangesAsync();

                _logger.LogInformation("Profil güncellendi");

                return Ok(profile);
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
                // Varsayılan profil ID'si 1
                var resumes = await _profileRepository.GetResumesAsync(1);
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
                    ProfileId = 1 // Varsayılan profil ID'si
                };

                // Eğer varsayılan olarak işaretlendiyse, diğer özgeçmişlerin varsayılan durumunu kaldır
                if (model.IsDefault)
                {
                    await _profileRepository.SetDefaultResumeAsync(0, 1); // 0 ID'si geçici olarak kullanılıyor
                }

                // Özgeçmişi veritabanına kaydet
                await _profileRepository.AddResumeAsync(resume);
                await _profileRepository.SaveChangesAsync();

                // Eğer varsayılan olarak işaretlendiyse, yeni eklenen özgeçmişi varsayılan yap
                if (model.IsDefault)
                {
                    await _profileRepository.SetDefaultResumeAsync(resume.Id, 1);
                }

                _logger.LogInformation("Yeni özgeçmiş yüklendi: {FileName}", resume.FileName);

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
                var resume = await _profileRepository.GetResumeByIdAsync(id);
                if (resume == null)
                {
                    return NotFound(new { message = "Özgeçmiş bulunamadı" });
                }

                await _profileRepository.SetDefaultResumeAsync(id, 1); // Varsayılan profil ID'si 1
                _logger.LogInformation("Varsayılan özgeçmiş güncellendi: {Id}", id);

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
                var resume = await _profileRepository.GetResumeByIdAsync(id);
                if (resume == null)
                {
                    return NotFound(new { message = "Özgeçmiş bulunamadı" });
                }

                // Dosyayı sil
                string filePath = Path.Combine(_environment.WebRootPath, resume.FilePath.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                // Veritabanından kaydı sil
                await _profileRepository.DeleteResumeAsync(id, 1); // Varsayılan profil ID'si 1
                _logger.LogInformation("Özgeçmiş silindi: {Id}", id);

                return Ok(new { message = "Özgeçmiş başarıyla silindi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Özgeçmiş silinirken hata oluştu: {Id}", id);
                return StatusCode(500, new { message = "Özgeçmiş silinirken bir hata oluştu" });
            }
        }
    }

    public class ResumeUploadModel
    {
        public IFormFile File { get; set; }
        public bool IsDefault { get; set; } = false;
    }
}
