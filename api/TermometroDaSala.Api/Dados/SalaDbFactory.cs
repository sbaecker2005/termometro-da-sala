using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TermometroDaSala.Api.Dados;

public class SalaDbFactory : IDesignTimeDbContextFactory<SalaDb>
{
    public SalaDb CreateDbContext(string[] args)
    {
        var config = WebApplication.CreateBuilder(new WebApplicationOptions
        {
            Args = args,
            ContentRootPath = AppContext.BaseDirectory,
        }).Configuration;
        var opcoes = new DbContextOptionsBuilder<SalaDb>()
            .UseSqlServer(config.GetConnectionString("Sala")
                ?? throw new InvalidOperationException("Configure ConnectionStrings:Sala."))
            .Options;
        return new SalaDb(opcoes);
    }
}
