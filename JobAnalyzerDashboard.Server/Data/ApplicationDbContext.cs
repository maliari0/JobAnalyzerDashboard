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
        public DbSet<Announcement> Announcements { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Application>()
                .HasOne(a => a.Job)
                .WithMany()
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.Restrict);

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

            modelBuilder.Entity<User>()
                .HasOne(u => u.Profile)
                .WithOne(p => p.User)
                .HasForeignKey<Profile>(p => p.UserId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Username)
                .IsUnique();

            modelBuilder.Entity<Job>()
                .Property(j => j.Tags)
                .HasColumnType("text");

            modelBuilder.Entity<Job>()
                .Property(j => j.Description)
                .HasColumnType("text")
                .IsUnicode(true);

            modelBuilder.Entity<Job>()
                .Property(j => j.Title)
                .HasColumnType("text")
                .IsUnicode(true);

            modelBuilder.Entity<Job>()
                .Property(j => j.Company)
                .HasColumnType("text")
                .IsUnicode(true);

            modelBuilder.Entity<Profile>()
                .Property(p => p.PreferredCategories)
                .HasColumnType("text");

            modelBuilder.Entity<Announcement>()
                .HasOne(a => a.CreatedBy)
                .WithMany()
                .HasForeignKey(a => a.CreatedById)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Announcement>()
                .Property(a => a.Content)
                .HasColumnType("text")
                .IsUnicode(true);

            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
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
