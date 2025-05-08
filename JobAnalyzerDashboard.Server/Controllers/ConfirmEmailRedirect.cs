using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("")]
    public class ConfirmEmailRedirectController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<ConfirmEmailRedirectController> _logger;

        public ConfirmEmailRedirectController(IConfiguration configuration, ILogger<ConfirmEmailRedirectController> logger)
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
                
                _logger.LogInformation("E-posta doğrulama isteği frontend'e yönlendiriliyor: {RedirectUrl}", redirectUrl);
                
                // Frontend'e yönlendir
                return Redirect(redirectUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "E-posta doğrulama yönlendirmesi sırasında bir hata oluştu");
                return StatusCode(500, new { message = "E-posta doğrulama yönlendirmesi sırasında bir hata oluştu" });
            }
        }
    }
}
