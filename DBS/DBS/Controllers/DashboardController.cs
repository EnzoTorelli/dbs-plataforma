using DBS.Repositories;
using DBS.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace DBS.Controllers
{
    public class DashboardController : Controller
    {
        private const int LimiteEstoqueBaixo = 5; // mesmo limite do Produto.StatusEstoque

        private readonly ProdutoRepository _produtoRepo;
        private readonly PedidoRepository _pedidoRepo;
        private readonly DashboardRepository _dashRepo;

        public DashboardController(
            ProdutoRepository produtoRepo,
            PedidoRepository pedidoRepo,
            DashboardRepository dashRepo)
        {
            _produtoRepo = produtoRepo;
            _pedidoRepo = pedidoRepo;
            _dashRepo = dashRepo;
        }

        private bool Logado() => HttpContext.Session.GetString("Usuario") != null;

        public IActionResult Index()
        {
            if (!Logado())
                return RedirectToAction("Index", "Login");

            ViewData["Title"] = "Dashboard";
            ViewData["Pagina"] = "Dashboard";

            var (receitaMes, pedidosMes, receitaAnt) = _dashRepo.ResumoMes();
            var (totalClientes, novosMes) = _dashRepo.ResumoClientes();
            var (produtos, unidades, baixo) = _dashRepo.ResumoEstoque(LimiteEstoqueBaixo);

            var vm = new DashboardViewModel
            {
                ReceitaMes = receitaMes,
                PedidosPagosMes = pedidosMes,
                ReceitaMesAnteriorParcial = receitaAnt,
                TotalClientes = totalClientes,
                ClientesNovosMes = novosMes,
                TotalOrdensAbertas = _pedidoRepo.CountAbertos(),
                TotalProdutos = produtos,
                TotalUnidadesEstoque = unidades,
                ProdutosEstoqueBaixo = baixo,
                EstoqueBaixo = _produtoRepo.GetAll()
                                                .Where(p => p.Estoque <= LimiteEstoqueBaixo)
                                                .OrderBy(p => p.Estoque).ThenBy(p => p.Nome)
                                                .Take(6)
                                                .ToList(),
                ReceitaDiariaMes = _dashRepo.ReceitaDiaria("mes"),
                UltimasOrdens = _pedidoRepo.GetUltimas(6)
            };

            return View(vm);
        }

        /// <summary>
        /// Dados dos gráficos para o período escolhido na tela (7, 30, 90 dias ou mês atual).
        /// GET /Dashboard/Dados?periodo=30
        /// </summary>
        [HttpGet]
        public IActionResult Dados(string periodo = "30")
        {
            if (!Logado())
                return Unauthorized();

            if (!DashboardRepository.PeriodosValidos.Contains(periodo))
                periodo = "30";

            return Json(new
            {
                periodo,
                serie = _dashRepo.ReceitaDiaria(periodo),
                topProdutos = _dashRepo.TopProdutos(periodo, 5),
                status = _dashRepo.PedidosPorStatus(periodo)
            });
        }
    }
}
