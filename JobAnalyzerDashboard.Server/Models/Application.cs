using System;
using System.Text.Json.Serialization;

namespace JobAnalyzerDashboard.Server.Models
{
    public class Application
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public DateTime AppliedDate { get; set; }
        public string Status { get; set; } = string.Empty; // Pending, Accepted, Rejected, Interview
        public string ResponseDetails { get; set; } = string.Empty;
        public DateTime? ResponseDate { get; set; }
        public string AppliedMethod { get; set; } = string.Empty; // Email, Form, API

        // N8n entegrasyonu için eklenen alanlar
        public string SentMessage { get; set; } = string.Empty;
        // Veritabanı şeması ile uyumlu olması için Message alanı
        public string Message { get; set; } = string.Empty;
        public bool IsAutoApplied { get; set; } = false;
        public string NotionPageId { get; set; } = string.Empty; // old
        public bool CvAttached { get; set; } = false;
        public string TelegramNotificationSent { get; set; } = string.Empty;

        // LLM tarafından oluşturulan e-posta içeriği
        public string EmailContent { get; set; } = string.Empty;

        // Navigation property
        public Job? Job { get; set; }

        // İş ilanı silinmiş mi?
        public bool IsJobDeleted { get; set; } = false;

        // Kullanıcı ilişkisi
        public int? UserId { get; set; }
        [JsonIgnore]
        public User? User { get; set; }
    }
}
