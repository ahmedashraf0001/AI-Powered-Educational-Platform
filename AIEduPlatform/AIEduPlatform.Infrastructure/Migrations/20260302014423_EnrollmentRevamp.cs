using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEduPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class EnrollmentRevamp : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Courses_CourseId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Orders_OrderId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Courses_CourseId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Carts_CartId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId_CreatedAt",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Carts_UserId_Status",
                table: "Carts");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Orders",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "StripePaymentIntentId",
                table: "Orders",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "StripePaymentIntentClientSecret",
                table: "Orders",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Orders",
                type: "character varying(3)",
                maxLength: 3,
                nullable: false,
                defaultValue: "usd",
                oldClrType: typeof(string),
                oldType: "character varying(10)",
                oldMaxLength: 10);

            migrationBuilder.AlterColumn<Guid>(
                name: "CartId",
                table: "Orders",
                type: "uuid",
                nullable: true,
                oldClrType: typeof(Guid),
                oldType: "uuid");

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "OrderItems",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notifications",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255);

            migrationBuilder.AlterColumn<string>(
                name: "RelatedEntityType",
                table: "Notifications",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "character varying(2000)",
                maxLength: 2000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRead",
                table: "Notifications",
                type: "boolean",
                nullable: false,
                oldClrType: typeof(bool),
                oldType: "boolean",
                oldDefaultValue: false);

            migrationBuilder.AlterColumn<string>(
                name: "StripeRefundId",
                table: "Enrollments",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(255)",
                oldMaxLength: 255,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "RefundAmount",
                table: "Enrollments",
                type: "numeric",
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AmountPaid",
                table: "Enrollments",
                type: "numeric",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2,
                oldDefaultValue: 0m);

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceAtTimeOfAdding",
                table: "CartItems",
                type: "numeric(18,2)",
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(10,2)",
                oldPrecision: 10,
                oldScale: 2);

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId",
                table: "Orders",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead" });

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId",
                table: "Carts",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Courses_CourseId",
                table: "CartItems",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Orders_OrderId",
                table: "Enrollments",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Courses_CourseId",
                table: "OrderItems",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Carts_CartId",
                table: "Orders",
                column: "CartId",
                principalTable: "Carts",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CartItems_Courses_CourseId",
                table: "CartItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Enrollments_Orders_OrderId",
                table: "Enrollments");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderItems_Courses_CourseId",
                table: "OrderItems");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Carts_CartId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_UserId",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Notifications_UserId_IsRead",
                table: "Notifications");

            migrationBuilder.DropIndex(
                name: "IX_Carts_UserId",
                table: "Carts");

            migrationBuilder.AlterColumn<decimal>(
                name: "TotalAmount",
                table: "Orders",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "StripePaymentIntentId",
                table: "Orders",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "StripePaymentIntentClientSecret",
                table: "Orders",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Currency",
                table: "Orders",
                type: "character varying(10)",
                maxLength: 10,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(3)",
                oldMaxLength: 3,
                oldDefaultValue: "usd");

            migrationBuilder.AlterColumn<Guid>(
                name: "CartId",
                table: "Orders",
                type: "uuid",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"),
                oldClrType: typeof(Guid),
                oldType: "uuid",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "Price",
                table: "OrderItems",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.AlterColumn<string>(
                name: "Title",
                table: "Notifications",
                type: "character varying(255)",
                maxLength: 255,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(200)",
                oldMaxLength: 200);

            migrationBuilder.AlterColumn<string>(
                name: "RelatedEntityType",
                table: "Notifications",
                type: "character varying(50)",
                maxLength: 50,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "Message",
                table: "Notifications",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(2000)",
                oldMaxLength: 2000);

            migrationBuilder.AlterColumn<bool>(
                name: "IsRead",
                table: "Notifications",
                type: "boolean",
                nullable: false,
                defaultValue: false,
                oldClrType: typeof(bool),
                oldType: "boolean");

            migrationBuilder.AlterColumn<string>(
                name: "StripeRefundId",
                table: "Enrollments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "RefundAmount",
                table: "Enrollments",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: true,
                oldClrType: typeof(decimal),
                oldType: "numeric",
                oldNullable: true);

            migrationBuilder.AlterColumn<decimal>(
                name: "AmountPaid",
                table: "Enrollments",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                defaultValue: 0m,
                oldClrType: typeof(decimal),
                oldType: "numeric");

            migrationBuilder.AlterColumn<decimal>(
                name: "PriceAtTimeOfAdding",
                table: "CartItems",
                type: "numeric(10,2)",
                precision: 10,
                scale: 2,
                nullable: false,
                oldClrType: typeof(decimal),
                oldType: "numeric(18,2)");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_UserId_CreatedAt",
                table: "Orders",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "CreatedAt" },
                descending: new[] { false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId_IsRead_CreatedAt",
                table: "Notifications",
                columns: new[] { "UserId", "IsRead", "CreatedAt" },
                descending: new[] { false, false, true });

            migrationBuilder.CreateIndex(
                name: "IX_Carts_UserId_Status",
                table: "Carts",
                columns: new[] { "UserId", "Status" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_CartItems_Courses_CourseId",
                table: "CartItems",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Enrollments_Orders_OrderId",
                table: "Enrollments",
                column: "OrderId",
                principalTable: "Orders",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderItems_Courses_CourseId",
                table: "OrderItems",
                column: "CourseId",
                principalTable: "Courses",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Carts_CartId",
                table: "Orders",
                column: "CartId",
                principalTable: "Carts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
