using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BeachBar.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class NullableOmbrelloneId : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessioni_Ombrelloni_OmbrelloneId",
                table: "Sessioni");

            migrationBuilder.AlterColumn<int>(
                name: "OmbrelloneId",
                table: "Sessioni",
                type: "integer",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AddForeignKey(
                name: "FK_Sessioni_Ombrelloni_OmbrelloneId",
                table: "Sessioni",
                column: "OmbrelloneId",
                principalTable: "Ombrelloni",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Sessioni_Ombrelloni_OmbrelloneId",
                table: "Sessioni");

            migrationBuilder.AlterColumn<int>(
                name: "OmbrelloneId",
                table: "Sessioni",
                type: "integer",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "integer",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Sessioni_Ombrelloni_OmbrelloneId",
                table: "Sessioni",
                column: "OmbrelloneId",
                principalTable: "Ombrelloni",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
