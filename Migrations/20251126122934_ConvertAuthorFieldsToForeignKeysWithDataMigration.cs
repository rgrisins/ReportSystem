using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ReportSystem.Migrations
{
    /// <inheritdoc />
    public partial class ConvertAuthorFieldsToForeignKeysWithDataMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
                UPDATE ""Report"" r
                SET ""CreatedBy"" = u.""Id""
                FROM ""AspNetUsers"" u
                WHERE r.""CreatedBy"" = u.""UserName"";
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Report"" r
                SET ""LastModifiedBy"" = u.""Id""
                FROM ""AspNetUsers"" u
                WHERE r.""LastModifiedBy"" = u.""UserName"";
            ");

            migrationBuilder.CreateIndex(
                name: "IX_Report_CreatedBy",
                table: "Report",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Report_LastModifiedBy",
                table: "Report",
                column: "LastModifiedBy");

            migrationBuilder.AddForeignKey(
                name: "FK_Report_AspNetUsers_CreatedBy",
                table: "Report",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Report_AspNetUsers_LastModifiedBy",
                table: "Report",
                column: "LastModifiedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Report_AspNetUsers_CreatedBy",
                table: "Report");

            migrationBuilder.DropForeignKey(
                name: "FK_Report_AspNetUsers_LastModifiedBy",
                table: "Report");

            migrationBuilder.DropIndex(
                name: "IX_Report_CreatedBy",
                table: "Report");

            migrationBuilder.DropIndex(
                name: "IX_Report_LastModifiedBy",
                table: "Report");

            migrationBuilder.Sql(@"
                UPDATE ""Report"" r
                SET ""CreatedBy"" = u.""UserName""
                FROM ""AspNetUsers"" u
                WHERE r.""CreatedBy"" = u.""Id"";
            ");

            migrationBuilder.Sql(@"
                UPDATE ""Report"" r
                SET ""LastModifiedBy"" = u.""UserName""
                FROM ""AspNetUsers"" u
                WHERE r.""LastModifiedBy"" = u.""Id"";
            ");
        }
    }
}