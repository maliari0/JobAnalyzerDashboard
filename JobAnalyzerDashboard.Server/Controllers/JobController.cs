using Microsoft.AspNetCore.Mvc;
using JobAnalyzerDashboard.Server.Models;
using JobAnalyzerDashboard.Server.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobController : ControllerBase
    {
        private readonly ILogger<JobController> _logger;
        private readonly EmailService _emailService;

        public JobController(ILogger<JobController> logger, EmailService emailService)
        {
            _logger = logger;
            _emailService = emailService;
        }
        private static readonly List<Job> _jobs = new List<Job>
        {
            new Job
            {
                Id = 1,
                Title = "Frontend Developer",
                Description = "React.js bilen bir geliştirici arıyoruz. Modern web uygulamaları geliştirme deneyimi olan, UI/UX konularında bilgili adaylar arıyoruz.",
                Company = "TechSoft A.Ş.",
                Location = "İstanbul, Türkiye",
                EmploymentType = "Remote",
                Salary = "35.000 - 45.000 TL",
                PostedDate = DateTime.Now.AddDays(-5),
                ApplicationDeadline = DateTime.Now.AddDays(15),
                QualityScore = 4,
                Source = "Email",
                IsApplied = false,
                Category = "frontend",
                Tags = new List<string> { "React", "Remote", "UI/UX" },
                ActionSuggestion = "sakla",
                CompanyWebsite = "https://techsoft.com.tr",
                ContactEmail = "ik@techsoft.com.tr",
                Url = "https://techsoft.com.tr/kariyer",
                ParsedMinSalary = 35000
            },
            new Job
            {
                Id = 2,
                Title = "Backend Developer",
                Description = "C# ve .NET Core deneyimi olan, RESTful API tasarımı konusunda bilgili backend geliştiriciler arıyoruz.",
                Company = "Yazılım Evi Ltd.",
                Location = "Ankara, Türkiye",
                EmploymentType = "Full-time",
                Salary = "40.000 - 50.000 TL",
                PostedDate = DateTime.Now.AddDays(-2),
                ApplicationDeadline = DateTime.Now.AddDays(20),
                QualityScore = 5,
                Source = "Webhook",
                IsApplied = true,
                AppliedDate = DateTime.Now.AddDays(-1),
                Category = "backend",
                Tags = new List<string> { "C#", ".NET Core", "API" },
                ActionSuggestion = "bildir",
                CompanyWebsite = "https://yazilimevi.com.tr",
                ContactEmail = "kariyer@yazilimevi.com.tr",
                Url = "https://yazilimevi.com.tr/is-ilanlari/backend-developer",
                ParsedMinSalary = 40000
            },
            new Job
            {
                Id = 3,
                Title = "Full Stack Developer",
                Description = "Angular ve ASP.NET Core deneyimi olan, veritabanı tasarımı konusunda bilgili full stack geliştiriciler arıyoruz.",
                Company = "Global Tech Inc.",
                Location = "İzmir, Türkiye",
                EmploymentType = "Hybrid",
                Salary = "45.000 - 55.000 TL",
                PostedDate = DateTime.Now.AddDays(-7),
                ApplicationDeadline = DateTime.Now.AddDays(10),
                QualityScore = 3,
                Source = "Email",
                IsApplied = false,
                Category = "fullstack",
                Tags = new List<string> { "Angular", "ASP.NET Core", "SQL" },
                ActionSuggestion = "ilgisiz",
                CompanyWebsite = "https://globaltech.com",
                ContactEmail = "hr@globaltech.com",
                Url = "https://globaltech.com/careers",
                ParsedMinSalary = 45000
            },
            new Job
            {
                Id = 4,
                Title = "Junior Frontend Developer (React)",
                Description = "React.js ile modern web uygulamaları geliştirecek, takımımıza katılacak junior seviye frontend geliştirici arıyoruz. TypeScript deneyimi bir artı olacaktır.",
                Company = "StartupX",
                Location = "Remote",
                EmploymentType = "Remote",
                Salary = "30.000 - 40.000 TL",
                PostedDate = DateTime.Now.AddDays(-1),
                ApplicationDeadline = DateTime.Now.AddDays(30),
                QualityScore = 5,
                Source = "n8n",
                IsApplied = false,
                Category = "frontend",
                Tags = new List<string> { "React", "TypeScript", "Remote", "Junior" },
                ActionSuggestion = "sakla",
                CompanyWebsite = "https://startupx.io",
                ContactEmail = "jobs@startupx.io",
                Url = "https://startupx.io/careers/junior-frontend",
                ParsedMinSalary = 30000,
                IsJobPosting = true
            }
        };

        [HttpGet]
        public IActionResult GetAll([FromQuery] JobFilterModel filter)
        {
            var query = _jobs.AsQueryable();

            // Filtreleme işlemleri
            if (!string.IsNullOrEmpty(filter.Category))
            {
                query = query.Where(j => j.Category.ToLower() == filter.Category.ToLower());
            }

            if (filter.MinQualityScore.HasValue)
            {
                query = query.Where(j => j.QualityScore >= filter.MinQualityScore.Value);
            }

            if (filter.MaxQualityScore.HasValue)
            {
                query = query.Where(j => j.QualityScore <= filter.MaxQualityScore.Value);
            }

            if (!string.IsNullOrEmpty(filter.Tag))
            {
                query = query.Where(j => j.Tags.Any(t => t.ToLower() == filter.Tag.ToLower()));
            }

            if (!string.IsNullOrEmpty(filter.ActionSuggestion))
            {
                query = query.Where(j => j.ActionSuggestion.ToLower() == filter.ActionSuggestion.ToLower());
            }

            if (filter.MinSalary.HasValue)
            {
                query = query.Where(j => j.ParsedMinSalary >= filter.MinSalary.Value);
            }

            if (!string.IsNullOrEmpty(filter.EmploymentType))
            {
                query = query.Where(j => j.EmploymentType.ToLower().Contains(filter.EmploymentType.ToLower()));
            }

            if (filter.IsApplied.HasValue)
            {
                query = query.Where(j => j.IsApplied == filter.IsApplied.Value);
            }

            // Sıralama
            if (!string.IsNullOrEmpty(filter.SortBy))
            {
                switch (filter.SortBy.ToLower())
                {
                    case "date":
                        query = filter.SortDirection?.ToLower() == "desc" ?
                            query.OrderByDescending(j => j.PostedDate) :
                            query.OrderBy(j => j.PostedDate);
                        break;
                    case "quality":
                        query = filter.SortDirection?.ToLower() == "desc" ?
                            query.OrderByDescending(j => j.QualityScore) :
                            query.OrderBy(j => j.QualityScore);
                        break;
                    case "salary":
                        query = filter.SortDirection?.ToLower() == "desc" ?
                            query.OrderByDescending(j => j.ParsedMinSalary) :
                            query.OrderBy(j => j.ParsedMinSalary);
                        break;
                    default:
                        query = query.OrderByDescending(j => j.PostedDate);
                        break;
                }
            }
            else
            {
                // Varsayılan sıralama: En yeni ilanlar önce
                query = query.OrderByDescending(j => j.PostedDate);
            }

            return Ok(query.ToList());
        }

        [HttpGet("categories")]
        public IActionResult GetCategories()
        {
            var categories = _jobs
                .Where(j => !string.IsNullOrEmpty(j.Category))
                .Select(j => j.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToList();

            return Ok(categories);
        }

        [HttpGet("tags")]
        public IActionResult GetTags()
        {
            var tags = _jobs
                .SelectMany(j => j.Tags)
                .Distinct()
                .OrderBy(t => t)
                .ToList();

            return Ok(tags);
        }

        [HttpGet("stats")]
        public IActionResult GetStats()
        {
            var stats = new
            {
                TotalJobs = _jobs.Count,
                AppliedJobs = _jobs.Count(j => j.IsApplied),
                HighQualityJobs = _jobs.Count(j => j.QualityScore >= 4),
                AverageSalary = _jobs.Where(j => j.ParsedMinSalary > 0).Average(j => j.ParsedMinSalary),
                CategoryBreakdown = _jobs
                    .GroupBy(j => j.Category)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList(),
                QualityScoreBreakdown = _jobs
                    .GroupBy(j => j.QualityScore)
                    .Select(g => new { Score = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Score)
                    .ToList()
            };

            return Ok(stats);
        }

        [HttpGet("{id}")]
        public IActionResult GetById(int id)
        {
            var job = _jobs.FirstOrDefault(j => j.Id == id);
            if (job == null)
            {
                return NotFound();
            }

            return Ok(job);
        }

        [HttpPost("apply/{id}")]
        public async Task<IActionResult> Apply(int id, [FromBody] ApplicationRequestModel model)
        {
            try
            {
                var job = _jobs.FirstOrDefault(j => j.Id == id);
                if (job == null)
                {
                    return NotFound(new { success = false, message = "İş ilanı bulunamadı" });
                }

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
                if (model?.AttachCV == true)
                {
                    var resumesResult = profileController.GetResumes() as OkObjectResult;
                    var resumes = resumesResult?.Value as List<Resume>;

                    if (resumes != null && resumes.Any())
                    {
                        defaultResume = resumes.FirstOrDefault(r => r.IsDefault) ?? resumes.First();
                    }
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

                // E-posta gönder
                string? attachmentPath = null;
                if (defaultResume != null)
                {
                    attachmentPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", defaultResume.FilePath.TrimStart('/'));
                }

                bool emailSent = false;
                if (!string.IsNullOrEmpty(job.ContactEmail))
                {
                    string subject = $"Başvuru: {job.Title}";
                    emailSent = await _emailService.SendEmailAsync(job.ContactEmail, subject, message, attachmentPath);

                    _logger.LogInformation("Başvuru e-postası gönderildi: {email}, İlan: {title}", job.ContactEmail, job.Title);
                }


                job.IsApplied = true;
                job.AppliedDate = DateTime.Now;


                try
                {
                    var appLoggerFactory = HttpContext.RequestServices.GetService<ILoggerFactory>();
                    var applicationLogger = appLoggerFactory?.CreateLogger<ApplicationController>();

                    if (applicationLogger != null)
                    {
                        var applicationController = new ApplicationController(applicationLogger, _emailService);


                        var applicationModel = new ApplicationCreateModel
                        {
                            JobId = job.Id,
                            AppliedMethod = model?.Method ?? "Manual",
                            Message = message,
                            IsAutoApplied = false,
                            CvAttached = defaultResume != null
                        };


                        applicationController.Create(applicationModel);

                        _logger.LogInformation("Başvuru geçmişine eklendi: {Id} - {Title}", job.Id, job.Title);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Başvuru geçmişine eklenirken hata oluştu: {Id}", job.Id);

                }

                return Ok(new {
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
                _logger.LogError(ex, "Başvuru yapılırken hata oluştu: {id}", id);
                return StatusCode(500, new { success = false, message = "Başvuru yapılırken bir hata oluştu" });
            }
        }

        [HttpPost("webhook-notify/{id}")]
        public IActionResult WebhookNotify(int id)
        {
            var job = _jobs.FirstOrDefault(j => j.Id == id);
            if (job == null)
            {
                return NotFound();
            }

            // Burada n8n webhook'una bildirim gönderilecek
            // Şimdilik sadece başarılı yanıt döndürüyoruz

            return Ok(new { success = true, message = "Webhook bildirimi gönderildi", jobId = id });
        }

        [HttpDelete("{id}")]
        public IActionResult DeleteJob(int id)
        {
            var job = _jobs.FirstOrDefault(j => j.Id == id);
            if (job == null)
            {
                return NotFound(new { success = false, message = "İş ilanı bulunamadı" });
            }

            // İş ilanını listeden kaldır
            _jobs.Remove(job);

            _logger.LogInformation("İş ilanı silindi: {Id} - {Title}", job.Id, job.Title);

            return Ok(new { success = true, message = "İş ilanı başarıyla silindi" });
        }

        [HttpPost("auto-apply/{id}")]
        public async Task<IActionResult> AutoApply(int id)
        {
            try
            {
                var job = _jobs.FirstOrDefault(j => j.Id == id);
                if (job == null)
                {
                    return NotFound(new { success = false, message = "İş ilanı bulunamadı" });
                }

                using (var httpClient = new HttpClient())
                {

                    var webhookUrl = "https://n8n-service-a2yz.onrender.com/webhook/apply-auto";


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


                    var jsonData = System.Text.Json.JsonSerializer.Serialize(jobData);
                    var content = new StringContent(
                        jsonData,
                        System.Text.Encoding.UTF8,
                        "application/json");


                    _logger.LogInformation("Webhook'a gönderilen veri: {jsonData}", jsonData);
                    _logger.LogInformation("Webhook URL: {webhookUrl}", webhookUrl);


                    var response = await httpClient.PostAsync(webhookUrl, content);


                    var responseContent = await response.Content.ReadAsStringAsync();
                    _logger.LogInformation("Webhook yanıtı: {statusCode}, İçerik: {responseContent}",
                        response.StatusCode, responseContent);

                    if (response.IsSuccessStatusCode)
                    {

                        job.IsApplied = true;
                        job.AppliedDate = DateTime.Now;


                        try
                        {
                            var loggerFactory = HttpContext.RequestServices.GetService<ILoggerFactory>();
                            var applicationLogger = loggerFactory?.CreateLogger<ApplicationController>();

                            if (applicationLogger != null)
                            {
                                var applicationController = new ApplicationController(applicationLogger, _emailService);


                                var applicationModel = new ApplicationCreateModel
                                {
                                    JobId = job.Id,
                                    AppliedMethod = "n8n",
                                    Message = $"n8n webhook'u ile otomatik başvuru yapıldı: {job.Title}",
                                    IsAutoApplied = true,
                                    CvAttached = true
                                };


                                applicationController.Create(applicationModel);

                                _logger.LogInformation("Başvuru geçmişine eklendi: {Id} - {Title}", job.Id, job.Title);
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogError(ex, "Başvuru geçmişine eklenirken hata oluştu: {Id}", job.Id);

                        }

                        _logger.LogInformation("Otomatik başvuru webhook'u tetiklendi: {Id} - {Title}", job.Id, job.Title);

                        return Ok(new {
                            success = true,
                            message = "Otomatik başvuru işlemi başlatıldı",
                            job,
                            webhookResponse = responseContent
                        });
                    }
                    else
                    {
                        _logger.LogError("Webhook isteği başarısız: {statusCode}, Yanıt: {responseContent}",
                            response.StatusCode, responseContent);



                        job.IsApplied = true;
                        job.AppliedDate = DateTime.Now;

                        return StatusCode(500, new {
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
                _logger.LogError(ex, "Otomatik başvuru yapılırken hata oluştu: {id}", id);
                return StatusCode(500, new { success = false, message = "Otomatik başvuru yapılırken bir hata oluştu" });
            }
        }
    }

    public class JobFilterModel
    {
        public string? Category { get; set; }
        public int? MinQualityScore { get; set; }
        public int? MaxQualityScore { get; set; }
        public string? Tag { get; set; }
        public string? ActionSuggestion { get; set; }
        public int? MinSalary { get; set; }
        public string? EmploymentType { get; set; }
        public bool? IsApplied { get; set; }
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
