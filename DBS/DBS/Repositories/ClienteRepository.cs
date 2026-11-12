
using DBS.Models;
using Npgsql;

namespace DBS.Repositories
{
    public class ClienteRepository
    {
        private readonly string _connectionString;

        public ClienteRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public List<Cliente> GetAll()
        {
            var clientes = new List<Cliente>();

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                SELECT
                    id,
                    nome,
                    cnpj,
                    razao_social,
                    nome_fantasia,
                    responsavel,
                    email,
                    telefone,
                    celular,
                    cep,
                    logradouro,
                    numero,
                    data_cadastro
                FROM cliente
                ORDER BY id ASC", conn);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                clientes.Add(MapCliente(reader));
            }

            return clientes;
        }

        public int Count()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(
                "SELECT COUNT(*) FROM cliente", conn);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public int Insert(Cliente cliente)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                INSERT INTO cliente
                (
                    nome,
                    cnpj,
                    razao_social,
                    nome_fantasia,
                    responsavel,
                    email,
                    telefone,
                    celular,
                    cep,
                    logradouro,
                    numero,
                    data_cadastro
                )
                VALUES
                (
                    @nome,
                    @cnpj,
                    @razaoSocial,
                    @nomeFantasia,
                    @responsavel,
                    @email,
                    @telefone,
                    @celular,
                    @cep,
                    @logradouro,
                    @numero,
                    CURRENT_TIMESTAMP
                )
                RETURNING id", conn);

            AdicionarParametros(cmd, cliente);

            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void Delete(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            using var transaction = conn.BeginTransaction();

            try
            {
                var cmdItens = new NpgsqlCommand(@"
                    DELETE FROM item_pedido
                    WHERE id_pedido IN
                    (
                        SELECT id FROM pedido WHERE id_cliente = @id
                    )", conn, transaction);

                cmdItens.Parameters.AddWithValue("@id", id);
                cmdItens.ExecuteNonQuery();

                var cmdPedidos = new NpgsqlCommand(
                    "DELETE FROM pedido WHERE id_cliente = @id",
                    conn, transaction);

                cmdPedidos.Parameters.AddWithValue("@id", id);
                cmdPedidos.ExecuteNonQuery();

                var cmd = new NpgsqlCommand(
                    "DELETE FROM cliente WHERE id = @id",
                    conn, transaction);

                cmd.Parameters.AddWithValue("@id", id);
                cmd.ExecuteNonQuery();

                transaction.Commit();
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public Cliente? GetById(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                SELECT
                    id,
                    nome,
                    cnpj,
                    razao_social,
                    nome_fantasia,
                    responsavel,
                    email,
                    telefone,
                    celular,
                    cep,
                    logradouro,
                    numero,
                    data_cadastro
                FROM cliente
                WHERE id = @id", conn);

            cmd.Parameters.AddWithValue("@id", id);

            using var reader = cmd.ExecuteReader();

            if (reader.Read())
                return MapCliente(reader);

            return null;
        }

        public void Update(Cliente cliente)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmd = new NpgsqlCommand(@"
                UPDATE cliente SET
                    nome = @nome,
                    cnpj = @cnpj,
                    razao_social = @razaoSocial,
                    nome_fantasia = @nomeFantasia,
                    responsavel = @responsavel,
                    email = @email,
                    telefone = @telefone,
                    celular = @celular,
                    cep = @cep,
                    logradouro = @logradouro,
                    numero = @numero
                WHERE id = @id", conn);

            AdicionarParametros(cmd, cliente);
            cmd.Parameters.AddWithValue("@id", cliente.Id);

            cmd.ExecuteNonQuery();
        }

        private static void AdicionarParametros(
            NpgsqlCommand cmd, Cliente cliente)
        {
            string nome = !string.IsNullOrWhiteSpace(cliente.NomeFantasia)
                ? cliente.NomeFantasia
                : cliente.RazaoSocial;

            cmd.Parameters.AddWithValue("@nome", nome);
            cmd.Parameters.AddWithValue("@cnpj", Db(cliente.Cnpj));
            cmd.Parameters.AddWithValue("@razaoSocial", Db(cliente.RazaoSocial));
            cmd.Parameters.AddWithValue("@nomeFantasia", Db(cliente.NomeFantasia));
            cmd.Parameters.AddWithValue("@responsavel", Db(cliente.Responsavel));
            cmd.Parameters.AddWithValue("@email", Db(cliente.Email));
            cmd.Parameters.AddWithValue("@telefone", Db(cliente.Telefone));
            cmd.Parameters.AddWithValue("@celular", Db(cliente.Celular));
            cmd.Parameters.AddWithValue("@cep", Db(cliente.Cep));
            cmd.Parameters.AddWithValue("@logradouro", Db(cliente.Logradouro));
            cmd.Parameters.AddWithValue("@numero", Db(cliente.Numero));
        }

        private static object Db(string? valor)
        {
            return string.IsNullOrWhiteSpace(valor)
                ? DBNull.Value
                : valor;
        }

        private static Cliente MapCliente(NpgsqlDataReader reader)
        {
            string nome = LerTexto(reader, "nome") ?? string.Empty;
            string? razaoSocial = LerTexto(reader, "razao_social");

            return new Cliente
            {
                Id = reader.GetInt32(reader.GetOrdinal("id")),
                Nome = nome,

                RazaoSocial = !string.IsNullOrWhiteSpace(razaoSocial)
                    ? razaoSocial
                    : nome,

                Cnpj = LerTexto(reader, "cnpj"),
                NomeFantasia = LerTexto(reader, "nome_fantasia"),
                Responsavel = LerTexto(reader, "responsavel"),
                Email = LerTexto(reader, "email"),
                Telefone = LerTexto(reader, "telefone"),
                Celular = LerTexto(reader, "celular"),
                Cep = LerTexto(reader, "cep"),
                Logradouro = LerTexto(reader, "logradouro"),
                Numero = LerTexto(reader, "numero"),

                DataCadastro = reader.IsDBNull(
                    reader.GetOrdinal("data_cadastro"))
                    ? DateTime.MinValue
                    : reader.GetDateTime(
                        reader.GetOrdinal("data_cadastro"))
            };
        }

        private static string? LerTexto(
            NpgsqlDataReader reader, string coluna)
        {
            int indice = reader.GetOrdinal(coluna);

            return reader.IsDBNull(indice)
                ? null
                : reader.GetString(indice);
        }
    }
}
