
namespace DBS.Models
{
    public class Vendedor
    {
        public int Id { get; set; }

        public string Nome { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string SenhaHash { get; set; } = string.Empty;

        public bool Ativo { get; set; }

        public string Perfil { get; set; } = "Vendedor";

        public DateTime? DataNascimento { get; set; }

        public string Descricao { get; set; } = string.Empty;
    }
}
