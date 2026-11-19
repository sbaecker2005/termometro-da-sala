using System.Text.Json;

namespace TermometroDaSala.Desktop;

public static class Configuracao
{
    public static Uri LerUrl()
    {
        using var json = JsonDocument.Parse(File.ReadAllText(Path.Combine(AppContext.BaseDirectory, "appsettings.json")));
        if (!json.RootElement.TryGetProperty("ApiUrl", out var campo) || campo.ValueKind != JsonValueKind.String ||
            !Uri.TryCreate(campo.GetString(), UriKind.Absolute, out var uri) ||
            (uri.Scheme != "http" && uri.Scheme != "https") || !string.IsNullOrEmpty(uri.UserInfo) ||
            !string.IsNullOrEmpty(uri.Query) || !string.IsNullOrEmpty(uri.Fragment))
            throw new ArgumentException("ApiUrl deve ser uma URL HTTP ou HTTPS sem credenciais, consulta ou fragmento.");
        return new Uri(uri.AbsoluteUri.TrimEnd('/') + "/");
    }
}
