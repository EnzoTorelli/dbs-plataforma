using DBS.Models;
using Npgsql;

namespace DBS.Repositories;

public sealed class VendedorAppRepository
{
    private readonly string _connectionString;

    public VendedorAppRepository(IConfiguration configuration)
    {
        _connectionString =
            configuration.GetConnectionString("DefaultConnection") ?? "";
    }

    public async Task<VendedorApp?> BuscarPorEmailAsync(string email)
    {
        if (string.IsNullOrWhiteSpace(_connectionString))
        {
            throw new InvalidOperationException(
                "A conexão com o banco de dados não está configurada.");
        }

        const string sql = """
            SELECT id, nome, email, senha_hash
            FROM vendedor_app
            WHERE lower(email) = lower(@email)
              AND ativo = TRUE
            LIMIT 1;
            """;

        await using var connection = new NpgsqlConnection(_connectionString);
        await connection.OpenAsync();

        await using var command = new NpgsqlCommand(sql, connection);
        command.Parameters.AddWithValue("email", email.Trim());

        await using var reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
            return null;

        return new VendedorApp
        {
            Id = reader.GetInt32(0),
            Nome = reader.GetString(1),
            Email = reader.GetString(2),
            SenhaHash = reader.GetString(3)
        };
    }
}
