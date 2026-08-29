using FinancialSystemApi.Models;
using Microsoft.EntityFrameworkCore;

namespace FinancialSystemApi.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<Cliente> Clientes => Set<Cliente>();
        public DbSet<ContaBancaria> Contas => Set<ContaBancaria>();
        public DbSet<Transacao> Transacoes => Set<Transacao>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Cpf)
                .IsUnique();

            modelBuilder.Entity<Cliente>()
                .HasIndex(c => c.Email)
                .IsUnique();

            modelBuilder.Entity<ContaBancaria>()
                .HasIndex(c => c.NumeroConta)
                .IsUnique();

            modelBuilder.Entity<ContaBancaria>()
                .HasOne(c => c.Cliente)
                .WithMany(cl => cl.Contas)
                .HasForeignKey(c => c.ClienteId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Transacao>()
                .HasOne(t => t.ContaBancaria)
                .WithMany(c => c.Transacoes)
                .HasForeignKey(t => t.ContaBancariaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
