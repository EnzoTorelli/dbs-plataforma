using System.Data;
using Npgsql;

namespace DBS.WinForms
{
    public partial class FrmVenda : Form
    {
        private readonly DataTable carrinho = new();
        private DataTable produtos = new();

        public FrmVenda()
        {
            InitializeComponent();

            carrinho.Columns.Add("IdProduto", typeof(int));
            carrinho.Columns.Add("Produto", typeof(string));
            carrinho.Columns.Add("Qtd", typeof(int));
            carrinho.Columns.Add("Preco", typeof(decimal));
            carrinho.Columns.Add("Subtotal", typeof(decimal));
        }

        // ---------- Eventos (ligados no Designer) ----------

        private void FrmVenda_Load(object? sender, EventArgs e)
        {
            grid.DataSource = carrinho;
            grid.Columns["IdProduto"]!.Visible = false;
            grid.Columns["Preco"]!.HeaderText = "Preço";
            grid.Columns["Preco"]!.DefaultCellStyle.Format = "C2";
            grid.Columns["Subtotal"]!.DefaultCellStyle.Format = "C2";
            CarregarDados();
            AtualizarTotal();
        }

        private void btnAdicionar_Click(object? sender, EventArgs e) => Adicionar();

        private void btnRemover_Click(object? sender, EventArgs e) => Remover();

        private void btnLimpar_Click(object? sender, EventArgs e)
        {
            carrinho.Clear();
            AtualizarTotal();
        }

        private void btnFinalizar_Click(object? sender, EventArgs e) => Finalizar();

        // Volta para a tela principal (Esc faz o mesmo: CancelButton = btnVoltar)
        private void btnVoltar_Click(object? sender, EventArgs e) => Close();

        // Vale para o Voltar, o Esc e o X da janela: não perde o carrinho sem confirmar
        private void FrmVenda_FormClosing(object? sender, FormClosingEventArgs e)
        {
            if (e.CloseReason != CloseReason.UserClosing || carrinho.Rows.Count == 0) return;

            var resp = MessageBox.Show(
                "Há itens no carrinho. Sair e descartar esta venda?",
                "Venda em andamento", MessageBoxButtons.YesNo, MessageBoxIcon.Question,
                MessageBoxDefaultButton.Button2);
            if (resp != DialogResult.Yes) e.Cancel = true;
        }

        // ---------- Lógica ----------

        private void CarregarDados()
        {
            try
            {
                cboCliente.DisplayMember = "nome";
                cboCliente.ValueMember = "id";
                cboCliente.DataSource = Database.Query("SELECT id, nome FROM cliente ORDER BY nome");

                produtos = Database.Query(@"
                    SELECT id, nome, preco, COALESCE(estoque, 0) AS estoque,
                           CONCAT(nome, ' - R$ ', preco, ' (estoque: ', COALESCE(estoque, 0), ')') AS rotulo
                    FROM produto
                    WHERE COALESCE(estoque, 0) > 0
                    ORDER BY nome");
                cboProduto.DisplayMember = "rotulo";
                cboProduto.ValueMember = "id";
                cboProduto.DataSource = produtos;
            }
            catch (Exception ex) { Ui.Erro(ex); }
        }

        private void Adicionar()
        {
            if (cboProduto.SelectedItem is not DataRowView p)
            {
                MessageBox.Show("Selecione um produto.");
                return;
            }

            int id = Convert.ToInt32(p["id"]);
            int estoque = Convert.ToInt32(p["estoque"]);
            decimal preco = Convert.ToDecimal(p["preco"]);
            int qtd = (int)numQtd.Value;

            var existente = carrinho.AsEnumerable().FirstOrDefault(r => r.Field<int>("IdProduto") == id);
            int jaNoCarrinho = existente?.Field<int>("Qtd") ?? 0;

            if (jaNoCarrinho + qtd > estoque)
            {
                MessageBox.Show($"Estoque insuficiente. Disponível: {estoque} (já no carrinho: {jaNoCarrinho}).");
                return;
            }

            if (existente != null)
            {
                existente["Qtd"] = jaNoCarrinho + qtd;
                existente["Subtotal"] = (jaNoCarrinho + qtd) * preco;
            }
            else
            {
                carrinho.Rows.Add(id, (string)p["nome"], qtd, preco, qtd * preco);
            }

            numQtd.Value = 1;
            AtualizarTotal();
        }

        private void Remover()
        {
            if (grid.CurrentRow?.DataBoundItem is DataRowView r)
            {
                carrinho.Rows.Remove(r.Row);
                AtualizarTotal();
            }
        }

        private decimal Total() => carrinho.AsEnumerable().Sum(r => r.Field<decimal>("Subtotal"));

        private void AtualizarTotal() => lblTotal.Text = $"Total: {Total():C2}";

        private void Finalizar()
        {
            if (cboCliente.SelectedValue is not int idCliente)
            {
                MessageBox.Show("Selecione um cliente. Se ainda não existe, cadastre em Clientes.");
                return;
            }
            if (carrinho.Rows.Count == 0)
            {
                MessageBox.Show("Adicione pelo menos um produto.");
                return;
            }

            try
            {
                using var conn = Database.Open();
                using var tx = conn.BeginTransaction();

                int idPedido;
                using (var cmd = new NpgsqlCommand(
                    "INSERT INTO pedido (id_cliente, status, valor) VALUES (@c, 'Pago', @v) RETURNING id", conn, tx))
                {
                    cmd.Parameters.AddWithValue("@c", idCliente);
                    cmd.Parameters.AddWithValue("@v", Total());
                    idPedido = Convert.ToInt32(cmd.ExecuteScalar());
                }

                foreach (DataRow r in carrinho.Rows)
                {
                    int idProd = r.Field<int>("IdProduto");
                    int qtd = r.Field<int>("Qtd");
                    decimal preco = r.Field<decimal>("Preco");

                    // baixa de estoque só acontece se ainda houver quantidade suficiente
                    using (var upd = new NpgsqlCommand(
                        "UPDATE produto SET estoque = estoque - @q WHERE id = @p AND estoque >= @q", conn, tx))
                    {
                        upd.Parameters.AddWithValue("@q", qtd);
                        upd.Parameters.AddWithValue("@p", idProd);
                        if (upd.ExecuteNonQuery() == 0)
                            throw new InvalidOperationException(
                                $"Estoque insuficiente para \"{r.Field<string>("Produto")}\". A venda não foi registrada.");
                    }

                    using var ins = new NpgsqlCommand(
                        "INSERT INTO item_pedido (id_pedido, id_produto, quantidade, preco_unitario) VALUES (@ped, @prod, @q, @pu)",
                        conn, tx);
                    ins.Parameters.AddWithValue("@ped", idPedido);
                    ins.Parameters.AddWithValue("@prod", idProd);
                    ins.Parameters.AddWithValue("@q", qtd);
                    ins.Parameters.AddWithValue("@pu", preco);
                    ins.ExecuteNonQuery();
                }

                tx.Commit();

                MessageBox.Show($"Venda #{idPedido} registrada com sucesso!", "Venda concluída",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                carrinho.Clear();
                AtualizarTotal();
                CarregarDados(); // recarrega os produtos com o estoque atualizado
            }
            catch (Exception ex) { Ui.Erro(ex); }
        }
    }
}