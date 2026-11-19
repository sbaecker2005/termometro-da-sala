namespace TermometroDaSala.Desktop;

public partial class TelaCoordenacao
{
    private readonly Label totalSalas = new() { Text = "—" };
    private readonly Label totalAlertas = new() { Text = "—" };
    private readonly Label totalAcoes = new() { Text = "—" };

    private void AplicarVisual()
    {
        SuspendLayout();
        painel.SuspendLayout();
        TemaVisual.Aplicar(this);
        ClientSize = new Size(1220, 850);
        MinimumSize = new Size(900, 650);
        AutoScroll = true;
        painel.Dock = DockStyle.Top;
        painel.Padding = new Padding(20);
        painel.Controls.Clear();
        titulo.Dispose();
        painel.RowStyles.Clear();
        painel.RowCount = 10;
        for (var i = 0; i < painel.RowCount; i++)
            painel.RowStyles.Add(new RowStyle(i is 5 or 8 ? SizeType.Percent : SizeType.AutoSize, i == 5 ? 62 : 38));

        var cabecalho = TemaVisual.Cabecalho("Termômetro da Sala", "COORDENAÇÃO  /  Monitoramento e resposta térmica");
        painel.Controls.Add(cabecalho, 0, 0);
        status.Dock = DockStyle.Fill;
        status.BackColor = Color.White;
        status.Padding = new Padding(14, 10, 14, 10);
        status.Margin = new Padding(0, 0, 0, 12);
        status.Font = new Font(Font, FontStyle.Bold);
        painel.Controls.Add(status, 0, 1);

        var resumo = new TableLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 3, Margin = new Padding(0, 0, 0, 12) };
        for (var i = 0; i < 3; i++) resumo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100F / 3));
        resumo.Controls.Add(Cartao("SALAS MONITORADAS", "ambientes na origem atual", totalSalas, TemaVisual.Turquesa), 0, 0);
        resumo.Controls.Add(Cartao("ALERTAS TÉRMICOS", "nível atual diferente de Normal", totalAlertas, Color.FromArgb(180, 91, 22)), 1, 0);
        resumo.Controls.Add(Cartao("AÇÕES DO DIA", "registros de hoje · São Paulo", totalAcoes, TemaVisual.Turquesa), 2, 0);
        painel.Controls.Add(resumo, 0, 2);

        comandos.Margin = new Padding(0, 0, 0, 12);
        foreach (var botao in comandos.Controls.OfType<Button>())
            TemaVisual.Botao(botao, botao == botaoAtualizar || botao.Text == "&Mapa térmico", botao == botaoRevogar);
        painel.Controls.Add(comandos, 0, 3);
        legenda.Margin = new Padding(0, 0, 0, 10);
        rotuloLegenda.Text = "NÍVEIS";
        rotuloLegenda.ForeColor = TemaVisual.Secundario;
        var dicas = new ToolTip(components);
        foreach (var rotulo in legenda.Controls.OfType<Label>().Where(l => l != rotuloLegenda))
        {
            dicas.SetToolTip(rotulo, rotulo.Text);
            rotulo.AccessibleDescription = rotulo.Text;
            rotulo.Text = rotulo.Text.Split(':')[0];
            rotulo.BorderStyle = BorderStyle.None;
            rotulo.Padding = new Padding(8, 4, 8, 4);
            rotulo.Margin = new Padding(0, 0, 8, 4);
        }
        painel.Controls.Add(legenda, 0, 4);
        TemaVisual.Grade(salas);
        float[] pesos = [65, 135, 120, 140, 155, 270];
        for (var i = 0; i < salas.Columns.Count; i++) salas.Columns[i].FillWeight = pesos[i];
        painel.Controls.Add(salas, 0, 5);

        rotuloRegistro.Font = new Font("Segoe UI", 12F, FontStyle.Bold);
        rotuloRegistro.Margin = new Padding(0, 0, 0, 8);
        painel.Controls.Add(rotuloRegistro, 0, 6);
        registro.BackColor = Color.White;
        registro.Padding = new Padding(12);
        registro.Margin = new Padding(0, 0, 0, 12);
        descricao.PlaceholderText = "Descreva a ação tomada (5 a 300 caracteres)";
        descricao.Width = 345;
        nivel.Width = 165;
        TemaVisual.Botao(botaoRegistrar, principal: true);
        painel.Controls.Add(registro, 0, 7);
        TemaVisual.Grade(acoes);
        colunaAcaoSala.FillWeight = 70;
        colunaAcaoNivel.FillWeight = 110;
        colunaAcaoDescricao.FillWeight = 430;
        colunaAcaoHora.FillWeight = 130;
        painel.Controls.Add(acoes, 0, 8);
        rodape.ForeColor = TemaVisual.Secundario;
        rodape.Dock = DockStyle.Fill;
        rodape.Margin = Padding.Empty;
        painel.Controls.Add(rodape, 0, 9);
        ClientSizeChanged += (_, _) => AjustarPainel();
        painel.ResumeLayout(true);
        ResumeLayout(true);
        AjustarPainel();
    }

    private void AjustarPainel()
    {
        // O conteúdo continua acessível por rolagem em janelas menores e com escala de tela maior.
        var alturaMinima = (int)Math.Ceiling(760 * DeviceDpi / 96F);
        painel.Height = Math.Max(ClientSize.Height, alturaMinima);
    }

    private static Control Cartao(string titulo, string detalhe, Label numero, Color destaque)
    {
        var cartao = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 2, RowCount = 2,
            BackColor = Color.White, Padding = new Padding(14, 10, 14, 10), Margin = new Padding(0, 0, 10, 0)
        };
        cartao.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        cartao.ColumnStyles.Add(new ColumnStyle(SizeType.AutoSize));
        cartao.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        cartao.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        cartao.Controls.Add(new Label { Text = titulo, AutoSize = true, Dock = DockStyle.Fill,
            ForeColor = TemaVisual.Secundario, Font = new Font("Segoe UI", 9F, FontStyle.Bold), Margin = Padding.Empty }, 0, 0);
        cartao.Controls.Add(new Label { Text = detalhe, AutoSize = true, Dock = DockStyle.Fill,
            ForeColor = TemaVisual.Secundario, Font = new Font("Segoe UI", 8.5F), Margin = new Padding(0, 4, 0, 0) }, 0, 1);
        numero.AutoSize = true;
        numero.ForeColor = destaque;
        numero.Font = new Font("Segoe UI", 27F, FontStyle.Bold);
        numero.Margin = new Padding(10, 0, 0, 0);
        numero.AccessibleName = titulo;
        cartao.Controls.Add(numero, 1, 0);
        cartao.SetRowSpan(numero, 2);
        return cartao;
    }
}
