
using DBS.Models;
using DBS.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace DBS.Controllers
{
    public class OrdensController : Controller
    {
        private readonly PedidoRepository _repo;
        private readonly ClienteRepository _clienteRepo;
        private readonly ProdutoRepository _produtoRepo;

        public OrdensController(
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

        private bool PodeEditarOrdens()
        {
            return HttpContext.Session.GetString("Perfil") == "Administrador";
        }

        private bool PodeExcluirOrdens()
        {
            var perfil = HttpContext.Session.GetString("Perfil");

            return perfil == "Administrador" || perfil == "Gerente";
        }

        public IActionResult Index()
        {
            if (!UsuarioLogado())
                return RedirectToAction("Index", "Login");

            ViewData["Title"] = "Ordens";
            ViewData["Pagina"] = "Ordens";

            ViewBag.PodeEditarOrdens = PodeEditarOrdens();
            ViewBag.PodeExcluirOrdens = PodeExcluirOrdens();
            ViewBag.Clientes = _clienteRepo.GetAll();
            ViewBag.Produtos = _produtoRepo.GetAll();

            var ordens = _repo.GetAll();
            _repo.CarregarItens(ordens);

            return View(ordens);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Salvar(Pedido pedido)
        {
            if (!UsuarioLogado())
                return RedirectToAction("Index", "Login");

            try
            {
                _repo.Insert(pedido);
                TempData["Sucesso"] = "Ordem cadastrada com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Excluir(int id)
        {
            if (!UsuarioLogado())
                return RedirectToAction("Index", "Login");

            if (!PodeExcluirOrdens())
                return Forbid();

            _repo.Delete(id);

            return RedirectToAction("Index");
        }

        [HttpGet]
        public IActionResult Editar(int id)
        {
            if (!UsuarioLogado())
                return Unauthorized();

            if (!PodeEditarOrdens())
                return Forbid();

            var ordem = _repo.GetById(id);

            if (ordem == null)
                return NotFound();

            _repo.CarregarItens(new[] { ordem });

            return Json(ordem);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Atualizar(Pedido pedido)
        {
            if (!UsuarioLogado())
                return RedirectToAction("Index", "Login");

            if (!PodeEditarOrdens())
                return Forbid();

            try
            {
                _repo.AtualizarComItens(pedido);
                TempData["Sucesso"] = "Ordem atualizada com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
