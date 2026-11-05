using System.Text.Json;
using System.Text.Json.Serialization;

namespace TermometroDaSala.Desktop;

/// <summary>Faixas do índice de calor, as mesmas da API: abaixo de 27 normal, 27 atenção, 32 cuidado, 41 perigo.</summary>
[JsonConverter(typeof(NivelCalorJson))]
public enum NivelCalor { Normal, Atencao, Cuidado, Perigo }

/// <summary>No JSON o nível vai em minúsculas (normal, atencao, cuidado, perigo), como a API grava.</summary>
public sealed class NivelCalorJson() : JsonStringEnumConverter<NivelCalor>(JsonNamingPolicy.CamelCase);
