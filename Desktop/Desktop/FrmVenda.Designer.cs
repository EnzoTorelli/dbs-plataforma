namespace DBS.WinForms
{
    partial class FrmVenda
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
            pnlTopo = new System.Windows.Forms.Panel();
            lblCliente = new System.Windows.Forms.Label();
            cboCliente = new System.Windows.Forms.ComboBox();
            lblProduto = new System.Windows.Forms.Label();
            cboProduto = new System.Windows.Forms.ComboBox();
            lblQtd = new System.Windows.Forms.Label();
            numQtd = new System.Windows.Forms.NumericUpDown();
            btnAdicionar = new System.Windows.Forms.Button();
            pnlRodape = new System.Windows.Forms.Panel();
            lblTotal = new System.Windows.Forms.Label();
            btnRemover = new System.Windows.Forms.Button();
            btnLimpar = new System.Windows.Forms.Button();
            btnFinalizar = new System.Windows.Forms.Button();
            btnVoltar = new System.Windows.Forms.Button();
            grid = new System.Windows.Forms.DataGridView();
            pnlTopo.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)numQtd).BeginInit();
            pnlRodape.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)grid).BeginInit();
            SuspendLayout();
            //
            // pnlTopo
            //
            pnlTopo.Controls.Add(lblCliente);
            pnlTopo.Controls.Add(cboCliente);
            pnlTopo.Controls.Add(lblProduto);
            pnlTopo.Controls.Add(cboProduto);
            pnlTopo.Controls.Add(lblQtd);
            pnlTopo.Controls.Add(numQtd);
            pnlTopo.Controls.Add(btnAdicionar);
            pnlTopo.Dock = System.Windows.Forms.DockStyle.Top;
            pnlTopo.Location = new System.Drawing.Point(0, 0);
            pnlTopo.Name = "pnlTopo";
            pnlTopo.Size = new System.Drawing.Size(900, 90);
            pnlTopo.TabIndex = 0;
            //
            // lblCliente
            //
            lblCliente.AutoSize = true;
            lblCliente.Location = new System.Drawing.Point(10, 10);
            lblCliente.Name = "lblCliente";
            lblCliente.Size = new System.Drawing.Size(44, 15);
            lblCliente.TabIndex = 0;
            lblCliente.Text = "Cliente";
            //
            // cboCliente
            //
            cboCliente.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboCliente.FormattingEnabled = true;
            cboCliente.Location = new System.Drawing.Point(10, 32);
            cboCliente.Name = "cboCliente";
            cboCliente.Size = new System.Drawing.Size(300, 23);
            cboCliente.TabIndex = 1;
            //
            // lblProduto
            //
            lblProduto.AutoSize = true;
            lblProduto.Location = new System.Drawing.Point(330, 10);
            lblProduto.Name = "lblProduto";
            lblProduto.Size = new System.Drawing.Size(50, 15);
            lblProduto.TabIndex = 2;
            lblProduto.Text = "Produto";
            //
            // cboProduto
            //
            cboProduto.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            cboProduto.FormattingEnabled = true;
            cboProduto.Location = new System.Drawing.Point(330, 32);
            cboProduto.Name = "cboProduto";
            cboProduto.Size = new System.Drawing.Size(340, 23);
            cboProduto.TabIndex = 3;
            //
            // lblQtd
            //
            lblQtd.AutoSize = true;
            lblQtd.Location = new System.Drawing.Point(690, 10);
            lblQtd.Name = "lblQtd";
            lblQtd.Size = new System.Drawing.Size(26, 15);
            lblQtd.TabIndex = 4;
            lblQtd.Text = "Qtd";
            //
            // numQtd
            //
            numQtd.Location = new System.Drawing.Point(690, 32);
            numQtd.Maximum = new decimal(new int[] { 9999, 0, 0, 0 });
            numQtd.Minimum = new decimal(new int[] { 1, 0, 0, 0 });
            numQtd.Name = "numQtd";
            numQtd.Size = new System.Drawing.Size(70, 23);
            numQtd.TabIndex = 5;
            numQtd.Value = new decimal(new int[] { 1, 0, 0, 0 });
            //
            // btnAdicionar
            //
            btnAdicionar.Location = new System.Drawing.Point(780, 30);
            btnAdicionar.Name = "btnAdicionar";
            btnAdicionar.Size = new System.Drawing.Size(110, 34);
            btnAdicionar.TabIndex = 6;
            btnAdicionar.Text = "Adicionar";
            btnAdicionar.UseVisualStyleBackColor = true;
            btnAdicionar.Click += btnAdicionar_Click;
            //
            // pnlRodape
            //
            pnlRodape.Controls.Add(lblTotal);
            pnlRodape.Controls.Add(btnRemover);
            pnlRodape.Controls.Add(btnLimpar);
            pnlRodape.Controls.Add(btnFinalizar);
            pnlRodape.Controls.Add(btnVoltar);
            pnlRodape.Dock = System.Windows.Forms.DockStyle.Bottom;
            pnlRodape.Location = new System.Drawing.Point(0, 490);
            pnlRodape.Name = "pnlRodape";
            pnlRodape.Size = new System.Drawing.Size(900, 70);
            pnlRodape.TabIndex = 2;
            //
            // lblTotal
            //
            lblTotal.AutoSize = true;
            lblTotal.Font = new System.Drawing.Font("Segoe UI", 14F, System.Drawing.FontStyle.Bold);
            lblTotal.Location = new System.Drawing.Point(10, 20);
            lblTotal.Name = "lblTotal";
            lblTotal.Size = new System.Drawing.Size(143, 25);
            lblTotal.TabIndex = 0;
            lblTotal.Text = "Total: R$ 0,00";
            //
            // btnRemover
            //
            btnRemover.Location = new System.Drawing.Point(430, 18);
            btnRemover.Name = "btnRemover";
            btnRemover.Size = new System.Drawing.Size(130, 34);
            btnRemover.TabIndex = 1;
            btnRemover.Text = "Remover item";
            btnRemover.UseVisualStyleBackColor = true;
            btnRemover.Click += btnRemover_Click;
            //
            // btnLimpar
            //
            btnLimpar.Location = new System.Drawing.Point(570, 18);
            btnLimpar.Name = "btnLimpar";
            btnLimpar.Size = new System.Drawing.Size(100, 34);
            btnLimpar.TabIndex = 2;
            btnLimpar.Text = "Limpar";
            btnLimpar.UseVisualStyleBackColor = true;
            btnLimpar.Click += btnLimpar_Click;
            //
            // btnFinalizar
            //
            btnFinalizar.BackColor = System.Drawing.Color.SeaGreen;
            btnFinalizar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            btnFinalizar.ForeColor = System.Drawing.Color.White;
            btnFinalizar.Location = new System.Drawing.Point(690, 18);
            btnFinalizar.Name = "btnFinalizar";
            btnFinalizar.Size = new System.Drawing.Size(190, 34);
            btnFinalizar.TabIndex = 3;
            btnFinalizar.Text = "Finalizar venda";
            btnFinalizar.UseVisualStyleBackColor = false;
            btnFinalizar.Click += btnFinalizar_Click;
            // 
            // btnVoltar
            // 
            btnVoltar.Location = new System.Drawing.Point(320, 18);
            btnVoltar.Name = "btnVoltar";
            btnVoltar.Size = new System.Drawing.Size(100, 34);
            btnVoltar.TabIndex = 4;
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
            grid.Location = new System.Drawing.Point(0, 90);
            grid.MultiSelect = false;
            grid.Name = "grid";
            grid.ReadOnly = true;
            grid.RowHeadersVisible = false;
            grid.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            grid.Size = new System.Drawing.Size(900, 400);
            grid.TabIndex = 1;
            //
            // FrmVenda
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            CancelButton = btnVoltar;
            ClientSize = new System.Drawing.Size(900, 560);
            Controls.Add(grid);
            Controls.Add(pnlTopo);
            Controls.Add(pnlRodape);
            Name = "FrmVenda";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            Text = "Nova venda";
            FormClosing += FrmVenda_FormClosing;
            Load += FrmVenda_Load;
            pnlTopo.ResumeLayout(false);
            pnlTopo.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)numQtd).EndInit();
            pnlRodape.ResumeLayout(false);
            pnlRodape.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)grid).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Panel pnlTopo;
        private System.Windows.Forms.Label lblCliente;
        private System.Windows.Forms.ComboBox cboCliente;
        private System.Windows.Forms.Label lblProduto;
        private System.Windows.Forms.ComboBox cboProduto;
        private System.Windows.Forms.Label lblQtd;
        private System.Windows.Forms.NumericUpDown numQtd;
        private System.Windows.Forms.Button btnAdicionar;
        private System.Windows.Forms.Panel pnlRodape;
        private System.Windows.Forms.Label lblTotal;
        private System.Windows.Forms.Button btnRemover;
        private System.Windows.Forms.Button btnLimpar;
        private System.Windows.Forms.Button btnFinalizar;
        private System.Windows.Forms.Button btnVoltar;
        private System.Windows.Forms.DataGridView grid;
    }
}