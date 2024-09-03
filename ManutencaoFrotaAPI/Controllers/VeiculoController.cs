using ManutencaoFrotaAPI.DTOs.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Linq;

namespace ManutencaoFrotaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class VeiculoController : ControllerBase
    {
        private readonly IGerenciadorDados _gerenciador;

        public VeiculoController(IGerenciadorDados gerenciador)
        {
            _gerenciador = gerenciador;
        }

        /// <summary>
        /// Lista todos os veículos cadastrados com filtros opcionais.
        /// </summary>
        /// <param name="placa">Placa do veículo</param>
        /// <param name="proprietario">Nome do proprietário</param>
        /// <returns>Lista de veículos</returns>
        [HttpGet("Listar")]
        public IActionResult GetVeiculos([FromQuery] string placa = null, [FromQuery] string proprietario = null)
        {
            try
            {
                var veiculos = _gerenciador.CarregarVeiculos();
                var proprietarios = _gerenciador.CarregarProprietarios();

                // Filtra por placa, se fornecida
                if (!string.IsNullOrWhiteSpace(placa))
                {
                    veiculos = veiculos.Where(v => v.Placa.Equals(placa, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Filtra por proprietário, se fornecido
                if (!string.IsNullOrWhiteSpace(proprietario))
                {
                    var proprietariosFiltrados = proprietarios.Where(p => p.Nome.Contains(proprietario, StringComparison.OrdinalIgnoreCase)).ToList();
                    veiculos = veiculos.Where(v => proprietariosFiltrados.Any(p => p.ID == v.ProprietarioID)).ToList();
                }

                var veiculosDetalhes = veiculos.Select(veiculo =>
                {
                    var proprietarioEntidade = proprietarios.FirstOrDefault(p => p.ID == veiculo.ProprietarioID);
                    return new
                    {
                        veiculo.Placa,
                        Proprietario = proprietarioEntidade?.Nome,
                        Documento = proprietarioEntidade?.Documento,
                        veiculo.Quilometragem,
                        UltimaManutencao = veiculo.DataUltimaManutencao?.ToString("dd/MM/yyyy") ?? "Não há"
                    };
                });

                if (!veiculosDetalhes.Any())
                {
                    return NotFound("Não foram encontrados veículos com os filtros aplicados.");
                }

                return Ok(veiculosDetalhes);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Atualiza o proprietário de um veículo.
        /// </summary>
        /// <param name="placa">Placa do veículo a ser atualizado</param>
        /// <param name="request">Dados para atualização</param>
        /// <returns>Resultado da operação</returns>
        [HttpPatch("Atualizar/{placa}")]
        public IActionResult PatchVeiculoProprietario(string placa, [FromBody] RequisicaoAtualizarProprietario request)
        {
            if (request == null)
            {
                return BadRequest("Dados de atualização estão vazios");
            }

            var veiculos = _gerenciador.CarregarVeiculos();
            var veiculo = veiculos.FirstOrDefault(v => v.Placa.Equals(placa, StringComparison.OrdinalIgnoreCase));

            if (veiculo == null)
            {
                return NotFound("Veículo não encontrado");
            }

            var proprietario = _gerenciador.CarregarProprietarios().FirstOrDefault(p => p.ID == request.ProprietarioID);
            if (proprietario == null)
            {
                return NotFound("Proprietário não encontrado");
            }

            try
            {
                veiculo.ProprietarioID = request.ProprietarioID;

                _gerenciador.SalvarVeiculos(veiculos);
                return Ok("Proprietário do veículo atualizado com sucesso");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }

}
