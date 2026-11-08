using System.Security.Claims;
using DBS.Models;
using DBS.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DBS.Controllers;

[ApiController]
[Route("api/pedidos")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class ApiPedidosController : ControllerBase
{
    private readonly PedidoAppRepository _repo;

    public ApiPedidosController(PedidoAppRepository repo)
    {
        _repo = repo;
    }

    /// <summary>POST api/pedidos: o app envia o carrinho fechado.</summary>
    [HttpPost]
    public async Task<IActionResult> Criar([FromBody] NovoPedidoRequest req)
    {
        if (!TryGetVendedorId(out var idVendedor))
            return Unauthorized(new { mensagem = "Token sem identificação do vendedor." });

        var erro = Validar(req);
        if (erro is not null)
            return BadRequest(new { mensagem = erro });

        try
        {
            var criado = await _repo.CriarComItensAsync(idVendedor, req);
            return Created($"api/pedidos/{criado.Id}", criado);
        }
        catch (RegraNegocioException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }

    /// <summary>GET api/pedidos/hoje: pedidos do vendedor logado, hoje.</summary>
    [HttpGet("hoje")]
    public async Task<IActionResult> Hoje()
    {
        if (!TryGetVendedorId(out var idVendedor))
            return Unauthorized(new { mensagem = "Token sem identificação do vendedor." });

        return Ok(await _repo.ListarDoDiaAsync(idVendedor));
    }

    // ---------- helpers ----------

    private bool TryGetVendedorId(out int id) =>
        int.TryParse(User.FindFirstValue(ClaimTypes.NameIdentifier), out id);

    private static string? Validar(NovoPedidoRequest? req)
    {
        if (req is null) return "Corpo da requisição vazio.";
        if (req.IdCliente <= 0) return "Informe o cliente.";

        if (string.IsNullOrWhiteSpace(req.FormaPagamento)) return "Informe a forma de pagamento.";
        if (req.FormaPagamento.Trim().Length > 30) return "Forma de pagamento muito longa (máx. 30).";

        if (req.Parcelas is < 1 or > 24) return "Parcelas deve estar entre 1 e 24.";

        if (req.Itens is null || req.Itens.Count == 0) return "O pedido precisa de ao menos um item.";
        if (req.Itens.Count > 100) return "Itens demais no pedido (máx. 100).";
        if (req.Itens.Any(i => i.IdProduto <= 0)) return "Item com produto inválido.";
        if (req.Itens.Any(i => i.Quantidade is < 1 or > 10000)) return "Quantidade inválida em algum item.";

        return null;
    }
}
