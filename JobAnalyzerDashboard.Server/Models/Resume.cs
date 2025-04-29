using System;
using System.Text.Json.Serialization;

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
        [JsonIgnore]
        public Profile? Profile { get; set; }
    }
}
