using JobAnalyzerDashboard.Server.Data;
using JobAnalyzerDashboard.Server.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobAnalyzerDashboard.Server.Repositories
{
    public class JobRepository : Repository<Job>, IJobRepository
    {
        public JobRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Job>> GetJobsWithFiltersAsync(string category = null, int? minQualityScore = null,
            bool? isApplied = null, string searchTerm = null, string sortBy = null, string sortDirection = null)
        {
            var query = _dbSet.AsQueryable();

            // Filtreleri uygula
            if (!string.IsNullOrEmpty(category))
            {
                query = query.Where(j => j.Category.ToLower() == category.ToLower());
            }

            if (minQualityScore.HasValue)
            {
                query = query.Where(j => j.QualityScore >= minQualityScore.Value);
            }

            if (isApplied.HasValue)
            {
                query = query.Where(j => j.IsApplied == isApplied.Value);
            }

            if (!string.IsNullOrEmpty(searchTerm))
            {
                searchTerm = searchTerm.ToLower();
                query = query.Where(j => j.Title.ToLower().Contains(searchTerm) ||
                                        j.Description.ToLower().Contains(searchTerm) ||
                                        j.Company.ToLower().Contains(searchTerm) ||
                                        j.Location.ToLower().Contains(searchTerm));
            }

            // Sıralama
            if (!string.IsNullOrEmpty(sortBy))
            {
                switch (sortBy.ToLower())
                {
                    case "date":
                        query = sortDirection?.ToLower() == "desc" ?
                            query.OrderByDescending(j => j.PostedDate) :
                            query.OrderBy(j => j.PostedDate);
                        break;
                    case "quality":
                        query = sortDirection?.ToLower() == "desc" ?
                            query.OrderByDescending(j => j.QualityScore) :
                            query.OrderBy(j => j.QualityScore);
                        break;
                    case "title":
                        query = sortDirection?.ToLower() == "desc" ?
                            query.OrderByDescending(j => j.Title) :
                            query.OrderBy(j => j.Title);
                        break;
                    case "company":
                        query = sortDirection?.ToLower() == "desc" ?
                            query.OrderByDescending(j => j.Company) :
                            query.OrderBy(j => j.Company);
                        break;
                    default:
                        query = query.OrderByDescending(j => j.PostedDate);
                        break;
                }
            }
            else
            {
                query = query.OrderByDescending(j => j.PostedDate);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<string>> GetCategoriesAsync()
        {
            return await _dbSet
                .Where(j => !string.IsNullOrEmpty(j.Category))
                .Select(j => j.Category)
                .Distinct()
                .OrderBy(c => c)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetTagsAsync()
        {
            var jobs = await _dbSet.ToListAsync();
            var allTags = new List<string>();

            foreach (var job in jobs)
            {
                if (!string.IsNullOrEmpty(job.Tags))
                {
                    try
                    {
                        var tags = System.Text.Json.JsonSerializer.Deserialize<List<string>>(job.Tags);
                        if (tags != null)
                        {
                            allTags.AddRange(tags);
                        }
                    }
                    catch
                    {
                        // JSON deserialize hatası, yoksay
                    }
                }
            }

            return allTags.Distinct().OrderBy(t => t).ToList();
        }

        public async Task<object> GetStatsAsync()
        {
            var jobs = await _dbSet.ToListAsync();

            return new
            {
                TotalJobs = jobs.Count,
                AppliedJobs = jobs.Count(j => j.IsApplied),
                HighQualityJobs = jobs.Count(j => j.QualityScore >= 4),
                AverageSalary = jobs.Where(j => j.ParsedMinSalary > 0).DefaultIfEmpty().Average(j => j?.ParsedMinSalary ?? 0),
                CategoryBreakdown = jobs
                    .GroupBy(j => j.Category)
                    .Select(g => new { Category = g.Key, Count = g.Count() })
                    .OrderByDescending(x => x.Count)
                    .ToList(),
                QualityScoreBreakdown = jobs
                    .GroupBy(j => j.QualityScore)
                    .Select(g => new { Score = g.Key, Count = g.Count() })
                    .OrderBy(x => x.Score)
                    .ToList()
            };
        }
    }

    public interface IJobRepository : IRepository<Job>
    {
        Task<IEnumerable<Job>> GetJobsWithFiltersAsync(string category = null, int? minQualityScore = null,
            bool? isApplied = null, string searchTerm = null, string sortBy = null, string sortDirection = null);
        Task<IEnumerable<string>> GetCategoriesAsync();
        Task<IEnumerable<string>> GetTagsAsync();
        Task<object> GetStatsAsync();
    }
}
