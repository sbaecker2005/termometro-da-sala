namespace TermometroDaSala.Desktop;

public partial class Form1 : Form
{
    private readonly IServicoSalas api;
    private readonly IServicoSalas local;
    private IServicoSalas? servico;
    private readonly Label status = new() { AutoSize = true, Text = "Carregando salas…" };
    private readonly DataGridView salas = Grade("Salas");
    private readonly DataGridView acoes = Grade("Ações do dia");
    private readonly ComboBox nivel = new() { DropDownStyle = ComboBoxStyle.DropDownList, Width = 160 };
    private readonly TextBox descricao = new() { Width = 320, MaxLength = 300, AccessibleName = "Descrição da ação" };
    private readonly FlowLayoutPanel comandos = new() { AutoSize = true, Dock = DockStyle.Fill };
    private readonly FlowLayoutPanel registro = new() { AutoSize = true, Dock = DockStyle.Fill };
    private readonly System.Windows.Forms.Timer timer;
    private readonly ErrorProvider erros;
    private bool ocupado;

    public Form1(IServicoSalas api, IServicoSalas local)
    {
        InitializeComponent();
        this.api = api;
        this.local = local;
        Text = "Termômetro da Sala — Coordenação";
        ClientSize = new Size(1150, 760);
        MinimumSize = new Size(850, 600);
        erros = new ErrorProvider(components!) { ContainerControl = this, BlinkStyle = ErrorBlinkStyle.NeverBlink };
        timer = new System.Windows.Forms.Timer(components!) { Interval = 30_000 };
        var painel = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 7, Padding = new Padding(12) };
        painel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        painel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        painel.RowStyles.Add(new RowStyle(SizeType.Percent, 55));
        painel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        painel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        painel.RowStyles.Add(new RowStyle(SizeType.Percent, 45));
        painel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        Controls.Add(painel);
        painel.Controls.Add(status, 0, 0);
        painel.Controls.Add(comandos, 0, 1);
        Botao(comandos, "&Atualizar", Atualizar);
        Botao(comandos, "&Cadastrar sala", Cadastrar);
        Botao(comandos, "Re&vogar sensor", Revogar);
        Botao(comandos, "&Exportar dia (CSV)", Exportar);
        salas.Columns.Add("Id", "Sala");
        salas.Columns.Add("Nome", "Nome");
        salas.Columns.Add("Atual", "Nível atual");
        salas.Columns.Add("Leitura", "Última leitura (SP)");
        salas.Columns.Add("Previsto", "Nível previsto (+2 h)");
        salas.Columns.Add("Avisos", "Avisos / sensor");
        painel.Controls.Add(salas, 0, 2);
        painel.Controls.Add(new Label { Text = "Registrar ação para a sala selecionada", AutoSize = true }, 0, 3);
        nivel.DataSource = Enum.GetValues<NivelCalor>().Select(n => new { Valor = n, Texto = Validacoes.Texto(n) }).ToList();
        nivel.DisplayMember = "Texto";
        nivel.ValueMember = "Valor";
        registro.Controls.Add(new Label { Text = "&Nível:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
        registro.Controls.Add(nivel);
        registro.Controls.Add(new Label { Text = "&Descrição:", AutoSize = true, Padding = new Padding(0, 6, 0, 0) });
        registro.Controls.Add(descricao);
        Botao(registro, "&Registrar ação", Registrar);
        for (var i = 0; i < registro.Controls.Count; i++) registro.Controls[i].TabIndex = i;
        painel.Controls.Add(registro, 0, 4);
        acoes.Columns.Add("Sala", "Sala");
        acoes.Columns.Add("Nivel", "Nível");
        acoes.Columns.Add("Descricao", "Descrição");
        acoes.Columns.Add("Hora", "Hora (São Paulo)");
        painel.Controls.Add(acoes, 0, 5);
        painel.Controls.Add(new Label { AutoSize = true, Text = "Ações do dia de São Paulo (UTC−3). Dados locais não são sincronizados com a API." }, 0, 6);
        for (var i = 0; i < painel.Controls.Count; i++) painel.Controls[i].TabIndex = i;
        Shown += async (_, _) => { await Executar(Atualizar); timer.Start(); };
        timer.Tick += async (_, _) => await Executar(Atualizar);
    }

    private static DataGridView Grade(string nome) => new()
    {
        Dock = DockStyle.Fill, ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
        MultiSelect = false, SelectionMode = DataGridViewSelectionMode.FullRowSelect,
        AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.Fill, RowHeadersVisible = false,
        AccessibleName = nome, BackgroundColor = SystemColors.Window
    };

    private void Botao(FlowLayoutPanel painel, string texto, Func<Task> acao)
    {
        var botao = new Button { Text = texto, AutoSize = true, TabIndex = painel.Controls.Count };
        botao.Click += async (_, _) => await Executar(acao);
        painel.Controls.Add(botao);
    }

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
            status.Text = "MODO LOCAL — API indisponível. Registros só neste computador; revogação sem efeito remoto.";
            status.ForeColor = Color.DarkRed;
        }
        if (anterior is not null && anterior != servico)
            status.Text += " A origem dos dados mudou.";
        foreach (var sala in lista)
        {
            Previsao? previsao = null;
            string aviso = servico == local ? "Sem dados do sensor" : "";
            try
            {
                previsao = await servico.Prever(sala.Id);
                if (previsao is null && servico != local) aviso = "Sem dados para prever: sem previsão da rua e sem leitura válida recente da sala";
            }
            catch (HttpRequestException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
            { aviso = "Sala não encontrada na API"; }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException)
            { aviso = "Previsão indisponível"; }
            if (previsao?.Demonstracao == true) aviso += " DEMONSTRAÇÃO";
            if (previsao?.RuaDisponivel == false) aviso += " Previsão da rua indisponível; previsão usa a última leitura válida da sala";
            else if (previsao?.SemLeiturasRecentes == true) aviso += " Sala sem leituras recentes; previsão da rua";
            if (sala.SensorRevogado) aviso += " Sensor revogado";
            if (sala.UltimaLeituraEm is { } hora && DateTimeOffset.UtcNow - hora > TimeSpan.FromMinutes(10)) aviso += " Leitura antiga";
            var linha = salas.Rows[salas.Rows.Add(sala.Id, sala.Nome,
                sala.Leitura is { } atual ? Validacoes.Texto(atual.Nivel) : "Sem leitura",
                sala.Leitura is { } l ? Validacoes.HoraEscola(l.MedidoEm).ToString("dd/MM HH:mm:ss") : "—",
                previsao is null ? "Sem previsão" : $"{Validacoes.Texto(previsao.NivelPrevisto)} às {Validacoes.HoraEscola(previsao.PrevistoPara):HH:mm}", aviso)];
            linha.Tag = sala;
            Cor(linha.Cells["Atual"], sala.Leitura?.Nivel);
            Cor(linha.Cells["Previsto"], previsao?.NivelPrevisto);
            if (sala.Id == selecionada) salas.CurrentCell = linha.Cells[0];
        }
        if (lista.Count == 0) status.Text += " Nenhuma sala cadastrada.";
        await CarregarAcoes();
    }

    private static void Cor(DataGridViewCell celula, NivelCalor? nivel)
    {
        celula.Style.BackColor = nivel switch
        {
            NivelCalor.normal => Color.LightGreen, NivelCalor.atencao => Color.Khaki,
            NivelCalor.cuidado => Color.LightSalmon, NivelCalor.perigo => Color.LightPink, _ => SystemColors.Window
        };
        celula.Style.ForeColor = Color.Black;
    }

    private Sala Selecionada() => salas.CurrentRow?.Tag as Sala ?? throw new ArgumentException("Selecione uma sala na grade.");

    private async Task Cadastrar()
    {
        if (servico is null) throw new ArgumentException("Atualize as salas primeiro.");
        using var cadastro = new CadastroSala();
        if (cadastro.ShowDialog(this) != DialogResult.OK) return;
        await servico.Cadastrar(cadastro.Sala);
        await Atualizar();
    }

    private async Task Registrar()
    {
        var sala = Selecionada();
        erros.SetError(descricao, Validacoes.Descricao(descricao.Text) ?? "");
        if (Validacoes.Descricao(descricao.Text) is not null) { descricao.Focus(); return; }
        await servico!.Registrar(new Acao { AmbienteId = sala.Id, Nivel = (NivelCalor)nivel.SelectedValue!, Descricao = descricao.Text.Trim() });
        descricao.Clear();
        await CarregarAcoes();
    }

    private async Task CarregarAcoes()
    {
        var lista = await servico!.AcoesDoDia(Validacoes.DiaEscola(DateTimeOffset.UtcNow));
        acoes.Rows.Clear();
        foreach (var acao in lista.OrderByDescending(a => a.RegistradoEm))
            acoes.Rows.Add(acao.AmbienteId, Validacoes.Texto(acao.Nivel), acao.Descricao, Validacoes.HoraEscola(acao.RegistradoEm).ToString("HH:mm:ss"));
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
        var dia = Validacoes.DiaEscola(DateTimeOffset.UtcNow);
        var lista = await servico.AcoesDoDia(dia);
        using var dialogo = new SaveFileDialog { Filter = "CSV (*.csv)|*.csv", FileName = $"acoes-{dia:yyyy-MM-dd}.csv", DefaultExt = "csv" };
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;
        File.WriteAllText(dialogo.FileName, ExportacaoCsv.Gerar(lista), new System.Text.UTF8Encoding(true));
    }
}

