using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TermometroDaSala.Api.Dados.Migrations
{
    /// <inheritdoc />
    public partial class SegredosPorDispositivo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Dispositivos",
                keyColumn: "Id",
                keyValue: "esp32-sala-01",
                column: "Segredo",
                value: "segredo-de-teste-sala-01");

            migrationBuilder.UpdateData(
                table: "Dispositivos",
                keyColumn: "Id",
                keyValue: "esp32-sala-02",
                column: "Segredo",
                value: "segredo-de-teste-sala-02");

            migrationBuilder.UpdateData(
                table: "Dispositivos",
                keyColumn: "Id",
                keyValue: "esp32-sala-04",
                column: "Segredo",
                value: "segredo-de-teste-sala-04");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Dispositivos",
                keyColumn: "Id",
                keyValue: "esp32-sala-01",
                column: "Segredo",
                value: "segredo-de-teste-sala-03");

            migrationBuilder.UpdateData(
                table: "Dispositivos",
                keyColumn: "Id",
                keyValue: "esp32-sala-02",
                column: "Segredo",
                value: "segredo-de-teste-sala-03");

            migrationBuilder.UpdateData(
                table: "Dispositivos",
                keyColumn: "Id",
                keyValue: "esp32-sala-04",
                column: "Segredo",
                value: "segredo-de-teste-sala-03");
        }
    }
}
