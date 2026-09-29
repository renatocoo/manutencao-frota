namespace ManutencaoFrotaAPI.DTOs.Responses
{
    public class RespostaVeiculo
    {
        public string Placa { get; set; }
        public string Proprietario { get; set; }
        public string Documento { get; set; }
        public int? Quilometragem { get; set; }
        public string UltimaManutencao { get; set; }
    }
}
