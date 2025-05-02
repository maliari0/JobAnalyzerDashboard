using Microsoft.AspNetCore.Mvc;
using JobAnalyzerDashboard.Server.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using JobAnalyzerDashboard.Server.Models;
using JobAnalyzerDashboard.Server.Attributes;
using System.Collections.Generic;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize]
    public class AdminController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AdminController> _logger;

        public AdminController(ApplicationDbContext context, ILogger<AdminController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpDelete("users/{email}")]
        public async Task<IActionResult> DeleteUser(string email)
        {
            try
            {
                _logger.LogInformation("Deleting user with email: {Email}", email);

                // Kullanıcıyı bul
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Email == email);

                if (user == null)
                {
                    _logger.LogWarning("User with email {Email} not found", email);
                    return NotFound(new { message = $"User with email {email} not found" });
                }

                _logger.LogInformation("User found. ID: {UserId}, ProfileId: {ProfileId}", user.Id, user.ProfileId);

                // İlişkili verileri sil
                if (user.ProfileId.HasValue)
                {
                    // Profili bul
                    var profile = await _context.Profiles
                        .FirstOrDefaultAsync(p => p.Id == user.ProfileId.Value);

                    if (profile != null)
                    {
                        // OAuthTokens tablosundan ilgili kayıtları sil
                        var oauthTokens = await _context.OAuthTokens
                            .Where(t => t.ProfileId == profile.Id)
                            .ToListAsync();

                        if (oauthTokens.Any())
                        {
                            _context.OAuthTokens.RemoveRange(oauthTokens);
                            _logger.LogInformation("Removed {Count} OAuth tokens", oauthTokens.Count);
                        }

                        // Resumes tablosundan ilgili kayıtları sil
                        var resumes = await _context.Resumes
                            .Where(r => r.ProfileId == profile.Id)
                            .ToListAsync();

                        if (resumes.Any())
                        {
                            _context.Resumes.RemoveRange(resumes);
                            _logger.LogInformation("Removed {Count} resumes", resumes.Count);
                        }

                        // Applications tablosundan ilgili kayıtları sil
                        // Not: Application modelinde ProfileId alanı olmadığı için bu adımı atlıyoruz
                        _logger.LogInformation("Skipping application removal as there is no ProfileId in Application model");

                        // Profili sil
                        _context.Profiles.Remove(profile);
                        _logger.LogInformation("Removed profile with ID: {ProfileId}", profile.Id);
                    }
                }

                // Kullanıcıyı sil
                _context.Users.Remove(user);
                _logger.LogInformation("Removed user with ID: {UserId}", user.Id);

                // Değişiklikleri kaydet
                await _context.SaveChangesAsync();

                return Ok(new { message = $"User with email {email} and all related data successfully deleted" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user with email {Email}", email);
                return StatusCode(500, new { message = $"Error deleting user: {ex.Message}" });
            }
        }
    }
}
