using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagmentTool.Migrations
{
    /// <inheritdoc />
    public partial class AddProjectsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Discussion_Project_ProjectID",
                table: "Discussion");

            migrationBuilder.DropForeignKey(
                name: "FK_Group_Project_ProjectID",
                table: "Group");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_AspNetUsers_ProjectManagerID",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_Project_Companies_CompanyID",
                table: "Project");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_Project_ProjectID",
                table: "Task");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Project_ProjectID",
                table: "UserRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Project",
                table: "Project");

            migrationBuilder.RenameTable(
                name: "Project",
                newName: "Projects");

            migrationBuilder.RenameIndex(
                name: "IX_Project_ProjectManagerID",
                table: "Projects",
                newName: "IX_Projects_ProjectManagerID");

            migrationBuilder.RenameIndex(
                name: "IX_Project_CompanyID",
                table: "Projects",
                newName: "IX_Projects_CompanyID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Projects",
                table: "Projects",
                column: "ProjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_Discussion_Projects_ProjectID",
                table: "Discussion",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ProjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_Group_Projects_ProjectID",
                table: "Group",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ProjectID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_AspNetUsers_ProjectManagerID",
                table: "Projects",
                column: "ProjectManagerID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Projects_Companies_CompanyID",
                table: "Projects",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Projects_ProjectID",
                table: "Task",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ProjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Projects_ProjectID",
                table: "UserRole",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ProjectID");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Discussion_Projects_ProjectID",
                table: "Discussion");

            migrationBuilder.DropForeignKey(
                name: "FK_Group_Projects_ProjectID",
                table: "Group");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_AspNetUsers_ProjectManagerID",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Projects_Companies_CompanyID",
                table: "Projects");

            migrationBuilder.DropForeignKey(
                name: "FK_Task_Projects_ProjectID",
                table: "Task");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Projects_ProjectID",
                table: "UserRole");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Projects",
                table: "Projects");

            migrationBuilder.RenameTable(
                name: "Projects",
                newName: "Project");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_ProjectManagerID",
                table: "Project",
                newName: "IX_Project_ProjectManagerID");

            migrationBuilder.RenameIndex(
                name: "IX_Projects_CompanyID",
                table: "Project",
                newName: "IX_Project_CompanyID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Project",
                table: "Project",
                column: "ProjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_Discussion_Project_ProjectID",
                table: "Discussion",
                column: "ProjectID",
                principalTable: "Project",
                principalColumn: "ProjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_Group_Project_ProjectID",
                table: "Group",
                column: "ProjectID",
                principalTable: "Project",
                principalColumn: "ProjectID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Project_AspNetUsers_ProjectManagerID",
                table: "Project",
                column: "ProjectManagerID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Project_Companies_CompanyID",
                table: "Project",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Task_Project_ProjectID",
                table: "Task",
                column: "ProjectID",
                principalTable: "Project",
                principalColumn: "ProjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Project_ProjectID",
                table: "UserRole",
                column: "ProjectID",
                principalTable: "Project",
                principalColumn: "ProjectID");
        }
    }
}
