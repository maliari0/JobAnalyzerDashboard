using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Configuration;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/redirect")]
    public class RedirectController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<RedirectController> _logger;

        public RedirectController(IConfiguration configuration, ILogger<RedirectController> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpGet("confirm-email")]
        [AllowAnonymous]
        public IActionResult RedirectToFrontendConfirmEmail([FromQuery] string token, [FromQuery] string email)
        {
            try
            {
                // Frontend URL'sini al
                var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "https://jobanalyzerdashboard.onrender.com";

                // Tam yönlendirme URL'sini oluştur
                var redirectUrl = $"{frontendUrl}/confirm-email?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";

                _logger.LogInformation("E-posta doğrulama isteği frontend'e yönlendiriliyor: {RedirectUrl} (FrontendUrl: {FrontendUrl})", redirectUrl, frontendUrl);

                // Frontend'e yönlendir
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta doğrulama yönlendirmesi sırasında bir hata oluştu");
                return StatusCode(500, new { message = "E-posta doğrulama yönlendirmesi sırasında bir hata oluştu" });
            }
        }

        [HttpGet("reset-password")]
        [AllowAnonymous]
        public IActionResult RedirectToFrontendResetPassword([FromQuery] string token, [FromQuery] string email)
        {
            try
            {
                // Frontend URL'sini al
                var frontendUrl = _configuration["AppSettings:FrontendUrl"] ?? "https://jobanalyzerdashboard.onrender.com";

                // Tam yönlendirme URL'sini oluştur
                var redirectUrl = $"{frontendUrl}/reset-password?token={Uri.EscapeDataString(token)}&email={Uri.EscapeDataString(email)}";

                _logger.LogInformation("Şifre sıfırlama isteği frontend'e yönlendiriliyor: {RedirectUrl} (FrontendUrl: {FrontendUrl})", redirectUrl, frontendUrl);

                // Frontend'e yönlendir
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Şifre sıfırlama yönlendirmesi sırasında bir hata oluştu");
                return StatusCode(500, new { message = "Şifre sıfırlama yönlendirmesi sırasında bir hata oluştu" });
            }
        }
    }
}
