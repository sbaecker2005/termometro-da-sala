using System.Text.Json.Serialization;
using System.Text.RegularExpressions;

namespace TermometroDaSala.Desktop;

/// <summary>Ação tomada pela coordenação numa sala. O construtor valida; depois de criada, só o número muda (Numerar).</summary>
public sealed class Acao
{
    // A API gera o id (long). No modo local, o serviço numera em sequência.
    [JsonInclude] public long Id { get; private set; }
    [JsonInclude] public string AmbienteId { get; private set; } = "";
    [JsonInclude] public NivelCalor Nivel { get; private set; }
    [JsonInclude] public string Descricao { get; private set; } = "";
    [JsonInclude] public DateTimeOffset RegistradoEm { get; private set; }

    [JsonConstructor] private Acao() { }

    public Acao(string ambienteId, NivelCalor nivel, string? descricao, DateTimeOffset? registradoEm = null)
    {
        AmbienteId = ambienteId;
        Nivel = nivel;
        Descricao = descricao?.Trim() ?? "";
        RegistradoEm = registradoEm ?? DateTimeOffset.UtcNow;
        Conferir();
    }

    public static string? ValidarDescricao(string? texto) => (texto?.Trim().Length ?? 0) is < 5 or > 300
        ? "Descreva a ação com 5 a 300 caracteres." : null;

    /// <summary>Usado também nas ações que chegam da API ou do arquivo local.</summary>
    public void Conferir()
    {
        if (!Regex.IsMatch(AmbienteId ?? "", @"\Asala-[0-9]{2}\z") || !Enum.IsDefined(Nivel))
            throw new ArgumentException("Sala ou nível inválido.");
        if (ValidarDescricao(Descricao) is { } erro) throw new ArgumentException(erro);
    }

    internal void Numerar(long id) => Id = id;
}
