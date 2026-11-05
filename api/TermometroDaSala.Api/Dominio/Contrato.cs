using System.Globalization;
using System.Security.Cryptography;
using System.Text;

namespace TermometroDaSala.Api.Dominio;

public static class IndiceCalor
{
    // Faixas iguais à semente de NivelAcao: 27 atenção, 32 cuidado, 41 perigo.
    public static string Nivel(double indice)
    {
        if (indice < 27) return "normal";
        if (indice < 32) return "atencao";
        if (indice < 41) return "cuidado";
        return "perigo";
    }

    public static bool EhNivel(string nivel) =>
        nivel is "normal" or "atencao" or "cuidado" or "perigo";

    public static double Calcular(double tempC, double umidade)
    {
        var t = tempC * 9 / 5 + 32;
        var rh = umidade;
        var simples = 0.5 * (t + 61.0 + (t - 68.0) * 1.2 + rh * 0.094);
        double hi;
        if ((simples + t) / 2 < 80)
        {
            hi = simples;
        }
        else
        {
            hi = -42.379 + 2.04901523 * t + 10.14333127 * rh - 0.22475541 * t * rh
                - 0.00683783 * t * t - 0.05481717 * rh * rh + 0.00122874 * t * t * rh
                + 0.00085282 * t * rh * rh - 0.00000199 * t * t * rh * rh;
            if (rh < 13 && t is >= 80 and <= 112)
                hi -= ((13 - rh) / 4) * Math.Sqrt((17 - Math.Abs(t - 95)) / 17);
            else if (rh > 85 && t is >= 80 and <= 87)
                hi += ((rh - 85) / 10) * ((87 - t) / 5);
        }
        return Math.Round((hi - 32) * 5 / 9, 1);
    }
}

public static class Relogio
{
    public static readonly TimeZoneInfo SaoPaulo =
        TimeZoneInfo.FindSystemTimeZoneById("E. South America Standard Time");

    public static DateTimeOffset HoraLocal(DateTimeOffset utc) => TimeZoneInfo.ConvertTime(utc, SaoPaulo);
}

public static class Assinatura
{
    public static string Canonico(string ambienteId, string dispositivo, double temperaturaC, int umidadePct, long medidoEm) =>
        string.Join('|',
            ambienteId,
            dispositivo,
            temperaturaC.ToString("0.0", CultureInfo.InvariantCulture),
            umidadePct.ToString(CultureInfo.InvariantCulture),
            medidoEm.ToString(CultureInfo.InvariantCulture));

    public static string Hex(string segredo, string ambienteId, string dispositivo, double temperaturaC, int umidadePct, long medidoEm)
    {
        var hash = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(segredo),
            Encoding.UTF8.GetBytes(Canonico(ambienteId, dispositivo, temperaturaC, umidadePct, medidoEm)));
        return Convert.ToHexString(hash).ToLowerInvariant();
    }

    public static bool Confere(string segredo, string ambienteId, string dispositivo, double temperaturaC, int umidadePct, long medidoEm, string assinaturaHex)
    {
        byte[] informado;
        try { informado = Convert.FromHexString(assinaturaHex); }
        catch (FormatException) { return false; }
        var hash = HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(segredo),
            Encoding.UTF8.GetBytes(Canonico(ambienteId, dispositivo, temperaturaC, umidadePct, medidoEm)));
        return informado.Length == hash.Length && CryptographicOperations.FixedTimeEquals(hash, informado);
    }
}

public static class ValidadorLeitura
{
    public static string? Motivo(
        string? ambienteDoDispositivo,
        bool? ativo,
        string segredo,
        string ambienteId,
        string dispositivo,
        double temperaturaC,
        int umidadePct,
        long medidoEm,
        string assinatura,
        DateTimeOffset agora,
        long? ultimoMedidoEm)
    {
        if (ambienteDoDispositivo is null
            || ambienteDoDispositivo != ambienteId
            || !Assinatura.Confere(segredo, ambienteId, dispositivo, temperaturaC, umidadePct, medidoEm, assinatura))
            return "assinatura-invalida";
        if (ativo == false) return "dispositivo-revogado";
        if (ultimoMedidoEm is not null && medidoEm <= ultimoMedidoEm) return "reenvio";
        var quando = DateTimeOffset.FromUnixTimeSeconds(medidoEm);
        if (quando < agora.AddMinutes(-10) || quando > agora.AddMinutes(1)) return "atrasada";
        if (temperaturaC is < -10 or > 60 || umidadePct is < 0 or > 100) return "fora-da-faixa";
        return null;
    }

    public static bool Suspeita(double temperaturaC, double? temperaturaAnterior, double? temperaturaRua) =>
        (temperaturaAnterior is not null && Math.Abs(temperaturaC - temperaturaAnterior.Value) > 5)
        || (temperaturaRua is > 30 && temperaturaC < temperaturaRua.Value - 8);
}
