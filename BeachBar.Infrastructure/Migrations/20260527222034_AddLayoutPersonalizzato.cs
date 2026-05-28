using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeachBar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddLayoutPersonalizzato : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CellaIndice",
                table: "Ombrelloni",
                type: "integer",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BordiOrizzontali",
                table: "ImpostazioniSpiaggia",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "BordiVerticali",
                table: "ImpostazioniSpiaggia",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "NumeroRighe",
                table: "ImpostazioniSpiaggia",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "ImpostazioniSpiaggia",
                keyColumn: "Id",
                keyValue: 1,
                columns: new[] { "BordiOrizzontali", "BordiVerticali", "NumeroRighe" },
                values: new object[] { null, null, 5 });

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 1,
                column: "CellaIndice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 2,
                column: "CellaIndice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 3,
                column: "CellaIndice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 4,
                column: "CellaIndice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 5,
                column: "CellaIndice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 6,
                column: "CellaIndice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 7,
                column: "CellaIndice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 8,
                column: "CellaIndice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 9,
                column: "CellaIndice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 10,
                column: "CellaIndice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 11,
                column: "CellaIndice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 12,
                column: "CellaIndice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 13,
                column: "CellaIndice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 14,
                column: "CellaIndice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 15,
                column: "CellaIndice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 16,
                column: "CellaIndice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 17,
                column: "CellaIndice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 18,
                column: "CellaIndice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 19,
                column: "CellaIndice",
                value: null);

            migrationBuilder.UpdateData(
                table: "Ombrelloni",
                keyColumn: "Id",
                keyValue: 20,
                column: "CellaIndice",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CellaIndice",
                table: "Ombrelloni");

            migrationBuilder.DropColumn(
                name: "BordiOrizzontali",
                table: "ImpostazioniSpiaggia");

            migrationBuilder.DropColumn(
                name: "BordiVerticali",
                table: "ImpostazioniSpiaggia");

            migrationBuilder.DropColumn(
                name: "NumeroRighe",
                table: "ImpostazioniSpiaggia");
        }
    }
}
