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
        titulo = "Aviso de Confidencialidade e Proteção de Dados - LGPD (Lei nº 13.709/2018)",
        itens = new[]
        {
            "Confidencialidade (Art. 46): Os dados de clientes são estritamente confidenciais e protegidos por lei.",
            "Necessidade de Acesso (Art. 6º, III): Acesse apenas os dados estritamente necessários para o desempenho das suas funções.",
            "Restrição de Compartilhamento (Art. 46): É proibido compartilhar dados por mensagens, e-mails, capturas de ecrã (prints) ou meios não autorizados.",
            "Segurança de Credenciais (Art. 46): A sua palavra-passe de acesso é pessoal e intransferível. Não a compartilhe."
        }
    });
}