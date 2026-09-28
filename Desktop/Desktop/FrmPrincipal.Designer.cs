namespace DBS.WinForms
{
    partial class FrmPrincipal
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
            btnVenda = new System.Windows.Forms.Button();
            btnClientes = new System.Windows.Forms.Button();
            SuspendLayout();
            //
            // btnVenda
            //
            btnVenda.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            btnVenda.Location = new System.Drawing.Point(60, 40);
            btnVenda.Name = "btnVenda";
            btnVenda.Size = new System.Drawing.Size(280, 70);
            btnVenda.TabIndex = 0;
            btnVenda.Text = "Nova venda";
            btnVenda.UseVisualStyleBackColor = true;
            btnVenda.Click += btnVenda_Click;
            //
            // btnClientes
            //
            btnClientes.Font = new System.Drawing.Font("Segoe UI", 13F, System.Drawing.FontStyle.Bold);
            btnClientes.Location = new System.Drawing.Point(60, 130);
            btnClientes.Name = "btnClientes";
            btnClientes.Size = new System.Drawing.Size(280, 70);
            btnClientes.TabIndex = 1;
            btnClientes.Text = "Clientes";
            btnClientes.UseVisualStyleBackColor = true;
            btnClientes.Click += btnClientes_Click;
            //
            // FrmPrincipal
            //
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            ClientSize = new System.Drawing.Size(400, 280);
            Controls.Add(btnClientes);
            Controls.Add(btnVenda);
            FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            Name = "FrmPrincipal";
            StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            Text = "TechPoint Informática";
            ResumeLayout(false);
        }

        #endregion

        private System.Windows.Forms.Button btnVenda;
        private System.Windows.Forms.Button btnClientes;
    }
}