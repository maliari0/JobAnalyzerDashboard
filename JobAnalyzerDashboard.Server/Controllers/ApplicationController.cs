using Microsoft.AspNetCore.Mvc;
using JobAnalyzerDashboard.Server.Models;
using JobAnalyzerDashboard.Server.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationController : ControllerBase
    {
        private static readonly List<Application> _applications = new List<Application>
        {
            new Application
            {
                Id = 1,
                JobId = 2,
                AppliedDate = DateTime.Now.AddDays(-1),
                Status = "Pending",
                AppliedMethod = "Email",
                IsAutoApplied = false,
                SentMessage = "Merhaba,\n\nBackend Developer pozisyonu için başvurumu yapmak istiyorum. 3 yıllık C# ve .NET Core deneyimim bulunmaktadır. Özgeçmişimi ekte bulabilirsiniz.\n\nAli Yılmaz"
            },
            new Application
            {
                Id = 2,
                JobId = 4,
                AppliedDate = DateTime.Now.AddHours(-12),
                Status = "Interview",
                AppliedMethod = "n8n",
                IsAutoApplied = true,
                SentMessage = "Merhaba,\n\nJunior Frontend Developer pozisyonu için başvurumu yapmak istiyorum. React ve TypeScript konularında deneyimim bulunmaktadır. Özgeçmişimi ekte bulabilirsiniz.\n\nAli Yılmaz",
                ResponseDetails = "Mülakat için davet edildiniz. Tarih: " + DateTime.Now.AddDays(3).ToString("dd/MM/yyyy HH:mm"),
                ResponseDate = DateTime.Now.AddHours(-2),
                CvAttached = true,
                TelegramNotificationSent = "Otomatik Başvuru Yapıldı! Başlık: Junior Frontend Developer (React)"
            }
        };

        private readonly ILogger<ApplicationController> _logger;
        private readonly EmailService _emailService;
        private static readonly List<Job> _jobs; // Gerçek uygulamada veritabanı kullanılacak

        static ApplicationController()
        {
            // JobController'dan _jobs listesini almak için reflection kullanılıyor
            // Gerçek uygulamada bu bir veritabanı olacağı için değişiklik yapılacak.
            var jobControllerType = typeof(JobController);
            var jobsField = jobControllerType.GetField("_jobs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            _jobs = jobsField?.GetValue(null) as List<Job> ?? new List<Job>();

            foreach (var application in _applications)
            {
                application.Job = _jobs.FirstOrDefault(j => j.Id == application.JobId);
            }
        }

        public ApplicationController(ILogger<ApplicationController> logger, EmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }

        [HttpGet]
        public IActionResult GetAll([FromQuery] ApplicationFilterModel filter)
        {
            var query = _applications.AsQueryable();

            if (filter.JobId.HasValue)
            {
                query = query.Where(a => a.JobId == filter.JobId.Value);
            }

            if (!string.IsNullOrEmpty(filter.Status))
            {
                query = query.Where(a => a.Status.ToLower() == filter.Status.ToLower());
            }

            if (!string.IsNullOrEmpty(filter.AppliedMethod))
            {
                query = query.Where(a => a.AppliedMethod.ToLower() == filter.AppliedMethod.ToLower());
            }

            if (filter.IsAutoApplied.HasValue)
            {
                query = query.Where(a => a.IsAutoApplied == filter.IsAutoApplied.Value);
            }

            if (filter.FromDate.HasValue)
            {
                query = query.Where(a => a.AppliedDate >= filter.FromDate.Value);
            }

            if (filter.ToDate.HasValue)
            {
                query = query.Where(a => a.AppliedDate <= filter.ToDate.Value);
            }

            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                switch (filter.SortBy.ToLower())
                {
                    case "date":
                        query = filter.SortDirection?.ToLower() == "desc" ?
                            query.OrderByDescending(a => a.AppliedDate) :
                            query.OrderBy(a => a.AppliedDate);
                        break;
                    case "status":
                        query = filter.SortDirection?.ToLower() == "desc" ?
                            query.OrderByDescending(a => a.Status) :
                            query.OrderBy(a => a.Status);
                        break;
                    default:
                        query = query.OrderByDescending(a => a.AppliedDate);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(a => a.AppliedDate);
            }

            var result = query.ToList();

            foreach (var application in result)
            {
                if (application.Job == null && application.JobId > 0)
                {
                    application.Job = _jobs.FirstOrDefault(j => j.Id == application.JobId);
                }
            }

            return Ok(result);
        }

        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            var stats = new
            {
                TotalApplications = _applications.Count,
                PendingApplications = _applications.Count(a => a.Status == "Pending"),
                AcceptedApplications = _applications.Count(a => a.Status == "Accepted"),
                RejectedApplications = _applications.Count(a => a.Status == "Rejected"),
                InterviewApplications = _applications.Count(a => a.Status == "Interview"),
                AutoAppliedCount = _applications.Count(a => a.IsAutoApplied),
                ManualAppliedCount = _applications.Count(a => !a.IsAutoApplied),
                ApplicationMethodBreakdown = _applications
                    .GroupBy(a => a.AppliedMethod)
                    .Select(g => new { Method = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList(),
                StatusBreakdown = _applications
                    .GroupBy(a => a.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList(),
                RecentApplications = _applications
                    .OrderByDescending(a => a.AppliedDate)
                    .Take(5)
                    .Select(a => new {
                        a.Id,
                        a.JobId,
                        JobTitle = a.Job?.Title ?? "Bilinmeyen İlan",
                        a.AppliedDate,
                        a.Status
                    })
                    .ToList()
            };

            return Ok(stats);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var application = _applications.FirstOrDefault(a => a.Id == id);
            if (application == null)
            {
                return NotFound();
            }

            if (application.Job == null && application.JobId > 0)
            {
                application.Job = _jobs.FirstOrDefault(j => j.Id == application.JobId);
            }

            return Ok(application);
        }

        [HttpPost]
        public IActionResult Create(ApplicationCreateModel model)
        {
            if (model == null)
            {
                return BadRequest("Geçersiz başvuru verisi");
            }

            // İş ilanını kontrol et
            var job = _jobs.FirstOrDefault(j => j.Id == model.JobId);
            if (job == null)
            {
                return BadRequest("Geçersiz iş ilanı ID'si");
            }

            // Zaten başvuru yapılmış mı kontrol et
            if (_applications.Any(a => a.JobId == model.JobId))
            {
                return BadRequest("Bu iş ilanına zaten başvuru yapılmış");
            }

            // Yeni başvuru oluştur
            var application = new Application
            {
                Id = _applications.Count > 0 ? _applications.Max(a => a.Id) + 1 : 1,
                JobId = model.JobId,
                AppliedDate = DateTime.Now,
                Status = "Pending",
                AppliedMethod = model.AppliedMethod ?? "Manual",
                SentMessage = model.Message ?? "",
                IsAutoApplied = model.IsAutoApplied,
                CvAttached = model.CvAttached,
                Job = job
            };

            // İş ilanını güncelle
            job.IsApplied = true;
            job.AppliedDate = DateTime.Now;

            // Başvuruyu ekle
            _applications.Add(application);

            _logger.LogInformation("Yeni başvuru oluşturuldu: {id} - {title}", application.Id, job.Title);

            return CreatedAtAction(nameof(GetById), new { id = application.Id }, application);
        }

        [HttpPut("{id}/status")]
        public IActionResult UpdateStatus(int id, [FromBody] StatusUpdateModel model)
        {
            var application = _applications.FirstOrDefault(a => a.Id == id);
            if (application == null)
            {
                return NotFound();
            }

            application.Status = model.Status;
            application.ResponseDate = DateTime.Now;
            application.ResponseDetails = model.Details ?? "";

            _logger.LogInformation("Başvuru durumu güncellendi: {id} - {status}", id, model.Status);

            return Ok(application);
        }

        [HttpPost("auto-apply")]
        public async Task<IActionResult> AutoApply(AutoApplyModel model)
        {
            if (model == null || model.JobId <= 0)
            {
                return BadRequest("Geçersiz başvuru verisi");
            }

            // İş ilanını kontrol et
            var job = _jobs.FirstOrDefault(j => j.Id == model.JobId);
            if (job == null)
            {
                return BadRequest("Geçersiz iş ilanı ID'si");
            }

            // Zaten başvuru yapılmış mı kontrol et
            if (_applications.Any(a => a.JobId == model.JobId))
            {
                return BadRequest("Bu iş ilanına zaten başvuru yapılmış");
            }

            // Yeni başvuru oluştur
            var application = new Application
            {
                Id = _applications.Count > 0 ? _applications.Max(a => a.Id) + 1 : 1,
                JobId = model.JobId,
                AppliedDate = DateTime.Now,
                Status = "Pending",
                AppliedMethod = "n8n",
                SentMessage = model.Message ?? "Otomatik oluşturulan başvuru mesajı",
                IsAutoApplied = true,
                CvAttached = true,
                TelegramNotificationSent = model.TelegramNotification ?? "",
                NotionPageId = model.NotionPageId ?? "",
                Job = job
            };

            // Profil bilgilerini al
            var loggerFactory = HttpContext.RequestServices.GetService<ILoggerFactory>();
            var profileLogger = loggerFactory?.CreateLogger<ProfileController>();
            var environment = HttpContext.RequestServices.GetService<IWebHostEnvironment>();

            if (profileLogger == null || environment == null)
            {
                return StatusCode(500, new { success = false, message = "Servis bağımlılıkları alınamadı" });
            }

            var profileController = new ProfileController(profileLogger, environment);
            var profileResult = profileController.Get() as OkObjectResult;
            var profile = profileResult?.Value as Profile;

            if (profile == null)
            {
                return BadRequest(new { success = false, message = "Profil bilgileri bulunamadı" });
            }

            // Varsayılan özgeçmişi al
            Resume? defaultResume = null;
            var resumesResult = profileController.GetResumes() as OkObjectResult;
            var resumes = resumesResult?.Value as List<Resume>;

            if (resumes != null && resumes.Any())
            {
                defaultResume = resumes.FirstOrDefault(r => r.IsDefault) ?? resumes.First();
            }

            // Başvuru mesajını hazırla
            string? messageInput = model?.Message;
            string message = string.Empty;

            if (string.IsNullOrEmpty(messageInput))
            {
                message = $"Merhaba,\n\n{job.Title} pozisyonu için başvurumu yapmak istiyorum. Özgeçmişimi ekte bulabilirsiniz.\n\nSaygılarımla,\n{profile.FullName}";
            }
            else
            {
                message = messageInput;
            }

            // Başvuru mesajını güncelle
            application.SentMessage = message;

            // E-posta gönder
            string? attachmentPath = null;
            if (defaultResume != null)
            {
                attachmentPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", defaultResume.FilePath.TrimStart('/'));
                application.CvAttached = true;
            }

            bool emailSent = false;
            if (!string.IsNullOrEmpty(job.ContactEmail))
            {
                string subject = $"Başvuru: {job.Title}";
                emailSent = await _emailService.SendEmailAsync(job.ContactEmail, subject, message, attachmentPath);

                _logger.LogInformation("Başvuru e-postası gönderildi: {email}, İlan: {title}", job.ContactEmail, job.Title);
            }

            // İş ilanını güncelle
            job.IsApplied = true;
            job.AppliedDate = DateTime.Now;

            // Başvuruyu ekle
            _applications.Add(application);

            _logger.LogInformation("Otomatik başvuru oluşturuldu: {id} - {title}", application.Id, job.Title);

            return Ok(new {
                success = true,
                message = "Otomatik başvuru başarıyla yapıldı" + (emailSent ? " ve e-posta gönderildi" : ""),
                applicationId = application.Id,
                jobId = job.Id,
                jobTitle = job.Title,
                emailSent = emailSent,
                resumeAttached = defaultResume != null
            });
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var application = _applications.FirstOrDefault(a => a.Id == id);
            if (application == null)
            {
                return NotFound();
            }

            // Başvuruyu sil
            _applications.Remove(application);

            // İş ilanını güncelle (eğer başka başvuru yoksa)
            if (!_applications.Any(a => a.JobId == application.JobId))
            {
                var job = _jobs.FirstOrDefault(j => j.Id == application.JobId);
                if (job != null)
                {
                    job.IsApplied = false;
                    job.AppliedDate = null;
                }
            }

            _logger.LogInformation("Başvuru silindi: {id}", id);

            return Ok(new { success = true, message = "Başvuru başarıyla silindi" });
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
    }

    public class ApplicationCreateModel
    {
        public int JobId { get; set; }
        public string? AppliedMethod { get; set; }
        public string? Message { get; set; }
        public bool IsAutoApplied { get; set; } = false;
        public bool CvAttached { get; set; } = false;
    }

    public class StatusUpdateModel
    {
        public string Status { get; set; } = "Pending";
        public string? Details { get; set; }
    }

    public class AutoApplyModel
    {
        public int JobId { get; set; }
        public string? Message { get; set; }
        public string? TelegramNotification { get; set; }
        public string? NotionPageId { get; set; }
    }
}
