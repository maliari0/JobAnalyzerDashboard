using Microsoft.AspNetCore.Mvc;
using JobAnalyzerDashboard.Server.Models;
using JobAnalyzerDashboard.Server.Repositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IJobRepository _jobRepository;

        public WebhookController(
            ILogger<WebhookController> logger,
            IJobRepository jobRepository)
        {
            _logger = logger;
            _jobRepository = jobRepository;
        }

        [HttpPost("job-intake")]
        public async Task<IActionResult> JobIntake([FromBody] JobIntakeModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new { success = false, message = "Geçersiz veri" });
                }

                _logger.LogInformation("Webhook job-intake çağrıldı: {Title}", model.Title);

                // Yeni iş ilanı oluştur
                var job = new Job
                {
                    Title = model.Title,
                    Description = model.Description,
                    Company = model.Company ?? "Bilinmeyen Şirket",
                    Location = model.Location ?? "Belirtilmemiş",
                    EmploymentType = model.EmploymentType,
                    Salary = model.Salary,
                    PostedDate = DateTime.UtcNow,
                    QualityScore = 3, // Varsayılan kalite puanı
                    Source = "Webhook",
                    IsApplied = false,
                    ActionSuggestion = "store",
                    Category = "other", // Varsayılan kategori
                    CompanyWebsite = model.CompanyWebsite,
                    ContactEmail = model.ContactEmail,
                    Url = model.Url,
                    IsJobPosting = true,
                    ParsedMinSalary = 0 // Varsayılan değer
                };

                await _jobRepository.AddAsync(job);
                await _jobRepository.SaveChangesAsync();

                _logger.LogInformation("Yeni iş ilanı oluşturuldu: {Id} - {Title}", job.Id, job.Title);

                return Ok(new
                {
                    success = true,
                    message = "İş ilanı başarıyla alındı",
                    jobId = job.Id,
                    jobTitle = job.Title
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook job-intake işlenirken hata oluştu");
                return StatusCode(500, new { success = false, message = "İş ilanı işlenirken bir hata oluştu" });
            }
        }
    }

    public class JobIntakeModel
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? Company { get; set; }
        public string? Location { get; set; }
        public string EmploymentType { get; set; } = string.Empty;
        public string Salary { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string CompanyWebsite { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
