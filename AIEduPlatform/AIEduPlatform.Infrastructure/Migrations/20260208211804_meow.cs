using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEduPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class meow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "GradingCriteria",
                table: "Questions",
                type: "jsonb",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ModelAnswer",
                table: "Questions",
                type: "character varying(5000)",
                maxLength: 5000,
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "Questions",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "Indexed",
                table: "Materials",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Questions_ExamId_Order",
                table: "Questions",
                columns: new[] { "ExamId", "Order" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Questions_ExamId_Order",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "GradingCriteria",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "ModelAnswer",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Order",
                table: "Questions");

            migrationBuilder.DropColumn(
                name: "Indexed",
                table: "Materials");
        }
    }
}
