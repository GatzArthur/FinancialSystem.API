using System.ComponentModel.DataAnnotations;

namespace FinancialSystemApi.DTOs
{
    public class ClienteDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Cpf { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Telefone { get; set; }
        public DateTime DataCadastro { get; set; }
    }

    public class ClienteCreateDto
    {
        [Required, MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        [Required, MaxLength(14)]
        public string Cpf { get; set; } = string.Empty;

        [Required, MaxLength(150), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefone { get; set; }
    }

    public class ClienteUpdateDto
    {
        [Required, MaxLength(150)]
        public string Nome { get; set; } = string.Empty;

        [Required, MaxLength(150), EmailAddress]
        public string Email { get; set; } = string.Empty;

        [MaxLength(20)]
        public string? Telefone { get; set; }
    }
}
