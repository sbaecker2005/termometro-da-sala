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

    public TelaMapa(string nome, Func<bool, Task<Mapa?>> carregar)
    {
        this.carregar = carregar;
        Text = $"Mapa térmico — {nome}";
        Size = new(1100, 700);
        MinimumSize = new(850, 550);
        var comandos = new FlowLayoutPanel { Dock = DockStyle.Top, AutoSize = true };
        var exportar = new Button { Text = "Exportar PNG", AutoSize = true };
        periodo.Items.AddRange(["Agora", "Previsão +1 h"]);
        comandos.Controls.AddRange([periodo, exportar]);
        Controls.AddRange([desenho, lista, aviso, comandos]);
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
            lista.Text = mapa is null ? "" : "Legenda: azul = Frio; verde = Normal; amarelo = Atenção; vermelho = Cuidado extremo / Perigo; cinza = sem leitura.\r\nBola maior = maior desvio absoluto da média (limite visual: 8 °C).\r\n\r\n"
                + string.Join("\r\n", mapa.Pontos.Select(p => p.Texto)) + "\r\n\r\nPrecauções:\r\n" + string.Join("\r\n", mapa.Precaucoes.Select(p => "• " + p));
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
            var d = Mapa.Diametro(p.DesvioC);
            using var cor = new SolidBrush(Mapa.Cor(p.SemLeitura ? null : p.Nivel));
            g.FillEllipse(cor, pos.X - d / 2, pos.Y - d / 2, d, d);
            g.DrawEllipse(Pens.Black, pos.X - d / 2, pos.Y - d / 2, d, d);
            var texto = p.Texto;
            var tamanho = g.MeasureString(texto, Font);
            g.DrawString(texto, Font, Brushes.Black, Math.Clamp(pos.X, 0, Math.Max(0, area.Width - tamanho.Width)), Math.Max(0, pos.Y - d / 2 - tamanho.Height));
        }
    }

    private void Exportar()
    {
        if (mapa is null) return;
        using var dialogo = new SaveFileDialog { Filter = "PNG (*.png)|*.png", DefaultExt = "png", FileName = "mapa-termico.png" };
        if (dialogo.ShowDialog(this) != DialogResult.OK) return;
        try
        {
            using var imagem = new Bitmap(ClientSize.Width, ClientSize.Height);
            DrawToBitmap(imagem, ClientRectangle);
            imagem.Save(dialogo.FileName, ImageFormat.Png);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or System.Runtime.InteropServices.ExternalException)
        { MessageBox.Show(this, ex.Message, "Não foi possível exportar"); }
    }
}
