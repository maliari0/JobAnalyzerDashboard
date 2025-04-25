using System;
using System.Collections.Generic;

namespace JobAnalyzerDashboard.Server.Models
{
    public class Job
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Company { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string EmploymentType { get; set; } = string.Empty;
        public string Salary { get; set; } = string.Empty;
        public DateTime PostedDate { get; set; }
        public DateTime? ApplicationDeadline { get; set; }
        public int QualityScore { get; set; } // n8n tarafından verilen puan (1-5 arası)
        public string Source { get; set; } = string.Empty; // Email veya Webhook
        public bool IsApplied { get; set; } // Başvuru yapıldı mı?
        public DateTime? AppliedDate { get; set; } // Başvuru tarihi

        // N8n entegrasyonu için eklenen alanlar
        public string ActionSuggestion { get; set; } = string.Empty; // sakla, bildir, ilgisiz
        public string Category { get; set; } = string.Empty; // frontend, backend, mobile, devops, data science, diğer
        public List<string> Tags { get; set; } = new List<string>(); // remote, junior, b2b, vb.
        public string CompanyWebsite { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public bool IsJobPosting { get; set; } = true; // İş ilanı mı?
        public int ParsedMinSalary { get; set; } // Maaş aralığından çıkarılan minimum maaş değeri
    }
}
