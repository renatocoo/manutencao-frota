public class Veiculo
{
    public int ID { get; set; } // Chave primária
    public string Placa { get; set; }
    public int? Quilometragem { get; set; } // Tipo de dados int permitindo valor nulo
    public DateTime? DataUltimaManutencao { get; set; }// Permitindo valor nulo
    public int ProprietarioID { get; set; }
}
