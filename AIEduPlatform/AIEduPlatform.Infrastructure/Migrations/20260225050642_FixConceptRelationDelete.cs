using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEduPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class FixConceptRelationDelete : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConceptRelations_Concepts_FromConceptId",
                table: "ConceptRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_ConceptRelations_Concepts_ToConceptId",
                table: "ConceptRelations");

            migrationBuilder.AddForeignKey(
                name: "FK_ConceptRelations_Concepts_FromConceptId",
                table: "ConceptRelations",
                column: "FromConceptId",
                principalTable: "Concepts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_ConceptRelations_Concepts_ToConceptId",
                table: "ConceptRelations",
                column: "ToConceptId",
                principalTable: "Concepts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ConceptRelations_Concepts_FromConceptId",
                table: "ConceptRelations");

            migrationBuilder.DropForeignKey(
                name: "FK_ConceptRelations_Concepts_ToConceptId",
                table: "ConceptRelations");

            migrationBuilder.AddForeignKey(
                name: "FK_ConceptRelations_Concepts_FromConceptId",
                table: "ConceptRelations",
                column: "FromConceptId",
                principalTable: "Concepts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_ConceptRelations_Concepts_ToConceptId",
                table: "ConceptRelations",
                column: "ToConceptId",
                principalTable: "Concepts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
