using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OfficeOpenXml;
using ManutencaoFrotaAPI.DTOs.Requests;
using ManutencaoFrotaAPI.DTOs.Responses;
using FluentValidation;
using FluentValidation.Results;

namespace ManutencaoFrotaAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OrdemManutencaoController : ControllerBase
    {
        private readonly IGerenciadorDados gerenciador;
        private readonly ILogger<OrdemManutencaoController> logger;
        private readonly IValidator<RequisicaoIncluirOrdem> validadorIncluirOrdem;
        private readonly IValidator<RequisicaoAtualizarOrdem> validadorAtualizarOrdem;
        private readonly IValidator<RequisicaoOrdem> validadorOrdem;

        public OrdemManutencaoController(IGerenciadorDados gerenciador, ILogger<OrdemManutencaoController> logger,
                                         IValidator<RequisicaoIncluirOrdem> validadorIncluirOrdem,
                                         IValidator<RequisicaoAtualizarOrdem> validadorAtualizarOrdem,
                                         IValidator<RequisicaoOrdem> validadorOrdem)
        {
            this.gerenciador = gerenciador;
            this.logger = logger;
            this.validadorIncluirOrdem = validadorIncluirOrdem;
            this.validadorAtualizarOrdem = validadorAtualizarOrdem;
            this.validadorOrdem = validadorOrdem;
        }

        /// <summary>
        /// Lista todas as ordens de manutenção com filtros opcionais.
        /// </summary>
        /// <param name="placa">Placa do veículo</param>
        /// <param name="responsavelOrdem">Responsável pela ordem</param>
        /// <param name="proprietario">Proprietário atual do veículo</param>
        /// <returns>Lista de ordens de manutenção</returns>
        [HttpGet("Listar")]
        public IActionResult ListarManutencoes([FromQuery] string placa = null, [FromQuery] string responsavelOrdem = null, [FromQuery] string proprietario = null)
        {
            try
            {
                var ordens = gerenciador.CarregarOrdens();
                if (!ordens.Any())
                {
                    return NotFound("Não há manutenções registradas.");
                }

                var veiculos = gerenciador.CarregarVeiculos();
                var proprietarios = gerenciador.CarregarProprietarios();

                // Filtra por placa, se fornecida
                if (!string.IsNullOrWhiteSpace(placa))
                {
                    veiculos = veiculos.Where(v => v.Placa.Equals(placa, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Filtra por responsável pela ordem, se fornecido
                if (!string.IsNullOrWhiteSpace(responsavelOrdem))
                {
                    var responsaveisFiltrados = proprietarios.Where(p => p.Nome.Contains(responsavelOrdem, StringComparison.OrdinalIgnoreCase)).ToList();
                    ordens = ordens.Where(o => responsaveisFiltrados.Any(p => p.ID == o.ResponsavelID)).ToList();
                }

                // Filtra por proprietário atual, se fornecido
                if (!string.IsNullOrWhiteSpace(proprietario))
                {
                    var proprietariosFiltrados = proprietarios.Where(p => p.Nome.Contains(proprietario, StringComparison.OrdinalIgnoreCase)).ToList();
                    veiculos = veiculos.Where(v => proprietariosFiltrados.Any(p => p.ID == v.ProprietarioID)).ToList();
                }

                // Gera o resultado final combinando informações de ordens, veículos e proprietários
                var resultado = (from ordem in ordens
                                 join veiculo in veiculos on ordem.VeiculoID equals veiculo.ID
                                 join responsavelOrdemEntidade in proprietarios on ordem.ResponsavelID equals responsavelOrdemEntidade.ID
                                 join proprietarioAtualEntidade in proprietarios on veiculo.ProprietarioID equals proprietarioAtualEntidade.ID
                                 select new RespostaOrdemManutencao
                                 {
                                     ID = ordem.ID,
                                     VeiculoPlaca = veiculo?.Placa,
                                     ResponsavelOrdem = responsavelOrdemEntidade?.Nome,
                                     ProprietarioAtual = proprietarioAtualEntidade?.Nome,
                                     DataOrdem = ordem.DataOrdem,
                                     DataFinalizacao = ordem.DataFinalizacao?.ToString("dd/MM/yyyy") ?? "Em Aberto",
                                     Status = ordem.Status.ToString(),
                                     QuilometragemManutencao = ordem.QuilometragemManutencao
                                 }).ToList();

                if (!resultado.Any())
                {
                    return NotFound("Não foram encontradas manutenções com os filtros aplicados.");
                }

                return Ok(resultado);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro interno ao listar ordens de manutenção");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Adiciona uma nova ordem de manutenção.
        /// </summary>
        /// <param name="request">Requisição contendo a placa e a quilometragem do veículo no ato da manutenção</param>
        /// <returns>Mensagem de sucesso se a ordem for criada com sucesso</returns>
        [HttpPost("Incluir")]
        public IActionResult IncluirOrdem([FromBody] RequisicaoIncluirOrdem request)
        {
            ValidationResult resultadoValidacao = validadorIncluirOrdem.Validate(request);
            if (!resultadoValidacao.IsValid)
            {
                return BadRequest(resultadoValidacao.Errors);
            }

            try
            {
                var veiculos = gerenciador.CarregarVeiculos();
                var ordens = gerenciador.CarregarOrdens();

                // Busca o veículo correspondente à placa fornecida
                var veiculo = veiculos.FirstOrDefault(v => v.Placa == request.PlacaVeiculo);
                if (veiculo == null)
                {
                    return NotFound("Veículo não encontrado.");
                }

                // Verifica se já existe uma ordem de manutenção em aberto para o veículo
                if (ordens.Any(o => o.VeiculoID == veiculo.ID && o.Status == StatusManutencao.EmAberto))
                {
                    return Conflict("Já existe uma ordem de manutenção em aberto para este veículo.");
                }

                // Verifica se a quilometragem da manutenção é maior que a quilometragem atual do veículo
                if (request.KmManutencao <= veiculo.Quilometragem)
                {
                    return BadRequest("A quilometragem da manutenção deve ser maior que a quilometragem atual do veículo.");
                }

                // Cria uma nova ordem de manutenção
                var novaOrdem = new OrdemManutencao
                {
                    VeiculoID = veiculo.ID,
                    ResponsavelID = veiculo.ProprietarioID,
                    DataOrdem = DateTime.Now,
                    QuilometragemManutencao = request.KmManutencao,
                    Status = StatusManutencao.EmAberto
                };

                gerenciador.AdicionarOrdem(novaOrdem);

                return Ok("Ordem de Manutenção incluída com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro interno ao incluir ordem de manutenção");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Atualiza uma ordem de manutenção em aberto.
        /// </summary>
        /// <param name="placa">Placa do veículo</param>
        /// <param name="request">Dados para atualização</param>
        /// <returns>Resultado da operação</returns>
        [HttpPatch("Atualizar/{placa}")]
        public IActionResult AtualizarOrdem(string placa, [FromBody] RequisicaoAtualizarOrdem request)
        {
            ValidationResult resultadoValidacao = validadorAtualizarOrdem.Validate(request);
            if (!resultadoValidacao.IsValid)
            {
                return BadRequest(resultadoValidacao.Errors);
            }

            try
            {
                var veiculos = gerenciador.CarregarVeiculos();
                // Busca o veículo correspondente à placa fornecida
                var veiculo = veiculos.FirstOrDefault(v => v.Placa.Equals(placa, StringComparison.OrdinalIgnoreCase));

                if (veiculo == null)
                {
                    return NotFound("Veículo não encontrado.");
                }

                var ordens = gerenciador.CarregarOrdens();
                // Busca a ordem de manutenção em aberto correspondente ao veículo
                var ordemExistente = ordens.FirstOrDefault(o => o.VeiculoID == veiculo.ID && o.Status == StatusManutencao.EmAberto);

                if (ordemExistente == null)
                {
                    return NotFound("Ordem de manutenção não encontrada ou não está em aberto.");
                }

                // Atualiza a quilometragem de manutenção, se fornecida
                if (request.KmManutencao.HasValue)
                {
                    ordemExistente.QuilometragemManutencao = request.KmManutencao.Value;
                }

                // Verifica se a quilometragem da manutenção é maior que a quilometragem atual do veículo
                if (request.KmManutencao <= veiculo.Quilometragem)
                {
                    return BadRequest("A quilometragem da manutenção deve ser maior que a quilometragem atual do veículo.");
                }

                gerenciador.SalvarOrdens(ordens);
                return Ok("Ordem de Manutenção atualizada com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro interno ao atualizar ordem de manutenção");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Encerra uma ordem de manutenção em aberto.
        /// </summary>
        /// <param name="request">Requisição contendo a placa do veículo</param>
        /// <returns>Mensagem de sucesso se a ordem for encerrada com sucesso</returns>
        [HttpPatch("Encerrar")]
        public IActionResult EncerrarOrdem([FromBody] RequisicaoOrdem request)
        {
            ValidationResult resultadoValidacao = validadorOrdem.Validate(request);
            if (!resultadoValidacao.IsValid)
            {
                return BadRequest(resultadoValidacao.Errors);
            }

            try
            {
                var veiculos = gerenciador.CarregarVeiculos();
                var ordens = gerenciador.CarregarOrdens();

                // Busca o veículo correspondente à placa fornecida
                var veiculo = veiculos.FirstOrDefault(v => v.Placa == request.PlacaVeiculo);
                if (veiculo == null)
                {
                    return NotFound("Veículo não localizado.");
                }

                // Busca a ordem de manutenção em aberto correspondente ao veículo
                var ordemParaEncerrar = ordens.FirstOrDefault(o => o.VeiculoID == veiculo.ID && o.Status == StatusManutencao.EmAberto);
                if (ordemParaEncerrar == null)
                {
                    return NotFound("Não há ordem de manutenção em aberto para o veículo.");
                }

                // Atualiza os dados de encerramento da ordem de manutenção
                veiculo.DataUltimaManutencao = DateTime.Now;
                veiculo.Quilometragem = ordemParaEncerrar.QuilometragemManutencao;

                ordemParaEncerrar.Status = StatusManutencao.Concluida;
                ordemParaEncerrar.DataFinalizacao = DateTime.Now;

                gerenciador.SalvarVeiculos(veiculos);
                gerenciador.SalvarOrdens(ordens);

                return Ok("Ordem de Manutenção encerrada com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro interno ao encerrar ordem de manutenção");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Cancela uma ordem de manutenção em aberto.
        /// </summary>
        /// <param name="request">Requisição contendo a placa do veículo</param>
        /// <returns>Mensagem de sucesso se a ordem for cancelada com sucesso</returns>
        [HttpPatch("Cancelar")]
        public IActionResult CancelarOrdem([FromBody] RequisicaoOrdem request)
        {
            ValidationResult resultadoValidacao = validadorOrdem.Validate(request);
            if (!resultadoValidacao.IsValid)
            {
                return BadRequest(resultadoValidacao.Errors);
            }

            try
            {
                var veiculos = gerenciador.CarregarVeiculos();
                var ordens = gerenciador.CarregarOrdens();

                // Busca o veículo correspondente à placa fornecida
                var veiculo = veiculos.FirstOrDefault(v => v.Placa == request.PlacaVeiculo);
                if (veiculo == null)
                {
                    return NotFound("Veículo não localizado.");
                }

                // Busca a última ordem de manutenção em aberto correspondente ao veículo
                var ordemParaCancelar = ordens.LastOrDefault(o => o.VeiculoID == veiculo.ID && o.Status == StatusManutencao.EmAberto);
                if (ordemParaCancelar == null)
                {
                    return NotFound("Não há ordem de manutenção em aberto para o veículo.");
                }

                // Atualiza os dados de cancelamento da ordem de manutenção
                ordemParaCancelar.Status = StatusManutencao.Cancelada;
                ordemParaCancelar.DataFinalizacao = DateTime.Now;

                gerenciador.SalvarOrdens(ordens);

                return Ok("Ordem de Manutenção cancelada com sucesso.");
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro interno ao cancelar ordem de manutenção");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }

        /// <summary>
        /// Exporta ordens de manutenção para um arquivo Excel com filtros opcionais.
        /// </summary>
        /// <param name="placa">Placa do veículo</param>
        /// <param name="responsavelOrdem">Responsável pela ordem</param>
        /// <param name="proprietario">Proprietário atual do veículo</param>
        /// <returns>Base64 do arquivo Excel exportado</returns>
        [HttpGet("Exportar")]
        public IActionResult ExportarParaExcel([FromQuery] string placa = null, [FromQuery] string responsavelOrdem = null, [FromQuery] string proprietario = null)
        {
            try
            {
                var ordens = gerenciador.CarregarOrdens();
                if (!ordens.Any())
                {
                    return NotFound("Não há manutenções registradas.");
                }

                var veiculos = gerenciador.CarregarVeiculos();
                var proprietarios = gerenciador.CarregarProprietarios();

                // Filtra por placa, se fornecida
                if (!string.IsNullOrWhiteSpace(placa))
                {
                    veiculos = veiculos.Where(v => v.Placa.Equals(placa, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                // Filtra por responsável pela ordem, se fornecido
                if (!string.IsNullOrWhiteSpace(responsavelOrdem))
                {
                    var responsaveisFiltrados = proprietarios.Where(p => p.Nome.Contains(responsavelOrdem, StringComparison.OrdinalIgnoreCase)).ToList();
                    ordens = ordens.Where(o => responsaveisFiltrados.Any(p => p.ID == o.ResponsavelID)).ToList();
                }

                // Filtra por proprietário atual, se fornecido
                if (!string.IsNullOrWhiteSpace(proprietario))
                {
                    var proprietariosFiltrados = proprietarios.Where(p => p.Nome.Contains(proprietario, StringComparison.OrdinalIgnoreCase)).ToList();
                    veiculos = veiculos.Where(v => proprietariosFiltrados.Any(p => p.ID == v.ProprietarioID)).ToList();
                }

                // Gera o resultado final combinando informações de ordens, veículos e proprietários
                var resultado = (from ordem in ordens
                                 join veiculo in veiculos on ordem.VeiculoID equals veiculo.ID
                                 join responsavelOrdemEntidade in proprietarios on ordem.ResponsavelID equals responsavelOrdemEntidade.ID
                                 join proprietarioAtualEntidade in proprietarios on veiculo.ProprietarioID equals proprietarioAtualEntidade.ID
                                 select new RespostaOrdemManutencao
                                 {
                                     ID = ordem.ID,
                                     VeiculoPlaca = veiculo?.Placa,
                                     ResponsavelOrdem = responsavelOrdemEntidade?.Nome,
                                     ProprietarioAtual = proprietarioAtualEntidade?.Nome,
                                     DataOrdem = ordem.DataOrdem,
                                     DataFinalizacao = ordem.DataFinalizacao?.ToString("dd/MM/yyyy") ?? "Em Aberto",
                                     Status = ordem.Status.ToString(),
                                     QuilometragemManutencao = ordem.QuilometragemManutencao
                                 }).ToList();

                if (!resultado.Any())
                {
                    return NotFound("Não foram encontradas manutenções com os filtros aplicados.");
                }

                // Exporta o resultado para um arquivo Excel
                ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
                using (var pacote = new ExcelPackage())
                {
                    var planilha = pacote.Workbook.Worksheets.Add("Manutencoes");

                    // Cabeçalhos da planilha
                    planilha.Cells["A1"].Value = "Placa";
                    planilha.Cells["B1"].Value = "Responsável pela Ordem";
                    planilha.Cells["C1"].Value = "Proprietário Atual";
                    planilha.Cells["D1"].Value = "Data da Ordem";
                    planilha.Cells["E1"].Value = "Data de Finalização";
                    planilha.Cells["F1"].Value = "Status";
                    planilha.Cells["G1"].Value = "KM da Manutenção";

                    int linha = 2;
                    // Popula a planilha com os dados filtrados
                    foreach (var ordem in resultado)
                    {
                        planilha.Cells[linha, 1].Value = ordem.VeiculoPlaca;
                        planilha.Cells[linha, 2].Value = ordem.ResponsavelOrdem;
                        planilha.Cells[linha, 3].Value = ordem.ProprietarioAtual;
                        planilha.Cells[linha, 4].Value = ordem.DataOrdem.ToString("dd/MM/yyyy");
                        planilha.Cells[linha, 5].Value = ordem.DataFinalizacao;
                        planilha.Cells[linha, 6].Value = ordem.Status;
                        planilha.Cells[linha, 7].Value = ordem.QuilometragemManutencao;
                        linha++;
                    }

                    var arquivoConteudo = pacote.GetAsByteArray();
                    var arquivoBase64 = Convert.ToBase64String(arquivoConteudo);

                    return Ok(new
                    {
                        Nome = "Manutencoes.xlsx",
                        Tipo = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                        Base64 = arquivoBase64
                    });
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Erro interno ao exportar ordens de manutenção para Excel");
                return StatusCode(500, $"Erro interno: {ex.Message}");
            }
        }
    }
}
