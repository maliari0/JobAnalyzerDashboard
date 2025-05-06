using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobAnalyzerDashboard.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddMissingSentMessageColumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SentMessage",
                table: "Applications",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SentMessage",
                table: "Applications");
        }
    }
}
