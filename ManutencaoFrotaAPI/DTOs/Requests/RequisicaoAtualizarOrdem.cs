using System.ComponentModel.DataAnnotations;

namespace ManutencaoFrotaAPI.DTOs.Requests
{
    public class RequisicaoAtualizarOrdem
    {
        /// <summary>
        /// Quilometragem do veículo no momento da manutenção
        /// </summary>
        [Required(ErrorMessage = "O campo KmManutencao é obrigatório.")]
        public int? KmManutencao { get; set; }
    }
}