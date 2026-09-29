namespace ManutencaoFrotaAPI.DTOs.Responses
{
    public class RespostaOrdemManutencao
    {
        public int ID { get; set; }
        public string VeiculoPlaca { get; set; }
        public string ResponsavelOrdem { get; set; }
        public string ProprietarioAtual { get; set; }
        public DateTime DataOrdem { get; set; }
        public string DataFinalizacao { get; set; }
        public string Status { get; set; }
        public int QuilometragemManutencao { get; set; }
    }
}
