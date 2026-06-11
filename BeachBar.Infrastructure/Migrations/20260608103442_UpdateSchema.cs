using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeachBar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessioni_OmbrelloneId",
                table: "Sessioni");

            migrationBuilder.DropIndex(
                name: "IX_Consumazioni_SessioneId",
                table: "Consumazioni");

            migrationBuilder.CreateIndex(
                name: "IX_Sessioni_Chiusa_DataRiferimento",
                table: "Sessioni",
                columns: new[] { "Chiusa", "DataRiferimento" });

            migrationBuilder.CreateIndex(
                name: "IX_Sessioni_OmbrelloneId_Chiusa_Date",
                table: "Sessioni",
                columns: new[] { "OmbrelloneId", "Chiusa", "DataRiferimento", "DataFine" });

            migrationBuilder.CreateIndex(
                name: "IX_Consumazioni_Sessione_Prodotto_Giorno",
                table: "Consumazioni",
                columns: new[] { "SessioneId", "ProdottoId", "Giorno" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_Sessioni_Chiusa_DataRiferimento",
                table: "Sessioni");

            migrationBuilder.DropIndex(
                name: "IX_Sessioni_OmbrelloneId_Chiusa_Date",
                table: "Sessioni");

            migrationBuilder.DropIndex(
                name: "IX_Consumazioni_Sessione_Prodotto_Giorno",
                table: "Consumazioni");

            migrationBuilder.CreateIndex(
                name: "IX_Sessioni_OmbrelloneId",
                table: "Sessioni",
                column: "OmbrelloneId");

            migrationBuilder.CreateIndex(
                name: "IX_Consumazioni_SessioneId",
                table: "Consumazioni",
                column: "SessioneId");
        }
    }
}
