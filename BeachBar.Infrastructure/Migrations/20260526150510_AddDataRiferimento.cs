using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeachBar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDataRiferimento : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateOnly>(
                name: "DataRiferimento",
                table: "Sessioni",
                type: "date",
                nullable: true);

            // Popola le sessioni storiche usando la data UTC di apertura.
            migrationBuilder.Sql("UPDATE \"Sessioni\" SET \"DataRiferimento\" = \"Apertura\"::date WHERE \"DataRiferimento\" IS NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DataRiferimento",
                table: "Sessioni");
        }
    }
}
