using System.Threading.Tasks;
using System.Security.Claims;
using JobAnalyzerDashboard.Server.Models;
using JobAnalyzerDashboard.Server.Models.DTOs;
using JobAnalyzerDashboard.Server.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/user")]
    public class UserController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AuthService _authService;
        private readonly ILogger<UserController> _logger;

        public UserController(AuthService authService, ILogger<UserController> logger, IConfiguration configuration)
        {
            _authService = authService;
            _logger = logger;
            _configuration = configuration;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.RegisterAsync(model);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.LoginAsync(model);

            if (result.Success)
            {
                return Ok(result);
            }

            return BadRequest(result);
        }

        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail([FromQuery] EmailConfirmationDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.ConfirmEmailAsync(model);

            if (result)
            {
                return Ok(new { success = true, message = "E-posta adresiniz başarıyla doğrulandı." });
            }

            return BadRequest(new { success = false, message = "E-posta doğrulama başarısız oldu. Lütfen geçerli bir bağlantı kullanın." });
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.ForgotPasswordAsync(model);

            // Güvenlik nedeniyle her zaman başarılı döndür
            return Ok(new { success = true, message = "Şifre sıfırlama bağlantısı e-posta adresinize gönderildi." });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _authService.ResetPasswordAsync(model);

            if (result)
            {
                return Ok(new { success = true, message = "Şifreniz başarıyla sıfırlandı." });
            }

            return BadRequest(new { success = false, message = "Şifre sıfırlama başarısız oldu. Lütfen geçerli bir bağlantı kullanın." });
        }

        [Authorize]
        [HttpPost("change-password")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordDTO model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Kullanıcı ID'sini al
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Oturum açmanız gerekiyor" });
            }

            var result = await _authService.ChangePasswordAsync(userId, model);

            if (result)
            {
                return Ok(new { success = true, message = "Şifreniz başarıyla değiştirildi." });
            }

            return BadRequest(new { success = false, message = "Şifre değiştirme başarısız oldu. Lütfen mevcut şifrenizi doğru girdiğinizden emin olun." });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetCurrentUser()
        {
            // Kullanıcı ID'sini al
            var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
            if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
            {
                return Unauthorized(new { message = "Oturum açmanız gerekiyor" });
            }

            var userDto = await _authService.GetUserByIdAsync(userId);

            if (userDto != null)
            {
                return Ok(userDto);
            }

            return NotFound(new { message = "Kullanıcı bulunamadı." });
        }
    }

    // Root level endpoint for e-posta doğrulama yönlendirmesi
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

                _logger.LogInformation("Şifre sıfırlama isteği frontend'e yönlendiriliyor: {RedirectUrl}", redirectUrl);

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
