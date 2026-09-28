using System.Data;
using Npgsql;

namespace DBS.WinForms
{
    public partial class FrmClientes : Form
    {
        private int? _idSelecionado;

        public FrmClientes()
        {
            InitializeComponent();
        }

        // ---------- Eventos (ligados no Designer) ----------

        private void FrmClientes_Load(object? sender, EventArgs e) => Carregar();

        private void txtBusca_TextChanged(object? sender, EventArgs e) => Carregar();

        private void grid_CellClick(object? sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex >= 0) Preencher(e.RowIndex);
        }

        private void btnSalvar_Click(object? sender, EventArgs e) => Salvar();

        private void btnNovo_Click(object? sender, EventArgs e) => Limpar();

        private void btnExcluir_Click(object? sender, EventArgs e) => Excluir();

        // Volta para a tela principal (Esc faz o mesmo: CancelButton = btnVoltar)
        private void btnVoltar_Click(object? sender, EventArgs e) => Close();

        // Com máscara, clicar no meio do campo vazio deixa o cursor perdido.
        // Se o campo estiver vazio, joga o cursor para o início.
        private void Mascara_Foco(object? sender, EventArgs e)
        {
            if (sender is MaskedTextBox m && m.Text.Length == 0)
                BeginInvoke(() => m.Select(0, 0));
        }

        // Ao sair do campo: mostra o ícone de erro, se houver.
        // TextMaskFormat = ExcludePromptAndLiterals, então .Text traz só os dígitos.
        private void txtCpf_Leave(object? sender, EventArgs e)
        {
            MarcarErro(txtCpf, txtCpf.Text.Length == 0 || Validacao.CpfValido(txtCpf.Text)
                ? "" : "CPF inválido.");
        }

        private void txtTelefone_Leave(object? sender, EventArgs e)
        {
            MarcarErro(txtTelefone, txtTelefone.Text.Length == 0 || TelefoneValido()
                ? "" : "Telefone inválido. Informe DDD + celular: (00) 90000-0000.");
        }

        private void txtEmail_Leave(object? sender, EventArgs e)
        {
            txtEmail.Text = Validacao.FormatarEmail(txtEmail.Text);
            MarcarErro(txtEmail, string.IsNullOrWhiteSpace(txtEmail.Text) || Validacao.EmailValido(txtEmail.Text)
                ? "" : "E-mail inválido.");
        }

        // ---------- Lógica ----------

        private void Carregar()
        {
            try
            {
                var busca = txtBusca.Text.Trim();
                var digitos = Validacao.SoDigitos(busca);

                // Busca por CPF funciona com ou sem máscara (123.456 ou 123456)
                var dt = Database.Query(
                    @"SELECT id, nome AS ""Nome"", cpf AS ""CPF"", telefone AS ""Telefone"", email AS ""Email""
                      FROM cliente
                      WHERE nome ILIKE @b
                         OR cpf ILIKE @b
                         OR regexp_replace(cpf, '[^0-9]', '', 'g') LIKE @bd
                      ORDER BY nome",
                    ("@b", $"%{busca}%"),
                    ("@bd", digitos.Length > 0 ? $"%{digitos}%" : "-"));
                grid.DataSource = dt;
                grid.Columns["id"]!.Visible = false;
                grid.ClearSelection();
            }
            catch (Exception ex) { Ui.Erro(ex); }
        }

        private void Preencher(int linha)
        {
            if (grid.Rows[linha].DataBoundItem is not DataRowView r) return;
            errorProvider.Clear();
            _idSelecionado = (int)r["id"];
            txtNome.Text = r["Nome"] as string ?? "";
            // As máscaras recebem só os dígitos; a formatação vem da própria máscara
            txtCpf.Text = Validacao.SoDigitos(r["CPF"] as string);
            txtTelefone.Text = Validacao.SoDigitos(r["Telefone"] as string);
            txtEmail.Text = r["Email"] as string ?? "";
        }

        private void Limpar()
        {
            _idSelecionado = null;
            errorProvider.Clear();
            txtNome.Clear();
            txtCpf.Clear();
            txtTelefone.Clear();
            txtEmail.Clear();
            grid.ClearSelection();
            txtNome.Focus();
        }

        private void MarcarErro(Control c, string msg) => errorProvider.SetError(c, msg);

        // A máscara (00) 00000-0000 só comporta celular: precisa estar completa (11 dígitos)
        private bool TelefoneValido() =>
            txtTelefone.MaskCompleted && Validacao.TelefoneValido(txtTelefone.Text);

