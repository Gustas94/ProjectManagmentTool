using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagmentTool.Migrations
{
    /// <inheritdoc />
    public partial class UpdateCompanyUserRelationships : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CompanyID",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "CEOID",
                table: "Companies",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_Companies_CEOID",
                table: "Companies",
                column: "CEOID",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CompanyID",
                table: "AspNetUsers",
                column: "CompanyID");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_AspNetUsers_CEOID",
                table: "Companies",
                column: "CEOID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_AspNetUsers_CEOID",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_Companies_CEOID",
                table: "Companies");

            migrationBuilder.DropIndex(
                name: "IX_AspNetUsers_CompanyID",
                table: "AspNetUsers");

            migrationBuilder.AlterColumn<string>(
                name: "CEOID",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.CreateIndex(
                name: "IX_AspNetUsers_CompanyID",
                table: "AspNetUsers",
                column: "CompanyID",
                unique: true,
                filter: "[CompanyID] IS NOT NULL");
        }
    }
}
