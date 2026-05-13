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
                name: "Ombrelloni",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Numero = table.Column<int>(type: "integer", nullable: false),
                    Occupato = table.Column<bool>(type: "boolean", nullable: false)
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
                    Disponibile = table.Column<bool>(type: "boolean", nullable: false)
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
                    OmbrelloneId = table.Column<int>(type: "integer", nullable: false),
                    NomeCliente = table.Column<string>(type: "text", nullable: true),
                    Apertura = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Chiusura = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Chiusa = table.Column<bool>(type: "boolean", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Sessioni", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Sessioni_Ombrelloni_OmbrelloneId",
                        column: x => x.OmbrelloneId,
                        principalTable: "Ombrelloni",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
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
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
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
                table: "Ombrelloni",
                columns: new[] { "Id", "Numero", "Occupato" },
                values: new object[,]
                {
                    { 1, 1, false },
                    { 2, 2, false },
                    { 3, 3, false },
                    { 4, 4, false },
                    { 5, 5, false },
                    { 6, 6, false },
                    { 7, 7, false },
                    { 8, 8, false },
                    { 9, 9, false },
                    { 10, 10, false },
                    { 11, 11, false },
                    { 12, 12, false },
                    { 13, 13, false },
                    { 14, 14, false },
                    { 15, 15, false },
                    { 16, 16, false },
                    { 17, 17, false },
                    { 18, 18, false },
                    { 19, 19, false },
                    { 20, 20, false }
                });

            migrationBuilder.InsertData(
                table: "Prodotti",
                columns: new[] { "Id", "Disponibile", "Nome", "Prezzo" },
                values: new object[,]
                {
                    { 1, true, "Acqua", 1.50m },
                    { 2, true, "Coca Cola", 2.50m },
                    { 3, true, "Birra", 3.00m },
                    { 4, true, "Spritz", 4.00m },
                    { 5, true, "Caffè", 1.20m },
                    { 6, true, "Gelato", 2.50m }
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
                name: "Prodotti");

            migrationBuilder.DropTable(
                name: "Sessioni");

            migrationBuilder.DropTable(
                name: "Ombrelloni");
        }
    }
}
