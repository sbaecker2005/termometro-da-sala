using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TermometroDaSala.Api.Dados.Migrations
{
    /// <inheritdoc />
    public partial class AlertaFrio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "NiveisAcao",
                keyColumn: "Nivel",
                keyValue: "normal",
                column: "IndiceMin",
                value: 18.0);

            migrationBuilder.InsertData(
                table: "NiveisAcao",
                columns: new[] { "Nivel", "AcaoSugerida", "IndiceMin", "Nome" },
                values: new object[] { "frio", "Fechar janelas, agasalhar e oferecer cobertor; à noite, avisar o plantão e levar quem estiver no cômodo mais frio para outro", -273.14999999999998, "Frio" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "NiveisAcao",
                keyColumn: "Nivel",
                keyValue: "frio");

            migrationBuilder.UpdateData(
                table: "NiveisAcao",
                keyColumn: "Nivel",
                keyValue: "normal",
                column: "IndiceMin",
                value: 0.0);
        }
    }
}
