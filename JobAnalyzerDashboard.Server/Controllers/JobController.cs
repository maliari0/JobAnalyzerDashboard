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
using JobAnalyzerDashboard.Server.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

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
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public JobController(
            ILogger<JobController> logger,
            IJobRepository jobRepository,
            IApplicationRepository applicationRepository,
            IProfileRepository profileRepository,
            EmailService emailService,
            ApplicationDbContext context,
            IWebHostEnvironment environment)
        {
            _logger = logger;
            _jobRepository = jobRepository;
            _applicationRepository = applicationRepository;
            _profileRepository = profileRepository;
            _emailService = emailService;
            _context = context;
            _environment = environment;
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
                // Doğrudan veritabanından silme işlemi
                var job = await _context.Jobs.FindAsync(id);
                if (job == null)
                {
                    return NotFound(new { success = false, message = "İş ilanı bulunamadı" });
                }

                // İlgili başvuruları bul
                var applications = await _context.Applications.Where(a => a.JobId == id).ToListAsync();
                if (applications.Count > 0)
                {
                    // Başvuruları sil
                    _context.Applications.RemoveRange(applications);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("İş ilanına ait {Count} başvuru silindi: {Id}", applications.Count, id);
                }

                // İş ilanını sil
                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();

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

        // n8n için özel endpoint - e-posta içeriğini kaydetmek için
        [AllowAnonymous]
        [HttpPost("n8n-save-email")]
        public async Task<IActionResult> N8nSaveEmail([FromBody] object rawData)
        {
            try
            {
                // Log the raw data for debugging
                string rawJson = JsonSerializer.Serialize(rawData);
                _logger.LogInformation("Received raw data from n8n: {RawData}", rawJson);

                // Extract email content from various formats
                string emailContent = "";
                int? applicationId = null;
                int? jobId = null;

                try
                {
                    // Try to parse as Dictionary
                    var dict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(rawJson);

                    // Check for direct emailContent
                    if (dict != null && dict.ContainsKey("emailContent"))
                    {
                        emailContent = dict["emailContent"].GetString() ?? "";
                    }
                    // Check for applicationId
                    if (dict != null && dict.ContainsKey("applicationId") && dict["applicationId"].ValueKind == JsonValueKind.Number)
                    {
                        applicationId = dict["applicationId"].GetInt32();
                    }
                    // Check for jobId
                    if (dict != null && dict.ContainsKey("jobId") && dict["jobId"].ValueKind == JsonValueKind.Number)
                    {
                        jobId = dict["jobId"].GetInt32();
                    }
                    // Check for body.emailContent
                    else if (dict != null && dict.ContainsKey("body"))
                    {
                        var bodyElement = dict["body"];

                        // If body is a string, use it directly
                        if (bodyElement.ValueKind == JsonValueKind.String)
                        {
                            emailContent = bodyElement.GetString() ?? "";
                        }
                        // If body is an object, look for emailContent
                        else if (bodyElement.ValueKind == JsonValueKind.Object)
                        {
                            var bodyDict = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(bodyElement.GetRawText());

                            if (bodyDict != null && bodyDict.ContainsKey("emailContent"))
                            {
                                emailContent = bodyDict["emailContent"].GetString() ?? "";
                            }
                            // Check for applicationId in body
                            if (bodyDict != null && bodyDict.ContainsKey("applicationId") &&
                                bodyDict["applicationId"].ValueKind == JsonValueKind.Number)
                            {
                                applicationId = bodyDict["applicationId"].GetInt32();
                            }
                            // Check for jobId in body
                            if (bodyDict != null && bodyDict.ContainsKey("jobId") &&
                                bodyDict["jobId"].ValueKind == JsonValueKind.Number)
                            {
                                jobId = bodyDict["jobId"].GetInt32();
                            }
                        }
                    }
                    // If nothing else works, try using the raw data as string
                    else
                    {
                        emailContent = rawData.ToString() ?? "";
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Error parsing n8n data");
                    emailContent = rawData.ToString() ?? "";
                }

                // If still no content, return error
                if (string.IsNullOrEmpty(emailContent))
                {
                    return BadRequest(new { success = false, message = "Could not extract email content from request" });
                }

                Application application = null;

                // If applicationId is provided, find that specific application
                if (applicationId.HasValue)
                {
                    application = await _context.Applications.FindAsync(applicationId.Value);
                    if (application == null)
                    {
                        _logger.LogWarning("Application with ID {ApplicationId} not found", applicationId.Value);
                    }
                }

                // If application not found by ID but jobId is provided, find the latest application for that job
                if (application == null && jobId.HasValue)
                {
                    application = await _context.Applications
                        .Where(a => a.JobId == jobId.Value)
                        .OrderByDescending(a => a.AppliedDate)
                        .FirstOrDefaultAsync();

                    if (application == null)
                    {
                        _logger.LogWarning("No applications found for job ID {JobId}", jobId.Value);
                    }
                }

                // If still no application found, get the latest application overall
                if (application == null)
                {
                    application = await _context.Applications
                        .OrderByDescending(a => a.AppliedDate)
                        .FirstOrDefaultAsync();
                }

                if (application == null)
                {
                    return NotFound(new { success = false, message = "No applications found" });
                }

                // Save the email content
                application.EmailContent = emailContent;

                try {
                    _context.Applications.Update(application);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("Email content saved for application: {Id}, Job: {JobId}",
                        application.Id, application.JobId);
                }
                catch (Exception ex) {
                    _logger.LogError(ex, "Error updating application with email content: {Id}", application.Id);
                    return StatusCode(500, new {
                        success = false,
                        message = "Error saving email content: " + ex.Message
                    });
                }

                _logger.LogInformation("Email content saved for application: {Id}, Job: {JobId}",
                    application.Id, application.JobId);

                return Ok(new {
                    success = true,
                    message = "Email content saved successfully",
                    applicationId = application.Id,
                    jobId = application.JobId
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving email content from n8n");
                return StatusCode(500, new {
                    success = false,
                    message = "Error saving email content: " + ex.Message
                });
            }
        }

        [HttpPost("force-delete/{id}")]
        public async Task<IActionResult> ForceDeleteJob(int id)
        {
            try
            {
                // Doğrudan veritabanından silme işlemi
                var job = await _context.Jobs.FindAsync(id);
                if (job == null)
                {
                    return NotFound(new { success = false, message = "İş ilanı bulunamadı" });
                }

                // İlgili başvuruları da sil
                var applications = await _context.Applications.Where(a => a.JobId == id).ToListAsync();
                if (applications.Any())
                {
                    _context.Applications.RemoveRange(applications);
                    await _context.SaveChangesAsync();
                    _logger.LogInformation("İş ilanına ait {Count} başvuru silindi: {Id}", applications.Count, id);
                }

                // İş ilanını sil
                _context.Jobs.Remove(job);
                await _context.SaveChangesAsync();

                _logger.LogInformation("İş ilanı zorla silindi: {Id} - {Title}", job.Id, job.Title);

                return Ok(new { success = true, message = "İş ilanı başarıyla silindi", jobId = id, title = job.Title });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "İş ilanı zorla silinirken hata oluştu: {Id}", id);
                return StatusCode(500, new { success = false, message = "İş ilanı zorla silinirken bir hata oluştu: " + ex.Message });
            }
        }

        [AllowAnonymous]
        [HttpPost("auto-apply/{id}")]
        public async Task<IActionResult> AutoApply(int id, [FromQuery] int userId = 0)
        {
            try
            {
                var job = await _jobRepository.GetByIdAsync(id);
                if (job == null)
                {
                    return NotFound(new { success = false, message = "İş ilanı bulunamadı" });
                }

                // Kullanıcı ID'si belirtilmemişse, varsayılan olarak admin kullanıcısını kullan
                if (userId <= 0)
                {
                    // Admin kullanıcısını bul
                    var adminUser = await _context.Users.FirstOrDefaultAsync(u => u.Email == "admin@admin.com");
                    if (adminUser != null && adminUser.ProfileId.HasValue)
                    {
                        userId = adminUser.Id;
                        _logger.LogInformation("Kullanıcı ID belirtilmediği için admin kullanıcısı kullanılıyor: {UserId}", userId);
                    }
                    else
                    {
                        // Admin kullanıcısı bulunamazsa, herhangi bir kullanıcıyı dene
                        var anyUser = await _context.Users.FirstOrDefaultAsync(u => u.ProfileId.HasValue);
                        if (anyUser != null)
                        {
                            userId = anyUser.Id;
                            _logger.LogInformation("Admin kullanıcısı bulunamadığı için başka bir kullanıcı kullanılıyor: {UserId}", userId);
                        }
                        else
                        {
                            return BadRequest(new { success = false, message = "Sistemde profil bilgisi olan kullanıcı bulunamadı" });
                        }
                    }
                }

                // Kullanıcıyı bul
                var user = await _context.Users.FindAsync(userId);
                if (user == null || !user.ProfileId.HasValue)
                {
                    return BadRequest(new { success = false, message = "Belirtilen kullanıcı bulunamadı veya profil bilgisi yok" });
                }

                using (var httpClient = new HttpClient())
                {
                    var webhookUrl = "https://n8n-service-a2yz.onrender.com/webhook/apply-auto"; // n8n oto başvuru webhook URL'i

                    // Kullanıcının özgeçmişini al
                    Resume defaultResume = null;
                    string resumeBase64 = null;
                    Models.Profile? profile = null;

                    try
                    {
                        // Kullanıcının profilini al
                        profile = await _profileRepository.GetProfileWithResumesAsync(user.ProfileId.Value);
                        if (profile != null)
                        {
                            defaultResume = await _profileRepository.GetDefaultResumeAsync(profile.Id);
                            if (defaultResume != null)
                            {
                                // Dosya yolunu oluştur
                                string filePath = Path.Combine(_environment.WebRootPath, defaultResume.FilePath.TrimStart('/'));

                                // Dosyanın varlığını kontrol et
                                if (System.IO.File.Exists(filePath))
                                {
                                    // Dosyayı oku
                                    var fileBytes = System.IO.File.ReadAllBytes(filePath);
                                    resumeBase64 = Convert.ToBase64String(fileBytes);
                                    _logger.LogInformation("Kullanıcı {UserId} için özgeçmiş alındı: {Id} - {FileName}", userId, defaultResume.Id, defaultResume.FileName);
                                }
                                else
                                {
                                    _logger.LogWarning("Kullanıcı {UserId} için özgeçmiş dosyası bulunamadı: {FilePath}", userId, filePath);
                                }
                            }
                            else
                            {
                                _logger.LogWarning("Kullanıcı {UserId} için varsayılan özgeçmiş bulunamadı", userId);
                            }
                        }
                        else
                        {
                            _logger.LogWarning("Kullanıcı {UserId} için profil bulunamadı", userId);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Kullanıcı {UserId} için özgeçmiş alınırken hata oluştu", userId);
                    }

                    var jobData = new
                    {
                        title = job.Title,
                        description = job.Description,
                        employment_type = job.EmploymentType?.ToLower(),
                        company_website = job.CompanyWebsite,
                        contact_email = job.ContactEmail,
                        url = job.Url,
                        location = job.Location?.ToLower(),
                        company = job.Company,
                        userId = userId,
                        resume = defaultResume != null ? new
                        {
                            fileContent = resumeBase64,
                            fileName = defaultResume.FileName,
                            filePath = defaultResume.FilePath,
                            fileType = defaultResume.FileType,
                            fileSize = defaultResume.FileSize,
                            uploadDate = defaultResume.UploadDate
                        } : null,
                        profile = profile != null ? new
                        {
                            fullName = profile.FullName,
                            email = profile.Email,
                            phone = profile.Phone,
                            linkedInUrl = profile.LinkedInUrl,
                            githubUrl = profile.GithubUrl,
                            portfolioUrl = profile.PortfolioUrl,
                            skills = profile.Skills,
                            education = profile.Education,
                            experience = profile.Experience,
                            preferredJobTypes = profile.PreferredJobTypes,
                            preferredLocations = profile.PreferredLocations,
                            position = profile.Position,
                            technologyStack = profile.TechnologyStack
                        } : null
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

                        // n8n webhook'undan dönen yanıtı parse et ve e-posta içeriğini çıkar
                        string emailContent = "";
                        try
                        {
                            // responseContent'i JSON olarak parse et
                            var responseJson = JsonSerializer.Deserialize<JsonElement>(responseContent);

                            // Telegram mesajını al
                            if (responseJson.TryGetProperty("ok", out var okProp) && okProp.GetBoolean() &&
                                responseJson.TryGetProperty("result", out var resultProp) &&
                                resultProp.TryGetProperty("text", out var textProp))
                            {
                                string telegramText = textProp.GetString() ?? "";

                                // "Gönderilen mesaj:" ile başlayan ve "Başvuru Tarihi:" ile biten kısmı al
                                int startIndex = telegramText.IndexOf("Gönderilen mesaj:");
                                int endIndex = telegramText.IndexOf("Başvuru Tarihi:");

                                if (startIndex >= 0 && endIndex > startIndex)
                                {
                                    startIndex += "Gönderilen mesaj:".Length;
                                    emailContent = telegramText.Substring(startIndex, endIndex - startIndex).Trim();
                                    _logger.LogInformation("E-posta içeriği çıkarıldı: {EmailContent}", emailContent);
                                }
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "E-posta içeriği çıkarılırken hata oluştu");
                        }

                        _logger.LogInformation("Otomatik başvuru webhook'u tetiklendi: {Id} - {Title}", job.Id, job.Title);

                        return Ok(new
                        {
                            success = true,
                            message = "Otomatik başvuru işlemi başlatıldı",
                            job,
                            emailContent = emailContent,
                            jobId = job.Id,
                            userId = userId,
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
