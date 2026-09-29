using DBS.Models;
using DBS.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DBS.Controllers
{
    public class OrdensController : Controller
    {
        private readonly PedidoRepository _repo;

        public OrdensController(PedidoRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Usuario") == null)
                return RedirectToAction("Index", "Login");

            ViewData["Title"] = "Ordens";
            ViewData["Pagina"] = "Ordens";

            var ordens = _repo.GetAll();
            _repo.CarregarItens(ordens); // produtos de cada ordem (uma consulta só)
            return View(ordens);
        }

        [HttpPost]
        public IActionResult Salvar(Pedido pedido)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
                return RedirectToAction("Index", "Login");

            _repo.Insert(pedido);
            return RedirectToAction("Index");
        }
        [HttpPost]
        public IActionResult Excluir(int id)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
                return RedirectToAction("Index", "Login");

            _repo.Delete(id);
            return RedirectToAction("Index");
        }
        [HttpGet]
        public IActionResult Editar(int id)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
                return RedirectToAction("Index", "Login");

            var ordem = _repo.GetById(id);
            if (ordem == null) return RedirectToAction("Index");
            return Json(ordem);
        }

        [HttpPost]
        public IActionResult Atualizar(Pedido pedido)
        {
            if (HttpContext.Session.GetString("Usuario") == null)
                return RedirectToAction("Index", "Login");

            _repo.Update(pedido);
            return RedirectToAction("Index");
        }
    }
}
