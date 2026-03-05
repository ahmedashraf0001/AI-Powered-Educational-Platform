using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEduPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NewFeatures : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "DurationSeconds",
                table: "Materials",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "TotalPages",
                table: "Materials",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "CurrentEnrollmentCount",
                table: "Courses",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<decimal>(
                name: "Price",
                table: "Courses",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "Bio",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationToken",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EmailVerificationTokenExpiry",
                table: "AspNetUsers",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "GradeLevel",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Interests",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsEmailVerified",
                table: "AspNetUsers",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Qualifications",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Subjects",
                table: "AspNetUsers",
                type: "text",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Description = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MaterialProgresses",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    LastPosition = table.Column<int>(type: "integer", nullable: false),
                    IsCompleted = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MaterialProgresses", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MaterialProgresses_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MaterialProgresses_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Payments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    StudentId = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    StripePaymentIntentId = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    StripeClientSecret = table.Column<string>(type: "text", nullable: true),
                    Amount = table.Column<decimal>(type: "numeric(10,2)", precision: 10, scale: 2, nullable: false),
                    Currency = table.Column<string>(type: "character varying(3)", maxLength: 3, nullable: false),
                    Status = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    PaidAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Payments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Payments_AspNetUsers_StudentId",
                        column: x => x.StudentId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Payments_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SemanticSections",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    MaterialId = table.Column<Guid>(type: "uuid", nullable: false),
                    Title = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: false),
                    Summary = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                    StartSeconds = table.Column<int>(type: "integer", nullable: true),
                    EndSeconds = table.Column<int>(type: "integer", nullable: true),
                    StartPage = table.Column<int>(type: "integer", nullable: true),
                    EndPage = table.Column<int>(type: "integer", nullable: true),
                    OrderIndex = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SemanticSections", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SemanticSections_Materials_MaterialId",
                        column: x => x.MaterialId,
                        principalTable: "Materials",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CourseCategories",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CourseId = table.Column<Guid>(type: "uuid", nullable: false),
                    CategoryId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CourseCategories", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CourseCategories_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CourseCategories_Courses_CourseId",
                        column: x => x.CourseId,
                        principalTable: "Courses",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Categories_Name",
                table: "Categories",
                column: "Name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CourseCategories_CategoryId",
                table: "CourseCategories",
                column: "CategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_CourseCategories_CourseId_CategoryId",
                table: "CourseCategories",
                columns: new[] { "CourseId", "CategoryId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MaterialProgresses_MaterialId",
                table: "MaterialProgresses",
                column: "MaterialId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialProgresses_StudentId",
                table: "MaterialProgresses",
                column: "StudentId");

            migrationBuilder.CreateIndex(
                name: "IX_MaterialProgresses_StudentId_MaterialId",
                table: "MaterialProgresses",
                columns: new[] { "StudentId", "MaterialId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_CourseId",
                table: "Payments",
                column: "CourseId");

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StripePaymentIntentId",
                table: "Payments",
                column: "StripePaymentIntentId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Payments_StudentId_CourseId",
                table: "Payments",
                columns: new[] { "StudentId", "CourseId" });

            migrationBuilder.CreateIndex(
                name: "IX_SemanticSections_MaterialId",
                table: "SemanticSections",
                column: "MaterialId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CourseCategories");

            migrationBuilder.DropTable(
                name: "MaterialProgresses");

            migrationBuilder.DropTable(
                name: "Payments");

            migrationBuilder.DropTable(
                name: "SemanticSections");

            migrationBuilder.DropTable(
                name: "Categories");

            migrationBuilder.DropColumn(
                name: "DurationSeconds",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "TotalPages",
                table: "Materials");

            migrationBuilder.DropColumn(
                name: "CurrentEnrollmentCount",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Price",
                table: "Courses");

            migrationBuilder.DropColumn(
                name: "Bio",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailVerificationToken",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "EmailVerificationTokenExpiry",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "GradeLevel",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Interests",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "IsEmailVerified",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Qualifications",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Subjects",
                table: "AspNetUsers");
        }
    }
}
