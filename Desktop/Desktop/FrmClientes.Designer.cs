namespace DBS.WinForms
{
    partial class FrmClientes
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
            components = new System.ComponentModel.Container();
            pnlTopo = new System.Windows.Forms.Panel();
            lblNome = new System.Windows.Forms.Label();
            txtNome = new System.Windows.Forms.TextBox();
            lblCpf = new System.Windows.Forms.Label();
            txtCpf = new System.Windows.Forms.MaskedTextBox();
            lblTelefone = new System.Windows.Forms.Label();
            txtTelefone = new System.Windows.Forms.MaskedTextBox();
            lblEmail = new System.Windows.Forms.Label();
            txtEmail = new System.Windows.Forms.TextBox();
            lblBusca = new System.Windows.Forms.Label();
            txtBusca = new System.Windows.Forms.TextBox();
            btnSalvar = new System.Windows.Forms.Button();
            btnNovo = new System.Windows.Forms.Button();
            btnExcluir = new System.Windows.Forms.Button();
            btnVoltar = new System.Windows.Forms.Button();
            grid = new System.Windows.Forms.DataGridView();
            errorProvider = new System.Windows.Forms.ErrorProvider(components);
            pnlTopo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            ((System.ComponentModel.ISupportInitialize)errorProvider).BeginInit();
            SuspendLayout();
            //
            // pnlTopo
            //
            pnlTopo.Controls.Add(lblNome);
            pnlTopo.Controls.Add(txtNome);
            pnlTopo.Controls.Add(lblCpf);
            pnlTopo.Controls.Add(txtCpf);
            pnlTopo.Controls.Add(lblTelefone);
            pnlTopo.Controls.Add(txtTelefone);
            pnlTopo.Controls.Add(lblEmail);
            pnlTopo.Controls.Add(txtEmail);
            pnlTopo.Controls.Add(lblBusca);
            pnlTopo.Controls.Add(txtBusca);
            pnlTopo.Controls.Add(btnSalvar);
            pnlTopo.Controls.Add(btnNovo);
            pnlTopo.Controls.Add(btnExcluir);
            pnlTopo.Controls.Add(btnVoltar);
            pnlTopo.Dock = System.Windows.Forms.DockStyle.Top;
            pnlTopo.Location = new System.Drawing.Point(0, 0);
            pnlTopo.Name = "pnlTopo";
            pnlTopo.Size = new System.Drawing.Size(900, 150);
            pnlTopo.TabIndex = 0;
            //
            // lblNome
            //
            lblNome.AutoSize = true;
            lblNome.Location = new System.Drawing.Point(10, 10);
            lblNome.Name = "lblNome";
            lblNome.Size = new System.Drawing.Size(50, 15);
            lblNome.TabIndex = 0;
            lblNome.Text = "Nome *";
            //
            // txtNome
            //
            txtNome.Location = new System.Drawing.Point(10, 30);
            txtNome.MaxLength = 150;
            txtNome.Name = "txtNome";
            txtNome.Size = new System.Drawing.Size(300, 23);
            txtNome.TabIndex = 1;
            //
            // lblCpf
            //
            lblCpf.AutoSize = true;
            lblCpf.Location = new System.Drawing.Point(330, 10);
            lblCpf.Name = "lblCpf";
            lblCpf.Size = new System.Drawing.Size(38, 15);
            lblCpf.TabIndex = 2;
            lblCpf.Text = "CPF *";
            //
            // txtCpf
            //
            txtCpf.CutCopyMaskFormat = System.Windows.Forms.MaskFormat.IncludeLiterals;
            txtCpf.Location = new System.Drawing.Point(330, 30);
            txtCpf.Mask = "000.000.000-00";
            txtCpf.Name = "txtCpf";
            txtCpf.Size = new System.Drawing.Size(150, 23);
            txtCpf.TabIndex = 3;
            txtCpf.TextMaskFormat = System.Windows.Forms.MaskFormat.ExcludePromptAndLiterals;
            txtCpf.Click += Mascara_Foco;
            txtCpf.Enter += Mascara_Foco;
            txtCpf.Leave += txtCpf_Leave;
            //
            // lblTelefone
            //
            lblTelefone.AutoSize = true;
            lblTelefone.Location = new System.Drawing.Point(500, 10);
            lblTelefone.Name = "lblTelefone";
            lblTelefone.Size = new System.Drawing.Size(61, 15);
            lblTelefone.TabIndex = 4;
            lblTelefone.Text = "Telefone *";
            //
            // txtTelefone
            //
            txtTelefone.CutCopyMaskFormat = System.Windows.Forms.MaskFormat.IncludeLiterals;
            txtTelefone.Location = new System.Drawing.Point(500, 30);
            txtTelefone.Mask = "(00) 00000-0000";
            txtTelefone.Name = "txtTelefone";
            txtTelefone.Size = new System.Drawing.Size(150, 23);
            txtTelefone.TabIndex = 5;
            txtTelefone.TextMaskFormat = System.Windows.Forms.MaskFormat.ExcludePromptAndLiterals;
            txtTelefone.Click += Mascara_Foco;
            txtTelefone.Enter += Mascara_Foco;
            txtTelefone.Leave += txtTelefone_Leave;
            //
            // lblEmail
            //
            lblEmail.AutoSize = true;
            lblEmail.Location = new System.Drawing.Point(10, 60);
            lblEmail.Name = "lblEmail";
            lblEmail.Size = new System.Drawing.Size(51, 15);
            lblEmail.TabIndex = 6;
            lblEmail.Text = "E-mail *";
            //
            // txtEmail
            //
            txtEmail.Location = new System.Drawing.Point(10, 80);
            txtEmail.MaxLength = 150;
            txtEmail.Name = "txtEmail";
            txtEmail.PlaceholderText = "nome@exemplo.com";
            txtEmail.Size = new System.Drawing.Size(300, 23);
            txtEmail.TabIndex = 7;
            txtEmail.Leave += txtEmail_Leave;
            //
            // lblBusca
            //
            lblBusca.AutoSize = true;
            lblBusca.Location = new System.Drawing.Point(640, 60);
            lblBusca.Name = "lblBusca";
            lblBusca.Size = new System.Drawing.Size(122, 15);
            lblBusca.TabIndex = 11;
            lblBusca.Text = "Buscar (nome ou CPF)";
            //
            // txtBusca
            //
            txtBusca.Location = new System.Drawing.Point(640, 80);
            txtBusca.Name = "txtBusca";
            txtBusca.Size = new System.Drawing.Size(220, 23);
            txtBusca.TabIndex = 12;
            txtBusca.TextChanged += txtBusca_TextChanged;
            //
            // btnSalvar
            //
            btnSalvar.Location = new System.Drawing.Point(10, 108);
            btnSalvar.Name = "btnSalvar";
            btnSalvar.Size = new System.Drawing.Size(100, 34);
            btnSalvar.TabIndex = 8;
            btnSalvar.Text = "Salvar";
            btnSalvar.UseVisualStyleBackColor = true;
            btnSalvar.Click += btnSalvar_Click;
            //
            // btnNovo
            //
            btnNovo.CausesValidation = false;
            btnNovo.Location = new System.Drawing.Point(120, 108);
            btnNovo.Name = "btnNovo";
            btnNovo.Size = new System.Drawing.Size(100, 34);
            btnNovo.TabIndex = 9;
            btnNovo.Text = "Novo";
            btnNovo.UseVisualStyleBackColor = true;
            btnNovo.Click += btnNovo_Click;
            //
            // btnExcluir
            //
            btnExcluir.Location = new System.Drawing.Point(230, 108);
            btnExcluir.Name = "btnExcluir";
            btnExcluir.Size = new System.Drawing.Size(100, 34);
            btnExcluir.TabIndex = 10;
            btnExcluir.Text = "Excluir";
            btnExcluir.UseVisualStyleBackColor = true;
            btnExcluir.Click += btnExcluir_Click;
            //
            // btnVoltar
            //
            btnVoltar.CausesValidation = false;
            btnVoltar.Location = new System.Drawing.Point(760, 108);
            btnVoltar.Name = "btnVoltar";
            btnVoltar.Size = new System.Drawing.Size(100, 34);
            btnVoltar.TabIndex = 13;
            btnVoltar.Text = "← Voltar";
            btnVoltar.UseVisualStyleBackColor = true;
            btnVoltar.Click += btnVoltar_Click;
            //
            // grid
            //
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.AutoSizeColumnsMode = System.Windows.Forms.DataGridViewAutoSizeColumnsMode.Fill;
            grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            grid.Dock = System.Windows.Forms.DockStyle.Fill;
            grid.Location = new System.Drawing.Point(0, 150);
            grid.MultiSelect = false;
            grid.Name = "grid";
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            grid.Size = new System.Drawing.Size(900, 410);
            grid.TabIndex = 1;
            grid.CellClick += grid_CellClick;
            //
            // errorProvider
            //
            errorProvider.BlinkStyle = System.Windows.Forms.ErrorBlinkStyle.NeverBlink;
            errorProvider.ContainerControl = this;
            //
            // FrmClientes
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnVoltar;
            ClientSize = new System.Drawing.Size(900, 560);
            Controls.Add(grid);
            Controls.Add(pnlTopo);
            Name = "FrmClientes";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Clientes";
            Load += FrmClientes_Load;
            pnlTopo.ResumeLayout(false);
            pnlTopo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ((System.ComponentModel.ISupportInitialize)errorProvider).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlTopo;
        private System.Windows.Forms.Label lblNome;
        private System.Windows.Forms.TextBox txtNome;
        private System.Windows.Forms.Label lblCpf;
        private System.Windows.Forms.MaskedTextBox txtCpf;
        private System.Windows.Forms.Label lblTelefone;
        private System.Windows.Forms.MaskedTextBox txtTelefone;
        private System.Windows.Forms.Label lblEmail;
        private System.Windows.Forms.TextBox txtEmail;
        private System.Windows.Forms.Label lblBusca;
        private System.Windows.Forms.TextBox txtBusca;
        private System.Windows.Forms.Button btnSalvar;
        private System.Windows.Forms.Button btnNovo;
        private System.Windows.Forms.Button btnExcluir;
        private System.Windows.Forms.Button btnVoltar;
        private System.Windows.Forms.DataGridView grid;
        private System.Windows.Forms.ErrorProvider errorProvider;
    }
}