using ManutencaoFrotaAPI.DTOs.Requests;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ManutencaoFrotaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AutenticacaoController : ControllerBase
    {
        private readonly IConfiguration _configuration;

        public AutenticacaoController(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        /// <summary>
        /// Realiza o login e gera um token JWT.
        /// </summary>
        /// <param name="loginModel">Requisicão contendo o nome de usuário e senha</param>
        /// <returns>Token JWT</returns>
        [HttpPost("Login")]
        public IActionResult Login([FromBody] RequisicaoAutenticacao loginModel)
        {
            if (loginModel.Usuario == "Renato" && loginModel.Senha == "Active123")
            {
                var token = GerarTokenJwt(loginModel.Usuario);
                return Ok(new { Token = token });
            }
            return Unauthorized("Usuário ou senha inválido.");
        }

        /// <summary>
        /// Gera um token JWT para um usuário.
        /// </summary>
        /// <param name="username">Nome do usuário</param>
        /// <returns>Token JWT</returns>
        private string GerarTokenJwt(string username)
        {
            var chaveDeSeguranca = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));
            var credenciais = new SigningCredentials(chaveDeSeguranca, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, username),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Issuer"],
                claims: claims,
                expires: DateTime.Now.AddMinutes(30),
                signingCredentials: credenciais);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
