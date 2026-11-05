namespace TermometroDaSala.Desktop;

/// <summary>Tela principal da coordenação. O layout fica em TelaCoordenacao.Designer.cs (abre no Designer do Visual Studio).</summary>
public partial class TelaCoordenacao : Form
{
    private readonly IServicoSalas api;
    private readonly IServicoSalas local;
    private IServicoSalas? servico;
    private bool ocupado;

    public TelaCoordenacao(IServicoSalas api, IServicoSalas local)
    {
        InitializeComponent();
        this.api = api;
        this.local = local;
        Icon = Icone();
        nivel.DataSource = Enum.GetValues<NivelCalor>().Select(n => new { Valor = n, Texto = n.Texto() }).ToList();
        nivel.DisplayMember = "Texto";
        nivel.ValueMember = "Valor";
    }

    /// <summary>O mesmo termometro.ico do executável, embutido no assembly para não depender do caminho do .exe.</summary>
    internal static Icon Icone()
    {
        using var arquivo = typeof(TelaCoordenacao).Assembly.GetManifestResourceStream("termometro.ico")
            ?? throw new InvalidOperationException("Ícone não embutido no executável.");
        return new Icon(arquivo);
    }

    /// <summary>Cor da célula por nível. A legenda do Designer usa as mesmas cores (o autoteste confere).</summary>
    internal static Color CorDoNivel(NivelCalor? nivel) => nivel switch
    {
        NivelCalor.Normal => Color.LightGreen, NivelCalor.Atencao => Color.Khaki,
        NivelCalor.Cuidado => Color.LightSalmon, NivelCalor.Perigo => Color.LightPink, _ => SystemColors.Window
    };

    private async void AoMostrar(object? sender, EventArgs e) { await Executar(Atualizar); timer.Start(); }
    private async void AoAtualizar(object? sender, EventArgs e) => await Executar(Atualizar);
    private async void AoCadastrar(object? sender, EventArgs e) => await Executar(Cadastrar);
    private async void AoRevogar(object? sender, EventArgs e) => await Executar(Revogar);
    private async void AoExportar(object? sender, EventArgs e) => await Executar(Exportar);
    private async void AoRegistrar(object? sender, EventArgs e) => await Executar(Registrar);

    private async Task Executar(Func<Task> acao)
    {
        if (ocupado) return;
        ocupado = true;
        comandos.Enabled = registro.Enabled = false;
        try { await acao(); }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or IOException or
            UnauthorizedAccessException or System.Text.Json.JsonException or ArgumentException)
        {
            MessageBox.Show(this, ex.Message + "\nSe uma gravação perdeu a conexão, confira o resultado antes de tentar de novo.",
                "Operação não concluída", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }
        finally { ocupado = false; comandos.Enabled = registro.Enabled = true; }
    }

