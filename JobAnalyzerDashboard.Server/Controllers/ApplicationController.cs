using Microsoft.AspNetCore.Mvc;
using JobAnalyzerDashboard.Server.Models;
using JobAnalyzerDashboard.Server.Repositories;
using JobAnalyzerDashboard.Server.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace JobAnalyzerDashboard.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationController : ControllerBase
    {
        private readonly ILogger<ApplicationController> _logger;
        private readonly IApplicationRepository _applicationRepository;
        private readonly IJobRepository _jobRepository;
        private readonly EmailService _emailService;

        public ApplicationController(
            ILogger<ApplicationController> logger,
            IApplicationRepository applicationRepository,
            IJobRepository jobRepository,
            EmailService emailService)
        {
            _logger = logger;
            _applicationRepository = applicationRepository;
            _jobRepository = jobRepository;
            _emailService = emailService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll([FromQuery] ApplicationFilterModel filter)
        {
            try
            {
                _logger.LogInformation("Başvurular alınıyor. Filtreler: JobId={JobId}, Status={Status}, AppliedMethod={AppliedMethod}, IsAutoApplied={IsAutoApplied}",
                    filter.JobId, filter.Status, filter.AppliedMethod, filter.IsAutoApplied);

                var applications = await _applicationRepository.GetApplicationsWithFiltersAsync(
                    filter.JobId,
                    filter.Status,
                    filter.AppliedMethod,
                    filter.IsAutoApplied,
                    filter.FromDate,
                    filter.ToDate,
                    filter.SortBy,
                    filter.SortDirection);

                _logger.LogInformation("Başvurular başarıyla alındı. Toplam: {Count}", applications?.Count() ?? 0);

                return Ok(applications);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Başvurular alınırken hata oluştu: {ErrorMessage}, StackTrace: {StackTrace}", ex.Message, ex.StackTrace);

                // İç içe istisnalar varsa onları da logla
                var innerException = ex.InnerException;
                while (innerException != null)
                {
                    _logger.LogError(innerException, "İç istisna: {ErrorMessage}, StackTrace: {StackTrace}",
                        innerException.Message, innerException.StackTrace);
                    innerException = innerException.InnerException;
                }

                return StatusCode(500, new { message = "Başvurular alınırken bir hata oluştu", error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var application = await _applicationRepository.GetApplicationWithJobAsync(id);
                if (application == null)
                {
                    return NotFound(new { message = "Başvuru bulunamadı" });
                }

                return Ok(application);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Başvuru alınırken hata oluştu: {Id}", id);
                return StatusCode(500, new { message = "Başvuru alınırken bir hata oluştu" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> Create(ApplicationCreateModel model)
        {
            try
            {
                if (model == null)
                {
                    return BadRequest(new { message = "Geçersiz başvuru verisi" });
                }

                var job = await _jobRepository.GetByIdAsync(model.JobId);
                if (job == null)
                {
                    return NotFound(new { message = "İş ilanı bulunamadı" });
                }

                var application = new Application
                {
                    JobId = model.JobId,
                    AppliedDate = DateTime.UtcNow,
                    Status = "Pending",
                    AppliedMethod = model.AppliedMethod,
                    SentMessage = model.Message,
                    IsAutoApplied = model.IsAutoApplied,
                    CvAttached = model.CvAttached,
                    TelegramNotificationSent = model.TelegramNotification
                };

                await _applicationRepository.AddAsync(application);
                await _jobRepository.SaveChangesAsync();

                _logger.LogInformation("Yeni başvuru oluşturuldu: {Id} - {JobId}", application.Id, application.JobId);

                return CreatedAtAction(nameof(GetById), new { id = application.Id }, application);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Başvuru oluşturulurken hata oluştu");
                return StatusCode(500, new { message = "Başvuru oluşturulurken bir hata oluştu" });
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, Application application)
        {
            try
            {
                if (application == null || id != application.Id)
                {
                    return BadRequest(new { message = "Geçersiz başvuru verisi" });
                }

                var existingApplication = await _applicationRepository.GetByIdAsync(id);
                if (existingApplication == null)
                {
                    return NotFound(new { message = "Başvuru bulunamadı" });
                }

                await _applicationRepository.UpdateAsync(application);
                await _applicationRepository.SaveChangesAsync();

                _logger.LogInformation("Başvuru güncellendi: {Id}", application.Id);

                return Ok(application);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Başvuru güncellenirken hata oluştu: {Id}", id);
                return StatusCode(500, new { message = "Başvuru güncellenirken bir hata oluştu" });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var application = await _applicationRepository.GetByIdAsync(id);
                if (application == null)
                {
                    return NotFound(new { message = "Başvuru bulunamadı" });
                }

                await _applicationRepository.DeleteAsync(application);
                await _applicationRepository.SaveChangesAsync();

                _logger.LogInformation("Başvuru silindi: {Id}", id);

                return Ok(new { message = "Başvuru başarıyla silindi" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Başvuru silinirken hata oluştu: {Id}", id);
                return StatusCode(500, new { message = "Başvuru silinirken bir hata oluştu" });
            }
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            try
            {
                var stats = await _applicationRepository.GetStatsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Başvuru istatistikleri alınırken hata oluştu");
                return StatusCode(500, new { message = "Başvuru istatistikleri alınırken bir hata oluştu" });
            }
        }

        [HttpPost("auto-apply")]
        public async Task<IActionResult> AutoApply(AutoApplyModel model)
        {
            try
            {
                if (model == null || model.JobId <= 0)
                {
                    return BadRequest(new { message = "Geçersiz otomatik başvuru verisi" });
                }

                var job = await _jobRepository.GetByIdAsync(model.JobId);
                if (job == null)
                {
                    return NotFound(new { message = "İş ilanı bulunamadı" });
                }

                // Başvuru oluştur
                var application = new Application
                {
                    JobId = model.JobId,
                    AppliedDate = DateTime.UtcNow,
                    Status = "Pending",
                    AppliedMethod = "n8n",
                    SentMessage = model.Message,
                    IsAutoApplied = true,
                    CvAttached = true,
                    TelegramNotificationSent = model.TelegramNotification
                };

                await _applicationRepository.AddAsync(application);
                await _applicationRepository.SaveChangesAsync();

                job.IsApplied = true;
                job.AppliedDate = DateTime.UtcNow;
                await _jobRepository.UpdateAsync(job);
                await _jobRepository.SaveChangesAsync();

                _logger.LogInformation("Otomatik başvuru oluşturuldu: {Id} - {Title}", application.Id, job.Title);

                return Ok(new
                {
                    success = true,
                    message = "Otomatik başvuru başarıyla yapıldı",
                    applicationId = application.Id,
                    jobId = job.Id,
                    jobTitle = job.Title
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Otomatik başvuru yapılırken hata oluştu");
                return StatusCode(500, new { message = "Otomatik başvuru yapılırken bir hata oluştu" });
            }
        }
    }

    public class ApplicationFilterModel
    {
        public int? JobId { get; set; }
        public string? Status { get; set; }
        public string? AppliedMethod { get; set; }
        public bool? IsAutoApplied { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public string? SortBy { get; set; }
        public string? SortDirection { get; set; }
    }

    public class ApplicationCreateModel
    {
        public int JobId { get; set; }
        public string AppliedMethod { get; set; } = "Manual";
        public string? Message { get; set; }
        public bool IsAutoApplied { get; set; } = false;
        public bool CvAttached { get; set; } = false;
        public string? TelegramNotification { get; set; }
    }

    public class AutoApplyModel
    {
        public int JobId { get; set; }
        public string? Message { get; set; }
        public string? TelegramNotification { get; set; }
    }
}
