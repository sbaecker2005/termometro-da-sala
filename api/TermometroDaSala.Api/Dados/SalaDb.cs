using Microsoft.EntityFrameworkCore;
using TermometroDaSala.Api.Dominio.Entidades;

namespace TermometroDaSala.Api.Dados;

public class SalaDb : DbContext
{
    public SalaDb(DbContextOptions<SalaDb> opcoes) : base(opcoes) { }

    public DbSet<Ambiente> Ambientes => Set<Ambiente>();
    public DbSet<Lugar> Lugares => Set<Lugar>();
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
        model.Entity<Ambiente>().Property(a => a.Tipo).HasConversion<string>().HasMaxLength(4);
        model.Entity<Ambiente>().Property(a => a.FaceSol).HasConversion<string>().HasMaxLength(5);

        model.Entity<Dispositivo>().Property(d => d.Id).HasMaxLength(64);
        model.Entity<Dispositivo>().Property(d => d.Rotulo).HasMaxLength(80);
        model.Entity<Lugar>().Property(l => l.Nome).HasMaxLength(80);
        model.Entity<Lugar>().HasIndex(l => new { l.AmbienteId, l.Nome }).IsUnique();
        model.Entity<Lugar>().HasOne(l => l.Ambiente).WithMany(a => a.Lugares).HasForeignKey(l => l.AmbienteId);
        model.Entity<Dispositivo>().Property(d => d.Segredo).HasMaxLength(128);
        model.Entity<Dispositivo>().HasOne(e => e.Ambiente).WithMany(a => a.Dispositivos).HasForeignKey(d => d.AmbienteId);

        model.Entity<Leitura>().Property(l => l.Nivel).HasConversion<string>().HasMaxLength(7);
        model.Entity<Leitura>().Property(l => l.Origem).HasConversion<string>().HasMaxLength(11);
        model.Entity<Leitura>().HasOne(e => e.Ambiente).WithMany(a => a.Leituras).HasForeignKey(l => l.AmbienteId);
        model.Entity<Leitura>().HasOne(l => l.Dispositivo).WithMany(d => d.Leituras).HasForeignKey(l => l.DispositivoId)
            .OnDelete(DeleteBehavior.Restrict); // evita o erro 1785: Leituras já recebe cascata por Ambientes
        model.Entity<Leitura>().HasIndex(l => new { l.AmbienteId, l.MedidoEm });

        model.Entity<LeituraRua>().HasIndex(r => r.MedidoEm);
        model.Entity<Previsao>().HasOne(e => e.Ambiente).WithMany(a => a.Previsoes).HasForeignKey(p => p.AmbienteId);
        model.Entity<Previsao>().Property(p => p.NivelPrevisto).HasConversion<string>().HasMaxLength(7);

        model.Entity<Ocorrencia>().Property(o => o.Descricao).HasMaxLength(500);
        model.Entity<Ocorrencia>().Property(o => o.CaminhoFoto).HasMaxLength(80);
        model.Entity<Ocorrencia>().HasOne(e => e.Ambiente).WithMany(a => a.Ocorrencias).HasForeignKey(o => o.AmbienteId);

        model.Entity<Acao>().Property(a => a.Descricao).HasMaxLength(500);
        model.Entity<Acao>().Property(a => a.Nivel).HasConversion<string>().HasMaxLength(7);
        model.Entity<Acao>().HasOne(e => e.Ambiente).WithMany(a => a.Acoes).HasForeignKey(a => a.AmbienteId);

        model.Entity<NivelAcao>().HasKey(n => n.Nivel);
        model.Entity<NivelAcao>().Property(n => n.Nivel).HasConversion<string>().HasMaxLength(7);

        model.Entity<NivelAcao>().HasData(
            // Zero absoluto como início físico da faixa aberta de frio.
            new NivelAcao { Nivel = Nivel.frio, IndiceMin = -273.15, Nome = "Frio", AcaoSugerida = "Fechar janelas, agasalhar e oferecer cobertor; à noite, avisar o plantão e levar quem estiver no cômodo mais frio para outro" },
            new NivelAcao { Nivel = Nivel.normal, IndiceMin = 18, Nome = "Normal", AcaoSugerida = "nenhuma" },
            new NivelAcao { Nivel = Nivel.atencao, IndiceMin = 27, Nome = "Atenção", AcaoSugerida = "liberar água, abrir janelas e ligar ventiladores" },
            new NivelAcao { Nivel = Nivel.cuidado, IndiceMin = 32, Nome = "Cuidado extremo", AcaoSugerida = "pausa, troca de sala ou adiar a Educação Física" },
            new NivelAcao { Nivel = Nivel.perigo, IndiceMin = 41, Nome = "Perigo", AcaoSugerida = "tirar a turma da sala" });

        // Segredo de demonstração por dispositivo. Não é chave de produção.
        model.Entity<Ambiente>().HasData(
            new Ambiente { Id = "sala-01", Nome = "Sala 1", Tipo = TipoAmbiente.sala, Andar = 0, FaceSol = FaceSol.leste, TemVentilador = true },
            new Ambiente { Id = "sala-02", Nome = "Sala 2", Tipo = TipoAmbiente.sala, Andar = 0, FaceSol = FaceSol.norte, TemVentilador = true },
            new Ambiente { Id = "sala-03", Nome = "Sala 3", Tipo = TipoAmbiente.sala, Andar = 1, FaceSol = FaceSol.oeste, TemVentilador = true },
            new Ambiente { Id = "sala-04", Nome = "Sala 4", Tipo = TipoAmbiente.sala, Andar = 2, FaceSol = FaceSol.sul, TemVentilador = false });
        model.Entity<Dispositivo>().HasData(
            new Dispositivo { Id = "esp32-sala-01", AmbienteId = "sala-01", Segredo = "segredo-de-teste-sala-01" },
            new Dispositivo { Id = "esp32-sala-02", AmbienteId = "sala-02", Segredo = "segredo-de-teste-sala-02" },
            new Dispositivo { Id = "esp32-sala-03", AmbienteId = "sala-03", Segredo = "segredo-de-teste-sala-03" },
            new Dispositivo { Id = "esp32-sala-04", AmbienteId = "sala-04", Segredo = "segredo-de-teste-sala-04" });
    }
}

