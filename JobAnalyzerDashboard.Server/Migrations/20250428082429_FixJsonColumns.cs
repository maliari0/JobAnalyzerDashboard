using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobAnalyzerDashboard.Server.Migrations
{
    /// <inheritdoc />
    public partial class FixJsonColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "PreferredCategories",
                table: "Profiles",
                type: "text",
                nullable: false,
                oldClrType: typeof(List<string>),
                oldType: "jsonb");

            migrationBuilder.AlterColumn<string>(
                name: "Tags",
                table: "Jobs",
                type: "text",
                nullable: false,
                oldClrType: typeof(List<string>),
                oldType: "jsonb");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<List<string>>(
                name: "PreferredCategories",
                table: "Profiles",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<List<string>>(
                name: "Tags",
                table: "Jobs",
                type: "jsonb",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");
        }
    }
}
