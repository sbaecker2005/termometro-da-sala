namespace TermometroDaSala.Desktop;

static class Program
{
    /// <summary>
    ///  The main entry point for the application.
    /// </summary>
    [STAThread]
    static int Main(string[] args)
    {
        if (args.Contains("--autoteste")) return ValidacoesAutoteste.Executar();
        ApplicationConfiguration.Initialize();
        try
        {
            using var http = new HttpClient { BaseAddress = Configuracao.LerUrl(), Timeout = TimeSpan.FromSeconds(5) };
            Application.Run(new Form1(new ServicoSalasApi(http), new ServicoSalasLocal(
                Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "TermometroDaSala", "dados.json"))));
            return 0;
        }
        catch (Exception ex) when (ex is IOException or System.Text.Json.JsonException or ArgumentException or UnauthorizedAccessException)
        {
            MessageBox.Show("Não foi possível iniciar. Confira appsettings.json e o acesso ao arquivo local.\n" + ex.Message,
                "Termômetro da Sala", MessageBoxButtons.OK, MessageBoxIcon.Error);
            return 1;
        }
    }    
}
