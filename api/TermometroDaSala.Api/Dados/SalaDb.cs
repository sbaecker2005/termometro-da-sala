using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace TermometroDaSala.Api.Dados;

public class Ambiente
{
    public string Id { get; set; } = "";
    public string Nome { get; set; } = "";
    public string Tipo { get; set; } = "sala";
    public int Andar { get; set; }
    public string FaceSol { get; set; } = "";
    public bool TemVentilador { get; set; }
}

public class Dispositivo
{
    public string Id { get; set; } = "";
    public string AmbienteId { get; set; } = "";
    public string Segredo { get; set; } = "";
    public bool Ativo { get; set; } = true;
    public DateTimeOffset? UltimoMedidoEm { get; set; }
}

public class Leitura
{
    public long Id { get; set; }
    public string AmbienteId { get; set; } = "";
    public string DispositivoId { get; set; } = "";
    public double TemperaturaC { get; set; }
    public int UmidadePct { get; set; }
    public double IndiceCalor { get; set; }
    public string Nivel { get; set; } = "";
    public DateTimeOffset MedidoEm { get; set; }
    public string Origem { get; set; } = "dispositivo";
    public bool Suspeita { get; set; }
}

public class LeituraRua
{
    public long Id { get; set; }
    public DateTimeOffset MedidoEm { get; set; }
    public double TemperaturaC { get; set; }
    public int UmidadePct { get; set; }
    public double IndiceCalor { get; set; }
}

public class Previsao
{
    public long Id { get; set; }
    public string AmbienteId { get; set; } = "";
    public DateTimeOffset PrevistoPara { get; set; }
    public double IndiceRuaPrevisto { get; set; }
    public double? DeltaSala { get; set; }
    public double? IndiceSalaPrevisto { get; set; }
    public string NivelPrevisto { get; set; } = "";
    public bool SemLeiturasRecentes { get; set; }
    public DateTimeOffset CalculadaEm { get; set; }
}

public class Ocorrencia
{
    public Guid Id { get; set; }
    public string AmbienteId { get; set; } = "";
    public string Descricao { get; set; } = "";
    public string CaminhoFoto { get; set; } = "";
    public DateTimeOffset CriadaEm { get; set; }
}

public class Acao
{
    public long Id { get; set; }
    public string AmbienteId { get; set; } = "";
    public string Nivel { get; set; } = "";
    public string Descricao { get; set; } = "";
    public DateTimeOffset RegistradaEm { get; set; }
}

public class NivelAcao
{
    public string Nivel { get; set; } = "";
    public string Nome { get; set; } = "";
    public string AcaoSugerida { get; set; } = "";
    public double IndiceMin { get; set; }
}

public class SalaDb : DbContext
{
    public SalaDb(DbContextOptions<SalaDb> opcoes) : base(opcoes) { }

    public DbSet<Ambiente> Ambientes => Set<Ambiente>();
    public DbSet<Dispositivo> Dispositivos => Set<Dispositivo>();
    public DbSet<Leitura> Leituras => Set<Leitura>();
    public DbSet<LeituraRua> LeiturasRua => Set<LeituraRua>();
    public DbSet<Previsao> Previsoes => Set<Previsao>();
    public DbSet<Ocorrencia> Ocorrencias => Set<Ocorrencia>();
    public DbSet<Acao> Acoes => Set<Acao>();
    public DbSet<NivelAcao> NiveisAcao => Set<NivelAcao>();

