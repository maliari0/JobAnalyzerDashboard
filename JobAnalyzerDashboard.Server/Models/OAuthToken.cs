using System;
using System.ComponentModel.DataAnnotations;

namespace JobAnalyzerDashboard.Server.Models
{
    public class OAuthToken
    {
        public int Id { get; set; }
        public int ProfileId { get; set; }
        public string Provider { get; set; } = "Google"; // Google, Microsoft, vb.
        public string AccessToken { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public DateTime ExpiresAt { get; set; }
        public string Email { get; set; } = string.Empty;
        public string Scope { get; set; } = string.Empty;
        
        // Navigation property
        public Profile? Profile { get; set; }
    }
}
