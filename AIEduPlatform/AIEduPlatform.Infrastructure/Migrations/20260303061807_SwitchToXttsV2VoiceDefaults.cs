using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AIEduPlatform.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SwitchToXttsV2VoiceDefaults : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TeacherVoiceId",
                table: "UserVoiceSettings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Damien Black",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "p267");

            migrationBuilder.AlterColumn<string>(
                name: "StudentVoiceId",
                table: "UserVoiceSettings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "Daisy Studious",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "p230");

            migrationBuilder.AlterColumn<int>(
                name: "SampleRate",
                table: "UserVoiceSettings",
                type: "integer",
                nullable: false,
                defaultValue: 24000,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 48000);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "TeacherVoiceId",
                table: "UserVoiceSettings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "p267",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Damien Black");

            migrationBuilder.AlterColumn<string>(
                name: "StudentVoiceId",
                table: "UserVoiceSettings",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                defaultValue: "p230",
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50,
                oldDefaultValue: "Daisy Studious");

            migrationBuilder.AlterColumn<int>(
                name: "SampleRate",
                table: "UserVoiceSettings",
                type: "integer",
                nullable: false,
                defaultValue: 48000,
                oldClrType: typeof(int),
                oldType: "integer",
                oldDefaultValue: 24000);
        }
    }
}
