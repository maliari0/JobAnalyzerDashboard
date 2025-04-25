using System;

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
        public string SentMessage { get; set; } = string.Empty; // Gönderilen başvuru mesajı
        public bool IsAutoApplied { get; set; } = false; // Otomatik başvuru yapıldı mı?
        public string NotionPageId { get; set; } = string.Empty; // Notion'daki sayfa ID'si
        public bool CvAttached { get; set; } = false; // CV eklendi mi?
        public string TelegramNotificationSent { get; set; } = string.Empty; // Telegram bildirimi gönderildi mi?

        // Navigation property
        public Job? Job { get; set; }
    }
}