public sealed class CadastroSala : Form
{
    public Sala Sala { get; private set; } = new();
    private readonly TextBox id = new() { AccessibleName = "Identificador da sala" };
    private readonly TextBox nome = new() { AccessibleName = "Nome da sala" };
    private readonly NumericUpDown andar = new() { Minimum = 0, Maximum = 20, AccessibleName = "Andar" };
    private readonly ComboBox face = new() { DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Face do sol" };
    private readonly CheckBox ventilador = new() { Text = "Tem &ventilador", AutoSize = true };
    private readonly ErrorProvider erros;

    public CadastroSala()
    {
        Text = "Cadastrar sala";
        ClientSize = new Size(460, 300);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = MinimizeBox = false;
        StartPosition = FormStartPosition.CenterParent;
        erros = new ErrorProvider { ContainerControl = this, BlinkStyle = ErrorBlinkStyle.NeverBlink };
        face.DataSource = Validacoes.Faces;
        var tabela = new TableLayoutPanel { Dock = DockStyle.Fill, Padding = new Padding(15), ColumnCount = 2, RowCount = 7 };
        tabela.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 40));
        tabela.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 60));
        Controls.Add(tabela);
        string[] rotulos = ["&ID (sala-NN):", "&Nome:", "&Andar (0–20):", "&Face do sol:"];
        Control[] campos = [id, nome, andar, face];
        for (var i = 0; i < campos.Length; i++)
        {
            tabela.Controls.Add(new Label { Text = rotulos[i], AutoSize = true, TabIndex = i * 2 }, 0, i);
            campos[i].TabIndex = i * 2 + 1;
            campos[i].Dock = DockStyle.Fill;
            tabela.Controls.Add(campos[i], 1, i);
        }
        ventilador.TabIndex = 8;
        tabela.Controls.Add(ventilador, 1, 4);
        var salvar = new Button { Text = "&Salvar", AutoSize = true, TabIndex = 9 };
        var cancelar = new Button { Text = "&Cancelar", DialogResult = DialogResult.Cancel, AutoSize = true, TabIndex = 10 };
        tabela.Controls.Add(salvar, 0, 5);
        tabela.Controls.Add(cancelar, 1, 5);
        AcceptButton = salvar;
        CancelButton = cancelar;
        salvar.Click += (_, _) =>
        {
            erros.Clear();
            Sala = new Sala { Id = id.Text.Trim(), Nome = nome.Text.Trim(), Andar = (int)andar.Value,
                FaceSol = face.Text, TemVentilador = ventilador.Checked };
            var problemas = Validacoes.Sala(Sala);
            var controles = new Dictionary<string, Control> { [nameof(Sala.Id)] = id, [nameof(Sala.Nome)] = nome,
                [nameof(Sala.Andar)] = andar, [nameof(Sala.FaceSol)] = face };
            foreach (var problema in problemas) erros.SetError(controles[problema.Key], problema.Value);
            if (problemas.Count == 0) DialogResult = DialogResult.OK;
            else controles[problemas.First().Key].Focus();
        };
    }

    protected override void Dispose(bool disposing)
    {
        if (disposing) erros.Dispose();
        base.Dispose(disposing);
    }
}
