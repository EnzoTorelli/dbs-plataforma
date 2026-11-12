
using DBS.Models;
using Npgsql;

namespace DBS.Repositories
{
    public class VendedorRepository
    {
        private readonly string _connectionString;

        public VendedorRepository(IConfiguration configuration)
        {
            _connectionString =
                configuration.GetConnectionString("DefaultConnection")!;
        }

        public Vendedor? BuscarPorEmail(string email)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(@"
                SELECT
                    id, nome, email, senha_hash, ativo, perfil,
                    data_nascimento, descricao
                FROM vendedor_app
                WHERE LOWER(email) = LOWER(@email)
                LIMIT 1", conn);

            cmd.Parameters.AddWithValue("@email", email.Trim());

            using var reader = cmd.ExecuteReader();

            if (!reader.Read())
                return null;

            return new Vendedor
            {
                Id = reader.GetInt32(0),
                Nome = reader.GetString(1),
                Email = reader.GetString(2),
                SenhaHash = reader.GetString(3),
                Ativo = reader.GetBoolean(4),
                Perfil = reader.GetString(5),
                DataNascimento = reader.IsDBNull(6)
                    ? null
                    : reader.GetDateTime(6),
                Descricao = reader.IsDBNull(7)
                    ? string.Empty
                    : reader.GetString(7)
            };
        }

        public void Criar(Vendedor vendedor, string perfil)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(@"
                INSERT INTO vendedor_app
                    (
                        nome, email, senha_hash, ativo, perfil,
                        data_nascimento, descricao
                    )
                VALUES
                    (
                        @nome, @email, @senhaHash, TRUE, @perfil,
                        @dataNascimento, @descricao
                    )", conn);

            cmd.Parameters.AddWithValue("@nome", vendedor.Nome);
            cmd.Parameters.AddWithValue("@email", vendedor.Email);
            cmd.Parameters.AddWithValue("@senhaHash", vendedor.SenhaHash);
            cmd.Parameters.AddWithValue("@perfil", perfil);

            cmd.Parameters.AddWithValue(
                "@dataNascimento",
                (object?)vendedor.DataNascimento ?? DBNull.Value);

            cmd.Parameters.AddWithValue(
                "@descricao",
                vendedor.Descricao ?? string.Empty);

            cmd.ExecuteNonQuery();
        }

        public List<Vendedor> ListarVendedores()
        {
            var vendedores = new List<Vendedor>();

            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(@"
                SELECT
                    id, nome, email, ativo, perfil,
                    data_nascimento, descricao
                FROM vendedor_app
                WHERE perfil IN ('VendedorExterno', 'VendedorInterno')
                ORDER BY nome", conn);

            using var reader = cmd.ExecuteReader();

            while (reader.Read())
            {
                vendedores.Add(new Vendedor
                {
                    Id = reader.GetInt32(0),
                    Nome = reader.GetString(1),
                    Email = reader.GetString(2),
                    Ativo = reader.GetBoolean(3),
                    Perfil = reader.GetString(4),
                    DataNascimento = reader.IsDBNull(5)
                        ? null
                        : reader.GetDateTime(5),
                    Descricao = reader.IsDBNull(6)
                        ? string.Empty
                        : reader.GetString(6)
                });
            }

            return vendedores;
        }

        public void AtualizarStatus(int id, bool ativo)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(@"
                UPDATE vendedor_app
                SET ativo = @ativo
                WHERE id = @id
                  AND perfil IN ('VendedorExterno', 'VendedorInterno')", conn);

            cmd.Parameters.AddWithValue("@ativo", ativo);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
        }

        public void AtualizarSenhaHash(int id, string senhaHash)
        {
            using var conn = new NpgsqlConnection(_connectionString);
            conn.Open();

            using var cmd = new NpgsqlCommand(@"
                UPDATE vendedor_app
                SET senha_hash = @senhaHash
                WHERE id = @id", conn);

            cmd.Parameters.AddWithValue("@senhaHash", senhaHash);
            cmd.Parameters.AddWithValue("@id", id);

            cmd.ExecuteNonQuery();
        }
    }
}
