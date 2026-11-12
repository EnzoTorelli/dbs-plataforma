
using DBS.Models;
using DBS.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DBS.Controllers.Api
{
    [ApiController]
    [Route("api/pedidos")]
    [Authorize(Roles = "VendedorExterno")]
    public class PedidosApiController : ControllerBase
    {
        private readonly PedidoRepository _repository;

        public PedidosApiController(PedidoRepository repository)
        {
            _repository = repository;
        }

        private int? ObterIdVendedor()
        {
            var id = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (int.TryParse(id, out int idVendedor))
                return idVendedor;

            return null;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            var idVendedor = ObterIdVendedor();

            if (!idVendedor.HasValue)
                return Unauthorized();

            var pedidos = _repository.GetByVendedor(idVendedor.Value);
            _repository.CarregarItens(pedidos);

            return Ok(pedidos);
        }

        [HttpGet("ultimos")]
        public IActionResult ListarUltimos(
            [FromQuery] int quantidade = 5)
        {
            if (quantidade < 1 || quantidade > 100)
            {
                return BadRequest(new
                {
                    mensagem = "A quantidade deve estar entre 1 e 100."
                });
            }

            var idVendedor = ObterIdVendedor();

            if (!idVendedor.HasValue)
                return Unauthorized();

            var pedidos = _repository.GetUltimasPorVendedor(
                idVendedor.Value,
                quantidade);

            _repository.CarregarItens(pedidos);

            return Ok(pedidos);
        }

        [HttpGet("{id:int}")]
        public IActionResult BuscarPorId(int id)
        {
            var idVendedor = ObterIdVendedor();

            if (!idVendedor.HasValue)
                return Unauthorized();

            var pedido = _repository.GetByIdVendedor(
                id,
                idVendedor.Value);

            if (pedido == null)
            {
                return NotFound(new
                {
                    mensagem = "Pedido não encontrado."
                });
            }

            _repository.CarregarItens(new[] { pedido });

            return Ok(pedido);
        }

        [HttpPost]
        public IActionResult Criar(
            [FromBody] CriarPedidoRequest request)
        {
            if (request == null)
            {
                return BadRequest(new
                {
                    mensagem = "Dados do pedido não informados."
                });
            }

            var idVendedor = ObterIdVendedor();

            if (!idVendedor.HasValue)
                return Unauthorized();

            if (request.IdCliente <= 0)
            {
                return BadRequest(new
                {
                    mensagem = "Selecione um cliente válido."
                });
            }

            if (request.Itens == null || request.Itens.Count == 0)
            {
                return BadRequest(new
                {
                    mensagem = "Adicione pelo menos um produto ao pedido."
                });
            }

            if (request.Itens.Any(i =>
                i.IdProduto <= 0 || i.Quantidade <= 0))
            {
                return BadRequest(new
                {
                    mensagem = "Existem produtos ou quantidades inválidas."
                });
            }

            var formaPagamento = request.FormaPagamento?.Trim();

            var formasPermitidas = new[]
            {
                "Dinheiro",
                "Pix",
                "Faturado",
                "Cartão de crédito"
            };

            if (string.IsNullOrWhiteSpace(formaPagamento) ||
                !formasPermitidas.Contains(
                    formaPagamento,
                    StringComparer.OrdinalIgnoreCase))
            {
                return BadRequest(new
                {
                    mensagem = "Forma de pagamento inválida."
                });
            }

            bool cartao = string.Equals(
                formaPagamento,
                "Cartão de crédito",
                StringComparison.OrdinalIgnoreCase);

            if (cartao &&
                (!request.Parcelas.HasValue ||
                 request.Parcelas.Value < 1 ||
                 request.Parcelas.Value > 6))
            {
                return BadRequest(new
                {
                    mensagem = "O cartão deve ter entre 1 e 6 parcelas."
                });
            }

            if (!cartao && request.Parcelas.HasValue)
            {
                return BadRequest(new
                {
                    mensagem = "Parcelas só podem ser informadas para cartão."
                });
            }

            try
            {
                var itens = request.Itens.Select(i =>
                    new ItemPedidoResumo
                    {
                        IdProduto = i.IdProduto,
                        Quantidade = i.Quantidade
                    }).ToList();

                int idPedido = _repository.RegistrarPedidoMobile(
                    request.IdCliente,
                    idVendedor.Value,
                    formaPagamento,
                    request.Parcelas,
                    itens);

                return Ok(new
                {
                    mensagem = "Pedido enviado para validação interna.",
                    idPedido
                });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new
                {
                    mensagem = ex.Message
                });
            }
        }
    }

    public class CriarPedidoRequest
    {
        public int IdCliente { get; set; }
        public string? FormaPagamento { get; set; }
        public int? Parcelas { get; set; }
        public List<ItemPedidoRequest> Itens { get; set; } = new();
    }

    public class ItemPedidoRequest
    {
        public int IdProduto { get; set; }
        public int Quantidade { get; set; }
    }
}
