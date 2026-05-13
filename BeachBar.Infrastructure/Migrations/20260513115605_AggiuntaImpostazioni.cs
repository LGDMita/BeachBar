using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace BeachBar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AggiuntaImpostazioni : Migration
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
                    NumeroColonne = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ImpostazioniSpiaggia", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "ImpostazioniSpiaggia",
                columns: new[] { "Id", "NumeroColonne", "NumeroOmbrelloni" },
                values: new object[] { 1, 4, 20 });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ImpostazioniSpiaggia");
        }
    }
}
