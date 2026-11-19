using System.Text.Json;
using System.Text.Json.Serialization;

namespace TermometroDaSala.Desktop;

/// <summary>Faixas da API: abaixo de 18 frio, 18 normal, 27 atenção, 32 cuidado, 41 perigo.</summary>
[JsonConverter(typeof(NivelCalorJson))]
public enum NivelCalor { Normal, Atencao, Cuidado, Perigo, Frio }

/// <summary>No JSON o nível vai em minúsculas, como a API grava.</summary>
public sealed class NivelCalorJson() : JsonStringEnumConverter<NivelCalor>(JsonNamingPolicy.CamelCase);
