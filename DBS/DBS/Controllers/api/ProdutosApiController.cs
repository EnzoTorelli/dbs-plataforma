
using DBS.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace DBS.Controllers.Api
{
    [ApiController]
    [Route("api/produtos")]
    [Authorize(Roles = "VendedorExterno")]
    public class ProdutosApiController : ControllerBase
    {
        private readonly ProdutoRepository _repository;

        public ProdutosApiController(ProdutoRepository repository)
        {
            _repository = repository;
        }

        [HttpGet]
        public IActionResult Listar()
        {
            var produtos = _repository.GetAll();
            return Ok(produtos);
        }
    }
}
