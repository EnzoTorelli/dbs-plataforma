
using DBS.Models;
using DBS.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace DBS.Controllers.Api
{
    [ApiController]
    [Route("api/clientes")]
    [Authorize(Roles = "VendedorExterno")]
    public class ClientesApiController : ControllerBase
    {
        private readonly ClienteRepository _repository;

        public ClientesApiController(ClienteRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            var clientes = _repository.GetAll();
            return Ok(clientes);
        }

        [HttpGet("{id:int}")]
        public IActionResult BuscarPorId(int id)
        {
            var cliente = _repository.GetById(id);

            if (cliente == null)
                return NotFound(new { mensagem = "Cliente não encontrado." });

            return Ok(cliente);
        }

        [HttpPost]
        public IActionResult Cadastrar([FromBody] Cliente cliente)
        {
            if (cliente == null)
                return BadRequest(new { mensagem = "Dados do cliente não informados." });

            Normalizar(cliente);

            var erros = ValidarCliente(cliente);

            if (erros.Count > 0)
            {
                return BadRequest(new
                {
                    mensagem = "Não foi possível cadastrar o cliente.",
                    erros
                });
            }

            try
            {
                cliente.Id = _repository.Insert(cliente);
                cliente.DataCadastro = DateTime.Now;

                return CreatedAtAction(
                    nameof(BuscarPorId),
                    new { id = cliente.Id },
                    cliente);
            }
            catch (Exception)
            {
                return StatusCode(500, new
                {
                    mensagem = "Ocorreu um erro ao salvar o cliente no banco de dados."
                });
            }
        }

        private List<string> ValidarCliente(Cliente cliente)
        {
            var erros = new List<string>();

            if (string.IsNullOrWhiteSpace(cliente.RazaoSocial))
                erros.Add("Informe a razão social.");

            if (string.IsNullOrWhiteSpace(cliente.NomeFantasia))
                erros.Add("Informe o nome fantasia.");

            if (string.IsNullOrWhiteSpace(cliente.Cnpj))
            {
                erros.Add("Informe o CNPJ.");
            }
            else if (!CnpjValido(cliente.Cnpj))
            {
                erros.Add("CNPJ inválido. Informe 12 letras ou números e 2 números finais.");
            }
            else if (CnpjJaCadastrado(cliente.Cnpj))
            {
                erros.Add("Já existe uma empresa cadastrada com este CNPJ.");
            }

            if (string.IsNullOrWhiteSpace(cliente.Responsavel))
                erros.Add("Informe o responsável.");

            if (string.IsNullOrWhiteSpace(cliente.Celular))
            {
                erros.Add("Informe o celular.");
            }
            else if (!TelefoneValido(cliente.Celular))
            {
                erros.Add("Celular inválido. Informe DDD e número.");
            }

            if (string.IsNullOrWhiteSpace(cliente.Email))
            {
                erros.Add("Informe o e-mail.");
            }
            else if (!EmailValido(cliente.Email))
            {
                erros.Add("E-mail inválido.");
            }

            if (!string.IsNullOrWhiteSpace(cliente.Cep) &&
                cliente.Cep.Length != 8)
            {
                erros.Add("O CEP deve conter 8 números.");
            }

            return erros;
        }

        private bool CnpjJaCadastrado(string cnpj)
        {
            return _repository.GetAll().Any(cliente =>
                !string.IsNullOrWhiteSpace(cliente.Cnpj) &&
                NormalizarCnpj(cliente.Cnpj) == cnpj);
        }

        private static bool CnpjValido(string cnpj)
        {
            return Regex.IsMatch(
                cnpj,
                @"^[A-Z0-9]{12}[0-9]{2}$");
        }

        private static bool TelefoneValido(string telefone)
        {
            string numeros = Regex.Replace(telefone, @"\D", "");

            return numeros.Length == 10 || numeros.Length == 11;
        }

        private static bool EmailValido(string email)
        {
            try
            {
                var endereco = new MailAddress(email.Trim());
                return endereco.Address == email.Trim();
            }
            catch
            {
                return false;
            }
        }

        private static string NormalizarCnpj(string? cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return string.Empty;

            return Regex.Replace(cnpj, @"[^a-zA-Z0-9]", "")
                .ToUpperInvariant();
        }

        private static void Normalizar(Cliente cliente)
        {
            cliente.Cnpj = NormalizarCnpj(cliente.Cnpj);

            cliente.RazaoSocial = cliente.RazaoSocial?.Trim()
                ?? string.Empty;

            cliente.NomeFantasia = cliente.NomeFantasia?.Trim();
            cliente.Responsavel = cliente.Responsavel?.Trim();

            cliente.Celular = FormatarTelefone(cliente.Celular);
            cliente.Telefone = cliente.Celular;

            cliente.Email = cliente.Email?.Trim().ToLowerInvariant();

            cliente.Cep = Regex.Replace(
                cliente.Cep ?? string.Empty,
                @"\D",
                "");

            cliente.Logradouro = cliente.Logradouro?.Trim();
            cliente.Numero = cliente.Numero?.Trim();

            cliente.Nome = !string.IsNullOrWhiteSpace(cliente.NomeFantasia)
                ? cliente.NomeFantasia
                : cliente.RazaoSocial;
        }

        private static string FormatarTelefone(string? telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return string.Empty;

            string numeros = Regex.Replace(telefone, @"\D", "");

            if (numeros.Length == 11)
                return $"({numeros[..2]}) {numeros.Substring(2, 5)}-{numeros.Substring(7, 4)}";

            if (numeros.Length == 10)
                return $"({numeros[..2]}) {numeros.Substring(2, 4)}-{numeros.Substring(6, 4)}";

            return numeros;
        }
    }
}
