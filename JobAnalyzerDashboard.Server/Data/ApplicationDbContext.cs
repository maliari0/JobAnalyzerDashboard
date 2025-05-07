using Microsoft.EntityFrameworkCore;
using JobAnalyzerDashboard.Server.Models;
using System.Collections.Generic;
using System.Text;

namespace JobAnalyzerDashboard.Server.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Job> Jobs { get; set; }
        public DbSet<Application> Applications { get; set; }
        public DbSet<Profile> Profiles { get; set; }
        public DbSet<Resume> Resumes { get; set; }
        public DbSet<OAuthToken> OAuthTokens { get; set; }
        public DbSet<User> Users { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // İlişkileri ve kısıtlamaları tanımla
            modelBuilder.Entity<Application>()
                .HasOne(a => a.Job)
                .WithMany()
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Restrict);

            // Application ve User ilişkisi
            modelBuilder.Entity<Application>()
                .HasOne(a => a.User)
                .WithMany()
                .HasForeignKey(a => a.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Resume>()
                .HasOne(r => r.Profile)
                .WithMany(p => p.Resumes)
                .HasForeignKey(r => r.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<OAuthToken>()
                .HasOne(t => t.Profile)
                .WithMany()
                .HasForeignKey(t => t.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // User ve Profile ilişkisi
            modelBuilder.Entity<User>()
                .HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<Profile>(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            // User entity'si için indeksler
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            // Job entity'si için Tags alanını JSON olarak sakla
            modelBuilder.Entity<Job>()
                .Property(j => j.Tags)
                .HasColumnType("text");

            // Job entity'si için Description alanını UTF-8 olarak sakla
            modelBuilder.Entity<Job>()
                .Property(j => j.Description)
                .HasColumnType("text")
                .IsUnicode(true);

            // Job entity'si için Title alanını UTF-8 olarak sakla
            modelBuilder.Entity<Job>()
                .Property(j => j.Title)
                .HasColumnType("text")
                .IsUnicode(true);

            // Job entity'si için Company alanını UTF-8 olarak sakla
            modelBuilder.Entity<Job>()
                .Property(j => j.Company)
                .HasColumnType("text")
                .IsUnicode(true);

            // Profile entity'si için PreferredCategories alanını JSON olarak sakla
            modelBuilder.Entity<Profile>()
                .Property(p => p.PreferredCategories)
                .HasColumnType("text");

            // Seed data ekle
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Varsayılan profil ekle
            modelBuilder.Entity<Profile>().HasData(
                new Profile
                {
                    Id = 1,
                    FullName = "Kullanıcı",
                    Email = "kullanici@example.com",
                    Phone = "",
                    LinkedInUrl = "",
                    GithubUrl = "",
                    PortfolioUrl = "",
                    Skills = "",
                    Education = "",
                    Experience = "",
                    PreferredJobTypes = "",
                    PreferredLocations = "",
                    MinimumSalary = "",
                    ResumeFilePath = "",
                    NotionPageId = "",
                    TelegramChatId = "",
                    PreferredModel = "",
                    TechnologyStack = "",
                    Position = "",
                    PreferredCategories = "[]",
                    MinQualityScore = 3,
                    AutoApplyEnabled = false
                }
            );
        }
    }
}
