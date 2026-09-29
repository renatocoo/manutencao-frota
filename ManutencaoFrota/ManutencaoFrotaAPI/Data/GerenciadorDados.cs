using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

/// <summary>
/// Classe responsável pelo gerenciamento dos dados no contexto do aplicativo.
/// </summary>
public class GerenciadorDados : IGerenciadorDados
{
    private readonly AppDbContext _contexto;
    private readonly ILogger<GerenciadorDados> _logger;

    /// <summary>
    /// Inicializa uma nova instância da classe GerenciadorDados.
    /// </summary>
    /// <param name="contexto">O contexto do banco de dados.</param>
    /// <param name="logger">O logger para registrar mensagens.</param>
    public GerenciadorDados(AppDbContext contexto, ILogger<GerenciadorDados> logger)
    {
        _contexto = contexto;
        _logger = logger;
    }

    /// <summary>
    /// Carrega todos os proprietários do banco de dados.
    /// </summary>
    /// <returns>Uma lista de proprietários.</returns>
    public List<Proprietario> CarregarProprietarios()
    {
        return _contexto.Proprietario.AsNoTracking().ToList();
    }

    /// <summary>
    /// Salva uma lista de proprietários no banco de dados.
    /// </summary>
    /// <param name="proprietarios">A lista de proprietários a ser salva.</param>
    public void SalvarProprietarios(List<Proprietario> proprietarios)
    {
        foreach (var proprietario in proprietarios)
        {
            // Verifica se o proprietário já existe pelo Documento (CPF ou CNPJ) se o ID não estiver presente
            if (proprietario.ID == 0)
            {
                var existingProprietario = _contexto.Proprietario.FirstOrDefault(p => p.Documento == proprietario.Documento);
                if (existingProprietario != null)
                {
                    proprietario.ID = existingProprietario.ID;
                    _contexto.Entry(existingProprietario).CurrentValues.SetValues(proprietario);
                    continue;
                }
                _contexto.Entry(proprietario).State = EntityState.Added;
            }
            else
            {
                _contexto.Entry(proprietario).State = EntityState.Modified;
            }
        }
        SalvarComTratamentoDeConcorrencia();
    }

    /// <summary>
    /// Carrega todos os veículos do banco de dados.
    /// </summary>
    /// <returns>Uma lista de veículos.</returns>
    public List<Veiculo> CarregarVeiculos()
    {
        return _contexto.Veiculo.AsNoTracking().ToList();
    }

    /// <summary>
    /// Salva uma lista de veículos no banco de dados.
    /// </summary>
    /// <param name="veiculos">A lista de veículos a ser salva.</param>
    public void SalvarVeiculos(List<Veiculo> veiculos)
    {
        foreach (var veiculo in veiculos)
        {
            // Verifica se o veículo já existe pela placa se o ID não estiver presente
            if (veiculo.ID == 0)
            {
                var existingVehicle = _contexto.Veiculo.FirstOrDefault(v => v.Placa == veiculo.Placa);
                if (existingVehicle != null)
                {
                    veiculo.ID = existingVehicle.ID;
                    _contexto.Entry(existingVehicle).CurrentValues.SetValues(veiculo);
                    continue;
                }
                _contexto.Entry(veiculo).State = EntityState.Added;
            }
            else
            {
                _contexto.Entry(veiculo).State = EntityState.Modified;
            }
        }
        SalvarComTratamentoDeConcorrencia();
    }

    /// <summary>
    /// Carrega todas as ordens de manutenção do banco de dados.
    /// </summary>
    /// <returns>Uma lista de ordens de manutenção.</returns>
    public List<OrdemManutencao> CarregarOrdens()
    {
        return _contexto.OrdemManutencao.AsNoTracking().ToList();
    }

    /// <summary>
    /// Salva uma lista de ordens de manutenção no banco de dados.
    /// </summary>
    /// <param name="ordens">A lista de ordens de manutenção a ser salva.</param>
    public void SalvarOrdens(List<OrdemManutencao> ordens)
    {
        foreach (var ordem in ordens)
        {
            _contexto.Entry(ordem).State = ordem.ID == 0 ? EntityState.Added : EntityState.Modified;
        }
        SalvarComTratamentoDeConcorrencia();
    }

    /// <summary>
    /// Adiciona uma nova ordem de manutenção no banco de dados.
    /// </summary>
    /// <param name="ordem">A ordem de manutenção a ser adicionada.</param>
    public void AdicionarOrdem(OrdemManutencao ordem)
    {
        _contexto.OrdemManutencao.Add(ordem);
        SalvarComTratamentoDeConcorrencia();
    }

    /// <summary>
    /// Salva as alterações no banco de dados com tratamento de concorrência.
    /// </summary>
    private void SalvarComTratamentoDeConcorrencia()
    {
        bool salvamentoFalhou;
        do
        {
            salvamentoFalhou = false;
            try
            {
                _contexto.SaveChanges();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                salvamentoFalhou = true;
                foreach (var entry in ex.Entries)
                {
                    var valoresBanco = entry.GetDatabaseValues();
                    if (valoresBanco == null)
                    {
                        _logger.LogError("Erro de concorrência: A entidade sendo atualizada ou excluída não existe mais no banco de dados. Entidade: {Entity}", entry.Entity.GetType().Name);
                        throw new Exception("A entidade sendo atualizada ou excluída não existe mais no banco de dados.");
                    }
                    entry.OriginalValues.SetValues(valoresBanco);
                }
            }
        } while (salvamentoFalhou);
    }
}
