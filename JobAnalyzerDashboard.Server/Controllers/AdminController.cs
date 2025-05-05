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

        [HttpGet("dashboard/stats")]
        public async Task<IActionResult> GetDashboardStats()
        {
            try
            {
                // İstatistikleri hesapla
                var totalUsers = await _context.Users.CountAsync();
                var activeUsers = await _context.Users.CountAsync(u => u.IsActive);
                var totalJobs = await _context.Jobs.CountAsync();
                var totalApplications = await _context.Applications.CountAsync();

                return Ok(new
                {
                    totalUsers,
                    activeUsers,
                    totalJobs,
                    totalApplications
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting dashboard stats");
                return StatusCode(500, new { message = "Error getting dashboard stats" });
            }
        }

        [HttpGet("users")]
        public async Task<IActionResult> GetUsers([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                // Toplam kullanıcı sayısını al
                var totalCount = await _context.Users.CountAsync();

                // Sayfalama ile kullanıcıları al
                var users = await _context.Users
                    .OrderByDescending(u => u.CreatedAt)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(u => new
                    {
                        u.Id,
                        u.Email,
                        u.Username,
                        u.FirstName,
                        u.LastName,
                        u.Role,
                        u.EmailConfirmed,
                        u.ProfileId,
                        u.IsActive,
                        CreatedAt = u.CreatedAt.ToString("o"),
                        LastLoginAt = u.LastLoginAt.HasValue ? u.LastLoginAt.Value.ToString("o") : null
                    })
                    .ToListAsync();

                return Ok(new { users, totalCount });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting users list");
                return StatusCode(500, new { message = "Error getting users list" });
            }
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(int id, [FromBody] UserUpdateRequest model)
        {
            try
            {
                // Kullanıcıyı bul
                var user = await _context.Users.FindAsync(id);

                if (user == null)
                {
                    _logger.LogWarning("User with ID {UserId} not found", id);
                    return NotFound(new { message = "User not found" });
                }

                // Kullanıcı bilgilerini güncelle
                user.IsActive = model.IsActive;
                user.Role = model.Role;

                _context.Users.Update(user);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "User updated successfully",
                    user = new
                    {
                        user.Id,
                        user.Email,
                        user.Username,
                        user.FirstName,
                        user.LastName,
                        user.Role,
                        user.EmailConfirmed,
                        user.ProfileId,
                        user.IsActive,
                        CreatedAt = user.CreatedAt.ToString("o"),
                        LastLoginAt = user.LastLoginAt.HasValue ? user.LastLoginAt.Value.ToString("o") : null
                    }
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user with ID {UserId}", id);
                return StatusCode(500, new { message = "Error updating user" });
            }
        }
    }

    public class UserUpdateRequest
    {
        public bool IsActive { get; set; }
        public string Role { get; set; } = "User";
    }
}
