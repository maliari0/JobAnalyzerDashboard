using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace JobAnalyzerDashboard.Server.Migrations
{
    /// <inheritdoc />
    public partial class AddUserIdToApplication : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Jobs_JobId",
                table: "Applications");

            // Message sütunu zaten mevcut olduğu için bu kodu atlıyoruz
            // migrationBuilder.AddColumn<string>(
            //     name: "Message",
            //     table: "Applications",
            //     type: "text",
            //     nullable: false,
            //     defaultValue: "");

            // Boş bir SQL komutu çalıştırarak migration'ın çalışmasını sağlayalım
            migrationBuilder.Sql("SELECT 1");

            // UserId sütunu zaten mevcut olduğu için bu kodu atlıyoruz
            // migrationBuilder.AddColumn<int>(
            //     name: "UserId",
            //     table: "Applications",
            //     type: "integer",
            //     nullable: true);

            // Boş bir SQL komutu çalıştırarak migration'ın çalışmasını sağlayalım
            migrationBuilder.Sql("SELECT 1");

            // Index zaten mevcut olduğu için bu kodu atlıyoruz
            // migrationBuilder.CreateIndex(
            //     name: "IX_Applications_UserId",
            //     table: "Applications",
            //     column: "UserId");

            // Boş bir SQL komutu çalıştırarak migration'ın çalışmasını sağlayalım
            migrationBuilder.Sql("SELECT 1");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Jobs_JobId",
                table: "Applications",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            // Foreign key constraint zaten mevcut olduğu için bu kodu atlıyoruz
            // migrationBuilder.AddForeignKey(
            //     name: "FK_Applications_Users_UserId",
            //     table: "Applications",
            //     column: "UserId",
            //     principalTable: "Users",
            //     principalColumn: "Id",
            //     onDelete: ReferentialAction.SetNull);

            // Boş bir SQL komutu çalıştırarak migration'ın çalışmasını sağlayalım
            migrationBuilder.Sql("SELECT 1");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Jobs_JobId",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Users_UserId",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_UserId",
                table: "Applications");

            // Message sütunu zaten mevcut olduğu için bu kodu atlıyoruz
            // migrationBuilder.DropColumn(
            //     name: "Message",
            //     table: "Applications");

            // Boş bir SQL komutu çalıştırarak migration'ın çalışmasını sağlayalım
            migrationBuilder.Sql("SELECT 1");

            migrationBuilder.DropColumn(
                name: "UserId",
                table: "Applications");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Jobs_JobId",
                table: "Applications",
                column: "JobId",
                principalTable: "Jobs",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
