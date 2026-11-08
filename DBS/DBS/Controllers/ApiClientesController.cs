using DBS.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DBS.Controllers;

[ApiController]
[Route("api/clientes")]
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public sealed class ApiClientesController : ControllerBase
{
    private readonly ClienteRepository _repo;

    public ApiClientesController(ClienteRepository repo)
    {
        _repo = repo;
    }

    /// <summary>GET api/clientes: lista para a tela de seleção do app.</summary>
    [HttpGet]
    public IActionResult Listar() => Ok(_repo.GetAll());

    /// <summary>GET api/clientes/5</summary>
    [HttpGet("{id:int}")]
    public IActionResult Obter(int id)
    {
        var cliente = _repo.GetById(id);
        return cliente is null
            ? NotFound(new { mensagem = "Cliente não encontrado." })
            : Ok(cliente);
    }
}
