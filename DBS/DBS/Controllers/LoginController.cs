
using DBS.Models;
using DBS.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DBS.Controllers
{
    public class LoginController : Controller
    {
        private readonly VendedorRepository _vendedorRepository;
        private readonly IPasswordHasher<Vendedor> _passwordHasher;

        public LoginController(
            VendedorRepository vendedorRepository,
            IPasswordHasher<Vendedor> passwordHasher)
        {
            _vendedorRepository = vendedorRepository;
            _passwordHasher = passwordHasher;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Usuario") != null)
                return RedirectToAction("Index", "Dashboard");

            return View();
        }

        [HttpPost]
        public IActionResult Index(string email, string senha)
        {
            if (string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(senha))
            {
                ViewBag.Erro = "Informe o e-mail e a senha.";
                return View();
            }

            var vendedor = _vendedorRepository.BuscarPorEmail(email);

            if (vendedor == null || !vendedor.Ativo ||
                string.IsNullOrWhiteSpace(vendedor.SenhaHash))
            {
                ViewBag.Erro = "E-mail ou senha inválidos.";
                return View();
            }

            var resultado = _passwordHasher.VerifyHashedPassword(
                vendedor,
                vendedor.SenhaHash,
                senha
            );

            if (resultado == PasswordVerificationResult.Failed)
            {
                ViewBag.Erro = "E-mail ou senha inválidos.";
                return View();
            }

            HttpContext.Session.SetString("Usuario", vendedor.Email);
            HttpContext.Session.SetString("VendedorNome", vendedor.Nome);
            HttpContext.Session.SetString("Perfil", vendedor.Perfil);
            HttpContext.Session.SetInt32("VendedorId", vendedor.Id);

            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Index", "Login");
        }
    }
}
