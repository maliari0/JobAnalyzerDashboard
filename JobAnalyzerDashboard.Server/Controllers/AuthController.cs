using System;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using System.Security.Claims;
using JobAnalyzerDashboard.Server.Data;
using JobAnalyzerDashboard.Server.Services;
using JobAnalyzerDashboard.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Authorization;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly OAuthService _oauthService;
        private readonly ILogger<AuthController> _logger;
        private readonly ApplicationDbContext _context;

        public AuthController(OAuthService oauthService, ILogger<AuthController> logger, ApplicationDbContext context)
        {
            _oauthService = oauthService;
            _logger = logger;
            _context = context;
        }

        [Authorize]
        [HttpGet("google/authorize")]
        public async Task<IActionResult> GoogleAuthorize()
        {
            try
            {
                // Kullanıcı ID'sini al
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { message = "Oturum açmanız gerekiyor" });
                }

                // Kullanıcıyı bul
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                // Kullanıcının profil ID'sini kontrol et
                if (!user.ProfileId.HasValue)
                {
                    return NotFound(new { message = "Kullanıcı profili bulunamadı" });
                }

                var authUrl = _oauthService.GetAuthorizationUrl("Google", user.ProfileId.Value);
                return Redirect(authUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Google authorization URL");
                return StatusCode(500, new { message = "Error generating Google authorization URL" });
            }
        }

        [HttpGet("google/callback")]
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state, [FromQuery] string error = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
            try
            {
                if (!string.IsNullOrEmpty(error))
                {
                    return Redirect($"/profile?error={Uri.EscapeDataString(error)}");
                }

                var stateData = Encoding.UTF8.GetString(Convert.FromBase64String(state)).Split(':');
                var provider = stateData[0];
                var profileId = int.Parse(stateData[1]);

                await _oauthService.ExchangeCodeForTokenAsync(provider, code, profileId);

                // Başarılı olduğunda doğrudan profil sayfasına yönlendir
                return Redirect("/profile");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Google callback");
                return Redirect($"/profile?error={Uri.EscapeDataString("Error processing Google callback")}");
            }
        }

        [Authorize]
        [HttpGet("oauth-status")]
        [Produces("application/json")]
        public async Task<IActionResult> GetOAuthStatus()
        {
            try
            {
                // Kullanıcı ID'sini al
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { message = "Oturum açmanız gerekiyor" });
                }

                // Kullanıcıyı bul
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                // Kullanıcının profil ID'sini kontrol et
                if (!user.ProfileId.HasValue)
                {
                    return NotFound(new { message = "Kullanıcı profili bulunamadı" });
                }

                _logger.LogInformation("Getting OAuth status for user {UserId} with profile {ProfileId}", userId, user.ProfileId.Value);
                var tokens = await _oauthService.GetOAuthTokensAsync(user.ProfileId.Value);

                var result = tokens.Select(t => new
                {
                    t.Id,
                    t.ProfileId,
                    t.Provider,
                    t.Email,
                    t.ExpiresAt
                }).ToList();

                _logger.LogInformation("Found {Count} OAuth tokens for user {UserId} with profile {ProfileId}", result.Count, userId, user.ProfileId.Value);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OAuth status");
                return StatusCode(500, new { message = "Error getting OAuth status" });
            }
        }

        [Authorize]
        [HttpDelete("revoke/{provider}")]
        [Produces("application/json")]
        public async Task<IActionResult> RevokeAccess(string provider)
        {
            try
            {
                // Kullanıcı ID'sini al
                var userIdClaim = User.Claims.FirstOrDefault(c => c.Type == ClaimTypes.NameIdentifier);
                if (userIdClaim == null || !int.TryParse(userIdClaim.Value, out int userId))
                {
                    return Unauthorized(new { message = "Oturum açmanız gerekiyor" });
                }

                // Kullanıcıyı bul
                var user = await _context.Users.FindAsync(userId);
                if (user == null)
                {
                    return NotFound(new { message = "Kullanıcı bulunamadı" });
                }

                // Kullanıcının profil ID'sini kontrol et
                if (!user.ProfileId.HasValue)
                {
                    return NotFound(new { message = "Kullanıcı profili bulunamadı" });
                }

                _logger.LogInformation("Revoking {Provider} access for user {UserId} with profile {ProfileId}", provider, userId, user.ProfileId.Value);
                var success = await _oauthService.RevokeTokenAsync(user.ProfileId.Value, provider);

                if (success)
                {
                    _logger.LogInformation("{Provider} access revoked successfully for user {UserId} with profile {ProfileId}", provider, userId, user.ProfileId.Value);
                    return Ok(new { success = true, message = $"{provider} access revoked successfully" });
                }
                else
                {
                    _logger.LogWarning("No {Provider} token found for user {UserId} with profile {ProfileId}", provider, userId, user.ProfileId.Value);
                    return NotFound(new { success = false, message = $"No {provider} token found for this profile" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking {Provider} access", provider);
                return StatusCode(500, new { message = $"Error revoking {provider} access" });
            }
        }
    }
}
