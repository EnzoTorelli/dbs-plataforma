using DBS.Helpers;
using DBS.Models;
using DBS.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DBS.Controllers
{
    public class ClientesController : Controller
    {
        private readonly ClienteRepository _repo;

        public ClientesController(ClienteRepository repo)
        {
            _repo = repo;
        }

        private bool Logado() => HttpContext.Session.GetString("Usuario") != null;

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

            var erros = ValidarCliente(cliente, idAtual: null);
            if (erros.Count > 0)
            {
                TempData["Erro"] = string.Join("|", erros);
                return RedirectToAction("Index");
            }

            Normalizar(cliente);
            _repo.Insert(cliente);
            TempData["Sucesso"] = "Cliente cadastrado com sucesso.";
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
            if (cliente == null) return RedirectToAction("Index");
            return Json(cliente);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Atualizar(Cliente cliente)
        {
            if (!Logado())
                return RedirectToAction("Index", "Login");

            var erros = ValidarCliente(cliente, idAtual: cliente.Id);
            if (erros.Count > 0)
            {
                TempData["Erro"] = string.Join("|", erros);
                return RedirectToAction("Index");
            }

            Normalizar(cliente);
            _repo.Update(cliente);
            TempData["Sucesso"] = "Cliente atualizado com sucesso.";
            return RedirectToAction("Index");
        }

        // ---------- Validação (o JavaScript da tela valida antes; aqui é a garantia) ----------

        private List<string> ValidarCliente(Cliente c, int? idAtual)
        {
            var erros = new List<string>();

            if (string.IsNullOrWhiteSpace(c.Nome))
                erros.Add("Informe o nome.");
            else if (!Validacao.NomeValido(c.Nome))
                erros.Add("Nome deve ter pelo menos 3 caracteres.");

            if (string.IsNullOrWhiteSpace(c.Cpf))
                erros.Add("Informe o CPF.");
            else if (!Validacao.CpfValido(c.Cpf))
                erros.Add("CPF inválido.");
            else if (CpfJaCadastrado(c.Cpf, idAtual))
                erros.Add("Já existe um cliente com este CPF.");

            if (string.IsNullOrWhiteSpace(c.Telefone))
                erros.Add("Informe o telefone.");
            else if (!Validacao.TelefoneValido(c.Telefone))
                erros.Add("Telefone inválido. Informe DDD + celular: (00) 90000-0000.");

            if (string.IsNullOrWhiteSpace(c.Email))
                erros.Add("Informe o e-mail.");
            else if (!Validacao.EmailValido(c.Email))
                erros.Add("E-mail inválido.");

            return erros;
        }

        private bool CpfJaCadastrado(string cpf, int? idAtual)
        {
            var digitos = Validacao.SoDigitos(cpf);
            return _repo.GetAll().Any(x =>
                x.Id != idAtual && Validacao.SoDigitos(x.Cpf) == digitos);
        }

        // Grava sempre no mesmo padrão: 000.000.000-00 / (00) 00000-0000 / e-mail minúsculo
        private static void Normalizar(Cliente c)
        {
            c.Nome = c.Nome?.Trim()!;
            c.Cpf = Validacao.FormatarCpf(c.Cpf);
            c.Telefone = Validacao.FormatarTelefone(c.Telefone);
            c.Email = Validacao.FormatarEmail(c.Email);
        }
    }
}