using DBS.Models;
using DBS.Repositories;
using DBS.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DBS.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class ApiAuthController : ControllerBase
{
    private readonly VendedorAppRepository _repository;
    private readonly JwtTokenService _tokenService;
    private readonly IPasswordHasher<string> _passwordHasher;

    public ApiAuthController(
        VendedorAppRepository repository,
        JwtTokenService tokenService,
        IPasswordHasher<string> passwordHasher)
    {
        _repository = repository;
        _tokenService = tokenService;
        _passwordHasher = passwordHasher;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var vendedor = await _repository.BuscarPorEmailAsync(request.Email);

        if (vendedor is null)
            return Unauthorized(new { mensagem = "E-mail ou senha inválidos." });

        var resultado = _passwordHasher.VerifyHashedPassword(
            vendedor.Email,
            vendedor.SenhaHash,
            request.Senha);

        if (resultado == PasswordVerificationResult.Failed)
            return Unauthorized(new { mensagem = "E-mail ou senha inválidos." });

        var token = _tokenService.GerarToken(vendedor);

        return Ok(new
        {
            token,
            tipo = "Bearer",
            expiraEmHoras = 8,
            vendedor = new
            {
                vendedor.Id,
                vendedor.Nome,
                vendedor.Email
            }
        });
    }
}
