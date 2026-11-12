
using DBS.Models;
using DBS.Repositories;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace DBS.Controllers.Api
{
    [ApiController]
    [Route("api/auth")]
    public class AuthApiController : ControllerBase
    {
        private readonly VendedorRepository _repository;
        private readonly IPasswordHasher<Vendedor> _passwordHasher;
        private readonly IConfiguration _configuration;

        public AuthApiController(
            VendedorRepository repository,
            IPasswordHasher<Vendedor> passwordHasher,
            IConfiguration configuration)
        {
            _repository = repository;
            _passwordHasher = passwordHasher;
            _configuration = configuration;
        }

        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest request)
        {
            if (request == null ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Senha))
            {
                return BadRequest(new
                {
                    mensagem = "Informe o e-mail e a senha."
                });
            }

            var vendedor = _repository.BuscarPorEmail(request.Email);

            if (vendedor == null || !vendedor.Ativo)
            {
                return Unauthorized(new
                {
                    mensagem = "E-mail ou senha inválidos."
                });
            }

            if (!string.Equals(
                vendedor.Perfil,
                "VendedorExterno",
                StringComparison.OrdinalIgnoreCase))
            {
                return StatusCode(403, new
                {
                    mensagem = "Este acesso é exclusivo para vendedores externos."
                });
            }

            var resultado = _passwordHasher.VerifyHashedPassword(
                vendedor,
                vendedor.SenhaHash,
                request.Senha);

            if (resultado == PasswordVerificationResult.Failed)
            {
                return Unauthorized(new
                {
                    mensagem = "E-mail ou senha inválidos."
                });
            }

            if (resultado == PasswordVerificationResult.SuccessRehashNeeded)
            {
                var novoHash = _passwordHasher.HashPassword(
                    vendedor,
                    request.Senha);

                _repository.AtualizarSenhaHash(
                    vendedor.Id,
                    novoHash);
            }

            var jwtKey = _configuration["JWT_SECRET"]
                ?? _configuration["Jwt:Key"];

            if (string.IsNullOrWhiteSpace(jwtKey))
            {
                return StatusCode(500, new
                {
                    mensagem = "Configuração de autenticação indisponível."
                });
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, vendedor.Id.ToString()),
                new Claim(ClaimTypes.Name, vendedor.Nome),
                new Claim(ClaimTypes.Email, vendedor.Email),
                new Claim(ClaimTypes.Role, vendedor.Perfil)
            };

            var chave = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtKey));

            var credenciais = new SigningCredentials(
                chave,
                SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddHours(8),
                signingCredentials: credenciais);

            var tokenGerado = new JwtSecurityTokenHandler()
                .WriteToken(token);

            return Ok(new
            {
                token = tokenGerado,
                vendedor = new
                {
                    id = vendedor.Id,
                    nome = vendedor.Nome,
                    email = vendedor.Email,
                    perfil = vendedor.Perfil
                }
            });
        }
    }

    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
    }
}
