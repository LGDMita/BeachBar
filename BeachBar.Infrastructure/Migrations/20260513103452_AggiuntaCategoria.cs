using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace BeachBar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AggiuntaCategoria : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Categoria",
                table: "Prodotti",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.UpdateData(
                table: "Prodotti",
                keyColumn: "Id",
                keyValue: 1,
                column: "Categoria",
                value: "Bibite");

            migrationBuilder.UpdateData(
                table: "Prodotti",
                keyColumn: "Id",
                keyValue: 2,
                column: "Categoria",
                value: "Bibite");

            migrationBuilder.UpdateData(
                table: "Prodotti",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Categoria", "Nome", "Prezzo" },
                values: new object[] { "Bibite", "Succo di frutta", 2.50m });

            migrationBuilder.UpdateData(
                table: "Prodotti",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Categoria", "Nome", "Prezzo" },
                values: new object[] { "Alcolici", "Birra", 3.00m });

            migrationBuilder.UpdateData(
                table: "Prodotti",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Categoria", "Nome", "Prezzo" },
                values: new object[] { "Alcolici", "Spritz", 4.00m });

            migrationBuilder.UpdateData(
                table: "Prodotti",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Categoria", "Nome", "Prezzo" },
                values: new object[] { "Alcolici", "Vino bianco", 3.50m });

            migrationBuilder.InsertData(
                table: "Prodotti",
                columns: new[] { "Id", "Categoria", "Disponibile", "Nome", "Prezzo" },
                values: new object[,]
                {
                    { 7, "Caffetteria", true, "Caffè", 1.20m },
                    { 8, "Caffetteria", true, "Cappuccino", 1.50m },
                    { 9, "Cibo", true, "Tramezzino", 2.50m },
                    { 10, "Cibo", true, "Piadina", 4.50m },
                    { 11, "Cibo", true, "Gelato", 2.50m }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Prodotti",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Prodotti",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Prodotti",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Prodotti",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Prodotti",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DropColumn(
                name: "Categoria",
                table: "Prodotti");

            migrationBuilder.UpdateData(
                table: "Prodotti",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Nome", "Prezzo" },
                values: new object[] { "Birra", 3.00m });

            migrationBuilder.UpdateData(
                table: "Prodotti",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Nome", "Prezzo" },
                values: new object[] { "Spritz", 4.00m });

            migrationBuilder.UpdateData(
                table: "Prodotti",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Nome", "Prezzo" },
                values: new object[] { "Caffè", 1.20m });

            migrationBuilder.UpdateData(
                table: "Prodotti",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Nome", "Prezzo" },
                values: new object[] { "Gelato", 2.50m });
        }
    }
}
