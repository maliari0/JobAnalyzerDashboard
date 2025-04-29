using Microsoft.AspNetCore.Mvc;
using JobAnalyzerDashboard.Server.Models;
using JobAnalyzerDashboard.Server.Repositories;
using JobAnalyzerDashboard.Server.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text;
using System.Text.Json;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobController : ControllerBase
    {
        private readonly ILogger<JobController> _logger;
        private readonly IJobRepository _jobRepository;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IProfileRepository _profileRepository;
        private readonly EmailService _emailService;

        public JobController(
            ILogger<JobController> logger,
            IJobRepository jobRepository,
            IApplicationRepository applicationRepository,
            IProfileRepository profileRepository,
            EmailService emailService)
        {
            _logger = logger;
            _jobRepository = jobRepository;
            _applicationRepository = applicationRepository;
            _profileRepository = profileRepository;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] JobFilterModel filter)
        {
            try
            {
                var jobs = await _jobRepository.GetJobsWithFiltersAsync(
                    filter.Category,
                    filter.MinQualityScore,
                    filter.IsApplied,
                    filter.SearchTerm,
                    filter.SortBy,
                    filter.SortDirection);

                return Ok(jobs);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İş ilanları alınırken hata oluştu");
                return StatusCode(500, new { message = "İş ilanları alınırken bir hata oluştu" });
            }
        }

        [HttpGet("categories")]
        public async Task<IActionResult> GetCategories()
        {
            try
            {
                var categories = await _jobRepository.GetCategoriesAsync();
                return Ok(categories);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kategoriler alınırken hata oluştu");
                return StatusCode(500, new { message = "Kategoriler alınırken bir hata oluştu" });
            }
        }

        [HttpGet("tags")]
        public async Task<IActionResult> GetTags()
        {
            try
            {
                var tags = await _jobRepository.GetTagsAsync();
                return Ok(tags);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Etiketler alınırken hata oluştu");
                return StatusCode(500, new { message = "Etiketler alınırken bir hata oluştu" });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _jobRepository.GetStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İstatistikler alınırken hata oluştu");
                return StatusCode(500, new { message = "İstatistikler alınırken bir hata oluştu" });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var job = await _jobRepository.GetByIdAsync(id);
                if (job == null)
                {
                    return NotFound(new { message = "İş ilanı bulunamadı" });
                }

                return Ok(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İş ilanı alınırken hata oluştu: {Id}", id);
                return StatusCode(500, new { message = "İş ilanı alınırken bir hata oluştu" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(Job job)
        {
            try
            {
                if (job == null)
                {
                    return BadRequest(new { message = "Geçersiz iş ilanı verisi" });
                }

                job.PostedDate = DateTime.UtcNow;
                job.IsApplied = false;

                await _jobRepository.AddAsync(job);
                await _jobRepository.SaveChangesAsync();

                _logger.LogInformation("Yeni iş ilanı oluşturuldu: {Id} - {Title}", job.Id, job.Title);

                return CreatedAtAction(nameof(GetById), new { id = job.Id }, job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İş ilanı oluşturulurken hata oluştu");
                return StatusCode(500, new { message = "İş ilanı oluşturulurken bir hata oluştu" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Job job)
        {
            try
            {
                if (job == null || id != job.Id)
                {
                    return BadRequest(new { message = "Geçersiz iş ilanı verisi" });
                }

                var existingJob = await _jobRepository.GetByIdAsync(id);
                if (existingJob == null)
                {
                    return NotFound(new { message = "İş ilanı bulunamadı" });
                }

                await _jobRepository.UpdateAsync(job);
                await _jobRepository.SaveChangesAsync();

                _logger.LogInformation("İş ilanı güncellendi: {Id} - {Title}", job.Id, job.Title);

                return Ok(job);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İş ilanı güncellenirken hata oluştu: {Id}", id);
                return StatusCode(500, new { message = "İş ilanı güncellenirken bir hata oluştu" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            try
            {
                var job = await _jobRepository.GetByIdAsync(id);
                if (job == null)
                {
                    return NotFound(new { success = false, message = "İş ilanı bulunamadı" });
                }

                // İş ilanını sil
                await _jobRepository.DeleteAsync(job);
                await _jobRepository.SaveChangesAsync();

                _logger.LogInformation("İş ilanı silindi: {Id} - {Title}", job.Id, job.Title);

                return Ok(new { success = true, message = "İş ilanı başarıyla silindi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İş ilanı silinirken hata oluştu: {Id}", id);
                return StatusCode(500, new { success = false, message = "İş ilanı silinirken bir hata oluştu" });
            }
        }

        [HttpPost("apply/{id}")]
        public async Task<IActionResult> Apply(int id, [FromBody] ApplicationRequestModel model)
        {
            try
            {
                var job = await _jobRepository.GetByIdAsync(id);
                if (job == null)
                {
                    return NotFound(new { success = false, message = "İş ilanı bulunamadı" });
                }

                // Profil bilgilerini al
                var profile = await _profileRepository.GetProfileWithResumesAsync(1); // Varsayılan profil ID'si
                if (profile == null)
                {
                    return BadRequest(new { success = false, message = "Profil bilgileri bulunamadı" });
                }

                // Varsayılan özgeçmişi al
                Resume defaultResume = null;
                if (model?.AttachCV == true)
                {
                    defaultResume = await _profileRepository.GetDefaultResumeAsync(profile.Id);
                }

                // Başvuru mesajını hazırla
                string messageInput = model?.Message;
                string message = string.Empty;

                if (string.IsNullOrEmpty(messageInput))
                {
                    message = $"Merhaba,\n\n{job.Title} pozisyonu için başvurumu yapmak istiyorum. Özgeçmişimi ekte bulabilirsiniz.\n\nSaygılarımla,\n{profile.FullName}";
                }
                else
                {
                    message = messageInput;
                }

                // E-posta gönder
                string attachmentPath = null;
                if (defaultResume != null)
                {
                    attachmentPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", defaultResume.FilePath.TrimStart('/'));
                }

                bool emailSent = false;
                if (!string.IsNullOrEmpty(job.ContactEmail))
                {
                    string subject = $"Başvuru: {job.Title}";
                    emailSent = await _emailService.SendEmailAsync(job.ContactEmail, subject, message, attachmentPath);

                    _logger.LogInformation("Başvuru e-postası gönderildi: {Email}, İlan: {Title}", job.ContactEmail, job.Title);
                }

                // İş ilanını güncelle
                job.IsApplied = true;
                job.AppliedDate = DateTime.UtcNow;
                await _jobRepository.UpdateAsync(job);
                await _jobRepository.SaveChangesAsync();

                // Başvuru geçmişine ekle
                try
                {
                    var application = new Application
                    {
                        JobId = job.Id,
                        AppliedDate = DateTime.UtcNow,
                        Status = "Pending",
                        AppliedMethod = model?.Method ?? "Manual",
                        SentMessage = message,
                        IsAutoApplied = false,
                        CvAttached = defaultResume != null
                    };

                    await _applicationRepository.AddAsync(application);
                    await _applicationRepository.SaveChangesAsync();

                    _logger.LogInformation("Başvuru geçmişine eklendi: {Id} - {Title}", job.Id, job.Title);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Başvuru geçmişine eklenirken hata oluştu: {Id}", job.Id);
                    // Hata olsa bile devam et
                }

                return Ok(new
                {
                    success = true,
                    message = "Başvuru başarıyla yapıldı" + (emailSent ? " ve e-posta gönderildi" : ""),
                    job = job,
                    applicationMethod = model?.Method ?? "Manual",
                    applicationMessage = message,
                    emailSent = emailSent,
                    resumeAttached = defaultResume != null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Başvuru yapılırken hata oluştu: {Id}", id);
                return StatusCode(500, new { success = false, message = "Başvuru yapılırken bir hata oluştu" });
            }
        }

        [HttpPost("webhook-notify/{id}")]
        public async Task<IActionResult> WebhookNotify(int id)
        {
            try
            {
                var job = await _jobRepository.GetByIdAsync(id);
                if (job == null)
                {
                    return NotFound(new { success = false, message = "İş ilanı bulunamadı" });
                }

                // Burada n8n webhook'una bildirim gönderilecek
                // Şimdilik sadece başarılı yanıt döndürüyoruz

                return Ok(new { success = true, message = "Webhook bildirimi gönderildi", jobId = id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook bildirimi gönderilirken hata oluştu: {Id}", id);
                return StatusCode(500, new { success = false, message = "Webhook bildirimi gönderilirken bir hata oluştu" });
            }
        }

        [HttpPost("auto-apply/{id}")]
        public async Task<IActionResult> AutoApply(int id)
        {
            try
            {
                var job = await _jobRepository.GetByIdAsync(id);
                if (job == null)
                {
                    return NotFound(new { success = false, message = "İş ilanı bulunamadı" });
                }

                using (var httpClient = new HttpClient())
                {
                    var webhookUrl = "https://n8n-service-a2yz.onrender.com/webhook/apply-auto"; // n8n oto başvuru webhook URL'i

                    var jobData = new
                    {
                        title = job.Title,
                        description = job.Description,
                        employment_type = job.EmploymentType?.ToLower(),
                        company_website = job.CompanyWebsite,
                        contact_email = job.ContactEmail,
                        url = job.Url,
                        location = job.Location?.ToLower(),
                        company = job.Company
                    };

                    var options = new JsonSerializerOptions
                    {
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                        WriteIndented = true
                    };
                    var jsonData = JsonSerializer.Serialize(jobData, options);
                    var content = new StringContent(
                        jsonData,
                        Encoding.UTF8,
                        "application/json");

                    _logger.LogInformation("Webhook'a gönderilen veri: {JsonData}", jsonData);
                    _logger.LogInformation("Webhook URL: {WebhookUrl}", webhookUrl);

                    var response = await httpClient.PostAsync(webhookUrl, content);

                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Webhook yanıtı: {StatusCode}, İçerik: {ResponseContent}",
                        response.StatusCode, responseContent);

                    if (response.IsSuccessStatusCode)
                    {
                        job.IsApplied = true;
                        job.AppliedDate = DateTime.UtcNow;
                        await _jobRepository.UpdateAsync(job);
                        await _jobRepository.SaveChangesAsync();

                        try
                        {
                            // Başvuru geçmişine ekle
                            var application = new Application
                            {
                                JobId = job.Id,
                                AppliedDate = DateTime.UtcNow,
                                Status = "Pending",
                                AppliedMethod = "n8n",
                                SentMessage = $"n8n webhook'u ile otomatik başvuru yapıldı: {job.Title}",
                                IsAutoApplied = true,
                                CvAttached = true
                            };

                            await _applicationRepository.AddAsync(application);
                            await _applicationRepository.SaveChangesAsync();

                            _logger.LogInformation("Başvuru geçmişine eklendi: {Id} - {Title}", job.Id, job.Title);
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Başvuru geçmişine eklenirken hata oluştu: {Id}", job.Id);
                        }

                        _logger.LogInformation("Otomatik başvuru webhook'u tetiklendi: {Id} - {Title}", job.Id, job.Title);

                        return Ok(new
                        {
                            success = true,
                            message = "Otomatik başvuru işlemi başlatıldı",
                            job,
                            webhookResponse = responseContent
                        });
                    }
                    else
                    {
                        _logger.LogError("Webhook isteği başarısız: {StatusCode}, Yanıt: {ResponseContent}",
                            response.StatusCode, responseContent);

                        // Başarısız durumda iş ilanını "Başvuruldu" olarak işaretleme
                        // job.IsApplied = true;
                        // job.AppliedDate = DateTime.UtcNow;
                        // await _jobRepository.UpdateAsync(job);
                        // await _jobRepository.SaveChangesAsync();

                        return StatusCode(500, new
                        {
                            success = false,
                            message = $"Webhook isteği başarısız oldu: {response.StatusCode}",
                            error = responseContent,
                            job
                        });
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otomatik başvuru yapılırken hata oluştu: {Id}", id);
                return StatusCode(500, new { success = false, message = "Otomatik başvuru yapılırken bir hata oluştu" });
            }
        }
    }

    public class JobFilterModel
    {
        public string? Category { get; set; }
        public int? MinQualityScore { get; set; }
        public bool? IsApplied { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
    }

    public class ApplicationRequestModel
    {
        public string? Method { get; set; }
        public string? Message { get; set; }
        public bool AttachCV { get; set; } = true;
    }
}
