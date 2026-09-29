using System.ComponentModel.DataAnnotations;

namespace ManutencaoFrotaAPI.DTOs.Requests
{
    public class RequisicaoIncluirOrdem
    {
        private string _placaVeiculo;

        /// <summary>
        /// Placa do veículo
        /// </summary>
        [Required(ErrorMessage = "O campo placaVeiculo é obrigatório.")]
        [MinLength(7)]
        public string PlacaVeiculo
        {
            get => _placaVeiculo;
            set => _placaVeiculo = value?.ToUpper();
        }

        /// <summary>
        /// Quilometragem do veículo no momento da manutenção
        /// </summary>
        [Required(ErrorMessage = "O campo KmManutencao é obrigatório.")]
        public int KmManutencao { get; set; }
    }
}
