using System.Text.RegularExpressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;
using TermometroDaSala.Api.Dados;
using TermometroDaSala.Api.Dominio.Entidades;

namespace TermometroDaSala.Api.Tests;

// O SQL Server recusa uma tabela que recebe exclusão em cascata por dois caminhos (erro 1785).
// Os testes da API usam SQLite, que aceita esse esquema, então a regra é conferida aqui à parte.
public class EsquemaTestes
{
    // Nenhuma conexão é aberta: o provedor só monta o modelo e o SQL.
    static SalaDb NovoBanco() => new(new DbContextOptionsBuilder<SalaDb>()
        .UseSqlServer("Server=nao-conecta;Database=Esquema;Trusted_Connection=True")
        .Options);

    [Fact]
    public void LeituraNaoApagaEmCascataPeloDispositivo()
    {
        using var db = NovoBanco();
        var fk = db.Model.FindEntityType(typeof(Leitura))!.GetForeignKeys()
            .Single(f => f.PrincipalEntityType.ClrType == typeof(Dispositivo));

        Assert.Equal(DeleteBehavior.Restrict, fk.DeleteBehavior);
    }

    [Fact]
    public void NenhumaTabelaRecebeCascataPorDoisCaminhos()
    {
        using var db = NovoBanco();
        foreach (var entidade in db.Model.GetEntityTypes())
        {
            var origens = Ancestrais(entidade).GroupBy(t => t.Name).Where(g => g.Count() > 1).Select(g => g.Key);
            Assert.True(!origens.Any(), $"{entidade.GetTableName()} recebe cascata de {string.Join(", ", origens)} por mais de um caminho.");
        }
    }

    [Fact]
    public void ScriptDasMigracoesTemUmaCascataSoEmLeituras()
    {
        using var db = NovoBanco();
        var sql = db.GetService<IMigrator>().GenerateScript(options: MigrationsSqlGenerationOptions.Idempotent);

        var criaLeituras = Regex.Match(sql, @"CREATE TABLE \[Leituras\] \((.*?)\);", RegexOptions.Singleline);
        Assert.True(criaLeituras.Success, "O script não cria a tabela Leituras.");
        Assert.Single(Regex.Matches(criaLeituras.Value, "ON DELETE CASCADE"));
    }

    // Cada tabela de onde parte uma cascata que chega até esta entidade, uma vez por caminho.
    static IEnumerable<IEntityType> Ancestrais(IEntityType entidade) =>
        entidade.GetForeignKeys()
            .Where(f => f.DeleteBehavior == DeleteBehavior.Cascade)
            .SelectMany(f => Ancestrais(f.PrincipalEntityType).Append(f.PrincipalEntityType));
}
