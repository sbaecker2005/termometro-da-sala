using System.Text;

namespace TermometroDaSala.Desktop;

public static class ExportacaoCsv
{
    private static string Campo(string valor)
    {
        // Planilhas interpretam fórmulas mesmo em células entre aspas.
        if (valor.TrimStart().FirstOrDefault() is '=' or '+' or '-' or '@' or '\t' or '\r' or '\n') valor = "'" + valor;
        return "\"" + valor.Replace("\"", "\"\"") + "\"";
    }

    public static string Gerar(IEnumerable<Acao> acoes)
    {
        var texto = new StringBuilder("Sala;Nível;Descrição;Hora (São Paulo)\r\n");
        foreach (var acao in acoes)
            texto.AppendLine(string.Join(";", new[] { acao.AmbienteId, acao.Nivel.Texto(), acao.Descricao,
                HorarioEscola.Hora(acao.RegistradoEm).ToString("yyyy-MM-dd HH:mm:ss zzz") }.Select(Campo)));
        return texto.ToString();
    }
}
