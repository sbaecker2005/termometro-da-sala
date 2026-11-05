namespace TermometroDaSala.Desktop;

partial class TelaCoordenacao
{
    /// <summary>
    ///  Required designer variable.
    /// </summary>
    private System.ComponentModel.IContainer components = null;

    /// <summary>
    ///  Clean up any resources being used.
    /// </summary>
    /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }
        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    /// <summary>
    ///  Required method for Designer support - do not modify
    ///  the contents of this method with the code editor.
    /// </summary>
    private void InitializeComponent()
    {
        this.components = new System.ComponentModel.Container();
        this.painel = new System.Windows.Forms.TableLayoutPanel();
        this.titulo = new System.Windows.Forms.Label();
        this.status = new System.Windows.Forms.Label();
        this.comandos = new System.Windows.Forms.FlowLayoutPanel();
        this.botaoAtualizar = new System.Windows.Forms.Button();
        this.botaoCadastrar = new System.Windows.Forms.Button();
        this.botaoRevogar = new System.Windows.Forms.Button();
        this.botaoExportar = new System.Windows.Forms.Button();
        this.legenda = new System.Windows.Forms.FlowLayoutPanel();
        this.rotuloLegenda = new System.Windows.Forms.Label();
        this.legendaNormal = new System.Windows.Forms.Label();
        this.legendaAtencao = new System.Windows.Forms.Label();
        this.legendaCuidado = new System.Windows.Forms.Label();
        this.legendaPerigo = new System.Windows.Forms.Label();
        this.legendaSemLeitura = new System.Windows.Forms.Label();
        this.salas = new System.Windows.Forms.DataGridView();
        this.colunaSala = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colunaNome = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colunaAtual = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colunaLeitura = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colunaPrevisto = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colunaAvisos = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.rotuloRegistro = new System.Windows.Forms.Label();
        this.registro = new System.Windows.Forms.FlowLayoutPanel();
        this.rotuloNivel = new System.Windows.Forms.Label();
        this.nivel = new System.Windows.Forms.ComboBox();
        this.rotuloDescricao = new System.Windows.Forms.Label();
        this.descricao = new System.Windows.Forms.TextBox();
        this.botaoRegistrar = new System.Windows.Forms.Button();
        this.acoes = new System.Windows.Forms.DataGridView();
        this.colunaAcaoSala = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colunaAcaoNivel = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colunaAcaoDescricao = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.colunaAcaoHora = new System.Windows.Forms.DataGridViewTextBoxColumn();
        this.rodape = new System.Windows.Forms.Label();
        this.timer = new System.Windows.Forms.Timer(this.components);
        this.erros = new System.Windows.Forms.ErrorProvider(this.components);
        this.painel.SuspendLayout();
        this.comandos.SuspendLayout();
        this.legenda.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.salas)).BeginInit();
        this.registro.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.acoes)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.erros)).BeginInit();
        this.SuspendLayout();
        //
        // painel
        //
        this.painel.ColumnCount = 1;
        this.painel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
        this.painel.Controls.Add(this.titulo, 0, 0);
        this.painel.Controls.Add(this.status, 0, 1);
        this.painel.Controls.Add(this.comandos, 0, 2);
        this.painel.Controls.Add(this.legenda, 0, 3);
        this.painel.Controls.Add(this.salas, 0, 4);
        this.painel.Controls.Add(this.rotuloRegistro, 0, 5);
        this.painel.Controls.Add(this.registro, 0, 6);
        this.painel.Controls.Add(this.acoes, 0, 7);
        this.painel.Controls.Add(this.rodape, 0, 8);
        this.painel.Dock = System.Windows.Forms.DockStyle.Fill;
        this.painel.Location = new System.Drawing.Point(0, 0);
        this.painel.Name = "painel";
        this.painel.Padding = new System.Windows.Forms.Padding(12);
        this.painel.RowCount = 9;
        this.painel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this.painel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this.painel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this.painel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this.painel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 55F));
        this.painel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this.painel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this.painel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 45F));
        this.painel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.AutoSize));
        this.painel.Size = new System.Drawing.Size(1150, 760);
        this.painel.TabIndex = 0;
        //
        // titulo
        //
        this.titulo.AutoSize = true;
        this.titulo.Font = new System.Drawing.Font("Segoe UI", 16F, System.Drawing.FontStyle.Bold);
        this.titulo.Margin = new System.Windows.Forms.Padding(3, 0, 3, 6);
        this.titulo.Name = "titulo";
        this.titulo.TabIndex = 0;
        this.titulo.Text = "Termômetro da Sala — Coordenação";
        //
        // status
        //
        this.status.AutoSize = true;
        this.status.Name = "status";
        this.status.TabIndex = 1;
        this.status.Text = "Carregando salas…";
        //
        // comandos
        //
        this.comandos.AutoSize = true;
        this.comandos.Controls.Add(this.botaoAtualizar);
        this.comandos.Controls.Add(this.botaoCadastrar);
        this.comandos.Controls.Add(this.botaoRevogar);
        this.comandos.Controls.Add(this.botaoExportar);
        this.comandos.Dock = System.Windows.Forms.DockStyle.Fill;
        this.comandos.Name = "comandos";
        this.comandos.TabIndex = 2;
        //
        // botaoAtualizar
        //
        this.botaoAtualizar.AutoSize = true;
        this.botaoAtualizar.Name = "botaoAtualizar";
        this.botaoAtualizar.TabIndex = 0;
        this.botaoAtualizar.Text = "&Atualizar";
        this.botaoAtualizar.Click += new System.EventHandler(this.AoAtualizar);
        //
        // botaoCadastrar
        //
        this.botaoCadastrar.AutoSize = true;
        this.botaoCadastrar.Name = "botaoCadastrar";
        this.botaoCadastrar.TabIndex = 1;
        this.botaoCadastrar.Text = "&Cadastrar sala";
        this.botaoCadastrar.Click += new System.EventHandler(this.AoCadastrar);
        //
        // botaoRevogar
        //
        this.botaoRevogar.AutoSize = true;
        this.botaoRevogar.Name = "botaoRevogar";
        this.botaoRevogar.TabIndex = 2;
        this.botaoRevogar.Text = "Re&vogar sensor";
        this.botaoRevogar.Click += new System.EventHandler(this.AoRevogar);
        //
        // botaoExportar
        //
        this.botaoExportar.AutoSize = true;
        this.botaoExportar.Name = "botaoExportar";
        this.botaoExportar.TabIndex = 3;
        this.botaoExportar.Text = "&Exportar dia (CSV)";
        this.botaoExportar.Click += new System.EventHandler(this.AoExportar);
        //
        // legenda
        //
        this.legenda.AccessibleName = "Legenda de cores";
        this.legenda.AutoSize = true;
        this.legenda.Controls.Add(this.rotuloLegenda);
        this.legenda.Controls.Add(this.legendaNormal);
        this.legenda.Controls.Add(this.legendaAtencao);
        this.legenda.Controls.Add(this.legendaCuidado);
        this.legenda.Controls.Add(this.legendaPerigo);
        this.legenda.Controls.Add(this.legendaSemLeitura);
        this.legenda.Dock = System.Windows.Forms.DockStyle.Fill;
        this.legenda.Name = "legenda";
        this.legenda.TabIndex = 3;
        //
        // rotuloLegenda
        //
        this.rotuloLegenda.AutoSize = true;
        this.rotuloLegenda.Name = "rotuloLegenda";
        this.rotuloLegenda.Padding = new System.Windows.Forms.Padding(0, 4, 0, 0);
        this.rotuloLegenda.TabIndex = 0;
        this.rotuloLegenda.Text = "Legenda:";
        //
        // legendaNormal
        //
        this.legendaNormal.AutoSize = true;
        this.legendaNormal.BackColor = System.Drawing.Color.LightGreen;
        this.legendaNormal.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.legendaNormal.ForeColor = System.Drawing.Color.Black;
        this.legendaNormal.Name = "legendaNormal";
        this.legendaNormal.Padding = new System.Windows.Forms.Padding(6, 2, 6, 2);
        this.legendaNormal.TabIndex = 1;
        this.legendaNormal.Text = "Normal: seguro";
        //
        // legendaAtencao
        //
        this.legendaAtencao.AutoSize = true;
        this.legendaAtencao.BackColor = System.Drawing.Color.Khaki;
        this.legendaAtencao.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.legendaAtencao.ForeColor = System.Drawing.Color.Black;
        this.legendaAtencao.Name = "legendaAtencao";
        this.legendaAtencao.Padding = new System.Windows.Forms.Padding(6, 2, 6, 2);
        this.legendaAtencao.TabIndex = 2;
        this.legendaAtencao.Text = "Atenção: acompanhar";
        //
        // legendaCuidado
        //
        this.legendaCuidado.AutoSize = true;
        this.legendaCuidado.BackColor = System.Drawing.Color.LightSalmon;
        this.legendaCuidado.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.legendaCuidado.ForeColor = System.Drawing.Color.Black;
        this.legendaCuidado.Name = "legendaCuidado";
        this.legendaCuidado.Padding = new System.Windows.Forms.Padding(6, 2, 6, 2);
        this.legendaCuidado.TabIndex = 3;
        this.legendaCuidado.Text = "Cuidado extremo: agir (ventilar, água, pausa)";
        //
        // legendaPerigo
        //
        this.legendaPerigo.AutoSize = true;
        this.legendaPerigo.BackColor = System.Drawing.Color.LightPink;
        this.legendaPerigo.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.legendaPerigo.ForeColor = System.Drawing.Color.Black;
        this.legendaPerigo.Name = "legendaPerigo";
        this.legendaPerigo.Padding = new System.Windows.Forms.Padding(6, 2, 6, 2);
        this.legendaPerigo.TabIndex = 4;
        this.legendaPerigo.Text = "Perigo: tirar a turma da sala";
        //
        // legendaSemLeitura
        //
        this.legendaSemLeitura.AutoSize = true;
        this.legendaSemLeitura.BackColor = System.Drawing.SystemColors.Window;
        this.legendaSemLeitura.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
        this.legendaSemLeitura.ForeColor = System.Drawing.SystemColors.WindowText;
        this.legendaSemLeitura.Name = "legendaSemLeitura";
        this.legendaSemLeitura.Padding = new System.Windows.Forms.Padding(6, 2, 6, 2);
        this.legendaSemLeitura.TabIndex = 5;
        this.legendaSemLeitura.Text = "Sem leitura / Sem previsão";
        //
        // salas
        //
        this.salas.AccessibleName = "Salas";
        this.salas.AllowUserToAddRows = false;
        this.salas.AllowUserToDeleteRows = false;
        this.salas.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this.salas.BackgroundColor = System.Drawing.SystemColors.Window;
        this.salas.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colunaSala,
            this.colunaNome,
            this.colunaAtual,
            this.colunaLeitura,
            this.colunaPrevisto,
            this.colunaAvisos});
        this.salas.Dock = System.Windows.Forms.DockStyle.Fill;
        this.salas.MultiSelect = false;
        this.salas.Name = "salas";
        this.salas.ReadOnly = true;
        this.salas.RowHeadersVisible = false;
        this.salas.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.salas.TabIndex = 4;
        //
        // colunaSala
        //
        this.colunaSala.HeaderText = "Sala";
        this.colunaSala.Name = "colunaSala";
        this.colunaSala.ReadOnly = true;
        //
        // colunaNome
        //
        this.colunaNome.HeaderText = "Nome";
        this.colunaNome.Name = "colunaNome";
        this.colunaNome.ReadOnly = true;
        //
        // colunaAtual
        //
        this.colunaAtual.HeaderText = "Nível atual";
        this.colunaAtual.Name = "colunaAtual";
        this.colunaAtual.ReadOnly = true;
        //
        // colunaLeitura
        //
        this.colunaLeitura.HeaderText = "Última leitura (SP)";
        this.colunaLeitura.Name = "colunaLeitura";
        this.colunaLeitura.ReadOnly = true;
        //
        // colunaPrevisto
        //
        this.colunaPrevisto.HeaderText = "Nível previsto (+2 h)";
        this.colunaPrevisto.Name = "colunaPrevisto";
        this.colunaPrevisto.ReadOnly = true;
        //
        // colunaAvisos
        //
        this.colunaAvisos.HeaderText = "Avisos / sensor";
        this.colunaAvisos.Name = "colunaAvisos";
        this.colunaAvisos.ReadOnly = true;
        //
        // rotuloRegistro
        //
        this.rotuloRegistro.AutoSize = true;
        this.rotuloRegistro.Name = "rotuloRegistro";
        this.rotuloRegistro.TabIndex = 5;
        this.rotuloRegistro.Text = "Registrar ação para a sala selecionada";
        //
        // registro
        //
        this.registro.AutoSize = true;
        this.registro.Controls.Add(this.rotuloNivel);
        this.registro.Controls.Add(this.nivel);
        this.registro.Controls.Add(this.rotuloDescricao);
        this.registro.Controls.Add(this.descricao);
        this.registro.Controls.Add(this.botaoRegistrar);
        this.registro.Dock = System.Windows.Forms.DockStyle.Fill;
        this.registro.Name = "registro";
        this.registro.TabIndex = 6;
        //
        // rotuloNivel
        //
        this.rotuloNivel.AutoSize = true;
        this.rotuloNivel.Name = "rotuloNivel";
        this.rotuloNivel.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
        this.rotuloNivel.TabIndex = 0;
        this.rotuloNivel.Text = "&Nível:";
        //
        // nivel
        //
        this.nivel.AccessibleName = "Nível da ação";
        this.nivel.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.nivel.Name = "nivel";
        this.nivel.Size = new System.Drawing.Size(160, 23);
        this.nivel.TabIndex = 1;
        //
        // rotuloDescricao
        //
        this.rotuloDescricao.AutoSize = true;
        this.rotuloDescricao.Name = "rotuloDescricao";
        this.rotuloDescricao.Padding = new System.Windows.Forms.Padding(0, 6, 0, 0);
        this.rotuloDescricao.TabIndex = 2;
        this.rotuloDescricao.Text = "&Descrição:";
        //
        // descricao
        //
        this.descricao.AccessibleName = "Descrição da ação";
        this.descricao.MaxLength = 300;
        this.descricao.Name = "descricao";
        this.descricao.Size = new System.Drawing.Size(320, 23);
        this.descricao.TabIndex = 3;
        //
        // botaoRegistrar
        //
        this.botaoRegistrar.AutoSize = true;
        this.botaoRegistrar.Name = "botaoRegistrar";
        this.botaoRegistrar.TabIndex = 4;
        this.botaoRegistrar.Text = "&Registrar ação";
        this.botaoRegistrar.Click += new System.EventHandler(this.AoRegistrar);
        //
        // acoes
        //
        this.acoes.AccessibleName = "Ações do dia";
        this.acoes.AllowUserToAddRows = false;
        this.acoes.AllowUserToDeleteRows = false;
        this.acoes.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
        this.acoes.BackgroundColor = System.Drawing.SystemColors.Window;
        this.acoes.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.colunaAcaoSala,
            this.colunaAcaoNivel,
            this.colunaAcaoDescricao,
            this.colunaAcaoHora});
        this.acoes.Dock = System.Windows.Forms.DockStyle.Fill;
        this.acoes.MultiSelect = false;
        this.acoes.Name = "acoes";
        this.acoes.ReadOnly = true;
        this.acoes.RowHeadersVisible = false;
        this.acoes.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
        this.acoes.TabIndex = 7;
        //
        // colunaAcaoSala
        //
        this.colunaAcaoSala.HeaderText = "Sala";
        this.colunaAcaoSala.Name = "colunaAcaoSala";
        this.colunaAcaoSala.ReadOnly = true;
        //
        // colunaAcaoNivel
        //
        this.colunaAcaoNivel.HeaderText = "Nível";
        this.colunaAcaoNivel.Name = "colunaAcaoNivel";
        this.colunaAcaoNivel.ReadOnly = true;
        //
        // colunaAcaoDescricao
        //
        this.colunaAcaoDescricao.HeaderText = "Descrição";
        this.colunaAcaoDescricao.Name = "colunaAcaoDescricao";
        this.colunaAcaoDescricao.ReadOnly = true;
        //
        // colunaAcaoHora
        //
        this.colunaAcaoHora.HeaderText = "Hora (São Paulo)";
        this.colunaAcaoHora.Name = "colunaAcaoHora";
        this.colunaAcaoHora.ReadOnly = true;
        //
        // rodape
        //
        this.rodape.AutoSize = true;
        this.rodape.Name = "rodape";
        this.rodape.TabIndex = 8;
        this.rodape.Text = "Ações do dia de São Paulo (UTC−3). Dados locais não são sincronizados com a API.";
        //
        // timer
        //
        this.timer.Interval = 30000;
        this.timer.Tick += new System.EventHandler(this.AoAtualizar);
        //
        // erros
        //
        this.erros.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
        this.erros.ContainerControl = this;
        //
        // TelaCoordenacao
        //
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.ClientSize = new System.Drawing.Size(1150, 760);
        this.Controls.Add(this.painel);
        this.MinimumSize = new System.Drawing.Size(850, 600);
        this.Name = "TelaCoordenacao";
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
        this.Text = "Termômetro da Sala — Coordenação";
        this.Shown += new System.EventHandler(this.AoMostrar);
        this.painel.ResumeLayout(false);
        this.painel.PerformLayout();
        this.comandos.ResumeLayout(false);
        this.comandos.PerformLayout();
        this.legenda.ResumeLayout(false);
        this.legenda.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.salas)).EndInit();
        this.registro.ResumeLayout(false);
        this.registro.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.acoes)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.erros)).EndInit();
        this.ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel painel;
    private System.Windows.Forms.Label titulo;
    private System.Windows.Forms.Label status;
    private System.Windows.Forms.FlowLayoutPanel comandos;
    private System.Windows.Forms.Button botaoAtualizar;
    private System.Windows.Forms.Button botaoCadastrar;
    private System.Windows.Forms.Button botaoRevogar;
    private System.Windows.Forms.Button botaoExportar;
    private System.Windows.Forms.FlowLayoutPanel legenda;
    private System.Windows.Forms.Label rotuloLegenda;
    private System.Windows.Forms.Label legendaNormal;
    private System.Windows.Forms.Label legendaAtencao;
    private System.Windows.Forms.Label legendaCuidado;
    private System.Windows.Forms.Label legendaPerigo;
    private System.Windows.Forms.Label legendaSemLeitura;
    private System.Windows.Forms.DataGridView salas;
    private System.Windows.Forms.DataGridViewTextBoxColumn colunaSala;
    private System.Windows.Forms.DataGridViewTextBoxColumn colunaNome;
    private System.Windows.Forms.DataGridViewTextBoxColumn colunaAtual;
    private System.Windows.Forms.DataGridViewTextBoxColumn colunaLeitura;
    private System.Windows.Forms.DataGridViewTextBoxColumn colunaPrevisto;
    private System.Windows.Forms.DataGridViewTextBoxColumn colunaAvisos;
    private System.Windows.Forms.Label rotuloRegistro;
    private System.Windows.Forms.FlowLayoutPanel registro;
    private System.Windows.Forms.Label rotuloNivel;
    private System.Windows.Forms.ComboBox nivel;
    private System.Windows.Forms.Label rotuloDescricao;
    private System.Windows.Forms.TextBox descricao;
    private System.Windows.Forms.Button botaoRegistrar;
    private System.Windows.Forms.DataGridView acoes;
    private System.Windows.Forms.DataGridViewTextBoxColumn colunaAcaoSala;
    private System.Windows.Forms.DataGridViewTextBoxColumn colunaAcaoNivel;
    private System.Windows.Forms.DataGridViewTextBoxColumn colunaAcaoDescricao;
    private System.Windows.Forms.DataGridViewTextBoxColumn colunaAcaoHora;
    private System.Windows.Forms.Label rodape;
    private System.Windows.Forms.Timer timer;
    private System.Windows.Forms.ErrorProvider erros;
}
