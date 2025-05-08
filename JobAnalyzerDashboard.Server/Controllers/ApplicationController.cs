using Microsoft.AspNetCore.Mvc;
using JobAnalyzerDashboard.Server.Models;
using JobAnalyzerDashboard.Server.Repositories;
using JobAnalyzerDashboard.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using JobAnalyzerDashboard.Server.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationController : ControllerBase
    {
        private readonly ILogger<ApplicationController> _logger;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IJobRepository _jobRepository;
        private readonly EmailService _emailService;
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ApplicationController(
            ILogger<ApplicationController> logger,
            IApplicationRepository applicationRepository,
            IJobRepository jobRepository,
            EmailService emailService,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _applicationRepository = applicationRepository;
            _jobRepository = jobRepository;
            _emailService = emailService;
            _context = context;
            _environment = environment;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ApplicationFilterModel filter)
        {
            try
            {
                // Kullanıcı kimliğini al
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                int? userId = null;

                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                {
                    userId = parsedUserId;

                    // Admin kullanıcıları için UserId filtresini uygulama
                    var userRoleClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
                    bool isAdmin = userRoleClaim != null && userRoleClaim.Value == "Admin";

                    // Eğer admin değilse, kullanıcı ID'sini filtre olarak ekle
                    if (!isAdmin)
                    {
                        filter.UserId = userId;
                    }
                    // Admin kullanıcıları için, eğer özel olarak bir UserId belirtilmişse, onu kullan
                }

                _logger.LogInformation("Başvurular alınıyor. Filtreler: JobId={JobId}, Status={Status}, AppliedMethod={AppliedMethod}, IsAutoApplied={IsAutoApplied}, UserId={UserId}",
                    filter.JobId, filter.Status, filter.AppliedMethod, filter.IsAutoApplied, filter.UserId);

#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8604 // Possible null reference argument.
#pragma warning disable CS8604 // Possible null reference argument.
                var applications = await _applicationRepository.GetApplicationsWithFiltersAsync(
                    filter.JobId,
                    filter.Status,
                    filter.AppliedMethod,
                    filter.IsAutoApplied,
                    filter.FromDate,
                    filter.ToDate,
                    filter.SortBy,
                    filter.SortDirection,
                    filter.UserId);
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8604 // Possible null reference argument.
#pragma warning restore CS8604 // Possible null reference argument.

                _logger.LogInformation("Başvurular başarıyla alındı. Toplam: {Count}", applications?.Count() ?? 0);

                return Ok(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Başvurular alınırken hata oluştu: {ErrorMessage}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);

                // İç içe istisnalar varsa onları da logla
                var innerException = ex.InnerException;
                while (innerException != null)
                {
                    _logger.LogError(innerException, "İç istisna: {ErrorMessage}, StackTrace: {StackTrace}",
                        innerException.Message, innerException.StackTrace);
                    innerException = innerException.InnerException;
                }

                return StatusCode(500, new { message = "Başvurular alınırken bir hata oluştu", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var application = await _applicationRepository.GetApplicationWithJobAsync(id);
                if (application == null)
                {
                    return NotFound(new { message = "Başvuru bulunamadı" });
                }

                return Ok(application);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Başvuru alınırken hata oluştu: {Id}", id);
                return StatusCode(500, new { message = "Başvuru alınırken bir hata oluştu" });
            }
        }

        [HttpGet("latest-by-job/{jobId}")]
        public async Task<IActionResult> GetLatestByJobId(int jobId)
        {
            try
            {
                // İş ilanına ait en son başvuruyu al
                var application = await _context.Applications
                    .Where(a => a.JobId == jobId)
                    .OrderByDescending(a => a.AppliedDate)
                    .Include(a => a.Job)
                    .FirstOrDefaultAsync();

                if (application == null)
                {
                    return NotFound(new { message = "Bu iş ilanına ait başvuru bulunamadı" });
                }

                return Ok(application);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İş ilanına ait en son başvuru alınırken hata oluştu: {JobId}", jobId);
                return StatusCode(500, new { message = "İş ilanına ait en son başvuru alınırken bir hata oluştu" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(ApplicationCreateModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new { message = "Geçersiz başvuru verisi" });
                }

                var job = await _jobRepository.GetByIdAsync(model.JobId);
                if (job == null)
                {
                    return NotFound(new { message = "İş ilanı bulunamadı" });
                }

                int? userId = model.UserId;

                // Eğer model'de UserId belirtilmemişse, mevcut kullanıcının ID'sini kullan
                if (!userId.HasValue)
                {
                    var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                    if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                    {
                        userId = parsedUserId;
                    }
                }

                if (userId.HasValue)
                {
                    bool hasApplied = await _applicationRepository.HasUserAppliedToJobAsync(model.JobId, userId.Value);
                    if (hasApplied)
                    {
                        return BadRequest(new { success = false, message = "Bu ilana daha önce başvurdunuz." });
                    }
                }

                var application = new Application
                {
                    JobId = model.JobId,
                    AppliedDate = DateTime.UtcNow,
                    Status = "Applying",
                    AppliedMethod = model.AppliedMethod,
                    SentMessage = model.Message,
                    Message = model.Message,
                    IsAutoApplied = model.IsAutoApplied,
                    CvAttached = model.CvAttached,
                    TelegramNotificationSent = model.TelegramNotification ?? string.Empty,
                    EmailContent = model.EmailContent ?? string.Empty,
                    UserId = userId
                };

                await _applicationRepository.AddAsync(application);
                await _jobRepository.SaveChangesAsync();

                _logger.LogInformation("Yeni başvuru oluşturuldu: {Id} - {JobId}", application.Id, application.JobId);

                return CreatedAtAction(nameof(GetById), new { id = application.Id }, application);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Başvuru oluşturulurken hata oluştu");
                return StatusCode(500, new { message = "Başvuru oluşturulurken bir hata oluştu" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Application application)
        {
            try
            {
                if (application == null || id != application.Id)
                {
                    return BadRequest(new { message = "Geçersiz başvuru verisi" });
                }

                var existingApplication = await _applicationRepository.GetByIdAsync(id);
                if (existingApplication == null)
                {
                    return NotFound(new { message = "Başvuru bulunamadı" });
                }

                await _applicationRepository.UpdateAsync(application);
                await _applicationRepository.SaveChangesAsync();

                _logger.LogInformation("Başvuru güncellendi: {Id}", application.Id);

                return Ok(application);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Başvuru güncellenirken hata oluştu: {Id}", id);
                return StatusCode(500, new { message = "Başvuru güncellenirken bir hata oluştu" });
            }
        }

        [HttpPut("{id}/status")]
        public async Task<IActionResult> UpdateStatus(int id, [FromBody] StatusUpdateModel model)
        {
            try
            {
                if (model == null || string.IsNullOrEmpty(model.Status))
                {
                    return BadRequest(new { message = "Geçersiz durum verisi" });
                }

                var application = await _applicationRepository.GetByIdAsync(id);
                if (application == null)
                {
                    return NotFound(new { message = "Başvuru bulunamadı" });
                }

                // Durum güncelleme
                application.Status = model.Status;
                if (!string.IsNullOrEmpty(model.Details))
                {
                    application.ResponseDetails = model.Details;
                }
                application.ResponseDate = DateTime.UtcNow;

                await _applicationRepository.UpdateAsync(application);
                await _applicationRepository.SaveChangesAsync();

                _logger.LogInformation("Başvuru durumu güncellendi: {Id}, Durum: {Status}", id, model.Status);

                return Ok(application);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Başvuru durumu güncellenirken hata oluştu: {Id}", id);
                return StatusCode(500, new { message = "Başvuru durumu güncellenirken bir hata oluştu" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var application = await _applicationRepository.GetByIdAsync(id);
                if (application == null)
                {
                    return NotFound(new { message = "Başvuru bulunamadı" });
                }

                await _applicationRepository.DeleteAsync(application);
                await _applicationRepository.SaveChangesAsync();

                _logger.LogInformation("Başvuru silindi: {Id}", id);

                return Ok(new { message = "Başvuru başarıyla silindi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Başvuru silinirken hata oluştu: {Id}", id);
                return StatusCode(500, new { message = "Başvuru silinirken bir hata oluştu" });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _applicationRepository.GetStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Başvuru istatistikleri alınırken hata oluştu");
                return StatusCode(500, new { message = "Başvuru istatistikleri alınırken bir hata oluştu" });
            }
        }

        [HttpPost("auto-apply")]
        public async Task<IActionResult> AutoApply(AutoApplyModel model)
        {
            try
            {
                if (model == null || model.JobId <= 0)
                {
                    return BadRequest(new { message = "Geçersiz otomatik başvuru verisi" });
                }

                var job = await _jobRepository.GetByIdAsync(model.JobId);
                if (job == null)
                {
                    return NotFound(new { message = "İş ilanı bulunamadı" });
                }

                int? userId = null;
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int parsedUserId))
                {
                    userId = parsedUserId;
                }

                if (userId.HasValue)
                {
                    bool hasApplied = await _applicationRepository.HasUserAppliedToJobAsync(model.JobId, userId.Value);
                    if (hasApplied)
                    {
                        return BadRequest(new { success = false, message = "Bu ilana daha önce başvurdunuz." });
                    }
                }

                // Başvuru oluştur
                var application = new Application
                {
                    JobId = model.JobId,
                    AppliedDate = DateTime.UtcNow,
                    Status = "Applying",
                    AppliedMethod = "n8n",
                    SentMessage = model.Message ?? string.Empty,
                    Message = model.Message ?? string.Empty,
                    IsAutoApplied = true,
                    CvAttached = true,
                    TelegramNotificationSent = model.TelegramNotification ?? string.Empty,
                    UserId = userId
                };

                await _applicationRepository.AddAsync(application);
                await _applicationRepository.SaveChangesAsync();

                job.IsApplied = true;
                job.AppliedDate = DateTime.UtcNow;
                await _jobRepository.UpdateAsync(job);
                await _jobRepository.SaveChangesAsync();

                _logger.LogInformation("Otomatik başvuru oluşturuldu: {Id} - {Title}", application.Id, job.Title);

                return Ok(new
                {
                    success = true,
                    message = "Otomatik başvuru başarıyla yapıldı",
                    applicationId = application.Id,
                    jobId = job.Id,
                    jobTitle = job.Title
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otomatik başvuru yapılırken hata oluştu");
                return StatusCode(500, new { message = "Otomatik başvuru yapılırken bir hata oluştu" });
            }
        }

        [HttpPost("send-email/{id}")]
        public async Task<IActionResult> SendEmail(int id, [FromBody] SendEmailModel model)
        {
            try
            {
                var application = await _applicationRepository.GetApplicationWithJobAsync(id);
                if (application == null)
                {
                    return NotFound(new { success = false, message = "Başvuru bulunamadı" });
                }

                if (application.Job == null)
                {
                    return NotFound(new { success = false, message = "İş ilanı bulunamadı" });
                }

                string emailContent = model?.CustomEmailContent ?? application.EmailContent;
                if (string.IsNullOrEmpty(emailContent))
                {
                    return BadRequest(new { success = false, message = "E-posta içeriği bulunamadı" });
                }

                if (string.IsNullOrEmpty(application.Job.ContactEmail))
                {
                    return BadRequest(new { success = false, message = "İş ilanında e-posta adresi bulunamadı" });
                }

                int profileId = model?.ProfileId ?? 1;

                string? attachmentPath = null;
                try
                {
                    _logger.LogInformation("Varsayılan CV dosyası alınıyor. ProfileId: {ProfileId}", profileId);

                    // Doğrudan profil ID'yi kullanarak varsayılan özgeçmişi getir
                    var profileRepository = HttpContext.RequestServices.GetRequiredService<IProfileRepository>();
                    var defaultResume = await profileRepository.GetDefaultResumeAsync(profileId);

                    if (defaultResume != null)
                    {
                        string filePath = Path.Combine(_environment.WebRootPath, defaultResume.FilePath.TrimStart('/'));

                        if (System.IO.File.Exists(filePath))
                        {
                            attachmentPath = filePath;
                            _logger.LogInformation("Varsayılan CV dosyası bulundu: {FilePath}", filePath);
                        }
                        else
                        {
                            _logger.LogWarning("Varsayılan CV dosyası bulunamadı: {FilePath}", filePath);
                        }
                    }
                    else
                    {
                        _logger.LogWarning("Kullanıcının varsayılan CV'si bulunamadı. ProfileId: {ProfileId}", profileId);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "CV dosyası alınırken hata oluştu. ProfileId: {ProfileId}", profileId);
                }

                // E-posta gönder
                string subject = $"Başvuru: {application.Job.Title}";
                bool emailSent = await _emailService.SendEmailAsync(
                    application.Job.ContactEmail,
                    subject,
                    emailContent,
                    attachmentPath, // Varsayılan CV dosyası
                    profileId
                );

                if (emailSent)
                {
                    application.Status = "Sent";
                    application.SentMessage = emailContent;
                    await _applicationRepository.UpdateAsync(application);
                    await _applicationRepository.SaveChangesAsync();

                    _logger.LogInformation("Başvuru e-postası gönderildi: {Id}, İş: {JobTitle}, Alıcı: {Email}",
                        id, application.Job.Title, application.Job.ContactEmail);

                    return Ok(new {
                        success = true,
                        message = "Başvuru e-postası başarıyla gönderildi",
                        applicationId = application.Id,
                        jobId = application.JobId,
                        jobTitle = application.Job.Title,
                        recipient = application.Job.ContactEmail
                    });
                }
                else
                {
                    _logger.LogWarning("Başvuru e-postası gönderilemedi: {Id}, İş: {JobTitle}, Alıcı: {Email}",
                        id, application.Job.Title, application.Job.ContactEmail);

                    return BadRequest(new {
                        success = false,
                        message = "E-posta gönderilemedi. Lütfen daha sonra tekrar deneyin."
                    });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Başvuru e-postası gönderilirken hata oluştu: {Id}", id);
                return StatusCode(500, new {
                    success = false,
                    message = "E-posta gönderilirken bir hata oluştu: " + ex.Message
                });
            }
        }
    }

    public class ApplicationFilterModel
    {
        public int? JobId { get; set; }
        public string? Status { get; set; }
        public string? AppliedMethod { get; set; }
        public bool? IsAutoApplied { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
        public int? UserId { get; set; }
    }

    public class ApplicationCreateModel
    {
        public int JobId { get; set; }
        public string AppliedMethod { get; set; } = "Manual";
        public string Message { get; set; } = string.Empty;
        public bool IsAutoApplied { get; set; } = false;
        public bool CvAttached { get; set; } = false;
        public string? TelegramNotification { get; set; }
        public string? EmailContent { get; set; }
        public int? UserId { get; set; }
    }

    public class AutoApplyModel
    {
        public int JobId { get; set; }
        public string? Message { get; set; }
        public string? TelegramNotification { get; set; }
    }

    public class SendEmailModel
    {
        public string? CustomEmailContent { get; set; }
        public int? ProfileId { get; set; }
    }

    public class StatusUpdateModel
    {
        public string Status { get; set; } = string.Empty;
        public string? Details { get; set; }
    }
}
