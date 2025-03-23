using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagmentTool.Migrations
{
    /// <inheritdoc />
    public partial class AddGroupIDToUserProject : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "GroupID",
                table: "UserProjects",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_UserProjects_GroupID",
                table: "UserProjects",
                column: "GroupID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserProjects_Groups_GroupID",
                table: "UserProjects",
                column: "GroupID",
                principalTable: "Groups",
                principalColumn: "GroupID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_UserProjects_Groups_GroupID",
                table: "UserProjects");

            migrationBuilder.DropIndex(
                name: "IX_UserProjects_GroupID",
                table: "UserProjects");

            migrationBuilder.DropColumn(
                name: "GroupID",
                table: "UserProjects");
        }
    }
}
