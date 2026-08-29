using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using FinancialSystemApi.Models.Enums;

namespace FinancialSystemApi.Models
{
    public class ContaBancaria
    {
        public int Id { get; set; }

        [Required, MaxLength(20)]
        public string NumeroConta { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Agencia { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Saldo { get; set; } = 0;

        public TipoConta Tipo { get; set; } = TipoConta.Corrente;

        public bool Ativa { get; set; } = true;

        public DateTime DataCriacao { get; set; } = DateTime.UtcNow;

        public int ClienteId { get; set; }
        public Cliente? Cliente { get; set; }

        public ICollection<Transacao> Transacoes { get; set; } = new List<Transacao>();
    }
}
