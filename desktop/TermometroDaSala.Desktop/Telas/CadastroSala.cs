namespace TermometroDaSala.Desktop;

/// <summary>Diálogo de cadastro. Quem valida é Sala.Criar; a tela só marca cada erro no campo dele.</summary>
public partial class CadastroSala : Form
{
    public Sala? Sala { get; private set; }

    public CadastroSala()
    {
        InitializeComponent();
        AplicarVisual();
        Icon = TelaCoordenacao.Icone();
        face.DataSource = Sala.Faces;
    }

    private void AplicarVisual()
    {
        SuspendLayout();
        TemaVisual.Aplicar(this);
        ClientSize = new Size(560, 470);
        var estrutura = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 3 };
        estrutura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estrutura.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        estrutura.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        estrutura.Controls.Add(TemaVisual.Cabecalho("Cadastrar sala", "Identifique o ambiente e suas características."), 0, 0);
        tabela.BackColor = Color.White;
        tabela.Padding = new Padding(20, 18, 28, 12);
        tabela.Margin = new Padding(18, 18, 18, 10);
        foreach (Control controle in tabela.Controls)
        {
            controle.Margin = new Padding(3, 8, 10, 8);
            if (controle is Label rotulo) rotulo.Anchor = AnchorStyles.Left;
        }
        id.PlaceholderText = "sala-05";
        nome.PlaceholderText = "Ex.: Sala de aula 5";
        TemaVisual.Botao(salvar, principal: true);
        TemaVisual.Botao(cancelar);
        estrutura.Controls.Add(tabela, 0, 1);
        estrutura.Controls.Add(new Label
        {
            Text = "Enter para salvar · Esc para cancelar", AutoSize = true, Dock = DockStyle.Fill,
            ForeColor = TemaVisual.Secundario, Padding = new Padding(20, 0, 20, 16)
        }, 0, 2);
        Controls.Add(estrutura);
        ResumeLayout(true);
    }

    private void AoSalvar(object? sender, EventArgs e) => Salvar();

    /// <summary>Monta a sala com o que foi digitado; devolve false e marca os campos quando o cadastro é recusado.</summary>
    internal bool Salvar()
    {
        erros.Clear();
        try
        {
            Sala = Sala.Criar(id.Text.Trim(), nome.Text, (int)andar.Value, face.Text, ventilador.Checked);
            DialogResult = DialogResult.OK;
            return true;
        }
        catch (CadastroInvalidoException ex)
        {
            var controles = new Dictionary<string, Control> { [nameof(Sala.Id)] = id, [nameof(Sala.Nome)] = nome,
                [nameof(Sala.Andar)] = andar, [nameof(Sala.FaceSol)] = face };
            foreach (var (campo, mensagem) in ex.Erros) erros.SetError(controles[campo], mensagem);
            controles[ex.Erros.Keys.First()].Focus();
            return false;
        }
    }
}
