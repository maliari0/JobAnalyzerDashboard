using System.Collections.Generic;

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
        public string ResumeFilePath { get; set; } = string.Empty; // Eski alan, geriye uyumluluk için

        // Özgeçmiş yönetimi için eklenen alanlar
        public List<Resume> Resumes { get; set; } = new List<Resume>();

        // N8n entegrasyonu için eklenen alanlar
        public string NotionPageId { get; set; } = string.Empty; // Notion'daki kullanıcı profil sayfası ID'si
        public string TelegramChatId { get; set; } = string.Empty; // Telegram chat ID'si
        public string PreferredModel { get; set; } = string.Empty; // Tercih edilen başvuru mesajı modeli
        public string TechnologyStack { get; set; } = string.Empty; // Teknoloji stack'i
        public string Position { get; set; } = string.Empty; // Güncel pozisyon
        public List<string> PreferredCategories { get; set; } = new List<string>(); // Tercih edilen kategoriler (frontend, backend, vb.)
        public int MinQualityScore { get; set; } = 3; // Minimum kalite puanı (1-5 arası)
        public bool AutoApplyEnabled { get; set; } = false; // Otomatik başvuru etkin mi?
    }
}
