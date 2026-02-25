using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Pgvector;

#nullable disable

namespace AIEduPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addedConcepts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Concepts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    NormalizedName = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Summary = table.Column<string>(type: "text", nullable: false),
                    Embedding = table.Column<Vector>(type: "vector(1536)", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Concepts", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Concepts_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Concepts_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConceptChunkMaps",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    ConceptId = table.Column<Guid>(type: "uuid", nullable: false),
                    ChunkId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptChunkMaps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConceptChunkMaps_Chunks_ChunkId",
                        column: x => x.ChunkId,
                        principalTable: "Chunks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ConceptChunkMaps_Concepts_ConceptId",
                        column: x => x.ConceptId,
                        principalTable: "Concepts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "ConceptRelations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    FromConceptId = table.Column<Guid>(type: "uuid", nullable: false),
                    ToConceptId = table.Column<Guid>(type: "uuid", nullable: false),
                    RelationType = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConceptRelations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConceptRelations_Concepts_FromConceptId",
                        column: x => x.FromConceptId,
                        principalTable: "Concepts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ConceptRelations_Concepts_ToConceptId",
                        column: x => x.ToConceptId,
                        principalTable: "Concepts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ConceptChunkMaps_ChunkId",
                table: "ConceptChunkMaps",
                column: "ChunkId");

            migrationBuilder.CreateIndex(
                name: "IX_ConceptChunkMaps_ConceptId",
                table: "ConceptChunkMaps",
                column: "ConceptId");

            migrationBuilder.CreateIndex(
                name: "IX_ConceptChunkMaps_ConceptId_ChunkId",
                table: "ConceptChunkMaps",
                columns: new[] { "ConceptId", "ChunkId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ConceptRelations_FromConceptId",
                table: "ConceptRelations",
                column: "FromConceptId");

            migrationBuilder.CreateIndex(
                name: "IX_ConceptRelations_ToConceptId",
                table: "ConceptRelations",
                column: "ToConceptId");

            migrationBuilder.CreateIndex(
                name: "IX_Concepts_CourseId",
                table: "Concepts",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Concepts_CourseId_NormalizedName",
                table: "Concepts",
                columns: new[] { "CourseId", "NormalizedName" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Concepts_MaterialId",
                table: "Concepts",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_Concepts_NormalizedName",
                table: "Concepts",
                column: "NormalizedName");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConceptChunkMaps");

            migrationBuilder.DropTable(
                name: "ConceptRelations");

            migrationBuilder.DropTable(
                name: "Concepts");
        }
    }
}
