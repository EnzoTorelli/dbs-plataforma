using DBS.Models;
using DBS.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication.JwtBearer;

namespace DBS.Controllers
{
    public class ProdutosController : Controller
    {
        private readonly ProdutoRepository _repo;

        public ProdutosController(ProdutoRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Usuario") == null)
                return RedirectToAction("Index", "Login");

            ViewData["Title"] = "Produtos";
            ViewData["Pagina"] = "Produtos";

            var produtos = _repo.GetAll();
            return View(produtos);
        }

        [HttpPost]
        public IActionResult Salvar(Produto produto)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
                return RedirectToAction("Index", "Login");

            _repo.Insert(produto);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Excluir(int id)
        {
            _repo.Delete(id);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Editar(int id)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
                return RedirectToAction("Index", "Login");

            var produto = _repo.GetById(id);
            if (produto == null) return RedirectToAction("Index");
            return Json(produto);
        }

        [HttpGet]
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        public IActionResult Api()
        {
            var produtos = _repo.GetAll();
            return Json(produtos);
        }

        [HttpPost]
        public IActionResult Atualizar(Produto produto)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
                return RedirectToAction("Index", "Login");

            _repo.Update(produto);
            return RedirectToAction("Index");
        }
    }
}
