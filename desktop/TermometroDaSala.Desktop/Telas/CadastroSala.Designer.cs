namespace TermometroDaSala.Desktop;

partial class CadastroSala
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
        this.tabela = new System.Windows.Forms.TableLayoutPanel();
        this.rotuloId = new System.Windows.Forms.Label();
        this.id = new System.Windows.Forms.TextBox();
        this.rotuloNome = new System.Windows.Forms.Label();
        this.nome = new System.Windows.Forms.TextBox();
        this.rotuloAndar = new System.Windows.Forms.Label();
        this.andar = new System.Windows.Forms.NumericUpDown();
        this.rotuloFace = new System.Windows.Forms.Label();
        this.face = new System.Windows.Forms.ComboBox();
        this.ventilador = new System.Windows.Forms.CheckBox();
        this.salvar = new System.Windows.Forms.Button();
        this.cancelar = new System.Windows.Forms.Button();
        this.erros = new System.Windows.Forms.ErrorProvider(this.components);
        this.tabela.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)(this.andar)).BeginInit();
        ((System.ComponentModel.ISupportInitialize)(this.erros)).BeginInit();
        this.SuspendLayout();
        //
        // tabela
        //
        this.tabela.ColumnCount = 2;
        this.tabela.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 40F));
        this.tabela.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 60F));
        this.tabela.Controls.Add(this.rotuloId, 0, 0);
        this.tabela.Controls.Add(this.id, 1, 0);
        this.tabela.Controls.Add(this.rotuloNome, 0, 1);
        this.tabela.Controls.Add(this.nome, 1, 1);
        this.tabela.Controls.Add(this.rotuloAndar, 0, 2);
        this.tabela.Controls.Add(this.andar, 1, 2);
        this.tabela.Controls.Add(this.rotuloFace, 0, 3);
        this.tabela.Controls.Add(this.face, 1, 3);
        this.tabela.Controls.Add(this.ventilador, 1, 4);
        this.tabela.Controls.Add(this.salvar, 0, 5);
        this.tabela.Controls.Add(this.cancelar, 1, 5);
        this.tabela.Dock = System.Windows.Forms.DockStyle.Fill;
        this.tabela.Location = new System.Drawing.Point(0, 0);
        this.tabela.Name = "tabela";
        this.tabela.Padding = new System.Windows.Forms.Padding(15);
        this.tabela.RowCount = 7;
        this.tabela.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tabela.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tabela.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tabela.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tabela.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tabela.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tabela.RowStyles.Add(new System.Windows.Forms.RowStyle());
        this.tabela.Size = new System.Drawing.Size(460, 300);
        this.tabela.TabIndex = 0;
        //
        // rotuloId
        //
        this.rotuloId.AutoSize = true;
        this.rotuloId.Name = "rotuloId";
        this.rotuloId.TabIndex = 0;
        this.rotuloId.Text = "&ID (sala-NN):";
        //
        // id
        //
        this.id.AccessibleName = "Identificador da sala";
        this.id.Dock = System.Windows.Forms.DockStyle.Fill;
        this.id.Name = "id";
        this.id.TabIndex = 1;
        //
        // rotuloNome
        //
        this.rotuloNome.AutoSize = true;
        this.rotuloNome.Name = "rotuloNome";
        this.rotuloNome.TabIndex = 2;
        this.rotuloNome.Text = "&Nome:";
        //
        // nome
        //
        this.nome.AccessibleName = "Nome da sala";
        this.nome.Dock = System.Windows.Forms.DockStyle.Fill;
        this.nome.Name = "nome";
        this.nome.TabIndex = 3;
        //
        // rotuloAndar
        //
        this.rotuloAndar.AutoSize = true;
        this.rotuloAndar.Name = "rotuloAndar";
        this.rotuloAndar.TabIndex = 4;
        this.rotuloAndar.Text = "&Andar (0–20):";
        //
        // andar
        //
        this.andar.AccessibleName = "Andar";
        this.andar.Dock = System.Windows.Forms.DockStyle.Fill;
        this.andar.Maximum = new decimal(new int[] { 20, 0, 0, 0 });
        this.andar.Name = "andar";
        this.andar.TabIndex = 5;
        //
        // rotuloFace
        //
        this.rotuloFace.AutoSize = true;
        this.rotuloFace.Name = "rotuloFace";
        this.rotuloFace.TabIndex = 6;
        this.rotuloFace.Text = "&Face do sol:";
        //
        // face
        //
        this.face.AccessibleName = "Face do sol";
        this.face.Dock = System.Windows.Forms.DockStyle.Fill;
        this.face.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
        this.face.Name = "face";
        this.face.TabIndex = 7;
        //
        // ventilador
        //
        this.ventilador.AutoSize = true;
        this.ventilador.Name = "ventilador";
        this.ventilador.TabIndex = 8;
        this.ventilador.Text = "Tem &ventilador";
        //
        // salvar
        //
        this.salvar.AutoSize = true;
        this.salvar.Name = "salvar";
        this.salvar.TabIndex = 9;
        this.salvar.Text = "&Salvar";
        this.salvar.Click += new System.EventHandler(this.AoSalvar);
        //
        // cancelar
        //
        this.cancelar.AutoSize = true;
        this.cancelar.DialogResult = System.Windows.Forms.DialogResult.Cancel;
        this.cancelar.Name = "cancelar";
        this.cancelar.TabIndex = 10;
        this.cancelar.Text = "&Cancelar";
        //
        // erros
        //
        this.erros.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
        this.erros.ContainerControl = this;
        //
        // CadastroSala
        //
        this.AcceptButton = this.salvar;
        this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
        this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
        this.CancelButton = this.cancelar;
        this.ClientSize = new System.Drawing.Size(460, 300);
        this.Controls.Add(this.tabela);
        this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
        this.MaximizeBox = false;
        this.MinimizeBox = false;
        this.Name = "CadastroSala";
        this.ShowInTaskbar = false;
        this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
        this.Text = "Cadastrar sala";
        this.tabela.ResumeLayout(false);
        this.tabela.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)(this.andar)).EndInit();
        ((System.ComponentModel.ISupportInitialize)(this.erros)).EndInit();
        this.ResumeLayout(false);
    }

    #endregion

    private System.Windows.Forms.TableLayoutPanel tabela;
    private System.Windows.Forms.Label rotuloId;
    private System.Windows.Forms.TextBox id;
    private System.Windows.Forms.Label rotuloNome;
    private System.Windows.Forms.TextBox nome;
    private System.Windows.Forms.Label rotuloAndar;
    private System.Windows.Forms.NumericUpDown andar;
    private System.Windows.Forms.Label rotuloFace;
    private System.Windows.Forms.ComboBox face;
    private System.Windows.Forms.CheckBox ventilador;
    private System.Windows.Forms.Button salvar;
    private System.Windows.Forms.Button cancelar;
    private System.Windows.Forms.ErrorProvider erros;
}
