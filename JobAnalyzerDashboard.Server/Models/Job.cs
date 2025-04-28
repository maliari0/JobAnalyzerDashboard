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
        public int QualityScore { get; set; } 
        public string Source { get; set; } = string.Empty; 
        public bool IsApplied { get; set; } 
        public DateTime? AppliedDate { get; set; } 

        // N8n entegrasyonu için eklenen alanlar
        public string ActionSuggestion { get; set; } = string.Empty; 
        public string Category { get; set; } = string.Empty; 
        public List<string> Tags { get; set; } = new List<string>(); 
        public string CompanyWebsite { get; set; } = string.Empty;
        public string ContactEmail { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
        public bool IsJobPosting { get; set; } = true; 
        public int ParsedMinSalary { get; set; } 
    }
}
