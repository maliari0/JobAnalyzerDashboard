using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobAnalyzerDashboard.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddDefaultProfile : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Profiles",
                columns: new[] { "Id", "AutoApplyEnabled", "Education", "Email", "Experience", "FullName", "GithubUrl", "LinkedInUrl", "MinQualityScore", "MinimumSalary", "NotionPageId", "Phone", "PortfolioUrl", "Position", "PreferredCategories", "PreferredJobTypes", "PreferredLocations", "PreferredModel", "ResumeFilePath", "Skills", "TechnologyStack", "TelegramChatId" },
                values: new object[] { 1, false, "", "kullanici@example.com", "", "Kullanıcı", "", "", 3, "", "", "", "", "", "[]", "", "", "", "", "", "", "" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Profiles",
                keyColumn: "Id",
                keyValue: 1);
        }
    }
}
