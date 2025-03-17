using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ProjectManagmentTool.Migrations
{
    /// <inheritdoc />
    public partial class AddIndustryEntity : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Industry",
                table: "Companies");

            migrationBuilder.AddColumn<int>(
                name: "IndustryId",
                table: "Companies",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Industries",
                columns: table => new
                {
                    IndustryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Industries", x => x.IndustryId);
                });

            migrationBuilder.InsertData(
                table: "Industries",
                columns: new[] { "IndustryId", "Name" },
                values: new object[,]
                {
                    { 1, "Medical" },
                    { 2, "Technology" },
                    { 3, "Software Development" },
                    { 4, "Finance" },
                    { 5, "Retail" },
                    { 6, "Education" },
                    { 7, "Hospitality" },
                    { 8, "Manufacturing" },
                    { 9, "Transportation" },
                    { 10, "Energy" },
                    { 11, "Agriculture" },
                    { 12, "Real Estate" },
                    { 13, "Entertainment" },
                    { 14, "Telecommunications" },
                    { 15, "Food & Beverage" },
                    { 16, "Sports" },
                    { 17, "Consulting" },
                    { 18, "Government" },
                    { 19, "Non-Profit" },
                    { 20, "Other" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Companies_IndustryId",
                table: "Companies",
                column: "IndustryId");

            migrationBuilder.AddForeignKey(
                name: "FK_Companies_Industries_IndustryId",
                table: "Companies",
                column: "IndustryId",
                principalTable: "Industries",
                principalColumn: "IndustryId",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Companies_Industries_IndustryId",
                table: "Companies");

            migrationBuilder.DropTable(
                name: "Industries");

            migrationBuilder.DropIndex(
                name: "IX_Companies_IndustryId",
                table: "Companies");

            migrationBuilder.DropColumn(
                name: "IndustryId",
                table: "Companies");

            migrationBuilder.AddColumn<string>(
                name: "Industry",
                table: "Companies",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
