using Microsoft.AspNetCore.Mvc;
using JobAnalyzerDashboard.Server.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using System;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/test-email")]
    public class TestEmailController : ControllerBase
    {
        private readonly EmailService _emailService;
        private readonly ILogger<TestEmailController> _logger;

        public TestEmailController(EmailService emailService, ILogger<TestEmailController> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        [HttpGet("send")]
        public async Task<IActionResult> SendTestEmail([FromQuery] string to)
        {
            try
            {
                if (string.IsNullOrEmpty(to))
                {
                    to = "aliari.test@gmail.com"; // Varsayılan alıcı
                }

                var subject = "Test E-postası - " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                var body = @"
                    <h2>Bu bir test e-postasıdır</h2>
                    <p>Bu e-posta, e-posta gönderim sisteminin çalışıp çalışmadığını test etmek için gönderilmiştir.</p>
                    <p>Gönderim zamanı: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + @"</p>
                    <p>Saygılarımızla,<br>Job Analyzer Dashboard Ekibi</p>
                ";

                var result = await _emailService.SendEmailAsync(to, subject, body);

                if (result)
                {
                    _logger.LogInformation("Test e-postası başarıyla gönderildi: {To}", to);
                    return Ok(new { success = true, message = "Test e-postası başarıyla gönderildi." });
                }
                else
                {
                    _logger.LogWarning("Test e-postası gönderilemedi: {To}", to);
                    return BadRequest(new { success = false, message = "Test e-postası gönderilemedi." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Test e-postası gönderilirken bir hata oluştu");
                return StatusCode(500, new { success = false, message = $"Hata: {ex.Message}", stackTrace = ex.StackTrace });
            }
        }
    }
}
