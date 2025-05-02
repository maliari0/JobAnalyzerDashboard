using System;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;
using JobAnalyzerDashboard.Server.Data;
using JobAnalyzerDashboard.Server.Services;
using JobAnalyzerDashboard.Server.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

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

        [HttpGet("google/authorize")]
        public IActionResult GoogleAuthorize([FromQuery] int profileId = 1)
        {
            try
            {
                var authUrl = _oauthService.GetAuthorizationUrl("Google", profileId);
                return Redirect(authUrl);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating Google authorization URL");
                return StatusCode(500, new { message = "Error generating Google authorization URL" });
            }
        }

        [HttpGet("google/callback")]
        public async Task<IActionResult> GoogleCallback([FromQuery] string code, [FromQuery] string state, [FromQuery] string error = null)
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

                return Redirect("/profile?oauth=success");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Google callback");
                return Redirect($"/profile?error={Uri.EscapeDataString("Error processing Google callback")}");
            }
        }

        [HttpGet("oauth-status")]
        [Produces("application/json")]
        public async Task<IActionResult> GetOAuthStatus([FromQuery] int profileId = 1)
        {
            try
            {
                _logger.LogInformation("Getting OAuth status for profile {ProfileId}", profileId);
                var tokens = await _oauthService.GetOAuthTokensAsync(profileId);

                var result = tokens.Select(t => new
                {
                    t.Id,
                    t.ProfileId,
                    t.Provider,
                    t.Email,
                    t.ExpiresAt
                }).ToList();

                _logger.LogInformation("Found {Count} OAuth tokens for profile {ProfileId}", result.Count, profileId);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting OAuth status for profile {ProfileId}", profileId);
                return StatusCode(500, new { message = "Error getting OAuth status" });
            }
        }

        [HttpDelete("revoke/{provider}")]
        [Produces("application/json")]
        public async Task<IActionResult> RevokeAccess(string provider, [FromQuery] int profileId = 1)
        {
            try
            {
                _logger.LogInformation("Revoking {Provider} access for profile {ProfileId}", provider, profileId);
                var success = await _oauthService.RevokeTokenAsync(profileId, provider);

                if (success)
                {
                    _logger.LogInformation("{Provider} access revoked successfully for profile {ProfileId}", provider, profileId);
                    return Ok(new { success = true, message = $"{provider} access revoked successfully" });
                }
                else
                {
                    _logger.LogWarning("No {Provider} token found for profile {ProfileId}", provider, profileId);
                    return NotFound(new { success = false, message = $"No {provider} token found for this profile" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking {Provider} access for profile {ProfileId}", provider, profileId);
                return StatusCode(500, new { message = $"Error revoking {provider} access" });
            }
        }
    }
}
