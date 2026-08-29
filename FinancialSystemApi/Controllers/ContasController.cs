using FinancialSystemApi.Data;
using FinancialSystemApi.DTOs;
using FinancialSystemApi.Models;
using FinancialSystemApi.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialSystemApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ContasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ContasController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/contas
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ContaDto>>> GetContas()
        {
            var contas = await _context.Contas
                .Include(c => c.Cliente)
                .Select(c => new ContaDto
                {
                    Id = c.Id,
                    NumeroConta = c.NumeroConta,
                    Agencia = c.Agencia,
                    Saldo = c.Saldo,
                    Tipo = c.Tipo,
                    Ativa = c.Ativa,
                    DataCriacao = c.DataCriacao,
                    ClienteId = c.ClienteId,
                    ClienteNome = c.Cliente!.Nome
                })
                .ToListAsync();

            return Ok(contas);
        }

        // GET: api/contas/
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ContaDto>> GetConta(int id)
        {
            var conta = await _context.Contas
                .Include(c => c.Cliente)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conta == null)
                return NotFound(new { mensagem = $"Conta com Id {id} não encontrada." });

            return Ok(new ContaDto
            {
                Id = conta.Id,
                NumeroConta = conta.NumeroConta,
                Agencia = conta.Agencia,
                Saldo = conta.Saldo,
                Tipo = conta.Tipo,
                Ativa = conta.Ativa,
                DataCriacao = conta.DataCriacao,
                ClienteId = conta.ClienteId,
                ClienteNome = conta.Cliente?.Nome
            });
        }

        // POST: api/contas
        [HttpPost]
        public async Task<ActionResult<ContaDto>> PostConta(ContaCreateDto dto)
        {
            var cliente = await _context.Clientes.FindAsync(dto.ClienteId);
            if (cliente == null)
                return BadRequest(new { mensagem = "Cliente informado não existe." });

            if (await _context.Contas.AnyAsync(c => c.NumeroConta == dto.NumeroConta))
                return Conflict(new { mensagem = "Já existe uma conta com este número." });

            var conta = new ContaBancaria
            {
                NumeroConta = dto.NumeroConta,
                Agencia = dto.Agencia,
                Tipo = dto.Tipo,
                Saldo = dto.SaldoInicial,
                Ativa = true,
                DataCriacao = DateTime.UtcNow,
                ClienteId = dto.ClienteId
            };

            _context.Contas.Add(conta);
            await _context.SaveChangesAsync();

            var resultado = new ContaDto
            {
                Id = conta.Id,
                NumeroConta = conta.NumeroConta,
                Agencia = conta.Agencia,
                Saldo = conta.Saldo,
                Tipo = conta.Tipo,
                Ativa = conta.Ativa,
                DataCriacao = conta.DataCriacao,
                ClienteId = conta.ClienteId,
                ClienteNome = cliente.Nome
            };

            return CreatedAtAction(nameof(GetConta), new { id = conta.Id }, resultado);
        }

        // PUT: api/contas/5
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutConta(int id, ContaUpdateDto dto)
        {
            var conta = await _context.Contas.FindAsync(id);
            if (conta == null)
                return NotFound(new { mensagem = $"Conta com Id {id} não encontrada." });

            conta.Agencia = dto.Agencia;
            conta.Tipo = dto.Tipo;
            conta.Ativa = dto.Ativa;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/contas/5
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteConta(int id)
        {
            var conta = await _context.Contas.FindAsync(id);
            if (conta == null)
                return NotFound(new { mensagem = $"Conta com Id {id} não encontrada." });

            if (conta.Saldo != 0)
                return BadRequest(new { mensagem = "Não é possível excluir uma conta com saldo diferente de zero." });

            _context.Contas.Remove(conta);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/contas/5/deposito
        [HttpPost("{id:int}/deposito")]
        public async Task<ActionResult<ContaDto>> Depositar(int id, MovimentacaoDto dto)
        {
            var conta = await _context.Contas.Include(c => c.Cliente).FirstOrDefaultAsync(c => c.Id == id);
            if (conta == null)
                return NotFound(new { mensagem = $"Conta com Id {id} não encontrada." });

            if (!conta.Ativa)
                return BadRequest(new { mensagem = "Não é possível movimentar uma conta inativa." });

            using var transaction = await _context.Database.BeginTransactionAsync();

            conta.Saldo += dto.Valor;

            _context.Transacoes.Add(new Transacao
            {
                Tipo = TipoTransacao.Deposito,
                Valor = dto.Valor,
                Descricao = dto.Descricao ?? "Depósito",
                ContaBancariaId = conta.Id,
                DataTransacao = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(MapConta(conta));
        }

        // POST: api/contas/5/saque
        [HttpPost("{id:int}/saque")]
        public async Task<ActionResult<ContaDto>> Sacar(int id, MovimentacaoDto dto)
        {
            var conta = await _context.Contas.Include(c => c.Cliente).FirstOrDefaultAsync(c => c.Id == id);
            if (conta == null)
                return NotFound(new { mensagem = $"Conta com Id {id} não encontrada." });

            if (!conta.Ativa)
                return BadRequest(new { mensagem = "Não é possível movimentar uma conta inativa." });

            if (conta.Saldo < dto.Valor)
                return BadRequest(new { mensagem = "Saldo insuficiente para realizar o saque." });

            using var transaction = await _context.Database.BeginTransactionAsync();

            conta.Saldo -= dto.Valor;

            _context.Transacoes.Add(new Transacao
            {
                Tipo = TipoTransacao.Saque,
                Valor = dto.Valor,
                Descricao = dto.Descricao ?? "Saque",
                ContaBancariaId = conta.Id,
                DataTransacao = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(MapConta(conta));
        }

        // POST: api/contas/transferencia
        [HttpPost("transferencia")]
        public async Task<ActionResult> Transferir(TransferenciaDto dto)
        {
            if (dto.ContaOrigemId == dto.ContaDestinoId)
                return BadRequest(new { mensagem = "Conta de origem e destino não podem ser a mesma." });

            var contaOrigem = await _context.Contas.FirstOrDefaultAsync(c => c.Id == dto.ContaOrigemId);
            var contaDestino = await _context.Contas.FirstOrDefaultAsync(c => c.Id == dto.ContaDestinoId);

            if (contaOrigem == null)
                return NotFound(new { mensagem = "Conta de origem não encontrada." });

            if (contaDestino == null)
                return NotFound(new { mensagem = "Conta de destino não encontrada." });

            if (!contaOrigem.Ativa || !contaDestino.Ativa)
                return BadRequest(new { mensagem = "Ambas as contas precisam estar ativas para transferência." });

            if (contaOrigem.Saldo < dto.Valor)
                return BadRequest(new { mensagem = "Saldo insuficiente para realizar a transferência." });

            using var transaction = await _context.Database.BeginTransactionAsync();

            contaOrigem.Saldo -= dto.Valor;
            contaDestino.Saldo += dto.Valor;

            _context.Transacoes.Add(new Transacao
            {
                Tipo = TipoTransacao.TransferenciaEnviada,
                Valor = dto.Valor,
                Descricao = dto.Descricao ?? $"Transferência para conta {contaDestino.NumeroConta}",
                ContaBancariaId = contaOrigem.Id,
                ContaRelacionadaId = contaDestino.Id,
                DataTransacao = DateTime.UtcNow
            });

            _context.Transacoes.Add(new Transacao
            {
                Tipo = TipoTransacao.TransferenciaRecebida,
                Valor = dto.Valor,
                Descricao = dto.Descricao ?? $"Transferência recebida da conta {contaOrigem.NumeroConta}",
                ContaBancariaId = contaDestino.Id,
                ContaRelacionadaId = contaOrigem.Id,
                DataTransacao = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return Ok(new
            {
                mensagem = "Transferência realizada com sucesso.",
                contaOrigem = MapConta(contaOrigem),
                contaDestino = MapConta(contaDestino)
            });
        }

        private static ContaDto MapConta(ContaBancaria conta) => new()
        {
            Id = conta.Id,
            NumeroConta = conta.NumeroConta,
            Agencia = conta.Agencia,
            Saldo = conta.Saldo,
            Tipo = conta.Tipo,
            Ativa = conta.Ativa,
            DataCriacao = conta.DataCriacao,
            ClienteId = conta.ClienteId,
            ClienteNome = conta.Cliente?.Nome
        };
    }
}
