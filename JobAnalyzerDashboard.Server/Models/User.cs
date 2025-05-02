using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace JobAnalyzerDashboard.Server.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [EmailAddress]
        public string Email { get; set; } = string.Empty;

        [Required]
        public string Username { get; set; } = string.Empty;

        [JsonIgnore]
        public string PasswordHash { get; set; } = string.Empty;

        [JsonIgnore]
        public string PasswordSalt { get; set; } = string.Empty;

        public string FirstName { get; set; } = string.Empty;
        
        public string LastName { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        
        public DateTime? LastLoginAt { get; set; }

        public bool IsActive { get; set; } = true;

        public bool EmailConfirmed { get; set; } = false;

        [JsonIgnore]
        public string? EmailConfirmationToken { get; set; }

        [JsonIgnore]
        public DateTime? EmailConfirmationTokenExpiry { get; set; }

        [JsonIgnore]
        public string? PasswordResetToken { get; set; }

        [JsonIgnore]
        public DateTime? PasswordResetTokenExpiry { get; set; }

        // İlişkiler
        public Profile? Profile { get; set; }
        public int? ProfileId { get; set; }

        // Kullanıcı rolü (basit yetkilendirme için)
        public string Role { get; set; } = "User"; // User, Admin, vb.
    }
}
