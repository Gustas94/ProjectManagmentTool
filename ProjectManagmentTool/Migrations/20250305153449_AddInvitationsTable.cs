using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagmentTool.Migrations
{
    /// <inheritdoc />
    public partial class AddInvitationsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invitation_AspNetRoles_RoleID",
                table: "Invitation");

            migrationBuilder.DropForeignKey(
                name: "FK_Invitation_Companies_CompanyID",
                table: "Invitation");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Invitation",
                table: "Invitation");

            migrationBuilder.RenameTable(
                name: "Invitation",
                newName: "Invitations");

            migrationBuilder.RenameIndex(
                name: "IX_Invitation_RoleID",
                table: "Invitations",
                newName: "IX_Invitations_RoleID");

            migrationBuilder.RenameIndex(
                name: "IX_Invitation_CompanyID",
                table: "Invitations",
                newName: "IX_Invitations_CompanyID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Invitations",
                table: "Invitations",
                column: "InvitationID");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_AspNetRoles_RoleID",
                table: "Invitations",
                column: "RoleID",
                principalTable: "AspNetRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitations_Companies_CompanyID",
                table: "Invitations",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_AspNetRoles_RoleID",
                table: "Invitations");

            migrationBuilder.DropForeignKey(
                name: "FK_Invitations_Companies_CompanyID",
                table: "Invitations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Invitations",
                table: "Invitations");

            migrationBuilder.RenameTable(
                name: "Invitations",
                newName: "Invitation");

            migrationBuilder.RenameIndex(
                name: "IX_Invitations_RoleID",
                table: "Invitation",
                newName: "IX_Invitation_RoleID");

            migrationBuilder.RenameIndex(
                name: "IX_Invitations_CompanyID",
                table: "Invitation",
                newName: "IX_Invitation_CompanyID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Invitation",
                table: "Invitation",
                column: "InvitationID");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitation_AspNetRoles_RoleID",
                table: "Invitation",
                column: "RoleID",
                principalTable: "AspNetRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Invitation_Companies_CompanyID",
                table: "Invitation",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
