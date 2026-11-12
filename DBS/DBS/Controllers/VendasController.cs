
using DBS.Models;
using DBS.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DBS.Controllers
{
    // Vendas = somente pedidos com status "Pago"
    public class VendasController : Controller
    {
        private readonly PedidoRepository _repo;
        private readonly ClienteRepository _clienteRepo;
        private readonly ProdutoRepository _produtoRepo;

        public VendasController(
            PedidoRepository repo,
            ClienteRepository clienteRepo,
            ProdutoRepository produtoRepo)
        {
            _repo = repo;
            _clienteRepo = clienteRepo;
            _produtoRepo = produtoRepo;
        }

        private bool UsuarioLogado()
        {
            return HttpContext.Session.GetString("Usuario") != null;
        }

        private bool PodeGerenciarVendas()
        {
            var perfil = HttpContext.Session.GetString("Perfil");

            return perfil == "Administrador" ||
                   perfil == "Gerente";
        }

        public IActionResult Index()
        {
            if (!UsuarioLogado())
                return RedirectToAction("Index", "Login");

            ViewData["Title"] = "Vendas";
            ViewData["Pagina"] = "Vendas";

            ViewBag.Clientes = _clienteRepo.GetAll();
            ViewBag.Produtos = _produtoRepo.GetAll();

            var vendas = _repo.GetAll()
                .Where(p => string.Equals(
                    p.Status,
                    "Pago",
                    StringComparison.OrdinalIgnoreCase))
                .ToList();

            _repo.CarregarItens(vendas);

            return View(vendas);
        }

        [HttpPost]
        public IActionResult Salvar(Pedido pedido)
        {
            if (!UsuarioLogado())
                return RedirectToAction("Index", "Login");

            _repo.Insert(pedido);

            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Excluir(int id)
        {
            if (!UsuarioLogado())
                return RedirectToAction("Index", "Login");

            if (!PodeGerenciarVendas())
                return Forbid();

            _repo.Delete(id);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            if (!UsuarioLogado())
                return Unauthorized();

            if (!PodeGerenciarVendas())
                return Forbid();

            var pedido = _repo.GetById(id);

            if (pedido == null)
                return NotFound();

            _repo.CarregarItens(new[] { pedido });

            return Json(pedido);
        }

        [HttpPost]
        public IActionResult Atualizar(Pedido pedido)
        {
            if (!UsuarioLogado())
                return RedirectToAction("Index", "Login");

            if (!PodeGerenciarVendas())
                return Forbid();

            try
            {
                _repo.AtualizarComItens(pedido);

                TempData["Sucesso"] = "Venda atualizada com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
