using FinancialSystemApi.Models.Enums;

namespace FinancialSystemApi.DTOs
{
    public class TransacaoDto
    {
        public int Id { get; set; }
        public TipoTransacao Tipo { get; set; }
        public decimal Valor { get; set; }
        public DateTime DataTransacao { get; set; }
        public string? Descricao { get; set; }
        public int ContaBancariaId { get; set; }
        public int? ContaRelacionadaId { get; set; }
    }
}
