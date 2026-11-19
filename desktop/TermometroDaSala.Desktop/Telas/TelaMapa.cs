using System.Drawing.Imaging;

namespace TermometroDaSala.Desktop;

public sealed class TelaMapa : Form
{
    private readonly Func<bool, Task<Mapa?>> carregar;
    private Mapa? mapa;
    private readonly ComboBox periodo = new() { DropDownStyle = ComboBoxStyle.DropDownList, AccessibleName = "Período do mapa", Width = 170 };
    private readonly Panel desenho = new() { Dock = DockStyle.Fill, BackColor = Color.White };
    private readonly TextBox lista = new() { Dock = DockStyle.Right, Width = 340, Multiline = true, ReadOnly = true, ScrollBars = ScrollBars.Vertical, AccessibleName = "Pontos, legenda e precauções" };
    private readonly Label aviso = new() { Dock = DockStyle.Bottom, AutoSize = true };
    private readonly FlowLayoutPanel legenda = new() { Dock = DockStyle.Top, AutoSize = true };

    public TelaMapa(string nome, Func<bool, Task<Mapa?>> carregar)
    {
        this.carregar = carregar;
        TemaVisual.Aplicar(this);
        Text = $"Mapa térmico — {nome}";
        Size = new(1160, 780);
        MinimumSize = new(900, 600);
        StartPosition = FormStartPosition.CenterParent;
        Icon = TelaCoordenacao.Icone();
        var estrutura = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 5, Padding = new Padding(18)
        };
        estrutura.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        for (var i = 0; i < 5; i++) estrutura.RowStyles.Add(new RowStyle(i == 3 ? SizeType.Percent : SizeType.AutoSize, 100));
        estrutura.Controls.Add(TemaVisual.Cabecalho("Mapa térmico", nome + " · Distribuição do conforto na sala"), 0, 0);
        var comandos = new FlowLayoutPanel { Dock = DockStyle.Fill, AutoSize = true, Margin = new Padding(0, 12, 0, 12) };
        var exportar = new Button { Text = "&Exportar PNG", AutoSize = true };
        TemaVisual.Botao(exportar, principal: true);
        periodo.Items.AddRange(["Agora", "Previsão +1 h"]);
        periodo.Width = 190;
        periodo.Margin = new Padding(0, 7, 18, 0);
        comandos.Controls.AddRange([new Label { Text = "&Período", AutoSize = true, Margin = new Padding(0, 10, 8, 0) }, periodo, exportar]);
        foreach (var nivel in new NivelCalor?[] { NivelCalor.Frio, NivelCalor.Normal, NivelCalor.Atencao, NivelCalor.Cuidado, NivelCalor.Perigo, null })
            legenda.Controls.Add(new Label { Text = nivel?.Texto() ?? "Sem leitura", BackColor = Mapa.Cor(nivel),
                ForeColor = Color.Black, AutoSize = true, Padding = new Padding(8, 4, 8, 4), Margin = new Padding(0, 0, 8, 4) });
        legenda.Controls.Add(new Label { Text = "○ S: sensor   □ C: cadeira", AutoSize = true, Padding = new Padding(5, 3, 5, 3) });
        legenda.Dock = DockStyle.Fill;
        legenda.Margin = new Padding(0, 0, 0, 10);
        var conteudo = new TableLayoutPanel { Dock = DockStyle.Fill, ColumnCount = 2, RowCount = 1, Margin = Padding.Empty };
        conteudo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 64));
        conteudo.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 36));
        conteudo.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        desenho.Margin = new Padding(0, 0, 12, 0);
        var lateral = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2, BackColor = Color.White,
            Padding = new Padding(14), Margin = Padding.Empty
        };
        lateral.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        lateral.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        lateral.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
        lateral.Controls.Add(new Label { Text = "PONTOS E PRECAUÇÕES", AutoSize = true,
            ForeColor = TemaVisual.Turquesa, Font = new Font(Font, FontStyle.Bold), Margin = new Padding(0, 0, 0, 12) }, 0, 0);
        lista.Dock = DockStyle.Fill;
        lista.BorderStyle = BorderStyle.None;
        lista.BackColor = Color.White;
        lista.ForeColor = TemaVisual.Texto;
        lateral.Controls.Add(lista, 0, 1);
        conteudo.Controls.Add(desenho, 0, 0);
        conteudo.Controls.Add(lateral, 1, 0);
        aviso.Dock = DockStyle.Fill;
        aviso.Padding = new Padding(0, 12, 0, 0);
        aviso.ForeColor = TemaVisual.Secundario;
        estrutura.Controls.Add(comandos, 0, 1);
        estrutura.Controls.Add(legenda, 0, 2);
        estrutura.Controls.Add(conteudo, 0, 3);
        estrutura.Controls.Add(aviso, 0, 4);
        Controls.Add(estrutura);
        desenho.Paint += (_, e) => Desenhar(e.Graphics, desenho.ClientRectangle);
        desenho.Resize += (_, _) => desenho.Invalidate();
        periodo.SelectedIndexChanged += async (_, _) => await Atualizar();
        exportar.Click += (_, _) => Exportar();
        Shown += (_, _) => periodo.SelectedIndex = 0;
    }

    private async Task Atualizar()
    {
        periodo.Enabled = false;
        mapa = null;
        lista.Text = "";
        aviso.Text = "Carregando mapa…";
        desenho.Invalidate();
        try
        {
            mapa = await carregar(periodo.SelectedIndex == 1);
            aviso.Text = mapa is null ? "Previsão +1 h por ponto indisponível nesta origem de dados." : $"{periodo.Text} — planta {mapa.LarguraM} × {mapa.ProfundidadeM} m; origem no canto inferior esquerdo.";
            lista.Text = mapa is null ? "" : $"{mapa.Pontos.Count(p => p.Tipo == "lugar")} cadeiras · {mapa.Pontos.Count(p => p.Tipo == "sensor")} sensores\r\nCírculo = sensor; quadrado = cadeira. Azul = Frio; verde = Normal; amarelo = Atenção; laranja = Cuidado extremo; vermelho = Perigo; cinza = sem leitura.\r\nSímbolo maior = maior desvio absoluto da média (limite visual: 8 °C).\r\nC identifica cadeiras; S identifica sensores. Os mesmos códigos aparecem na planta e nesta lista.\r\n\r\n"
                + string.Join("\r\n", mapa.PontosIdentificados().Select(p => $"{p.Codigo} — {p.Ponto.Texto}")) + "\r\n\r\nPrecauções:\r\n" + string.Join("\r\n", mapa.Precaucoes.Select(p => "• " + p));
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or System.Text.Json.JsonException or IOException or ArgumentException)
        { aviso.Text = "Mapa indisponível: " + ex.Message; }
        finally { periodo.Enabled = true; desenho.Invalidate(); }
    }

    private void Desenhar(Graphics g, Rectangle area)
    {
        if (mapa is null) return;
        var margem = new RectangleF(35, 35, Math.Max(1, area.Width - 70), Math.Max(1, area.Height - 70));
        PointF Tela(double x, double y) => Mapa.ParaTela(x, y, mapa.LarguraM ?? 0, mapa.ProfundidadeM ?? 0, margem);
        var inicio = Tela(0, mapa.ProfundidadeM ?? 0);
        var fim = Tela(mapa.LarguraM ?? 0, 0);
        var planta = RectangleF.FromLTRB(inicio.X, inicio.Y, fim.X, fim.Y);
        foreach (var c in mapa.Grade)
        {
            var a = Tela(Math.Max(0, c.X - mapa.PassoM / 2), Math.Min(mapa.ProfundidadeM ?? 0, c.Y + mapa.PassoM / 2));
            var b = Tela(Math.Min(mapa.LarguraM ?? 0, c.X + mapa.PassoM / 2), Math.Max(0, c.Y - mapa.PassoM / 2));
            using var cor = new SolidBrush(Mapa.Cor(c.Nivel));
            g.FillRectangle(cor, a.X, a.Y, b.X - a.X, b.Y - a.Y);
        }
        g.DrawRectangle(Pens.Black, planta.X, planta.Y, planta.Width, planta.Height);
        foreach (var p in mapa.Pontos)
        {
            var pos = Tela(p.X, p.Y);
            var d = Math.Max(26, Mapa.Diametro(p.DesvioC));
            using var cor = new SolidBrush(Mapa.Cor(p.SemLeitura ? null : p.Nivel));
            if (p.Tipo == "lugar")
            {
                d = Math.Max(d, 34);
                g.FillRectangle(cor, pos.X - d / 2, pos.Y - d / 2, d, d);
                g.DrawRectangle(Pens.Black, pos.X - d / 2, pos.Y - d / 2, d, d);
            }
            else
            {
                g.FillEllipse(cor, pos.X - d / 2, pos.Y - d / 2, d, d);
                g.DrawEllipse(Pens.Black, pos.X - d / 2, pos.Y - d / 2, d, d);
            }
        }
        // shortcut: posições distintas muito próximas podem sobrepor códigos; distribuir rótulos se houver plantas densas.
        foreach (var grupo in mapa.PontosIdentificados().GroupBy(p => (p.Ponto.X, p.Ponto.Y)))
        {
            var ponto = grupo.First().Ponto;
            var pos = Tela(ponto.X, ponto.Y);
            var texto = string.Join("\n", grupo.Select(p => p.Codigo));
            var tamanho = g.MeasureString(texto, Font);
            var rotulo = new RectangleF(pos.X - tamanho.Width / 2, pos.Y - tamanho.Height / 2, tamanho.Width, tamanho.Height);
            g.FillRectangle(Brushes.White, rotulo);
            g.DrawString(texto, Font, Brushes.Black, rotulo.Location);
        }
    }

    private void Exportar()
    {
        if (mapa is null) return;
        using var dialogo = new SaveFileDialog { Filter = "PNG (*.png)|*.png", DefaultExt = "png", FileName = "mapa-termico.png" };
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            var largura = Math.Max(1000, ClientSize.Width);
            var larguraLista = Math.Max(340, lista.Width);
            var larguraTexto = larguraLista - 24;
            using var medida = CreateGraphics();
            using var titulo = new Font(Font, FontStyle.Bold);
            var alturaTitulo = (int)Math.Ceiling(medida.MeasureString(Text, titulo, largura - 24).Height);
            var alturaAviso = (int)Math.Ceiling(medida.MeasureString(aviso.Text, Font, largura - 24).Height);
            var topoLegenda = 24 + alturaTitulo + alturaAviso;
            var topo = topoLegenda + legenda.Height + 12;
            var alturaTexto = (int)Math.Ceiling(medida.MeasureString(lista.Text, lista.Font, larguraTexto).Height);
            var alturaPlanta = Math.Max(420, desenho.Height);
            var altura = topo + Math.Max(alturaTexto, alturaPlanta) + 40;
            using var imagem = new Bitmap(largura, altura);
            using var g = Graphics.FromImage(imagem);
            g.Clear(Color.White);
            g.DrawString(Text, titulo, Brushes.Black, new RectangleF(12, 8, largura - 24, alturaTitulo + 4));
            g.DrawString(aviso.Text, Font, Brushes.Black, new RectangleF(12, 12 + alturaTitulo, largura - 24, alturaAviso + 4));
            legenda.DrawToBitmap(imagem, new Rectangle(12, topoLegenda, legenda.Width, legenda.Height));
            var estado = g.Save();
            g.TranslateTransform(0, topo);
            Desenhar(g, new Rectangle(0, 0, largura - larguraLista, alturaPlanta));
            g.Restore(estado);
            g.DrawString(lista.Text, lista.Font, Brushes.Black,
                new RectangleF(largura - larguraLista + 12, topo, larguraTexto, alturaTexto + 12));
            g.DrawString("Termômetro da Sala · C = cadeira · S = sensor", Font, Brushes.DimGray, 12, altura - 24);
            imagem.Save(dialogo.FileName, ImageFormat.Png);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.ExternalException)
        { MessageBox.Show(this, ex.Message, "Não foi possível exportar"); }
    }
}
