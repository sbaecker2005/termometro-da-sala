using System.Text.Json;
using System.Text.Json.Nodes;

namespace TermometroDaSala.Desktop;

public sealed class ServicoSalasLocal(string arquivo) : IServicoSalas
{
    // Sem arquivo ainda, a leitura mostra as salas de DEMONSTRAÇÃO; o cadastro começa de uma lista vazia.
    private DadosLocais Ler(bool comDemonstracao = true)
    {
        if (!File.Exists(arquivo)) return comDemonstracao ? DadosDemonstracao.Gerar(DateTimeOffset.UtcNow) : new DadosLocais();
        var raiz = JsonNode.Parse(File.ReadAllText(arquivo)) as JsonObject
            ?? throw new JsonException("Arquivo local vazio ou inválido. Preserve-o antes de corrigir.");
        ConverterAcoesAntigas(raiz);
        var dados = raiz.Deserialize<DadosLocais>(ServicoSalasApi.Json)
            ?? throw new JsonException("Arquivo local vazio ou inválido. Preserve-o antes de corrigir.");
        if (dados.Salas is null || dados.Acoes is null || dados.Salas.Any(s => s.Problemas().Count > 0) ||
            dados.Salas.Select(s => s.Id).Distinct().Count() != dados.Salas.Count)
            throw new JsonException("Dados locais inválidos. O arquivo não foi sobrescrito.");
        foreach (var acao in dados.Acoes) acao.Conferir();
        return dados;
    }

    // Até a versão 0.1.0 o id da ação era um GUID em texto. Cada id em texto vira o próximo número livre,
    // preservando o registro; o arquivo só é regravado no próximo salvamento.
    internal static void ConverterAcoesAntigas(JsonObject raiz)
    {
        var chave = raiz.Select(p => p.Key).FirstOrDefault(k => k.Equals("acoes", StringComparison.OrdinalIgnoreCase));
        if (chave is null || raiz[chave] is not JsonArray acoes) return;
        // Números gravados como texto ("1") também reservam o id, para o convertido não colidir.
        var proximo = acoes.Select(a => a?["id"] ?? a?["Id"]).OfType<JsonValue>()
            .Select(v => v.TryGetValue<long>(out var n) ? n : v.TryGetValue<string>(out var t) && long.TryParse(t, out var m) ? m : 0)
            .DefaultIfEmpty(0).Max() + 1;
        foreach (var acao in acoes.OfType<JsonObject>())
        {
            var campo = acao.Select(p => p.Key).FirstOrDefault(k => k.Equals("id", StringComparison.OrdinalIgnoreCase));
            if (campo is null || acao[campo] is not JsonValue valor || !valor.TryGetValue<string>(out var texto)) continue;
            acao[campo] = long.TryParse(texto, out var numero) ? numero : proximo++;
        }
    }

    private void Salvar(DadosLocais dados)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(arquivo))!);
        var temporario = arquivo + ".tmp";
        File.WriteAllText(temporario, JsonSerializer.Serialize(dados, ServicoSalasApi.Json));
        File.Move(temporario, arquivo, true);
    }

    private static Sala SalaGravavel(DadosLocais dados, string id) =>
        dados.Salas.SingleOrDefault(s => s.Id == id) switch
        {
            null => throw new ArgumentException("Sala não encontrada."),
            { Demonstracao: true } => throw new ArgumentException("Sala de DEMONSTRAÇÃO: cadastre uma sala para registrar neste computador."),
            var sala => sala
        };

    public Task<List<Sala>> Listar() => Task.FromResult(Ler().Salas);

    // Fora da demonstração, o modo local não tem sensor nem modelo: não inventa previsão.
    public Task<Previsao?> Prever(string id) => Task.FromResult(
        Ler().Salas.Any(s => s.Id == id && s.Demonstracao) ? DadosDemonstracao.Prever(id, DateTimeOffset.UtcNow) : null);

    public Task Cadastrar(Sala sala)
    {
        if (sala.Problemas() is { Count: > 0 } erros) throw new CadastroInvalidoException(erros);
        var dados = Ler(comDemonstracao: false);
        if (dados.Salas.Any(s => s.Id == sala.Id)) throw new ArgumentException("Este identificador já está cadastrado.");
        dados.Salas.Add(sala);
        Salvar(dados);
        return Task.CompletedTask;
    }

    public Task Registrar(Acao acao)
    {
        acao.Conferir();
        var dados = Ler();
        SalaGravavel(dados, acao.AmbienteId);
        acao.Numerar(dados.Acoes.Count == 0 ? 1 : dados.Acoes.Max(a => a.Id) + 1);
        dados.Acoes.Add(acao);
        Salvar(dados);
        return Task.CompletedTask;
    }

    public Task<List<Acao>> AcoesDoDia(DateOnly dia) => Task.FromResult(
        Ler().Acoes.Where(a => HorarioEscola.Dia(a.RegistradoEm) == dia).ToList());

    public Task Revogar(Sala sala)
    {
        var dados = Ler();
        SalaGravavel(dados, sala.Id).Revogar();
        Salvar(dados);
        return Task.CompletedTask;
    }
}
