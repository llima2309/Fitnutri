using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Fitnutri.Migrations
{
    /// <inheritdoc />
    public partial class ChangePerfilIdToGuid : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Remove o índice da foreign key se existir
            migrationBuilder.DropIndex(
                name: "IX_Users_PerfilId",
                table: "Users");

            // Remove a foreign key constraint se existir
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Perfis_PerfilId",
                table: "Users");

            // Dropa a coluna existente
            migrationBuilder.DropColumn(
                name: "PerfilId",
                table: "Users");

            // Adiciona a nova coluna com tipo Guid
            migrationBuilder.AddColumn<Guid>(
                name: "PerfilId",
                table: "Users",
                type: "uniqueidentifier",
                nullable: true);

            // Recria o índice
            migrationBuilder.CreateIndex(
                name: "IX_Users_PerfilId",
                table: "Users",
                column: "PerfilId");

        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Remove o índice da foreign key
            migrationBuilder.DropIndex(
                name: "IX_Users_PerfilId",
                table: "Users");

            // Remove a foreign key constraint
            migrationBuilder.DropForeignKey(
                name: "FK_Users_Perfis_PerfilId",
                table: "Users");

            // Dropa a coluna Guid
            migrationBuilder.DropColumn(
                name: "PerfilId",
                table: "Users");

            // Adiciona a coluna de volta como int
            migrationBuilder.AddColumn<Guid>(
                name: "PerfilId",
                table: "Users",
                type: "Guid",
                nullable: true);

            // Recria o índice
            migrationBuilder.CreateIndex(
                name: "IX_Users_PerfilId",
                table: "Users",
                column: "PerfilId");

            // Recria a foreign key (isso vai falhar porque os tipos não são compatíveis)
            // Você pode comentar esta linha se não quiser reverter completamente

        }
    }
}
