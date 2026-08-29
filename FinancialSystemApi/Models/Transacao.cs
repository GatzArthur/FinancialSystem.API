using System.ComponentModel.DataAnnotations.Schema;
using FinancialSystemApi.Models.Enums;

namespace FinancialSystemApi.Models
{
    public class Transacao
    {
        public int Id { get; set; }

        public TipoTransacao Tipo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal Valor { get; set; }

        public DateTime DataTransacao { get; set; } = DateTime.UtcNow;

        public string? Descricao { get; set; }

        public int ContaBancariaId { get; set; }
        public ContaBancaria? ContaBancaria { get; set; }

        // Preenchido apenas em transferências, para rastrear a conta do outro lado
        public int? ContaRelacionadaId { get; set; }
    }
}
