using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ProjectManagmentTool.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIdentityRole : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_Role_RoleID",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Invitation_Role_RoleID",
                table: "Invitation");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_Role_RoleID",
                table: "RolePermission");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_Role_RoleID",
                table: "UserRole");

            migrationBuilder.DropTable(
                name: "Role");

            migrationBuilder.AddColumn<int>(
                name: "CompanyID",
                table: "AspNetRoles",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "CreatedAt",
                table: "AspNetRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetRoles",
                type: "nvarchar(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<bool>(
                name: "IsCompanyRole",
                table: "AspNetRoles",
                type: "bit",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "UpdatedAt",
                table: "AspNetRoles",
                type: "datetime2",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_AspNetRoles_CompanyID",
                table: "AspNetRoles",
                column: "CompanyID");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetRoles_Companies_CompanyID",
                table: "AspNetRoles",
                column: "CompanyID",
                principalTable: "Companies",
                principalColumn: "CompanyID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_AspNetRoles_RoleID",
                table: "AspNetUsers",
                column: "RoleID",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invitation_AspNetRoles_RoleID",
                table: "Invitation",
                column: "RoleID",
                principalTable: "AspNetRoles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermission_AspNetRoles_RoleID",
                table: "RolePermission",
                column: "RoleID",
                principalTable: "AspNetRoles",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_AspNetRoles_RoleID",
                table: "UserRole",
                column: "RoleID",
                principalTable: "AspNetRoles",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AspNetRoles_Companies_CompanyID",
                table: "AspNetRoles");

            migrationBuilder.DropForeignKey(
                name: "FK_AspNetUsers_AspNetRoles_RoleID",
                table: "AspNetUsers");

            migrationBuilder.DropForeignKey(
                name: "FK_Invitation_AspNetRoles_RoleID",
                table: "Invitation");

            migrationBuilder.DropForeignKey(
                name: "FK_RolePermission_AspNetRoles_RoleID",
                table: "RolePermission");

            migrationBuilder.DropForeignKey(
                name: "FK_UserRole_AspNetRoles_RoleID",
                table: "UserRole");

            migrationBuilder.DropIndex(
                name: "IX_AspNetRoles_CompanyID",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "CompanyID",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "IsCompanyRole",
                table: "AspNetRoles");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "AspNetRoles");

            migrationBuilder.CreateTable(
                name: "Role",
                columns: table => new
                {
                    RoleID = table.Column<string>(type: "nvarchar(450)", nullable: false),
                    CompanyID = table.Column<int>(type: "int", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsCompanyRole = table.Column<bool>(type: "bit", nullable: false),
                    RoleName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Role", x => x.RoleID);
                    table.ForeignKey(
                        name: "FK_Role_Companies_CompanyID",
                        column: x => x.CompanyID,
                        principalTable: "Companies",
                        principalColumn: "CompanyID",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Role_CompanyID",
                table: "Role",
                column: "CompanyID");

            migrationBuilder.AddForeignKey(
                name: "FK_AspNetUsers_Role_RoleID",
                table: "AspNetUsers",
                column: "RoleID",
                principalTable: "Role",
                principalColumn: "RoleID",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Invitation_Role_RoleID",
                table: "Invitation",
                column: "RoleID",
                principalTable: "Role",
                principalColumn: "RoleID");

            migrationBuilder.AddForeignKey(
                name: "FK_RolePermission_Role_RoleID",
                table: "RolePermission",
                column: "RoleID",
                principalTable: "Role",
                principalColumn: "RoleID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_UserRole_Role_RoleID",
                table: "UserRole",
                column: "RoleID",
                principalTable: "Role",
                principalColumn: "RoleID");
        }
    }
}
