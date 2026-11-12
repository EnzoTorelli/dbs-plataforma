
using System.ComponentModel.DataAnnotations;

namespace DBS.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        // Identificação da empresa
        [StringLength(14)]
        [RegularExpression(@"^[A-Za-z0-9]{12}[0-9]{2}$",
            ErrorMessage = "CNPJ inválido.")]
        public string? Cnpj { get; set; }

        [StringLength(150)]
        public string RazaoSocial { get; set; } = string.Empty;

        [StringLength(150)]
        public string? NomeFantasia { get; set; }

        // Contato
        [StringLength(150)]
        public string? Responsavel { get; set; }

        [StringLength(20)]
        public string? Celular { get; set; }

        [StringLength(20)]
        public string? Telefone { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        // Endereço
        [StringLength(8)]
        public string? Cep { get; set; }

        [StringLength(150)]
        public string? Logradouro { get; set; }

        [StringLength(20)]
        public string? Numero { get; set; }

        // Controle
        public string Nome { get; set; } = string.Empty;

        public DateTime DataCadastro { get; set; }
    }
}
