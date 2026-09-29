using System.ComponentModel.DataAnnotations;

namespace ManutencaoFrotaAPI.DTOs.Requests
{
    public class RequisicaoOrdem
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
    }
}
