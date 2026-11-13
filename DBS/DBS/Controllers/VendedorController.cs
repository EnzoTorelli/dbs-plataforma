
using DBS.Models;
using DBS.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DBS.Controllers
{
    public class VendedorController : Controller
    {
        private readonly VendedorRepository _repository;
        private readonly IPasswordHasher<Vendedor> _passwordHasher;

        public VendedorController(
            VendedorRepository repository,
            IPasswordHasher<Vendedor> passwordHasher)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
        }

        private bool UsuarioEhAdministrador()
        {
            return HttpContext.Session.GetString("Perfil") == "Administrador";
        }

        private void CarregarVendedores()
        {
            ViewBag.Vendedores = _repository.ListarVendedores();
        }

        [HttpGet]
        public IActionResult CadastrarExterno()
        {
            if (!UsuarioEhAdministrador())
                return RedirectToAction("Index", "Login");

            CarregarVendedores();

            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult CadastrarExterno(
            string nome,
            string email,
            string senha,
            string confirmarSenha,
            DateTime? dataNascimento,
            string descricao,
            string perfil)
        {
            if (!UsuarioEhAdministrador())
                return RedirectToAction("Index", "Login");

            if (string.IsNullOrWhiteSpace(nome) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(senha) ||
                string.IsNullOrWhiteSpace(confirmarSenha))
            {
                ViewBag.Erro = "Preencha todos os campos obrigatórios.";
                CarregarVendedores();
                return View();
            }

            if (perfil != "VendedorInterno" &&
                perfil != "VendedorExterno")
            {
                ViewBag.Erro = "Selecione um perfil válido.";
                CarregarVendedores();
                return View();
            }

            if (senha.Length < 6)
            {
                ViewBag.Erro = "A senha deve ter pelo menos 6 caracteres.";
                CarregarVendedores();
                return View();
            }

            if (senha != confirmarSenha)
            {
                ViewBag.Erro = "As senhas informadas não coincidem.";
                CarregarVendedores();
                return View();
            }

            if (_repository.BuscarPorEmail(email) != null)
            {
                ViewBag.Erro = "Já existe um usuário cadastrado com esse e-mail.";
                CarregarVendedores();
                return View();
            }

            var vendedor = new Vendedor
            {
                Nome = nome.Trim(),
                Email = email.Trim(),
                DataNascimento = dataNascimento,
                Descricao = descricao?.Trim() ?? string.Empty,
                Ativo = true,
                Perfil = perfil
            };

            vendedor.SenhaHash = _passwordHasher.HashPassword(
                vendedor,
                senha);

            _repository.Criar(vendedor, perfil);

            TempData["Sucesso"] = "Vendedor cadastrado com sucesso.";

            return RedirectToAction("CadastrarExterno");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AlterarStatus(int id)
        {
            if (!UsuarioEhAdministrador())
                return RedirectToAction("Index", "Login");

            bool? novoStatus = _repository.AlternarStatus(id);

            if (novoStatus == null)
            {
                TempData["Erro"] = "Não foi possível localizar o vendedor para alterar o acesso.";
            }
            else
            {
                TempData["Sucesso"] = novoStatus.Value
                    ? "Acesso do vendedor ativado."
                    : "Acesso do vendedor desativado.";
            }

            return RedirectToAction("CadastrarExterno");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Excluir(int id)
        {
            if (!UsuarioEhAdministrador())
                return RedirectToAction("Index", "Login");

            bool excluido = _repository.Excluir(id);

            if (excluido)
            {
                TempData["Sucesso"] =
                    "Vendedor excluído da lista. O histórico de vendas foi preservado.";
            }
            else
            {
                TempData["Erro"] =
                    "Não foi possível excluir o vendedor. Verifique se o cadastro existe.";
            }

            return RedirectToAction("CadastrarExterno");
        }
    }
}
