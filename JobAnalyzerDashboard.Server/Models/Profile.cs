using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace JobAnalyzerDashboard.Server.Models
{
    public class Profile
    {
        public int Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string LinkedInUrl { get; set; } = string.Empty;
        public string GithubUrl { get; set; } = string.Empty;
        public string PortfolioUrl { get; set; } = string.Empty;
        public string Skills { get; set; } = string.Empty;
        public string Education { get; set; } = string.Empty;
        public string Experience { get; set; } = string.Empty;
        public string PreferredJobTypes { get; set; } = string.Empty;
        public string PreferredLocations { get; set; } = string.Empty;
        public string MinimumSalary { get; set; } = string.Empty;
        public string ResumeFilePath { get; set; } = string.Empty;

        public List<Resume> Resumes { get; set; } = new List<Resume>();

        public string NotionPageId { get; set; } = string.Empty;
        public string TelegramChatId { get; set; } = string.Empty;
        public string PreferredModel { get; set; } = string.Empty;
        public string TechnologyStack { get; set; } = string.Empty;
        public string Position { get; set; } = string.Empty;
        public string PreferredCategories { get; set; } = "[]";
        public int MinQualityScore { get; set; } = 3;
        public bool AutoApplyEnabled { get; set; } = false;
    }
}
