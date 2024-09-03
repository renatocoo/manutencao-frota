using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ManutencaoFrotaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProprietarioController : ControllerBase
    {
        private readonly IGerenciadorDados _gerenciador;

        public ProprietarioController(IGerenciadorDados gerenciador)
        {
            _gerenciador = gerenciador;
        }

        /// <summary>
        /// Lista todos os proprietários cadastrados.
        /// </summary>
        /// <returns>Lista de proprietários</returns>
        [HttpGet("Listar")]
        public IActionResult ListarProprietarios()
        {
            try
            {
                var proprietarios = _gerenciador.CarregarProprietarios();
                if (!proprietarios.Any())
                {
                    return NotFound("Não há proprietários registrados.");
                }

                return Ok(proprietarios);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}
