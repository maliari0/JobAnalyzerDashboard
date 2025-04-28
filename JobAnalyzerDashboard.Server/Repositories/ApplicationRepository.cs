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

        public async Task<IEnumerable<Application>> GetApplicationsWithFiltersAsync(int? jobId = null, string status = null, 
            string appliedMethod = null, bool? isAutoApplied = null, DateTime? fromDate = null, DateTime? toDate = null, 
            string sortBy = null, string sortDirection = null)
        {
            var query = _dbSet.Include(a => a.Job).AsQueryable();

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

            // Sıralama
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "date":
                        query = sortDirection?.ToLower() == "desc" ?
                            query.OrderByDescending(a => a.AppliedDate) :
                            query.OrderBy(a => a.AppliedDate);
                        break;
                    case "status":
                        query = sortDirection?.ToLower() == "desc" ?
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

            var applications = await query.ToListAsync();
            
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

        public async Task<Application> GetApplicationWithJobAsync(int id)
        {
            var application = await _dbSet
                .Include(a => a.Job)
                .FirstOrDefaultAsync(a => a.Id == id);
                
            if (application != null && application.JobId > 0 && application.Job == null)
            {
                application.IsJobDeleted = true;
            }
            
            return application;
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
    }

    public interface IApplicationRepository : IRepository<Application>
    {
        Task<IEnumerable<Application>> GetApplicationsWithFiltersAsync(int? jobId = null, string status = null, 
            string appliedMethod = null, bool? isAutoApplied = null, DateTime? fromDate = null, DateTime? toDate = null, 
            string sortBy = null, string sortDirection = null);
        Task<Application> GetApplicationWithJobAsync(int id);
        Task<object> GetStatsAsync();
    }
}
