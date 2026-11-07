namespace TermometroDaSala.Desktop;

/// <summary>
/// Salas de exemplo do modo local, uma por nível, mostradas só enquanto este computador não tem arquivo de dados
/// (por exemplo, quem abre o ZIP sem a API). Tudo sai marcado como DEMONSTRAÇÃO e nada disso é gravado.
/// </summary>
public static class DadosDemonstracao
{
    // id, nome, andar, face, índice agora, nível agora, índice previsto (+2 h), nível previsto; faixas iguais às da API
    private static readonly (string Id, string Nome, int Andar, string Face, double Indice, NivelCalor Nivel, double Previsto, NivelCalor NivelPrevisto)[] Salas =
    [
        ("sala-01", "Sala 1 (exemplo)", 0, "sul", 25.8, NivelCalor.Normal, 27.5, NivelCalor.Atencao),
        ("sala-02", "Sala 2 (exemplo)", 1, "leste", 29.4, NivelCalor.Atencao, 31.0, NivelCalor.Atencao),
        ("sala-03", "Sala 3 (exemplo)", 1, "oeste", 35.1, NivelCalor.Cuidado, 38.6, NivelCalor.Cuidado),
        ("sala-04", "Sala 4 (exemplo)", 2, "oeste", 41.6, NivelCalor.Perigo, 43.0, NivelCalor.Perigo),
        ("sala-05", "Sala 5 (exemplo)", 0, "sul", 17.9, NivelCalor.Frio, 17.0, NivelCalor.Frio)
    ];

    public static DadosLocais Gerar(DateTimeOffset agora) => new()
    {
        Salas = Salas.Select(s => Sala.Exemplo(s.Id, s.Nome, s.Andar, s.Face, new Leitura(s.Indice, s.Nivel, agora.AddMinutes(-2)))).ToList(),
        Acoes =
        [
            new Acao("sala-03", NivelCalor.Cuidado, "DEMONSTRAÇÃO: janelas abertas e água para a turma", agora.AddMinutes(-20)),
            new Acao("sala-04", NivelCalor.Perigo, "DEMONSTRAÇÃO: turma levada para a biblioteca", agora.AddMinutes(-5))
        ]
    };

    public static Previsao? Prever(string id, DateTimeOffset agora) => Salas.Where(s => s.Id == id).Select(s => new Previsao
    {
        AmbienteId = s.Id, PrevistoPara = agora.AddHours(2), IndiceRuaPrevisto = s.Previsto - 3, DeltaSala = 3,
        IndiceSalaPrevisto = s.Previsto, NivelPrevisto = s.NivelPrevisto, SemLeiturasRecentes = false, Demonstracao = true, RuaDisponivel = true
    }).FirstOrDefault();
}
