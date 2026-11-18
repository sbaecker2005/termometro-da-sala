namespace TermometroDaSala.Desktop;

/// <summary>Identidade visual comum às três telas, usando controles nativos do Windows Forms.</summary>
internal static class TemaVisual
{
    internal static readonly Color Azul = Color.FromArgb(16, 38, 58);
    internal static readonly Color Turquesa = Color.FromArgb(0, 112, 119);
    internal static readonly Color Fundo = Color.FromArgb(239, 244, 248);
    internal static readonly Color Texto = Color.FromArgb(27, 47, 65);
    internal static readonly Color Secundario = Color.FromArgb(83, 104, 121);
    internal static readonly Color Borda = Color.FromArgb(210, 224, 232);

    internal static void Aplicar(Form tela)
    {
        tela.AutoScaleMode = AutoScaleMode.Dpi;
        tela.AutoScaleDimensions = new SizeF(96, 96);
        tela.Font = new Font("Segoe UI", 10F);
        tela.BackColor = Fundo;
        tela.ForeColor = Texto;
    }

    internal static void Botao(Button botao, bool principal = false, bool cuidado = false)
    {
        botao.FlatStyle = FlatStyle.Flat;
        botao.FlatAppearance.BorderSize = principal ? 0 : 1;
        botao.FlatAppearance.BorderColor = cuidado ? Color.FromArgb(193, 138, 138) : Borda;
        botao.BackColor = principal ? Turquesa : Color.White;
        botao.ForeColor = principal ? Color.White : cuidado ? Color.FromArgb(144, 42, 42) : Texto;
        botao.Font = new Font("Segoe UI", 10F, FontStyle.Bold);
        botao.Padding = new Padding(12, 7, 12, 7);
        botao.Margin = new Padding(0, 0, 10, 0);
        botao.AutoSize = true;
        botao.Cursor = Cursors.Hand;
        botao.UseVisualStyleBackColor = false;
    }

    internal static void Grade(DataGridView grade)
    {
        grade.BorderStyle = BorderStyle.None;
        grade.BackgroundColor = Color.White;
        grade.GridColor = Borda;
        grade.CellBorderStyle = DataGridViewCellBorderStyle.SingleHorizontal;
        grade.ColumnHeadersBorderStyle = DataGridViewHeaderBorderStyle.None;
        grade.EnableHeadersVisualStyles = false;
        grade.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Azul, ForeColor = Color.White, SelectionBackColor = Azul,
            SelectionForeColor = Color.White, Font = new Font("Segoe UI", 9.5F, FontStyle.Bold),
            Padding = new Padding(10, 8, 10, 8), WrapMode = DataGridViewTriState.True
        };
        grade.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.AutoSize;
        grade.DefaultCellStyle = new DataGridViewCellStyle
        {
            ForeColor = Texto, BackColor = Color.White, SelectionBackColor = Color.FromArgb(214, 238, 239),
            SelectionForeColor = Texto, Padding = new Padding(10, 8, 10, 8),
            Font = new Font("Segoe UI", 9.5F), WrapMode = DataGridViewTriState.True
        };
        grade.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 250, 252);
        grade.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        grade.RowTemplate.MinimumHeight = 40;
        grade.Margin = new Padding(0, 0, 0, 12);
    }

    internal static Panel Cabecalho(string titulo, string subtitulo)
    {
        var painel = new TableLayoutPanel
        {
            Dock = DockStyle.Fill, AutoSize = true, ColumnCount = 1, RowCount = 2,
            BackColor = Azul, Padding = new Padding(20, 14, 20, 14), Margin = Padding.Empty
        };
        painel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
        painel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        painel.RowStyles.Add(new RowStyle(SizeType.AutoSize));
        painel.Controls.Add(new Label
        {
            Text = titulo, AutoSize = true, Dock = DockStyle.Fill, ForeColor = Color.White,
            Font = new Font("Segoe UI", 21F, FontStyle.Bold), Margin = Padding.Empty
        }, 0, 0);
        painel.Controls.Add(new Label
        {
            Text = subtitulo, AutoSize = true, Dock = DockStyle.Fill,
            ForeColor = Color.FromArgb(164, 222, 225), Margin = new Padding(0, 4, 0, 0)
        }, 0, 1);
        return painel;
    }
}
