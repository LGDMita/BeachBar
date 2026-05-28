using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BeachBar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ImpostazioniSpiaggia",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    NumeroOmbrelloni = table.Column<int>(type: "integer", nullable: false),
                    NumeroColonne = table.Column<int>(type: "integer", nullable: false),
                    NumeroRighe = table.Column<int>(type: "integer", nullable: false),
                    BordiVerticali = table.Column<string>(type: "text", nullable: true),
                    BordiOrizzontali = table.Column<string>(type: "text", nullable: true),
                    UltimoResetStatistiche = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImpostazioniSpiaggia", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Ombrelloni",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Occupato = table.Column<bool>(type: "boolean", nullable: false),
                    CellaIndice = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ombrelloni", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Prodotti",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Prezzo = table.Column<decimal>(type: "numeric", nullable: false),
                    Disponibile = table.Column<bool>(type: "boolean", nullable: false),
                    Categoria = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Prodotti", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Sessioni",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    OmbrelloneId = table.Column<int>(type: "integer", nullable: true),
                    NomeCliente = table.Column<string>(type: "text", nullable: true),
                    Apertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Chiusura = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Chiusa = table.Column<bool>(type: "boolean", nullable: false),
                    DataRiferimento = table.Column<DateOnly>(type: "date", nullable: true),
                    DataFine = table.Column<DateOnly>(type: "date", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessioni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessioni_Ombrelloni_OmbrelloneId",
                        column: x => x.OmbrelloneId,
                        principalTable: "Ombrelloni",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Consumazioni",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    SessioneId = table.Column<int>(type: "integer", nullable: false),
                    ProdottoId = table.Column<int>(type: "integer", nullable: false),
                    Quantita = table.Column<int>(type: "integer", nullable: false),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Giorno = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Consumazioni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Consumazioni_Prodotti_ProdottoId",
                        column: x => x.ProdottoId,
                        principalTable: "Prodotti",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Consumazioni_Sessioni_SessioneId",
                        column: x => x.SessioneId,
                        principalTable: "Sessioni",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ImpostazioniSpiaggia",
                columns: new[] { "Id", "BordiOrizzontali", "BordiVerticali", "NumeroColonne", "NumeroOmbrelloni", "NumeroRighe", "UltimoResetStatistiche" },
                values: new object[] { 1, null, "9,15", 17, 77, 5, null });

            migrationBuilder.InsertData(
                table: "Ombrelloni",
                columns: new[] { "Id", "CellaIndice", "Numero", "Occupato" },
                values: new object[,]
                {
                    { 1, 69, 1, false },
                    { 2, 70, 2, false },
                    { 3, 71, 3, false },
                    { 4, 72, 4, false },
                    { 5, 73, 5, false },
                    { 6, 74, 6, false },
                    { 7, 75, 7, false },
                    { 8, 76, 8, false },
                    { 9, 77, 9, false },
                    { 10, 78, 10, false },
                    { 11, 79, 11, false },
                    { 12, 80, 12, false },
                    { 13, 81, 13, false },
                    { 14, 82, 14, false },
                    { 15, 83, 15, false },
                    { 16, 51, 16, false },
                    { 17, 52, 17, false },
                    { 18, 53, 18, false },
                    { 19, 54, 19, false },
                    { 20, 55, 20, false },
                    { 21, 56, 21, false },
                    { 22, 57, 22, false },
                    { 23, 58, 23, false },
                    { 24, 59, 24, false },
                    { 25, 60, 25, false },
                    { 26, 61, 26, false },
                    { 27, 62, 27, false },
                    { 28, 63, 28, false },
                    { 29, 64, 29, false },
                    { 30, 65, 30, false },
                    { 31, 66, 31, false },
                    { 32, 67, 32, false },
                    { 33, 34, 33, false },
                    { 34, 35, 34, false },
                    { 35, 36, 35, false },
                    { 36, 37, 36, false },
                    { 37, 38, 37, false },
                    { 38, 39, 38, false },
                    { 39, 40, 39, false },
                    { 40, 41, 40, false },
                    { 41, 42, 41, false },
                    { 42, 43, 42, false },
                    { 43, 44, 43, false },
                    { 44, 45, 44, false },
                    { 45, 46, 45, false },
                    { 46, 47, 46, false },
                    { 47, 48, 47, false },
                    { 48, 49, 48, false },
                    { 49, 50, 49, false },
                    { 50, 17, 50, false },
                    { 51, 18, 51, false },
                    { 52, 19, 52, false },
                    { 53, 20, 53, false },
                    { 54, 21, 54, false },
                    { 55, 22, 55, false },
                    { 56, 23, 56, false },
                    { 57, 24, 57, false },
                    { 58, 25, 58, false },
                    { 59, 26, 59, false },
                    { 60, 27, 60, false },
                    { 61, 28, 61, false },
                    { 62, 29, 62, false },
                    { 63, 30, 63, false },
                    { 64, 31, 64, false },
                    { 65, 32, 65, false },
                    { 66, 33, 66, false },
                    { 67, 0, 67, false },
                    { 68, 1, 68, false },
                    { 69, 2, 69, false },
                    { 70, 3, 70, false },
                    { 71, 4, 71, false },
                    { 72, 11, 72, false },
                    { 73, 12, 73, false },
                    { 74, 13, 74, false },
                    { 75, 14, 75, false },
                    { 76, 15, 76, false },
                    { 77, 16, 77, false },
                    { 78, null, 78, false }
                });

            migrationBuilder.InsertData(
                table: "Prodotti",
                columns: new[] { "Id", "Categoria", "Disponibile", "Nome", "Prezzo" },
                values: new object[,]
                {
                    { 1, "Panini", true, "Tempesta", 7.00m },
                    { 2, "Panini", true, "Scirocco", 7.00m },
                    { 3, "Panini", true, "Maestrale", 7.00m },
                    { 4, "Panini", true, "Ponente", 7.00m },
                    { 5, "Panini", true, "Hot Dog", 5.00m },
                    { 6, "Panini", true, "Maxi Toast", 6.00m },
                    { 7, "Panini", true, "Maxi Toast Grecale", 7.00m },
                    { 8, "Panini", true, "Hamburger", 9.00m },
                    { 9, "Panini", true, "Cheeseburger", 10.00m },
                    { 10, "Insalate", true, "Insalata", 12.00m },
                    { 11, "Insalate", true, "Condiglione", 12.00m },
                    { 12, "Insalate", true, "Nizzarda", 12.00m },
                    { 13, "Insalate", true, "Ultima Spiaggia", 14.00m },
                    { 14, "Pizza", true, "Pizzella Margherita", 6.00m },
                    { 15, "Pizza", true, "Pizzella Farcita", 8.00m },
                    { 16, "Piatti", true, "Caprese", 10.00m },
                    { 17, "Piatti", true, "Crudo e Melone", 12.00m },
                    { 18, "Piatti", true, "Bresaola Rucola Grana Pomodoro", 15.00m },
                    { 19, "Piatti", true, "Crudo Bufala Rucola", 15.00m },
                    { 20, "Piatti", true, "Patatine Fritte", 5.00m },
                    { 21, "Piatti", true, "Pan Fritto", 5.00m },
                    { 22, "Piatti", true, "Nuggets e Patatine", 10.00m },
                    { 23, "Piatti", true, "Pan Fritto con Crudo", 10.00m },
                    { 24, "Piatti", true, "Wurstel e Patatine", 10.00m },
                    { 25, "Piatti", true, "Cotoletta e Patatine", 10.00m },
                    { 26, "Piatti", true, "Torta di Verdure", 5.00m },
                    { 27, "Piatti", true, "Torta di Verdure con Contorno", 10.00m },
                    { 28, "Piatti", true, "Focaccia al Formaggio", 12.00m },
                    { 29, "Piatti", true, "Focaccia al Formaggio con Pomodoro", 14.00m },
                    { 30, "Pasta", true, "Penne", 8.00m },
                    { 31, "Pasta", true, "Gnocchi", 8.00m },
                    { 32, "Pasta", true, "Trofie", 8.00m },
                    { 33, "Pasta", true, "Ravioli", 8.00m },
                    { 34, "Pasta", true, "Lasagne", 10.00m },
                    { 35, "Frutta", true, "Anguria", 4.00m },
                    { 36, "Frutta", true, "Melone", 4.00m },
                    { 37, "Frutta", true, "Macedonia", 5.00m },
                    { 38, "Frutta", true, "Macedonia con Yogurt/Gelato", 7.00m },
                    { 39, "Caffetteria", true, "Caffè", 1.30m },
                    { 40, "Caffetteria", true, "Caffè Americano", 1.50m },
                    { 41, "Caffetteria", true, "Caffè Decaffeinato", 1.50m },
                    { 42, "Caffetteria", true, "Orzo/Ginseng", 1.50m },
                    { 43, "Caffetteria", true, "Orzo/Ginseng Grande", 2.00m },
                    { 44, "Caffetteria", true, "Caffè Corretto", 1.80m },
                    { 45, "Caffetteria", true, "Caffè Shakerato", 2.50m },
                    { 46, "Caffetteria", true, "Caffè Shakerato Special", 4.00m },
                    { 47, "Caffetteria", true, "Cappuccino", 2.00m },
                    { 48, "Caffetteria", true, "Latte", 1.50m },
                    { 49, "Caffetteria", true, "Latte Macchiato", 2.50m },
                    { 50, "Caffetteria", true, "Brioches", 1.50m },
                    { 51, "Caffetteria", true, "Focaccia", 2.00m },
                    { 52, "Caffetteria", true, "Pizza", 2.50m },
                    { 53, "Caffetteria", true, "Crema Caffè", 3.00m },
                    { 54, "Caffetteria", true, "Yogurt", 3.00m },
                    { 55, "Bibite", true, "Acqua 0.5L", 1.50m },
                    { 56, "Bibite", true, "Acqua 1L", 2.50m },
                    { 57, "Bibite", true, "Estathé Brick", 2.00m },
                    { 58, "Bibite", true, "Succhi di Frutta", 3.00m },
                    { 59, "Bibite", true, "Tonica Schweppes", 3.00m },
                    { 60, "Bibite", true, "Lemonsoda", 3.00m },
                    { 61, "Bibite", true, "Chinotto di Lurisia", 4.00m },
                    { 62, "Bibite", true, "Limonata del Tigullio", 4.00m },
                    { 63, "Bibite", true, "Aloe Vera", 5.00m },
                    { 64, "Bibite", true, "Lattine", 3.00m },
                    { 65, "Bibite", true, "Spremuta di Arancia", 4.00m },
                    { 66, "Bibite", true, "Smoothie di Frutta", 6.00m },
                    { 67, "Birre", true, "Birra Raffo Piccola", 3.50m },
                    { 68, "Birre", true, "Birra Raffo Media", 6.00m },
                    { 69, "Birre", true, "Birra Raffo Maxi", 7.00m },
                    { 70, "Birre", true, "Birra Raffo Ultra", 12.00m },
                    { 71, "Birre", true, "Heineken", 4.50m },
                    { 72, "Birre", true, "Beck's", 4.50m },
                    { 73, "Birre", true, "Raffo Grezza", 4.50m },
                    { 74, "Birre", true, "Nastro Azzurro Capri", 4.50m },
                    { 75, "Birre", true, "Ceres", 5.00m },
                    { 76, "Birre", true, "Nastro Azzurro 0", 4.50m },
                    { 77, "Vini", true, "Calice Pigato/Vermentino", 5.00m },
                    { 78, "Vini", true, "Calice Prosecco", 6.00m },
                    { 79, "Vini", true, "Pigato Le Creuze", 22.00m },
                    { 80, "Vini", true, "Vermentino Le Creuze", 22.00m },
                    { 81, "Vini", true, "Prosecco Marsuret", 25.00m },
                    { 82, "Vini", true, "Franciacorta Ca del Bosco", 55.00m },
                    { 83, "Aperitivi", true, "Crodino", 3.50m },
                    { 84, "Aperitivi", true, "Sanbitter", 3.50m },
                    { 85, "Aperitivi", true, "Campari Soda", 4.00m },
                    { 86, "Aperitivi", true, "Aperol Spritz", 7.00m },
                    { 87, "Aperitivi", true, "Campari Spritz", 8.00m },
                    { 88, "Aperitivi", true, "Hugo", 8.00m },
                    { 89, "Aperitivi", true, "Sprutz", 8.00m },
                    { 90, "Aperitivi", true, "Negroni", 8.00m },
                    { 91, "Aperitivi", true, "Americano", 8.00m },
                    { 92, "Aperitivi", true, "Negroni Sbagliato", 8.00m },
                    { 93, "Aperitivi", true, "Mojito", 8.00m },
                    { 94, "Aperitivi", true, "Gin Tonic", 8.00m },
                    { 95, "Aperitivi", true, "Gin Lemon", 8.00m },
                    { 96, "Aperitivi", true, "Vodka Tonic", 8.00m },
                    { 97, "Aperitivi", true, "Vodka Lemon", 8.00m },
                    { 98, "Aperitivi", true, "Campari Shakerato", 8.00m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Consumazioni_ProdottoId",
                table: "Consumazioni",
                column: "ProdottoId");

            migrationBuilder.CreateIndex(
                name: "IX_Consumazioni_SessioneId",
                table: "Consumazioni",
                column: "SessioneId");

            migrationBuilder.CreateIndex(
                name: "IX_Sessioni_OmbrelloneId",
                table: "Sessioni",
                column: "OmbrelloneId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Consumazioni");

            migrationBuilder.DropTable(
                name: "ImpostazioniSpiaggia");

            migrationBuilder.DropTable(
                name: "Prodotti");

            migrationBuilder.DropTable(
                name: "Sessioni");

            migrationBuilder.DropTable(
                name: "Ombrelloni");
        }
    }
}
