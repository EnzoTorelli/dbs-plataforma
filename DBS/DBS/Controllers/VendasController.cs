
using DBS.Models;
using DBS.Repositories;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Mvc;
using System.Security.Cryptography;

namespace DBS.Controllers
{
    public class VendasController : Controller
    {
        private readonly PedidoRepository _repo;
        private readonly ClienteRepository _clienteRepo;
        private readonly ProdutoRepository _produtoRepo;
        private readonly ITimeLimitedDataProtector _linkProtector;

        private static readonly TimeSpan ValidadeLink =
            TimeSpan.FromHours(24);

        public VendasController(
            PedidoRepository repo,
            ClienteRepository clienteRepo,
            ProdutoRepository produtoRepo,
            IDataProtectionProvider dataProtectionProvider)
        {
            _repo = repo;
            _clienteRepo = clienteRepo;
            _produtoRepo = produtoRepo;

            _linkProtector = dataProtectionProvider
                .CreateProtector("DBS.PagamentoSimulado.Link.v1")
                .ToTimeLimitedDataProtector();
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

        private string GerarTokenPagamento(int idPedido)
        {
            return _linkProtector.Protect(
                idPedido.ToString(),
                ValidadeLink);
        }

        private bool TentarObterIdPedido(
            string? token,
            out int idPedido)
        {
            idPedido = 0;

            if (string.IsNullOrWhiteSpace(token))
                return false;

            try
            {
                string valor = _linkProtector.Unprotect(
                    token,
                    out _);

                return int.TryParse(valor, out idPedido) &&
                       idPedido > 0;
            }
            catch (CryptographicException)
            {
                return false;
            }
            catch (ArgumentException)
            {
                return false;
            }
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
        [ValidateAntiForgeryToken]
        public IActionResult Salvar(Pedido pedido)
        {
            if (!UsuarioLogado())
                return RedirectToAction("Index", "Login");

            try
            {
                int? idVendedor =
                    HttpContext.Session.GetInt32("VendedorId");

                if (!idVendedor.HasValue || idVendedor.Value <= 0)
                {
                    throw new InvalidOperationException(
                        "Não foi possível identificar o vendedor logado. Faça login novamente.");
                }

                int idPedido = _repo.RegistrarVendaPendente(
                    pedido,
                    idVendedor.Value);

                TempData["Sucesso"] =
                    $"Venda #{idPedido} cadastrada com sucesso! " +
                    "Ela está aguardando validação.";

                if (pedido.FormaPagamento == "Cartão de crédito")
                {
                    string token = GerarTokenPagamento(idPedido);

                    TempData["LinkPagamento"] =
                        Url.Action(
                            nameof(PagamentoSimulado),
                            "Vendas",
                            new { token });
                }
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction("Index");
        }

        // CHECKOUT PÚBLICO COM TOKEN PROTEGIDO.
        // O cliente não precisa entrar no DBS ERP.

        [HttpGet]
        public IActionResult PagamentoSimulado(string token)
        {
            if (!TentarObterIdPedido(token, out int idPedido))
                return NotFound("Link de pagamento inválido ou expirado.");

            var pedido = _repo.GetById(idPedido);

            if (pedido == null)
                return NotFound("Pedido não encontrado.");

            if (!string.Equals(
                pedido.FormaPagamento,
                "Cartão de crédito",
                StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(
                    "Este pedido não foi cadastrado para pagamento com cartão.");
            }

            if (!string.Equals(pedido.Status, "Pendente",
                    StringComparison.OrdinalIgnoreCase) &&
                !string.Equals(pedido.Status, "Pago",
                    StringComparison.OrdinalIgnoreCase))
            {
                return BadRequest(
                    "Este pedido não está disponível para pagamento.");
            }

            _repo.CarregarItens(new[] { pedido });

            ViewData["Title"] = "Pagamento simulado";
            ViewData["Pagina"] = "Vendas";

            ViewBag.TokenPagamento = token;

            return View(pedido);
        }

        // CONFIRMAÇÃO DO CHECKOUT DEMONSTRATIVO.
        // Não recebe nem armazena dados de cartão.

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ConfirmarPagamentoSimulado(string token)
        {
            if (!TentarObterIdPedido(token, out int idPedido))
                return NotFound("Link de pagamento inválido ou expirado.");

            try
            {
                _repo.AprovarPagamentoDemonstrativo(idPedido);

                TempData["Sucesso"] =
                    "Pagamento demonstrativo aprovado! " +
                    "A venda foi atualizada no DBS ERP.";
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction(
                nameof(PagamentoSimulado),
                new { token });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
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
        [ValidateAntiForgeryToken]
        public IActionResult Atualizar(Pedido pedido)
        {
            if (!UsuarioLogado())
                return RedirectToAction("Index", "Login");

            if (!PodeGerenciarVendas())
                return Forbid();

            try
            {
                _repo.AtualizarComItens(pedido);

                TempData["Sucesso"] =
                    "Venda atualizada com sucesso!";
            }
            catch (Exception ex)
            {
                TempData["Erro"] = ex.Message;
            }

            return RedirectToAction("Index");
        }
    }
}
