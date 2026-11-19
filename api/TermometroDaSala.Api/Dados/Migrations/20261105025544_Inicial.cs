using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TermometroDaSala.Api.Dados.Migrations
{
    /// <inheritdoc />
    public partial class Inicial : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Ambientes",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(32)", maxLength: 32, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    Tipo = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Andar = table.Column<int>(type: "int", nullable: false),
                    FaceSol = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    TemVentilador = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ambientes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LeiturasRua",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    MedidoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    TemperaturaC = table.Column<double>(type: "float", nullable: false),
                    UmidadePct = table.Column<int>(type: "int", nullable: false),
                    IndiceCalor = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LeiturasRua", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "NiveisAcao",
                columns: table => new
                {
                    Nivel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Nome = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    AcaoSugerida = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    IndiceMin = table.Column<double>(type: "float", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_NiveisAcao", x => x.Nivel);
                });

            migrationBuilder.CreateTable(
                name: "Acoes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AmbienteId = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    Nivel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    RegistradaEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Acoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Acoes_Ambientes_AmbienteId",
                        column: x => x.AmbienteId,
                        principalTable: "Ambientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Dispositivos",
                columns: table => new
                {
                    Id = table.Column<string>(type: "nvarchar(64)", maxLength: 64, nullable: false),
                    AmbienteId = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    Segredo = table.Column<string>(type: "nvarchar(128)", maxLength: 128, nullable: false),
                    Ativo = table.Column<bool>(type: "bit", nullable: false),
                    UltimoMedidoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dispositivos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Dispositivos_Ambientes_AmbienteId",
                        column: x => x.AmbienteId,
                        principalTable: "Ambientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Ocorrencias",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    AmbienteId = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    Descricao = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    CaminhoFoto = table.Column<string>(type: "nvarchar(80)", maxLength: 80, nullable: false),
                    CriadaEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Ocorrencias", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Ocorrencias_Ambientes_AmbienteId",
                        column: x => x.AmbienteId,
                        principalTable: "Ambientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Previsoes",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AmbienteId = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    PrevistoPara = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    IndiceRuaPrevisto = table.Column<double>(type: "float", nullable: false),
                    DeltaSala = table.Column<double>(type: "float", nullable: true),
                    IndiceSalaPrevisto = table.Column<double>(type: "float", nullable: true),
                    NivelPrevisto = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    SemLeiturasRecentes = table.Column<bool>(type: "bit", nullable: false),
                    CalculadaEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Previsoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Previsoes_Ambientes_AmbienteId",
                        column: x => x.AmbienteId,
                        principalTable: "Ambientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Leituras",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    AmbienteId = table.Column<string>(type: "nvarchar(32)", nullable: false),
                    DispositivoId = table.Column<string>(type: "nvarchar(64)", nullable: false),
                    TemperaturaC = table.Column<double>(type: "float", nullable: false),
                    UmidadePct = table.Column<int>(type: "int", nullable: false),
                    IndiceCalor = table.Column<double>(type: "float", nullable: false),
                    Nivel = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    MedidoEm = table.Column<DateTimeOffset>(type: "datetimeoffset", nullable: false),
                    Origem = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Suspeita = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Leituras", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Leituras_Ambientes_AmbienteId",
                        column: x => x.AmbienteId,
                        principalTable: "Ambientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Leituras_Dispositivos_DispositivoId",
                        column: x => x.DispositivoId,
                        principalTable: "Dispositivos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.InsertData(
                table: "Ambientes",
                columns: new[] { "Id", "Andar", "FaceSol", "Nome", "TemVentilador", "Tipo" },
                values: new object[,]
                {
                    { "sala-01", 0, "leste", "Sala 1", true, "sala" },
                    { "sala-02", 0, "norte", "Sala 2", true, "sala" },
                    { "sala-03", 1, "oeste", "Sala 3", true, "sala" },
                    { "sala-04", 2, "sul", "Sala 4", false, "sala" }
                });

            migrationBuilder.InsertData(
                table: "NiveisAcao",
                columns: new[] { "Nivel", "AcaoSugerida", "IndiceMin", "Nome" },
                values: new object[,]
                {
                    { "atencao", "liberar água, abrir janelas e ligar ventiladores", 27.0, "Atenção" },
                    { "cuidado", "pausa, troca de sala ou adiar a Educação Física", 32.0, "Cuidado extremo" },
                    { "normal", "nenhuma", 0.0, "Normal" },
                    { "perigo", "tirar a turma da sala", 41.0, "Perigo" }
                });

            migrationBuilder.InsertData(
                table: "Dispositivos",
                columns: new[] { "Id", "AmbienteId", "Ativo", "Segredo", "UltimoMedidoEm" },
                values: new object[,]
                {
                    { "esp32-sala-01", "sala-01", true, "segredo-de-teste-sala-03", null },
                    { "esp32-sala-02", "sala-02", true, "segredo-de-teste-sala-03", null },
                    { "esp32-sala-03", "sala-03", true, "segredo-de-teste-sala-03", null },
                    { "esp32-sala-04", "sala-04", true, "segredo-de-teste-sala-03", null }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Acoes_AmbienteId",
                table: "Acoes",
                column: "AmbienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Dispositivos_AmbienteId",
                table: "Dispositivos",
                column: "AmbienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Leituras_AmbienteId_MedidoEm",
                table: "Leituras",
                columns: new[] { "AmbienteId", "MedidoEm" });

            migrationBuilder.CreateIndex(
                name: "IX_Leituras_DispositivoId",
                table: "Leituras",
                column: "DispositivoId");

            migrationBuilder.CreateIndex(
                name: "IX_LeiturasRua_MedidoEm",
                table: "LeiturasRua",
                column: "MedidoEm");

            migrationBuilder.CreateIndex(
                name: "IX_Ocorrencias_AmbienteId",
                table: "Ocorrencias",
                column: "AmbienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Previsoes_AmbienteId",
                table: "Previsoes",
                column: "AmbienteId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Acoes");

            migrationBuilder.DropTable(
                name: "Leituras");

            migrationBuilder.DropTable(
                name: "LeiturasRua");

            migrationBuilder.DropTable(
                name: "NiveisAcao");

            migrationBuilder.DropTable(
                name: "Ocorrencias");

            migrationBuilder.DropTable(
                name: "Previsoes");

            migrationBuilder.DropTable(
                name: "Dispositivos");

            migrationBuilder.DropTable(
                name: "Ambientes");
        }
    }
}
