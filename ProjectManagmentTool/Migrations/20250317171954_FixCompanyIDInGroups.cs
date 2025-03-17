using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagmentTool.Migrations
{
    /// <inheritdoc />
    public partial class FixCompanyIDInGroups : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "Groups",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_CompanyID",
                table: "Groups",
                column: "CompanyID");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Companies_CompanyID",
                table: "Groups",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Companies_CompanyID",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_CompanyID",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "Groups");
        }
    }
}