        /// <summary>
        /// Valida todos os campos, marca cada um com erro e retorna a lista de mensagens.
        /// </summary>
        private List<(Control campo, string msg)> Validar()
        {
            errorProvider.Clear();
            var erros = new List<(Control, string)>();

            // Nome
            if (string.IsNullOrWhiteSpace(txtNome.Text))
                erros.Add((txtNome, "Informe o nome."));
            else if (!Validacao.NomeValido(txtNome.Text))
                erros.Add((txtNome, "Nome deve ter pelo menos 3 caracteres."));

            // CPF
            if (txtCpf.Text.Length == 0)
                erros.Add((txtCpf, "Informe o CPF."));
            else if (!txtCpf.MaskCompleted || !Validacao.CpfValido(txtCpf.Text))
                erros.Add((txtCpf, "CPF inválido."));

            // Telefone
            if (txtTelefone.Text.Length == 0)
                erros.Add((txtTelefone, "Informe o telefone."));
            else if (!TelefoneValido())
                erros.Add((txtTelefone, "Telefone inválido. Informe DDD + celular: (00) 90000-0000."));

            // E-mail
            if (string.IsNullOrWhiteSpace(txtEmail.Text))
                erros.Add((txtEmail, "Informe o e-mail."));
            else if (!Validacao.EmailValido(txtEmail.Text))
                erros.Add((txtEmail, "E-mail inválido."));

            foreach (var (campo, msg) in erros)
                MarcarErro(campo, msg);

            return erros;
        }

        private bool CpfJaCadastrado(string cpf)
        {
            var dt = Database.Query(
                @"SELECT COUNT(*) FROM cliente
                  WHERE regexp_replace(cpf, '[^0-9]', '', 'g') = @d
                    AND id <> @id",
                ("@d", Validacao.SoDigitos(cpf)),
                ("@id", _idSelecionado ?? 0));
            return Convert.ToInt64(dt.Rows[0][0]) > 0;
        }

        private void Salvar()
        {
            // Normaliza antes de validar (CPF e telefone já vêm pela máscara)
            txtNome.Text = txtNome.Text.Trim();
            txtEmail.Text = Validacao.FormatarEmail(txtEmail.Text);

            var erros = Validar();
            if (erros.Count > 0)
            {
                erros[0].campo.Focus();
                MessageBox.Show(
                    "Corrija os campos abaixo:\n\n• " + string.Join("\n• ", erros.Select(x => x.msg)),
                    "Dados inválidos", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var nome = txtNome.Text;
                // Grava formatado: 000.000.000-00 e (00) 00000-0000
                var cpf = Validacao.FormatarCpf(txtCpf.Text);
                var tel = Validacao.FormatarTelefone(txtTelefone.Text);
                var email = txtEmail.Text;

                if (CpfJaCadastrado(cpf))
                {
                    MarcarErro(txtCpf, "CPF já cadastrado.");
                    txtCpf.Focus();
                    MessageBox.Show("Já existe um cliente com este CPF.", "CPF duplicado",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                if (_idSelecionado == null)
                    Database.Execute(
                        "INSERT INTO cliente (nome, cpf, email, telefone) VALUES (@n, @c, @e, @t)",
                        ("@n", nome), ("@c", cpf), ("@e", email), ("@t", tel));
                else
                    Database.Execute(
                        "UPDATE cliente SET nome=@n, cpf=@c, email=@e, telefone=@t WHERE id=@id",
                        ("@n", nome), ("@c", cpf), ("@e", email), ("@t", tel), ("@id", _idSelecionado.Value));

                Limpar();
                Carregar();
            }
            catch (Exception ex) { Ui.Erro(ex); }
        }

        private void Excluir()
        {
            if (_idSelecionado == null)
            {
                MessageBox.Show("Clique em um cliente da lista para excluir.");
                return;
            }

            var resp = MessageBox.Show(
                "Excluir este cliente e também todos os pedidos dele?",
                "Confirmar exclusão", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (resp != DialogResult.Yes) return;

            try
            {
                using var conn = Database.Open();
                using var tx = conn.BeginTransaction();
                Exec(conn, tx, "DELETE FROM item_pedido WHERE id_pedido IN (SELECT id FROM pedido WHERE id_cliente = @id)");
                Exec(conn, tx, "DELETE FROM pedido WHERE id_cliente = @id");
                Exec(conn, tx, "DELETE FROM cliente WHERE id = @id");
                tx.Commit();

                Limpar();
                Carregar();
            }
            catch (Exception ex) { Ui.Erro(ex); }
        }

        private void Exec(NpgsqlConnection conn, NpgsqlTransaction tx, string sql)
        {
            using var cmd = new NpgsqlCommand(sql, conn, tx);
            cmd.Parameters.AddWithValue("@id", _idSelecionado!.Value);
            cmd.ExecuteNonQuery();
        }
    }
}