
using DBS.Models;
using DBS.Repositories;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mail;
using System.Text.RegularExpressions;

namespace DBS.Controllers
{
    public class ClientesController : Controller
    {
        private readonly ClienteRepository _repo;

        public ClientesController(ClienteRepository repo)
        {
            _repo = repo;
        }

        private bool Logado() =>
            HttpContext.Session.GetString("Usuario") != null;

        public IActionResult Index()
        {
            if (!Logado())
                return RedirectToAction("Index", "Login");

            ViewData["Title"] = "Clientes";
            ViewData["Pagina"] = "Clientes";

            var clientes = _repo.GetAll();
            return View(clientes);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Salvar(Cliente cliente)
        {
            if (!Logado())
                return RedirectToAction("Index", "Login");

            Normalizar(cliente);

            var erros = ValidarCliente(cliente, null);

            if (erros.Count > 0)
            {
                TempData["Erro"] = string.Join("|", erros);
                return RedirectToAction("Index");
            }

            _repo.Insert(cliente);

            TempData["Sucesso"] = "Empresa cadastrada com sucesso.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Excluir(int id)
        {
            if (!Logado())
                return RedirectToAction("Index", "Login");

            _repo.Delete(id);
            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            if (!Logado())
                return RedirectToAction("Index", "Login");

            var cliente = _repo.GetById(id);

            if (cliente == null)
                return RedirectToAction("Index");

            return Json(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Atualizar(Cliente cliente)
        {
            if (!Logado())
                return RedirectToAction("Index", "Login");

            Normalizar(cliente);

            var erros = ValidarCliente(cliente, cliente.Id);

            if (erros.Count > 0)
            {
                TempData["Erro"] = string.Join("|", erros);
                return RedirectToAction("Index");
            }

            _repo.Update(cliente);

            TempData["Sucesso"] = "Empresa atualizada com sucesso.";
            return RedirectToAction("Index");
        }

        private List<string> ValidarCliente(Cliente c, int? idAtual)
        {
            var erros = new List<string>();

            if (string.IsNullOrWhiteSpace(c.RazaoSocial))
                erros.Add("Informe a razão social.");

            if (string.IsNullOrWhiteSpace(c.Cnpj))
            {
                erros.Add("Informe o CNPJ.");
            }
            else if (!CnpjValido(c.Cnpj))
            {
                erros.Add("CNPJ inválido. Informe 12 letras ou números e 2 números finais.");
            }
            else if (CnpjJaCadastrado(c.Cnpj, idAtual))
            {
                erros.Add("Já existe uma empresa cadastrada com este CNPJ.");
            }

            if (string.IsNullOrWhiteSpace(c.Responsavel))
                erros.Add("Informe o responsável.");

            if (string.IsNullOrWhiteSpace(c.Telefone))
                erros.Add("Informe o telefone.");
            else if (!TelefoneValido(c.Telefone))
                erros.Add("Telefone inválido. Informe DDD e número.");

            if (string.IsNullOrWhiteSpace(c.Email))
                erros.Add("Informe o e-mail.");
            else if (!EmailValido(c.Email))
                erros.Add("E-mail inválido.");

            return erros;
        }

        private static bool CnpjValido(string cnpj)
        {
            string documento = NormalizarCnpj(cnpj);

            return Regex.IsMatch(
                documento,
                @"^[A-Z0-9]{12}[0-9]{2}$"
            );
        }

        private bool CnpjJaCadastrado(string cnpj, int? idAtual)
        {
            string documento = NormalizarCnpj(cnpj);

            return _repo.GetAll().Any(x =>
                x.Id != idAtual &&
                !string.IsNullOrWhiteSpace(x.Cnpj) &&
                NormalizarCnpj(x.Cnpj) == documento);
        }

        private static string NormalizarCnpj(string? cnpj)
        {
            if (string.IsNullOrWhiteSpace(cnpj))
                return string.Empty;

            return Regex.Replace(cnpj, @"[^a-zA-Z0-9]", "")
                .ToUpperInvariant();
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

        private static string FormatarTelefone(string? telefone)
        {
            if (string.IsNullOrWhiteSpace(telefone))
                return string.Empty;

            string numeros = Regex.Replace(telefone, @"\D", "");

            if (numeros.Length == 11)
                return $"({numeros[..2]}) {numeros.Substring(2, 5)}-{numeros.Substring(7, 4)}";

            if (numeros.Length == 10)
                return $"({numeros[..2]}) {numeros.Substring(2, 4)}-{numeros.Substring(6, 4)}";

            return telefone.Trim();
        }

        private static void Normalizar(Cliente c)
        {
            c.Cnpj = NormalizarCnpj(c.Cnpj);
            c.RazaoSocial = c.RazaoSocial?.Trim() ?? string.Empty;
            c.NomeFantasia = c.NomeFantasia?.Trim();
            c.Responsavel = c.Responsavel?.Trim();
            c.Telefone = FormatarTelefone(c.Telefone);
            c.Celular = FormatarTelefone(c.Celular);
            c.Email = c.Email?.Trim().ToLowerInvariant();

            // Mantém somente os 8 números do CEP para salvar no banco.
            c.Cep = Regex.Replace(c.Cep ?? "", @"\D", "");

            c.Nome = !string.IsNullOrWhiteSpace(c.NomeFantasia)
                ? c.NomeFantasia
                : c.RazaoSocial;
        }
    }
}