    private async Task Atualizar()
    {
        var selecionada = (salas.CurrentRow?.Tag as Sala)?.Id;
        List<Sala> lista;
        var anterior = servico;
        servico = null;
        salas.Rows.Clear();
        acoes.Rows.Clear();
        status.Text = "Atualizando — aguarde. Em caso de erro, use Atualizar para tentar novamente.";
        try
        {
            lista = await api.Listar();
            servico = api;
            status.Text = "API conectada — dados do servidor";
            status.ForeColor = SystemColors.ControlText;
        }
        catch (Exception ex) when (ServicoSalasApi.Indisponivel(ex))
        {
            lista = await local.Listar();
            servico = local;
            status.Text = lista.Any(s => s.Demonstracao)
                ? "MODO LOCAL — DEMONSTRAÇÃO: API indisponível e nenhum dado neste computador. Salas de exemplo; cadastre uma sala para começar os registros locais."
                : "MODO LOCAL — API indisponível. Registros só neste computador; revogação sem efeito remoto.";
            status.ForeColor = Color.DarkRed;
        }
        if (anterior is not null && anterior != servico)
            status.Text += " A origem dos dados mudou.";
        foreach (var sala in lista)
        {
            Previsao? previsao = null;
            string aviso = servico == local && sala.Leitura is null ? "Sem dados do sensor" : "";
            try
            {
                previsao = await servico.Prever(sala.Id);
                if (previsao is null && servico != local) aviso = "Sem dados para prever: sem previsão da rua e sem leitura válida recente da sala";
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            { aviso = "Sala não encontrada na API"; }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            { aviso = "Previsão indisponível"; }
            if (sala.Demonstracao || previsao?.Demonstracao == true) aviso += " DEMONSTRAÇÃO";
            if (previsao?.RuaDisponivel == false) aviso += " Previsão da rua indisponível; previsão usa a última leitura válida da sala";
            else if (previsao?.SemLeiturasRecentes == true) aviso += " Sala sem leituras recentes; previsão da rua";
            if (sala.SensorRevogado) aviso += " Sensor revogado";
            if (sala.UltimaLeituraEm is { } hora && DateTimeOffset.UtcNow - hora > TimeSpan.FromMinutes(10)) aviso += " Leitura antiga";
            var linha = salas.Rows[salas.Rows.Add(sala.Id, sala.Nome,
                sala.Leitura is { } atual ? atual.Nivel.Texto() : "Sem leitura",
                sala.Leitura is { } l ? HorarioEscola.Hora(l.MedidoEm).ToString("dd/MM HH:mm:ss") : "—",
                previsao is null ? "Sem previsão" : $"{previsao.NivelPrevisto.Texto()} às {HorarioEscola.Hora(previsao.PrevistoPara):HH:mm}", aviso.Trim())];
            linha.Tag = sala;
            Cor(linha.Cells[colunaAtual.Index], sala.Leitura?.Nivel);
            Cor(linha.Cells[colunaPrevisto.Index], previsao?.NivelPrevisto);
            if (sala.Id == selecionada) salas.CurrentCell = linha.Cells[0];
        }
        if (lista.Count == 0) status.Text += " Nenhuma sala cadastrada.";
        await CarregarAcoes();
    }

    private static void Cor(DataGridViewCell celula, NivelCalor? nivel)
    {
        celula.Style.BackColor = CorDoNivel(nivel);
        celula.Style.ForeColor = Color.Black;
    }

    private Sala Selecionada() => salas.CurrentRow?.Tag as Sala ?? throw new ArgumentException("Selecione uma sala na grade.");

    private async Task Cadastrar()
    {
        if (servico is null) throw new ArgumentException("Atualize as salas primeiro.");
        using var cadastro = new CadastroSala();
        if (cadastro.ShowDialog(this) != DialogResult.OK || cadastro.Sala is not { } sala) return;
        await servico.Cadastrar(sala);
        await Atualizar();
    }

    private async Task Registrar()
    {
        var sala = Selecionada();
        erros.SetError(descricao, Acao.ValidarDescricao(descricao.Text) ?? "");
        if (Acao.ValidarDescricao(descricao.Text) is not null) { descricao.Focus(); return; }
        await servico!.Registrar(new Acao(sala.Id, (NivelCalor)nivel.SelectedValue!, descricao.Text));
        descricao.Clear();
        await CarregarAcoes();
    }

    private async Task CarregarAcoes()
    {
        var lista = await servico!.AcoesDoDia(HorarioEscola.Dia(DateTimeOffset.UtcNow));
        acoes.Rows.Clear();
        foreach (var acao in lista.OrderByDescending(a => a.RegistradoEm))
            acoes.Rows.Add(acao.AmbienteId, acao.Nivel.Texto(), acao.Descricao, HorarioEscola.Hora(acao.RegistradoEm).ToString("HH:mm:ss"));
    }

    private async Task Revogar()
    {
        var sala = Selecionada();
        var aviso = servico == local ? "Modo local: apenas marca o cadastro; NÃO bloqueia o sensor na API." : "O sensor deixará de enviar leituras aceitas pela API.";
        if (MessageBox.Show(this, $"Revogar o sensor de {sala.Nome}?\n{aviso}", "Confirmar revogação",
            MessageBoxButtons.YesNo, MessageBoxIcon.Warning, MessageBoxDefaultButton.Button2) != DialogResult.Yes) return;
        await servico!.Revogar(sala);
        await Atualizar();
    }

    private async Task Exportar()
    {
        if (servico is null) throw new ArgumentException("Atualize as salas primeiro.");
        var dia = HorarioEscola.Dia(DateTimeOffset.UtcNow);
        var lista = await servico.AcoesDoDia(dia);
        using var dialogo = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = $"acoes-{dia:yyyy-MM-dd}.csv", DefaultExt = "csv" };
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;
        File.WriteAllText(dialogo.FileName, ExportacaoCsv.Gerar(lista), new System.Text.UTF8Encoding(true));
    }
}
