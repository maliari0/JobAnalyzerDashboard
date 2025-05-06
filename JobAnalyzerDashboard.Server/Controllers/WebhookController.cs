using Microsoft.AspNetCore.Mvc;
using JobAnalyzerDashboard.Server.Models;
using JobAnalyzerDashboard.Server.Repositories;
using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using Microsoft.AspNetCore.Authorization;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class WebhookController : ControllerBase
    {
        private readonly ILogger<WebhookController> _logger;
        private readonly IJobRepository _jobRepository;

        public WebhookController(ILogger<WebhookController> logger, IJobRepository jobRepository)
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

                var job = new Job
                {
                    Title = model.Title,
                    Description = model.Description,
                    Company = model.Company ?? "Bilinmeyen Şirket",
                    Location = model.Location ?? "Belirtilmemiş",
                    EmploymentType = model.EmploymentType,
                    Salary = model.Salary,
                    PostedDate = DateTime.UtcNow,
                    QualityScore = 3, // Varsayılan değer
                    Source = "API Webhook",
                    IsApplied = false,
                    ActionSuggestion = "store",
                    Category = "other",
                    CompanyWebsite = model.CompanyWebsite,
                    ContactEmail = model.ContactEmail,
                    Url = model.Url,
                    IsJobPosting = true,
                    ParsedMinSalary = 0,
                    Tags = "[]",
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

        // Webhook test endpoint'i
        [HttpGet("test")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public IActionResult TestWebhook()
        {
            try
            {
                _logger.LogInformation("Webhook test endpoint'i çağrıldı");
                return Ok(new { success = true, message = "Webhook bağlantısı başarılı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook test işlenirken hata oluştu");
                return StatusCode(500, new { success = false, message = "Webhook test işlenirken bir hata oluştu" });
            }
        }

        // Ana webhook endpoint'i - n8n için
        [HttpPost]
        public async Task<IActionResult> HandleWebhook([FromBody] object payload)
        {
            try
            {
                if (payload == null)
                {
                    return BadRequest(new { success = false, message = "Geçersiz veri" });
                }

                // Payload'ı JSON olarak al
                var options = new JsonSerializerOptions
                {
                    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
                    WriteIndented = true
                };
                var jsonString = JsonSerializer.Serialize(payload, options);
                _logger.LogInformation("Gelen veri: {JsonData}", jsonString);

                // output alanını kontrol et
                try
                {
                    // UTF-8 kodlamasını kullanarak JSON'ı parse et
                    var jsonDocOptions = new System.Text.Json.JsonDocumentOptions
                    {
                        AllowTrailingCommas = true,
                        CommentHandling = System.Text.Json.JsonCommentHandling.Skip
                    };
                    var jsonDoc = JsonDocument.Parse(jsonString, jsonDocOptions);

                    if (jsonDoc.RootElement.TryGetProperty("output", out var outputElement))
                    {
                        // n8n'den gelen veriyi işle
                        // Gelen veriyi logla
                        _logger.LogInformation("n8n'den gelen veri: {JsonData}", outputElement.ToString());

                        var job = new Job
                        {
                            Title = GetJsonPropertyValue(outputElement, "title", "Başlıksız İş İlanı"),
                            Description = GetJsonPropertyValue(outputElement, "description", "Açıklama yok"),
                            Company = GetJsonPropertyValue(outputElement, "company", "Bilinmeyen Şirket"),
                            Location = GetJsonPropertyValue(outputElement, "location", "Belirtilmemiş"),
                            EmploymentType = GetJsonPropertyValue(outputElement, "employment_type", ""),
                            Salary = GetJsonPropertyValue(outputElement, "salary", ""),
                            PostedDate = DateTime.UtcNow,
                            QualityScore = GetJsonPropertyValueInt(outputElement, "quality_score", 3),
                            Source = "n8n Webhook",
                            IsApplied = false,
                            ActionSuggestion = GetJsonPropertyValue(outputElement, "action_suggestion", "store"),
                            Category = GetJsonPropertyValue(outputElement, "category", "other"),
                            CompanyWebsite = GetJsonPropertyValue(outputElement, "company_website", ""),
                            ContactEmail = GetJsonPropertyValue(outputElement, "contact_email", ""),
                            Url = GetJsonPropertyValue(outputElement, "url", ""),
                            IsJobPosting = GetJsonPropertyValueBool(outputElement, "is_job_posting", true),
                            ParsedMinSalary = ParseSalary(GetJsonPropertyValue(outputElement, "salary", "")),
                            Tags = GetJsonPropertyValueArray(outputElement, "tags")
                        };

                        // Şirket adı boşsa ve company alanı yoksa
                        if (string.IsNullOrEmpty(job.Company) || job.Company == "Bilinmeyen Şirket")
                        {
                            // Başlıktan şirket adını çıkarmaya çalış
                            var titleParts = job.Title.Split(new[] { '-', '|', '@' }, StringSplitOptions.RemoveEmptyEntries);
                            if (titleParts.Length > 1)
                            {
                                job.Company = titleParts[titleParts.Length - 1].Trim();
                            }
                        }

                        // Konum bilgisi boşsa veya "Belirtilmemiş" ise
                        if (string.IsNullOrEmpty(job.Location) || job.Location == "Belirtilmemiş")
                        {
                            // Önce employment_type'dan konum bilgisini çıkarmaya çalış
                            if (!string.IsNullOrEmpty(job.EmploymentType))
                            {
                                if (job.EmploymentType.Contains("remote", StringComparison.OrdinalIgnoreCase) ||
                                    job.EmploymentType.Contains("uzaktan", StringComparison.OrdinalIgnoreCase))
                                {
                                    job.Location = "Remote";
                                }
                                else if (job.EmploymentType.Contains("ofis", StringComparison.OrdinalIgnoreCase) ||
                                         job.EmploymentType.Contains("ofiste", StringComparison.OrdinalIgnoreCase) ||
                                         job.EmploymentType.Contains("office", StringComparison.OrdinalIgnoreCase))
                                {
                                    job.Location = "Ofiste";
                                }
                                else if (job.EmploymentType.Contains("hibrit", StringComparison.OrdinalIgnoreCase) ||
                                         job.EmploymentType.Contains("hybrid", StringComparison.OrdinalIgnoreCase))
                                {
                                    job.Location = "Hibrit";
                                }
                            }

                            // Hala boşsa, açıklamadan konum bilgisini çıkarmaya çalış
                            if (string.IsNullOrEmpty(job.Location) || job.Location == "Belirtilmemiş")
                            {
                                if (job.Description.Contains("remote", StringComparison.OrdinalIgnoreCase) ||
                                    job.Description.Contains("uzaktan", StringComparison.OrdinalIgnoreCase))
                                {
                                    job.Location = "Remote";
                                }
                                else if (job.Description.Contains("ofis", StringComparison.OrdinalIgnoreCase) ||
                                         job.Description.Contains("ofiste", StringComparison.OrdinalIgnoreCase) ||
                                         job.Description.Contains("office", StringComparison.OrdinalIgnoreCase))
                                {
                                    job.Location = "Ofiste";
                                }
                                else if (job.Description.Contains("hibrit", StringComparison.OrdinalIgnoreCase) ||
                                         job.Description.Contains("hybrid", StringComparison.OrdinalIgnoreCase))
                                {
                                    job.Location = "Hibrit";
                                }
                                else if (job.Description.Contains("istanbul", StringComparison.OrdinalIgnoreCase))
                                {
                                    job.Location = "İstanbul";
                                }
                                else if (job.Description.Contains("ankara", StringComparison.OrdinalIgnoreCase))
                                {
                                    job.Location = "Ankara";
                                }
                                else if (job.Description.Contains("izmir", StringComparison.OrdinalIgnoreCase))
                                {
                                    job.Location = "İzmir";
                                }
                            }
                        }

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
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "JSON verisi işlenirken hata oluştu");
                }

                return BadRequest(new { success = false, message = "Geçersiz veri formatı" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Webhook işlenirken hata oluştu");
                return StatusCode(500, new { success = false, message = "Webhook işlenirken bir hata oluştu" });
            }
        }

        // JSON yardımcı metotları
        private string GetJsonPropertyValue(JsonElement element, string propertyName, string defaultValue)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                if (property.ValueKind == JsonValueKind.String)
                {
                    return property.GetString() ?? defaultValue;
                }
                return property.ToString();
            }
            return defaultValue;
        }

        private int GetJsonPropertyValueInt(JsonElement element, string propertyName, int defaultValue)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                if (property.ValueKind == JsonValueKind.Number && property.TryGetInt32(out var value))
                {
                    return value;
                }
            }
            return defaultValue;
        }

        private bool GetJsonPropertyValueBool(JsonElement element, string propertyName, bool defaultValue)
        {
            if (element.TryGetProperty(propertyName, out var property))
            {
                if (property.ValueKind == JsonValueKind.True)
                {
                    return true;
                }
                else if (property.ValueKind == JsonValueKind.False)
                {
                    return false;
                }
                else if (property.ValueKind == JsonValueKind.String)
                {
                    var strValue = property.GetString()?.ToLower();
                    if (strValue == "true" || strValue == "evet" || strValue == "yes" || strValue == "1")
                    {
                        return true;
                    }
                    else if (strValue == "false" || strValue == "hayır" || strValue == "no" || strValue == "0")
                    {
                        return false;
                    }
                }
            }
            return defaultValue;
        }

        private string GetJsonPropertyValueArray(JsonElement element, string propertyName)
        {
            if (element.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.Array)
            {
                return property.ToString();
            }
            return "[]";
        }

        private int ParseSalary(string salaryText)
        {
            if (string.IsNullOrEmpty(salaryText))
                return 0;

            try
            {
                // Maaş aralığı formatlarını işle: "35.000 - 45.000 TL", "35K-45K", "35,000 TL" vb.
                salaryText = salaryText.ToLower()
                    .Replace("tl", "")
                    .Replace("₺", "")
                    .Replace(".", "")
                    .Replace(",", "")
                    .Replace(" ", "");

                // Aralık varsa (örn. 35000-45000)
                if (salaryText.Contains("-") || salaryText.Contains("–"))
                {
                    var parts = salaryText.Split(new[] { '-', '–' }, StringSplitOptions.RemoveEmptyEntries);
                    if (parts.Length >= 1)
                    {
                        // İlk kısmı al (minimum maaş)
                        var minSalaryPart = parts[0].Trim();

                        // "K" veya "k" ile biten değerleri işle (35k -> 35000)
                        if (minSalaryPart.EndsWith("k"))
                        {
                            minSalaryPart = minSalaryPart.TrimEnd('k');
                            if (int.TryParse(minSalaryPart, out int value))
                            {
                                return value * 1000;
                            }
                        }

                        // Doğrudan sayısal değeri işle
                        if (int.TryParse(minSalaryPart, out int directValue))
                        {
                            return directValue;
                        }
                    }
                }
                else
                {
                    // Aralık yoksa, doğrudan değeri işle
                    if (salaryText.EndsWith("k"))
                    {
                        salaryText = salaryText.TrimEnd('k');
                        if (int.TryParse(salaryText, out int value))
                        {
                            return value * 1000;
                        }
                    }

                    if (int.TryParse(salaryText, out int directValue))
                    {
                        return directValue;
                    }
                }
            }
            catch (Exception ex)
            {
                // Hata durumunda loglama yap
                Console.WriteLine($"Maaş ayrıştırma hatası: {ex.Message}");
            }

            return 0;
        }
    }
}

namespace JobAnalyzerDashboard.Server.Models
{
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
