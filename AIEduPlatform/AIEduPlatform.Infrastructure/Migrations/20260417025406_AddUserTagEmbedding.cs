using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace AIEduPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserTagEmbedding : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Vector>(
                name: "TagEmbedding",
                table: "AspNetUsers",
                type: "vector(384)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "TagEmbedding",
                table: "AspNetUsers");
        }
    }
}
