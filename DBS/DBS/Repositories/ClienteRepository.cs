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

            var cmd = new NpgsqlCommand(
                "SELECT id, nome, cpf, email, telefone, data_cadastro FROM cliente ORDER BY nome",
                conn);

            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                clientes.Add(new Cliente
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Nome = reader.GetString(reader.GetOrdinal("nome")),
                    Cpf = reader.IsDBNull(reader.GetOrdinal("cpf")) ? null : reader.GetString(reader.GetOrdinal("cpf")),
                    Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString(reader.GetOrdinal("email")),
                    Telefone = reader.IsDBNull(reader.GetOrdinal("telefone")) ? null : reader.GetString(reader.GetOrdinal("telefone")),
                    DataCadastro = reader.GetDateTime(reader.GetOrdinal("data_cadastro"))
                });
            }
            return clientes;
        }

        public int Count()
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            var cmd = new NpgsqlCommand("SELECT COUNT(*) FROM cliente", conn);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }

        public void Insert(Cliente cliente)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            var cmd = new NpgsqlCommand(
                "INSERT INTO cliente (nome, cpf, email, telefone) VALUES (@nome, @cpf, @email, @telefone)",
                conn);
            cmd.Parameters.AddWithValue("@nome", (object?)cliente.Nome ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cpf", (object?)cliente.Cpf ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@email", (object?)cliente.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@telefone", (object?)cliente.Telefone ?? DBNull.Value);
            cmd.ExecuteNonQuery();
        }

        public void Delete(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            var cmdItens = new NpgsqlCommand(@"
                DELETE FROM item_pedido
                WHERE id_pedido IN (SELECT id FROM pedido WHERE id_cliente = @id)", conn);
            cmdItens.Parameters.AddWithValue("@id", id);
            cmdItens.ExecuteNonQuery();

            var cmdPedidos = new NpgsqlCommand("DELETE FROM pedido WHERE id_cliente = @id", conn);
            cmdPedidos.Parameters.AddWithValue("@id", id);
            cmdPedidos.ExecuteNonQuery();

            var cmd = new NpgsqlCommand("DELETE FROM cliente WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }

        public Cliente? GetById(int id)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            var cmd = new NpgsqlCommand(
                "SELECT id, nome, cpf, email, telefone, data_cadastro FROM cliente WHERE id = @id", conn);
            cmd.Parameters.AddWithValue("@id", id);
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                return new Cliente
                {
                    Id = reader.GetInt32(reader.GetOrdinal("id")),
                    Nome = reader.GetString(reader.GetOrdinal("nome")),
                    Cpf = reader.IsDBNull(reader.GetOrdinal("cpf")) ? null : reader.GetString(reader.GetOrdinal("cpf")),
                    Email = reader.IsDBNull(reader.GetOrdinal("email")) ? null : reader.GetString(reader.GetOrdinal("email")),
                    Telefone = reader.IsDBNull(reader.GetOrdinal("telefone")) ? null : reader.GetString(reader.GetOrdinal("telefone")),
                    DataCadastro = reader.GetDateTime(reader.GetOrdinal("data_cadastro"))
                };
            }
            return null;
        }

        public void Update(Cliente cliente)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();
            var cmd = new NpgsqlCommand(
                "UPDATE cliente SET nome=@nome, cpf=@cpf, email=@email, telefone=@telefone WHERE id=@id",
                conn);
            cmd.Parameters.AddWithValue("@nome", (object?)cliente.Nome ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@cpf", (object?)cliente.Cpf ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@email", (object?)cliente.Email ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@telefone", (object?)cliente.Telefone ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@id", cliente.Id);
            cmd.ExecuteNonQuery();
        }
    }
}