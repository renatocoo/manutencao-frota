public class OrdemManutencao
{
    public int ID { get; set; } // Chave primária
    public int VeiculoID { get; set; } // Chave estrangeira
    public int ResponsavelID { get; set; } // Chave estrangeira para o responsável pela ordem
    public DateTime DataOrdem { get; set; } // Data de criação da ordem e início da manutenção
    public DateTime? DataFinalizacao { get; set; } // Data de finalização da manutenção
    public int QuilometragemManutencao { get; set; }
    public StatusManutencao Status { get; set; } // Enumerado para status da manutenção
}
