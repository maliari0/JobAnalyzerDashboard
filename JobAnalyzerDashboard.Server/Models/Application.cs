using System;
using System.Text.Json.Serialization;

namespace JobAnalyzerDashboard.Server.Models
{
    public class Application
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ResponseDetails { get; set; } = string.Empty;
        public DateTime? ResponseDate { get; set; }
        public string AppliedMethod { get; set; } = string.Empty;

        public string SentMessage { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public bool IsAutoApplied { get; set; } = false;
        public string NotionPageId { get; set; } = string.Empty;
        public bool CvAttached { get; set; } = false;
        public string TelegramNotificationSent { get; set; } = string.Empty;
        public string EmailContent { get; set; } = string.Empty;

        public Job? Job { get; set; }
        public bool IsJobDeleted { get; set; } = false;

        // Kullanıcı ilişkisi
        public int? UserId { get; set; }
        [JsonIgnore]
        public User? User { get; set; }
    }
}
