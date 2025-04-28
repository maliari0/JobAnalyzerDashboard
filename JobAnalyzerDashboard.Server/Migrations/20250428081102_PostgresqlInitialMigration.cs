using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace JobAnalyzerDashboard.Server.Migrations
{
    /// <inheritdoc />
    public partial class PostgresqlInitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Jobs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Company = table.Column<string>(type: "text", nullable: false),
                    Location = table.Column<string>(type: "text", nullable: false),
                    EmploymentType = table.Column<string>(type: "text", nullable: false),
                    Salary = table.Column<string>(type: "text", nullable: false),
                    PostedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ApplicationDeadline = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    QualityScore = table.Column<int>(type: "integer", nullable: false),
                    Source = table.Column<string>(type: "text", nullable: false),
                    IsApplied = table.Column<bool>(type: "boolean", nullable: false),
                    AppliedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ActionSuggestion = table.Column<string>(type: "text", nullable: false),
                    Category = table.Column<string>(type: "text", nullable: false),
                    Tags = table.Column<List<string>>(type: "jsonb", nullable: false),
                    CompanyWebsite = table.Column<string>(type: "text", nullable: false),
                    ContactEmail = table.Column<string>(type: "text", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    IsJobPosting = table.Column<bool>(type: "boolean", nullable: false),
                    ParsedMinSalary = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Jobs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Profiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FullName = table.Column<string>(type: "text", nullable: false),
                    Email = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: false),
                    LinkedInUrl = table.Column<string>(type: "text", nullable: false),
                    GithubUrl = table.Column<string>(type: "text", nullable: false),
                    PortfolioUrl = table.Column<string>(type: "text", nullable: false),
                    Skills = table.Column<string>(type: "text", nullable: false),
                    Education = table.Column<string>(type: "text", nullable: false),
                    Experience = table.Column<string>(type: "text", nullable: false),
                    PreferredJobTypes = table.Column<string>(type: "text", nullable: false),
                    PreferredLocations = table.Column<string>(type: "text", nullable: false),
                    MinimumSalary = table.Column<string>(type: "text", nullable: false),
                    ResumeFilePath = table.Column<string>(type: "text", nullable: false),
                    NotionPageId = table.Column<string>(type: "text", nullable: false),
                    TelegramChatId = table.Column<string>(type: "text", nullable: false),
                    PreferredModel = table.Column<string>(type: "text", nullable: false),
                    TechnologyStack = table.Column<string>(type: "text", nullable: false),
                    Position = table.Column<string>(type: "text", nullable: false),
                    PreferredCategories = table.Column<List<string>>(type: "jsonb", nullable: false),
                    MinQualityScore = table.Column<int>(type: "integer", nullable: false),
                    AutoApplyEnabled = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Profiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    JobId = table.Column<int>(type: "integer", nullable: false),
                    AppliedDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    ResponseDetails = table.Column<string>(type: "text", nullable: false),
                    ResponseDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    AppliedMethod = table.Column<string>(type: "text", nullable: false),
                    SentMessage = table.Column<string>(type: "text", nullable: false),
                    IsAutoApplied = table.Column<bool>(type: "boolean", nullable: false),
                    NotionPageId = table.Column<string>(type: "text", nullable: false),
                    CvAttached = table.Column<bool>(type: "boolean", nullable: false),
                    TelegramNotificationSent = table.Column<string>(type: "text", nullable: false),
                    IsJobDeleted = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Applications_Jobs_JobId",
                        column: x => x.JobId,
                        principalTable: "Jobs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "Resumes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    FileSize = table.Column<long>(type: "bigint", nullable: false),
                    FileType = table.Column<string>(type: "text", nullable: false),
                    UploadDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    IsDefault = table.Column<bool>(type: "boolean", nullable: false),
                    ProfileId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Resumes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Resumes_Profiles_ProfileId",
                        column: x => x.ProfileId,
                        principalTable: "Profiles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Applications_JobId",
                table: "Applications",
                column: "JobId");

            migrationBuilder.CreateIndex(
                name: "IX_Resumes_ProfileId",
                table: "Resumes",
                column: "ProfileId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "Resumes");

            migrationBuilder.DropTable(
                name: "Jobs");

            migrationBuilder.DropTable(
                name: "Profiles");
        }
    }
}
