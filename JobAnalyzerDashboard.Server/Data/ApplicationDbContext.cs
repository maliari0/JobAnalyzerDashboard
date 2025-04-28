using Microsoft.EntityFrameworkCore;
using JobAnalyzerDashboard.Server.Models;
using System.Collections.Generic;

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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // İlişkileri ve kısıtlamaları tanımla
            modelBuilder.Entity<Application>()
                .HasOne(a => a.Job)
                .WithMany()
                .HasForeignKey(a => a.JobId)
                .OnDelete(DeleteBehavior.SetNull);

            modelBuilder.Entity<Resume>()
                .HasOne(r => r.Profile)
                .WithMany(p => p.Resumes)
                .HasForeignKey(r => r.ProfileId)
                .OnDelete(DeleteBehavior.Cascade);

            // Job entity'si için Tags alanını JSON olarak sakla
            modelBuilder.Entity<Job>()
                .Property(j => j.Tags)
                .HasColumnType("text");

            // Profile entity'si için PreferredCategories alanını JSON olarak sakla
            modelBuilder.Entity<Profile>()
                .Property(p => p.PreferredCategories)
                .HasColumnType("text");

            // Seed data ekle
            SeedData(modelBuilder);
        }

        private void SeedData(ModelBuilder modelBuilder)
        {
            // Seed data'yı migration sonrası manuel olarak ekleyeceğiz
        }
    }
}
