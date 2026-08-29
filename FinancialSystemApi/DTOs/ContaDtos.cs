using System.ComponentModel.DataAnnotations;
using FinancialSystemApi.Models.Enums;

namespace FinancialSystemApi.DTOs
{
    public class ContaDto
    {
        public int Id { get; set; }
        public string NumeroConta { get; set; } = string.Empty;
        public string Agencia { get; set; } = string.Empty;
        public decimal Saldo { get; set; }
        public TipoConta Tipo { get; set; }
        public bool Ativa { get; set; }
        public DateTime DataCriacao { get; set; }
        public int ClienteId { get; set; }
        public string? ClienteNome { get; set; }
    }

    public class ContaCreateDto
    {
        [Required, MaxLength(20)]
        public string NumeroConta { get; set; } = string.Empty;

        [Required, MaxLength(10)]
        public string Agencia { get; set; } = string.Empty;

        public TipoConta Tipo { get; set; } = TipoConta.Corrente;

        [Range(0, double.MaxValue)]
        public decimal SaldoInicial { get; set; } = 0;

        [Required]
        public int ClienteId { get; set; }
    }

    public class ContaUpdateDto
    {
        [Required, MaxLength(10)]
        public string Agencia { get; set; } = string.Empty;

        public TipoConta Tipo { get; set; }

        public bool Ativa { get; set; }
    }

    public class MovimentacaoDto
    {
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }

        [MaxLength(250)]
        public string? Descricao { get; set; }
    }

    public class TransferenciaDto
    {
        [Required]
        public int ContaOrigemId { get; set; }

        [Required]
        public int ContaDestinoId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        public decimal Valor { get; set; }

        [MaxLength(250)]
        public string? Descricao { get; set; }
    }
}
