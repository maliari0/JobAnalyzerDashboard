using System;
using System.Text.Json.Serialization;

namespace JobAnalyzerDashboard.Server.Models
{
    public class Announcement
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }
        public string Type { get; set; } = "info"; // info, warning, success, error
        public int? CreatedById { get; set; }
        
        [JsonIgnore]
        public User? CreatedBy { get; set; }
    }
}
