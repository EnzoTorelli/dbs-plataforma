using DBS.Models;
using DBS.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DBS.Controllers
{
    // Vendas = pedidos com status "Pago"
    public class VendasController : Controller
    {
        private readonly PedidoRepository _repo;

        public VendasController(PedidoRepository repo)
        {
            _repo = repo;
        }

        public IActionResult Index()
        {
            if (HttpContext.Session.GetString("Usuario") == null)
                return RedirectToAction("Index", "Login");

            ViewData["Title"] = "Vendas";
            ViewData["Pagina"] = "Vendas";

            // Reutiliza GetAll — a view pode filtrar, ou podemos trazer todos os pedidos
            var pedidos = _repo.GetAll();
            _repo.CarregarItens(pedidos); // produtos de cada venda (uma consulta só)
            return View(pedidos);

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

            var pedido = _repo.GetById(id);
            if (pedido == null) return RedirectToAction("Index");
            return Json(pedido);
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
