namespace TermometroDaSala.Desktop;

/// <summary>Diálogo de cadastro. Quem valida é Sala.Criar; a tela só marca cada erro no campo dele.</summary>
public partial class CadastroSala : Form
{
    public Sala? Sala { get; private set; }

    public CadastroSala()
    {
        InitializeComponent();
        Icon = TelaCoordenacao.Icone();
        face.DataSource = Sala.Faces;
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
