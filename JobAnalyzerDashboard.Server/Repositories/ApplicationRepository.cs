using JobAnalyzerDashboard.Server.Data;
using JobAnalyzerDashboard.Server.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobAnalyzerDashboard.Server.Repositories
{
    public class ApplicationRepository : Repository<Application>, IApplicationRepository
    {
        public ApplicationRepository(ApplicationDbContext context) : base(context)
        {
        }

#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        public async Task<IEnumerable<Application>> GetApplicationsWithFiltersAsync(int? jobId = null, string status = null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            string appliedMethod = null, bool? isAutoApplied = null, DateTime? fromDate = null, DateTime? toDate = null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            string sortBy = null, string sortDirection = null, int? userId = null)
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        {
            try
            {
                // Null kontrolü
                status = status ?? string.Empty;
                appliedMethod = appliedMethod ?? string.Empty;
                sortBy = sortBy ?? string.Empty;
                sortDirection = sortDirection ?? string.Empty;

                // Sorguyu oluştur
                var query = _dbSet.AsQueryable();

                // İlişkili verileri dahil et
                try
                {
                    query = query.Include(a => a.Job);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Job ilişkisi dahil edilirken hata: {ex.Message}");
                    // İlişki dahil edilemezse devam et
                }

                // Filtreleri uygula
                if (jobId.HasValue)
                {
                    query = query.Where(a => a.JobId == jobId.Value);
                }

                if (!string.IsNullOrEmpty(status))
                {
                    query = query.Where(a => a.Status.ToLower() == status.ToLower());
                }

                if (!string.IsNullOrEmpty(appliedMethod))
                {
                    query = query.Where(a => a.AppliedMethod.ToLower() == appliedMethod.ToLower());
                }

                if (isAutoApplied.HasValue)
                {
                    query = query.Where(a => a.IsAutoApplied == isAutoApplied.Value);
                }

                if (fromDate.HasValue)
                {
                    query = query.Where(a => a.AppliedDate >= fromDate.Value);
                }

                if (toDate.HasValue)
                {
                    query = query.Where(a => a.AppliedDate <= toDate.Value);
                }

                // Kullanıcı ID'sine göre filtreleme
                if (userId.HasValue)
                {
                    query = query.Where(a => a.UserId == userId.Value);
                }

                // Sıralama
                if (!string.IsNullOrEmpty(sortBy))
                {
                    switch (sortBy.ToLower())
                    {
                        case "date":
                            query = sortDirection.ToLower() == "desc" ?
                                query.OrderByDescending(a => a.AppliedDate) :
                                query.OrderBy(a => a.AppliedDate);
                            break;
                        case "status":
                            query = sortDirection.ToLower() == "desc" ?
                                query.OrderByDescending(a => a.Status) :
                                query.OrderBy(a => a.Status);
                            break;
                        default:
                            query = query.OrderByDescending(a => a.AppliedDate);
                            break;
                    }
                }
                else
                {
                    query = query.OrderByDescending(a => a.AppliedDate);
                }

                // Sorguyu çalıştır
                Console.WriteLine("Sorgu çalıştırılıyor...");
                var applications = await query.ToListAsync();
                Console.WriteLine($"Sorgu başarıyla çalıştırıldı. Toplam {applications.Count} başvuru bulundu.");

                // İş ilanı silinmiş mi kontrol et
                foreach (var application in applications)
                {
                    if (application.JobId > 0 && application.Job == null)
                    {
                        application.IsJobDeleted = true;
                    }
                }

                return applications;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Başvurular alınırken hata oluştu: {ex.Message}");
                Console.WriteLine($"Stack Trace: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"İç istisna: {ex.InnerException.Message}");
                    Console.WriteLine($"İç istisna Stack Trace: {ex.InnerException.StackTrace}");
                }

                // Hata durumunda boş liste döndür
                return new List<Application>();
            }
        }

        public async Task<Application> GetApplicationWithJobAsync(int id)
        {
            var application = await _dbSet
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (application != null && application.JobId > 0 && application.Job == null)
            {
                application.IsJobDeleted = true;
            }

#pragma warning disable CS8603 // Possible null reference return.
            return application;
#pragma warning restore CS8603 // Possible null reference return.
        }

        public async Task<object> GetStatsAsync()
        {
            var applications = await _dbSet.Include(a => a.Job).ToListAsync();

            return new
            {
                TotalApplications = applications.Count,
                PendingApplications = applications.Count(a => a.Status == "Pending"),
                AcceptedApplications = applications.Count(a => a.Status == "Accepted"),
                RejectedApplications = applications.Count(a => a.Status == "Rejected"),
                InterviewApplications = applications.Count(a => a.Status == "Interview"),
                AutoAppliedCount = applications.Count(a => a.IsAutoApplied),
                ManualAppliedCount = applications.Count(a => !a.IsAutoApplied),
                ApplicationMethodBreakdown = applications
                    .GroupBy(a => a.AppliedMethod)
                    .Select(g => new { Method = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList(),
                StatusBreakdown = applications
                    .GroupBy(a => a.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList(),
                RecentApplications = applications
                    .OrderByDescending(a => a.AppliedDate)
                    .Take(5)
                    .Select(a => new {
                        a.Id,
                        a.JobId,
                        JobTitle = a.Job?.Title ?? "Bilinmeyen İlan",
                        a.AppliedDate,
                        a.Status
                    })
                    .ToList()
            };
        }

        public async Task<bool> HasUserAppliedToJobAsync(int jobId, int userId)
        {
            try
            {
                // Kullanıcının belirli bir iş ilanına başvurup başvurmadığını kontrol et
                return await _dbSet.AnyAsync(a => a.JobId == jobId && a.UserId == userId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Kullanıcının iş ilanına başvurup başvurmadığı kontrol edilirken hata oluştu: {ex.Message}");
                return false; // Hata durumunda false döndür
            }
        }
    }

    public interface IApplicationRepository : IRepository<Application>
    {
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
        Task<IEnumerable<Application>> GetApplicationsWithFiltersAsync(int? jobId = null, string status = null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            string appliedMethod = null, bool? isAutoApplied = null, DateTime? fromDate = null, DateTime? toDate = null,
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.
            string sortBy = null, string sortDirection = null, int? userId = null);
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
#pragma warning restore CS8625 // Cannot convert null literal to non-nullable reference type.
        Task<Application> GetApplicationWithJobAsync(int id);
        Task<object> GetStatsAsync();
        Task<bool> HasUserAppliedToJobAsync(int jobId, int userId);
    }
}
