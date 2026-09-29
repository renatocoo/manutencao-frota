using Microsoft.EntityFrameworkCore;

public class AppDbContext : DbContext
{
    public DbSet<Proprietario> Proprietario { get; set; }
    public DbSet<Veiculo> Veiculo { get; set; }
    public DbSet<OrdemManutencao> OrdemManutencao { get; set; }

    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Proprietario>().ToTable("Proprietario");
        modelBuilder.Entity<Veiculo>().ToTable("Veiculo");
        modelBuilder.Entity<OrdemManutencao>().ToTable("OrdemManutencao");
    }
}
