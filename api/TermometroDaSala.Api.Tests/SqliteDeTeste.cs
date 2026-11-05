using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace TermometroDaSala.Api.Tests;

// O SQLite não ordena nem compara DateTimeOffset. Só nos testes, as datas viram número (ticks UTC),
// para as mesmas consultas da API (WHERE e ORDER BY por data) rodarem no banco em memória.
public static class SqliteDeTeste
{
    public static void Usar(DbContextOptionsBuilder opcoes, SqliteConnection conexao) =>
        opcoes.UseSqlite(conexao).ReplaceService<IModelCustomizer, DatasComoNumero>();

    static readonly ValueConverter<DateTimeOffset, long> TicksUtc = new(d => d.UtcTicks, t => new DateTimeOffset(t, TimeSpan.Zero));

    sealed class DatasComoNumero(ModelCustomizerDependencies dependencias) : ModelCustomizer(dependencias)
    {
        public override void Customize(ModelBuilder model, DbContext contexto)
        {
            base.Customize(model, contexto);
            foreach (var propriedade in model.Model.GetEntityTypes().SelectMany(e => e.GetProperties())
                         .Where(p => p.ClrType == typeof(DateTimeOffset) || p.ClrType == typeof(DateTimeOffset?)))
                propriedade.SetValueConverter(TicksUtc);
        }
    }
}
