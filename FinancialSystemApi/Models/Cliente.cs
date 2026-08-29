using System.ComponentModel.DataAnnotations;

namespace FinancialSystemApi.Models
{
    public class Cliente
    {
        public int Id { get; set; }

        [Required, MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        [Required, MaxLength(14)]
        public string Cpf { get; set; } = string.Empty;

        [Required, MaxLength(150), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefone { get; set; }

        public DateTime DataCadastro { get; set; } = DateTime.UtcNow;

        public ICollection<ContaBancaria> Contas { get; set; } = new List<ContaBancaria>();
    }
}
