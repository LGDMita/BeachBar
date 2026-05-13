using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeachBar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AggiungiUltimoReset : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "UltimoResetStatistiche",
                table: "ImpostazioniSpiaggia",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "ImpostazioniSpiaggia",
                keyColumn: "Id",
                keyValue: 1,
                column: "UltimoResetStatistiche",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "UltimoResetStatistiche",
                table: "ImpostazioniSpiaggia");
        }
    }
}
