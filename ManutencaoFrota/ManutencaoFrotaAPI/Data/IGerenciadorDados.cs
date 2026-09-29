public interface IGerenciadorDados
{
    List<Proprietario> CarregarProprietarios();
    void SalvarProprietarios(List<Proprietario> proprietarios);
    List<Veiculo> CarregarVeiculos();
    void SalvarVeiculos(List<Veiculo> veiculos);
    List<OrdemManutencao> CarregarOrdens();
    void SalvarOrdens(List<OrdemManutencao> ordens);
    void AdicionarOrdem(OrdemManutencao ordem);
}
