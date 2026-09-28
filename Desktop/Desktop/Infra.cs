using System.Data;
using Npgsql;

namespace DBS.WinForms
{
    public static class Database
    {
        private static string? _cs;

        // Lê a connection string do arquivo conexao.txt, que fica ao lado do .exe
        public static string ConnectionString
        {
            get
            {
                if (_cs != null) return _cs;
                var path = Path.Combine(AppContext.BaseDirectory, "conexao.txt");
                if (!File.Exists(path))
                    throw new FileNotFoundException(
                        "Arquivo conexao.txt não encontrado ao lado do executável.", path);
                _cs = File.ReadAllText(path).Trim();
                return _cs;
            }
        }

        public static NpgsqlConnection Open()
        {
            var conn = new NpgsqlConnection(ConnectionString);
            conn.Open();
            return conn;
        }

        public static DataTable Query(string sql, params (string nome, object? valor)[] parametros)
        {
            using var conn = Open();
            using var cmd = new NpgsqlCommand(sql, conn);
            foreach (var (nome, valor) in parametros)
                cmd.Parameters.AddWithValue(nome, valor ?? DBNull.Value);
            using var adapter = new NpgsqlDataAdapter(cmd);
            var dt = new DataTable();
            adapter.Fill(dt);
            return dt;
        }

        public static int Execute(string sql, params (string nome, object? valor)[] parametros)
        {
            using var conn = Open();
            using var cmd = new NpgsqlCommand(sql, conn);
            foreach (var (nome, valor) in parametros)
                cmd.Parameters.AddWithValue(nome, valor ?? DBNull.Value);
            return cmd.ExecuteNonQuery();
        }
    }

    public static class Ui
    {
        public static Button Botao(string texto, int x, int y, int largura, EventHandler aoClicar)
        {
            var b = new Button { Text = texto, Left = x, Top = y, Width = largura, Height = 34 };
            b.Click += aoClicar;
            return b;
        }

        public static void Erro(Exception ex) =>
            MessageBox.Show(ex.Message, "Erro", MessageBoxButtons.OK, MessageBoxIcon.Error);
    }
}
