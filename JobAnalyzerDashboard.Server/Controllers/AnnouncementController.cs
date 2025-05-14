using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using JobAnalyzerDashboard.Server.Data;
using JobAnalyzerDashboard.Server.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/announcement")]
    public class AnnouncementController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<AnnouncementController> _logger;

        public AnnouncementController(ApplicationDbContext context, ILogger<AnnouncementController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Announcement>>> GetAnnouncements()
        {
            try
            {

                var now = DateTime.UtcNow;
                var announcements = await _context.Announcements
                    .Where(a => a.IsActive && (a.ExpiresAt == null || a.ExpiresAt > now))
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                return Ok(announcements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting announcements");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("all")]
        public async Task<ActionResult<IEnumerable<Announcement>>> GetAllAnnouncements()
        {
            try
            {

                var announcements = await _context.Announcements
                    .OrderByDescending(a => a.CreatedAt)
                    .ToListAsync();

                return Ok(announcements);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all announcements");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Announcement>> GetAnnouncement(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);

            if (announcement == null)
            {
                return NotFound();
            }

            return announcement;
        }

        [HttpPost]
        public async Task<ActionResult<Announcement>> CreateAnnouncement(AnnouncementCreateModel model)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }


                int? createdById = null;
                try
                {
                    var userId = User.FindFirst("id")?.Value;
                    createdById = userId != null ? int.Parse(userId) : null;
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Kullanıcı kimliği alınamadı, null kullanılıyor");
                }

                var announcement = new Announcement
                {
                    Title = model.Title,
                    Content = model.Content,
                    IsActive = model.IsActive,
                    CreatedAt = DateTime.UtcNow,
                    ExpiresAt = model.ExpiresAt,
                    Type = model.Type,
                    CreatedById = createdById
                };

                _context.Announcements.Add(announcement);
                await _context.SaveChangesAsync();

                return Ok(announcement);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating announcement");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAnnouncement(int id, AnnouncementUpdateModel model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }

            announcement.Title = model.Title;
            announcement.Content = model.Content;
            announcement.IsActive = model.IsActive;
            announcement.ExpiresAt = model.ExpiresAt;
            announcement.Type = model.Type;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!AnnouncementExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAnnouncement(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }

            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool AnnouncementExists(int id)
        {
            return _context.Announcements.Any(e => e.Id == id);
        }
    }

    public class AnnouncementCreateModel
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime? ExpiresAt { get; set; }
        public string Type { get; set; } = "info";
    }

    public class AnnouncementUpdateModel
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime? ExpiresAt { get; set; }
        public string Type { get; set; } = "info";
    }
}
