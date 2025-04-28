using JobAnalyzerDashboard.Server.Data;
using JobAnalyzerDashboard.Server.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace JobAnalyzerDashboard.Server.Repositories
{
    public class ProfileRepository : Repository<Profile>, IProfileRepository
    {
        public ProfileRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Profile> GetProfileWithResumesAsync(int id)
        {
            return await _dbSet
                .Include(p => p.Resumes)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<Resume>> GetResumesAsync(int profileId)
        {
            return await _context.Resumes
                .Where(r => r.ProfileId == profileId)
                .ToListAsync();
        }

        public async Task<Resume> GetDefaultResumeAsync(int profileId)
        {
            return await _context.Resumes
                .FirstOrDefaultAsync(r => r.ProfileId == profileId && r.IsDefault);
        }

        public async Task<Resume> GetResumeByIdAsync(int resumeId)
        {
            return await _context.Resumes.FindAsync(resumeId);
        }

        public async Task AddResumeAsync(Resume resume)
        {
            await _context.Resumes.AddAsync(resume);
        }

        public async Task SetDefaultResumeAsync(int resumeId, int profileId)
        {
            // Tüm özgeçmişlerin varsayılan durumunu kaldır
            var resumes = await _context.Resumes
                .Where(r => r.ProfileId == profileId)
                .ToListAsync();
                
            foreach (var resume in resumes)
            {
                resume.IsDefault = false;
            }
            
            // Seçilen özgeçmişi varsayılan yap
            var selectedResume = resumes.FirstOrDefault(r => r.Id == resumeId);
            if (selectedResume != null)
            {
                selectedResume.IsDefault = true;
            }
            
            await _context.SaveChangesAsync();
        }

        public async Task DeleteResumeAsync(int resumeId, int profileId)
        {
            var resume = await _context.Resumes.FindAsync(resumeId);
            if (resume != null && resume.ProfileId == profileId)
            {
                _context.Resumes.Remove(resume);
                
                // Eğer silinen özgeçmiş varsayılan ise ve başka özgeçmişler varsa, ilk özgeçmişi varsayılan yap
                if (resume.IsDefault)
                {
                    var otherResumes = await _context.Resumes
                        .Where(r => r.ProfileId == profileId && r.Id != resumeId)
                        .ToListAsync();
                        
                    if (otherResumes.Any())
                    {
                        otherResumes.First().IsDefault = true;
                    }
                }
                
                await _context.SaveChangesAsync();
            }
        }
    }

    public interface IProfileRepository : IRepository<Profile>
    {
        Task<Profile> GetProfileWithResumesAsync(int id);
        Task<IEnumerable<Resume>> GetResumesAsync(int profileId);
        Task<Resume> GetDefaultResumeAsync(int profileId);
        Task<Resume> GetResumeByIdAsync(int resumeId);
        Task AddResumeAsync(Resume resume);
        Task SetDefaultResumeAsync(int resumeId, int profileId);
        Task DeleteResumeAsync(int resumeId, int profileId);
    }
}
