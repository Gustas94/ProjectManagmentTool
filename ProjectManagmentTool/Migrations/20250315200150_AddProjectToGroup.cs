using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagmentTool.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectToGroup : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ProjectID",
                table: "Groups",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Groups_ProjectID",
                table: "Groups",
                column: "ProjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_Projects_ProjectID",
                table: "Groups",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ProjectID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Groups_Projects_ProjectID",
                table: "Groups");

            migrationBuilder.DropIndex(
                name: "IX_Groups_ProjectID",
                table: "Groups");

            migrationBuilder.DropColumn(
                name: "ProjectID",
                table: "Groups");
        }
    }
}
