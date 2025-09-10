using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fitnutri.Migrations
{
    /// <inheritdoc />
    public partial class AddEmailVerificationCode : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationToken",
                table: "Users");

            migrationBuilder.AddColumn<int>(
                name: "EmailVerificationCode",
                table: "Users",
                type: "int",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "EmailVerificationCode",
                table: "Users");

            migrationBuilder.AddColumn<string>(
                name: "EmailVerificationToken",
                table: "Users",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: true);
        }
    }
}
