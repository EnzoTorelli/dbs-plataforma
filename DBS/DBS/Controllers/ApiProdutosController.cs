using DBS.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DBS.Controllers;

[ApiController]
[Route("api/produtos")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class ApiProdutosController : ControllerBase
{
    private readonly ProdutoRepository _repo;

    public ApiProdutosController(ProdutoRepository repo)
    {
        _repo = repo;
    }

    /// <summary>GET api/produtos: catálogo para o app. Substitui o antigo /Produtos/Api.</summary>
    [HttpGet]
    public IActionResult Listar() => Ok(_repo.GetAll());

    /// <summary>GET api/produtos/4</summary>
    [HttpGet("{id:int}")]
    public IActionResult Obter(int id)
    {
        var produto = _repo.GetById(id);
        return produto is null
            ? NotFound(new { mensagem = "Produto não encontrado." })
            : Ok(produto);
    }
}
