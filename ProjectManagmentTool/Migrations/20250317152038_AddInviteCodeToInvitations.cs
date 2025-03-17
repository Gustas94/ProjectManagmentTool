using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagmentTool.Migrations
{
    /// <inheritdoc />
    public partial class AddInviteCodeToInvitations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InvitationLink",
                table: "Invitations",
                newName: "InviteCode");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "InviteCode",
                table: "Invitations",
                newName: "InvitationLink");
        }
    }
}
