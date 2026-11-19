using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TermometroDaSala.Api.Dados.Migrations
{
    /// <inheritdoc />
    public partial class MapaTermico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<double>(
                name: "PosX",
                table: "Dispositivos",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "PosY",
                table: "Dispositivos",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Rotulo",
                table: "Dispositivos",
                type: "nvarchar(80)",
                maxLength: 80,
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "LarguraM",
                table: "Ambientes",
                type: "float",
                nullable: true);

            migrationBuilder.AddColumn<double>(
                name: "ProfundidadeM",
                table: "Ambientes",
                type: "float",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "Lugares",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AmbienteId = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    X = table.Column<double>(type: "float", nullable: false),
                    Y = table.Column<double>(type: "float", nullable: false),
                    Vulneravel = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Lugares", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Lugares_Ambientes_AmbienteId",
                        column: x => x.AmbienteId,
                        principalTable: "Ambientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Lugares_AmbienteId_Nome",
                table: "Lugares",
                columns: new[] { "AmbienteId", "Nome" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Lugares");

            migrationBuilder.DropColumn(
                name: "PosX",
                table: "Dispositivos");

            migrationBuilder.DropColumn(
                name: "PosY",
                table: "Dispositivos");

            migrationBuilder.DropColumn(
                name: "Rotulo",
                table: "Dispositivos");

            migrationBuilder.DropColumn(
                name: "LarguraM",
                table: "Ambientes");

            migrationBuilder.DropColumn(
                name: "ProfundidadeM",
                table: "Ambientes");
        }
    }
}
