using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fitnutri.Migrations
{
    /// <inheritdoc />
    public partial class AddPerfilTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "PerfilId",
                table: "Users",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Perfis",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Tipo = table.Column<int>(type: "int", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Perfis", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_PerfilId",
                table: "Users",
                column: "PerfilId");

            migrationBuilder.AddForeignKey(
                name: "FK_Users_Perfis_PerfilId",
                table: "Users",
                column: "PerfilId",
                principalTable: "Perfis",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Perfis_PerfilId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "Perfis");

            migrationBuilder.DropIndex(
                name: "IX_Users_PerfilId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "PerfilId",
                table: "Users");
        }
    }
}