    protected override void OnModelCreating(ModelBuilder model)
    {
        model.Entity<Ambiente>().Property(a => a.Id).HasMaxLength(32);
        model.Entity<Ambiente>().Property(a => a.Nome).HasMaxLength(80);
        model.Entity<Ambiente>().Property(a => a.Tipo).HasMaxLength(20);
        model.Entity<Ambiente>().Property(a => a.FaceSol).HasMaxLength(20);

        model.Entity<Dispositivo>().Property(d => d.Id).HasMaxLength(64);
        model.Entity<Dispositivo>().Property(d => d.Segredo).HasMaxLength(128);
        model.Entity<Dispositivo>().HasOne<Ambiente>().WithMany().HasForeignKey(d => d.AmbienteId);

        model.Entity<Leitura>().Property(l => l.Nivel).HasMaxLength(20);
        model.Entity<Leitura>().Property(l => l.Origem).HasMaxLength(20);
        model.Entity<Leitura>().HasOne<Ambiente>().WithMany().HasForeignKey(l => l.AmbienteId);
        model.Entity<Leitura>().HasOne<Dispositivo>().WithMany().HasForeignKey(l => l.DispositivoId);
        model.Entity<Leitura>().HasIndex(l => new { l.AmbienteId, l.MedidoEm });

        model.Entity<LeituraRua>().HasIndex(r => r.MedidoEm);
        model.Entity<Previsao>().HasOne<Ambiente>().WithMany().HasForeignKey(p => p.AmbienteId);
        model.Entity<Previsao>().Property(p => p.NivelPrevisto).HasMaxLength(20);

        model.Entity<Ocorrencia>().Property(o => o.Descricao).HasMaxLength(500);
        model.Entity<Ocorrencia>().Property(o => o.CaminhoFoto).HasMaxLength(80);
        model.Entity<Ocorrencia>().HasOne<Ambiente>().WithMany().HasForeignKey(o => o.AmbienteId);

        model.Entity<Acao>().Property(a => a.Descricao).HasMaxLength(500);
        model.Entity<Acao>().Property(a => a.Nivel).HasMaxLength(20);
        model.Entity<Acao>().HasOne<Ambiente>().WithMany().HasForeignKey(a => a.AmbienteId);

        model.Entity<NivelAcao>().HasKey(n => n.Nivel);
        model.Entity<NivelAcao>().Property(n => n.Nivel).HasMaxLength(20);

        model.Entity<NivelAcao>().HasData(
            new NivelAcao { Nivel = "normal", IndiceMin = 0, Nome = "Normal", AcaoSugerida = "nenhuma" },
            new NivelAcao { Nivel = "atencao", IndiceMin = 27, Nome = "Atenção", AcaoSugerida = "liberar água, abrir janelas e ligar ventiladores" },
            new NivelAcao { Nivel = "cuidado", IndiceMin = 32, Nome = "Cuidado extremo", AcaoSugerida = "pausa, troca de sala ou adiar a Educação Física" },
            new NivelAcao { Nivel = "perigo", IndiceMin = 41, Nome = "Perigo", AcaoSugerida = "tirar a turma da sala" });

        // Segredo de demonstração do contrato, o mesmo nas quatro salas. Cada aparelho de verdade terá o seu.
        const string segredo = "segredo-de-teste-sala-03";
        model.Entity<Ambiente>().HasData(
            new Ambiente { Id = "sala-01", Nome = "Sala 1", Tipo = "sala", Andar = 0, FaceSol = "leste", TemVentilador = true },
            new Ambiente { Id = "sala-02", Nome = "Sala 2", Tipo = "sala", Andar = 0, FaceSol = "norte", TemVentilador = true },
            new Ambiente { Id = "sala-03", Nome = "Sala 3", Tipo = "sala", Andar = 1, FaceSol = "oeste", TemVentilador = true },
            new Ambiente { Id = "sala-04", Nome = "Sala 4", Tipo = "sala", Andar = 2, FaceSol = "sul", TemVentilador = false });
        model.Entity<Dispositivo>().HasData(
            new Dispositivo { Id = "esp32-sala-01", AmbienteId = "sala-01", Segredo = segredo, Ativo = true },
            new Dispositivo { Id = "esp32-sala-02", AmbienteId = "sala-02", Segredo = segredo, Ativo = true },
            new Dispositivo { Id = "esp32-sala-03", AmbienteId = "sala-03", Segredo = segredo, Ativo = true },
            new Dispositivo { Id = "esp32-sala-04", AmbienteId = "sala-04", Segredo = segredo, Ativo = true });
    }
}

public class SalaDbFactory : IDesignTimeDbContextFactory<SalaDb>
{
    public SalaDb CreateDbContext(string[] args)
    {
        var opcoes = new DbContextOptionsBuilder<SalaDb>()
            .UseSqlServer("Server=localhost;Database=TermometroDaSala;Trusted_Connection=True;TrustServerCertificate=True")
            .Options;
        return new SalaDb(opcoes);
    }
}
