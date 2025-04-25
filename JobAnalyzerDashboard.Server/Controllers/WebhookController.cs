using Microsoft.AspNetCore.Mvc;
using JobAnalyzerDashboard.Server.Models;
using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Linq;
using System.Threading.Tasks;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private static readonly List<Job> _jobs; // Gerçek uygulamada veritabanı kullanılacak

        static WebhookController()
        {
            // JobController'dan _jobs listesini almak için reflection kullanıyoruz
            // Gerçek uygulamada bu bir veritabanı olacak
            var jobControllerType = typeof(JobController);
            var jobsField = jobControllerType.GetField("_jobs", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static);
            _jobs = jobsField?.GetValue(null) as List<Job> ?? new List<Job>();
        }

        public WebhookController(ILogger<WebhookController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        public IActionResult ReceiveWebhook([FromBody] JsonElement data)
        {
            try
            {
                // Webhook verilerini detaylı logla
                _logger.LogInformation("Webhook alındı: {data}", data.ToString());
                _logger.LogInformation("Webhook headers: {headers}", JsonSerializer.Serialize(Request.Headers.ToDictionary(h => h.Key, h => h.Value.ToString())));
                _logger.LogInformation("Webhook content type: {contentType}", Request.ContentType);

                // n8n'den gelen verileri işle
                return ProcessWebhookData(data);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook işlenirken hata oluştu");
                return StatusCode(500, new { success = false, message = "Webhook işlenirken hata oluştu", error = ex.Message });
            }
        }

        [HttpPost("job-intake")]
        public IActionResult JobIntake([FromBody] JobIntakeModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest("Geçersiz veri formatı");
                }

                _logger.LogInformation("Job intake alındı: {title}", model.Title);

                // Yeni iş ilanı oluştur
                var job = new Job
                {
                    Id = _jobs.Count > 0 ? _jobs.Max(j => j.Id) + 1 : 1,
                    Title = model.Title,
                    Description = model.Description,
                    Company = model.Company ?? "Belirtilmemiş",
                    Location = model.Location ?? "Belirtilmemiş",
                    EmploymentType = model.EmploymentType,
                    Salary = model.Salary,
                    PostedDate = DateTime.Now,
                    QualityScore = 0, // AI tarafından doldurulacak
                    Source = "Webhook",
                    ContactEmail = model.ContactEmail,
                    CompanyWebsite = model.CompanyWebsite,
                    Url = model.Url
                };

                // Listeye ekle
                _jobs.Add(job);

                return Ok(new { success = true, message = "İş ilanı başarıyla alındı", jobId = job.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Job intake işlenirken hata oluştu");
                return StatusCode(500, new { success = false, message = "Job intake işlenirken hata oluştu" });
            }
        }

        [HttpGet("test")]
        public IActionResult Test()
        {
            // Test amaçlı endpoint
            return Ok(new { success = true, message = "Webhook test endpoint'i çalışıyor", timestamp = DateTime.Now });
        }

        private IActionResult ProcessWebhookData(JsonElement data)
        {
            try
            {
                // Gelen veriyi detaylı logla
                _logger.LogInformation("ProcessWebhookData başladı: {data}", data.ToString());

                // n8n'den gelen veriyi işle
                if (data.TryGetProperty("output", out JsonElement output))
                {
                    _logger.LogInformation("Output bulundu: {output}", output.ToString());

                    // AI Agent çıktısını işle
                    string title = GetStringProperty(output, "title");
                    string description = GetStringProperty(output, "description");
                    int qualityScore = GetIntProperty(output, "quality_score");
                    string actionSuggestion = GetStringProperty(output, "action_suggestion");
                    string category = GetStringProperty(output, "category");
                    string employmentType = GetStringProperty(output, "employment_type");
                    string salary = GetStringProperty(output, "salary");
                    string companyWebsite = GetStringProperty(output, "company_website");
                    string contactEmail = GetStringProperty(output, "contact_email");
                    string url = GetStringProperty(output, "url");
                    bool isJobPosting = GetStringProperty(output, "is_job_posting").ToLower() == "evet";

                    _logger.LogInformation("Parsed data - Title: {title}, Quality: {quality}, Action: {action}, Category: {category}",
                        title, qualityScore, actionSuggestion, category);

                    // Tags'i işle
                    List<string> tags = new List<string>();
                    if (output.TryGetProperty("tags", out JsonElement tagsElement) && tagsElement.ValueKind == JsonValueKind.Array)
                    {
                        foreach (JsonElement tag in tagsElement.EnumerateArray())
                        {
                            if (tag.ValueKind == JsonValueKind.String)
                            {
                                tags.Add(tag.GetString() ?? "");
                            }
                        }
                    }

                    // Yeni iş ilanı oluştur
                    var job = new Job
                    {
                        Id = _jobs.Count > 0 ? _jobs.Max(j => j.Id) + 1 : 1,
                        Title = title,
                        Description = description,
                        QualityScore = qualityScore,
                        ActionSuggestion = actionSuggestion,
                        Category = category,
                        Tags = tags,
                        EmploymentType = employmentType,
                        Salary = salary,
                        CompanyWebsite = companyWebsite,
                        ContactEmail = contactEmail,
                        Url = url,
                        IsJobPosting = isJobPosting,
                        PostedDate = DateTime.Now,
                        Source = "n8n"
                    };

                    // Maaş parsing
                    if (!string.IsNullOrEmpty(salary))
                    {
                        var match = System.Text.RegularExpressions.Regex.Match(salary, @"(\d{2,3}[.,]?\d{0,3})");
                        if (match.Success)
                        {
                            string numberStr = match.Groups[1].Value.Replace(".", "").Replace(",", ".");
                            if (float.TryParse(numberStr, out float minSalary))
                            {
                                job.ParsedMinSalary = (int)minSalary;
                            }
                        }
                    }

                    // Listeye ekle
                    _jobs.Add(job);

                    return Ok(new { success = true, message = "Webhook başarıyla işlendi", jobId = job.Id });
                }

                return BadRequest(new { success = false, message = "Geçersiz veri formatı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook verisi işlenirken hata oluştu");
                return StatusCode(500, new { success = false, message = "Webhook verisi işlenirken hata oluştu" });
            }
        }

        private string GetStringProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement property) && property.ValueKind == JsonValueKind.String)
            {
                return property.GetString() ?? "";
            }
            return "";
        }

        private int GetIntProperty(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out JsonElement property))
            {
                if (property.ValueKind == JsonValueKind.Number)
                {
                    return property.GetInt32();
                }
                else if (property.ValueKind == JsonValueKind.String && int.TryParse(property.GetString(), out int result))
                {
                    return result;
                }
            }
            return 0;
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
