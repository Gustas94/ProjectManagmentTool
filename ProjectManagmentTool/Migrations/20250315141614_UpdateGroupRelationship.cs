using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagmentTool.Migrations
{
    /// <inheritdoc />
    public partial class UpdateGroupRelationship : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Discussion_Group_GroupID",
                table: "Discussion");

            migrationBuilder.DropForeignKey(
                name: "FK_Group_AspNetUsers_GroupLeadID",
                table: "Group");

            migrationBuilder.DropForeignKey(
                name: "FK_Group_Projects_ProjectID",
                table: "Group");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Group_GroupID",
                table: "Tasks");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Group",
                table: "Group");

            migrationBuilder.DropIndex(
                name: "IX_Group_ProjectID",
                table: "Group");

            migrationBuilder.DropColumn(
                name: "ProjectID",
                table: "Group");

            migrationBuilder.RenameTable(
                name: "Group",
                newName: "Groups");

            migrationBuilder.RenameIndex(
                name: "IX_Group_GroupLeadID",
                table: "Groups",
                newName: "IX_Groups_GroupLeadID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Groups",
                table: "Groups",
                column: "GroupID");

            migrationBuilder.CreateTable(
                name: "ProjectGroups",
                columns: table => new
                {
                    ProjectID = table.Column<int>(type: "int", nullable: false),
                    GroupID = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ProjectGroups", x => new { x.ProjectID, x.GroupID });
                    table.ForeignKey(
                        name: "FK_ProjectGroups_Groups_GroupID",
                        column: x => x.GroupID,
                        principalTable: "Groups",
                        principalColumn: "GroupID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ProjectGroups_Projects_ProjectID",
                        column: x => x.ProjectID,
                        principalTable: "Projects",
                        principalColumn: "ProjectID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ProjectGroups_GroupID",
                table: "ProjectGroups",
                column: "GroupID");

            migrationBuilder.AddForeignKey(
                name: "FK_Discussion_Groups_GroupID",
                table: "Discussion",
                column: "GroupID",
                principalTable: "Groups",
                principalColumn: "GroupID");

            migrationBuilder.AddForeignKey(
                name: "FK_Groups_AspNetUsers_GroupLeadID",
                table: "Groups",
                column: "GroupLeadID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Groups_GroupID",
                table: "Tasks",
                column: "GroupID",
                principalTable: "Groups",
                principalColumn: "GroupID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Discussion_Groups_GroupID",
                table: "Discussion");

            migrationBuilder.DropForeignKey(
                name: "FK_Groups_AspNetUsers_GroupLeadID",
                table: "Groups");

            migrationBuilder.DropForeignKey(
                name: "FK_Tasks_Groups_GroupID",
                table: "Tasks");

            migrationBuilder.DropTable(
                name: "ProjectGroups");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Groups",
                table: "Groups");

            migrationBuilder.RenameTable(
                name: "Groups",
                newName: "Group");

            migrationBuilder.RenameIndex(
                name: "IX_Groups_GroupLeadID",
                table: "Group",
                newName: "IX_Group_GroupLeadID");

            migrationBuilder.AddColumn<int>(
                name: "ProjectID",
                table: "Group",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Group",
                table: "Group",
                column: "GroupID");

            migrationBuilder.CreateIndex(
                name: "IX_Group_ProjectID",
                table: "Group",
                column: "ProjectID");

            migrationBuilder.AddForeignKey(
                name: "FK_Discussion_Group_GroupID",
                table: "Discussion",
                column: "GroupID",
                principalTable: "Group",
                principalColumn: "GroupID");

            migrationBuilder.AddForeignKey(
                name: "FK_Group_AspNetUsers_GroupLeadID",
                table: "Group",
                column: "GroupLeadID",
                principalTable: "AspNetUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Group_Projects_ProjectID",
                table: "Group",
                column: "ProjectID",
                principalTable: "Projects",
                principalColumn: "ProjectID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Tasks_Group_GroupID",
                table: "Tasks",
                column: "GroupID",
                principalTable: "Group",
                principalColumn: "GroupID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
