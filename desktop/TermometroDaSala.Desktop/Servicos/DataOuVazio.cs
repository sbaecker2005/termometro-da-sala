using System.Text.Json;
using System.Text.Json.Serialization;

namespace TermometroDaSala.Desktop;

/// <summary>A API devolve ultimaLeituraEm como "" para sala sem leitura; aqui isso vira null.</summary>
public sealed class DataOuVazio : JsonConverter<DateTimeOffset?>
{
    public override DateTimeOffset? Read(ref Utf8JsonReader leitor, Type tipo, JsonSerializerOptions opcoes) =>
        leitor.TokenType == JsonTokenType.String && leitor.GetString() == "" ? null : leitor.GetDateTimeOffset();

    public override void Write(Utf8JsonWriter escritor, DateTimeOffset? valor, JsonSerializerOptions opcoes)
    {
        if (valor is { } data) escritor.WriteStringValue(data);
        else escritor.WriteNullValue();
    }
}
