using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DBS.Controllers;

[ApiController]
[Route("api/aviso-dados")]
public class AvisoController : ControllerBase
{
    [HttpGet]
    [AllowAnonymous]
    public IActionResult Obter() => Ok(new
    {
        versao = "1.0",
        titulo = "Cuidado com os dados de clientes",
        itens = new[]
        {
            "Os dados de clientes são confidenciais.",
            "Acesse apenas o que for necessário para o seu trabalho.",
            "Não compartilhe dados por mensagem, e-mail ou prints.",
            "Não compartilhe sua senha."
        }
    });
}