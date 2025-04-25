using System;

namespace JobAnalyzerDashboard.Server.Models
{
    public class Resume
    {
        public int Id { get; set; }
        public string FileName { get; set; } = string.Empty;
        public string FilePath { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public string FileType { get; set; } = string.Empty;
        public DateTime UploadDate { get; set; }
        public bool IsDefault { get; set; }
        public int ProfileId { get; set; }
        
        // Navigation property
        public Profile? Profile { get; set; }
    }
}
