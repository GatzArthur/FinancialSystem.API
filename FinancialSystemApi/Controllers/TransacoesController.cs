using FinancialSystemApi.Data;
using FinancialSystemApi.DTOs;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialSystemApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TransacoesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TransacoesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/transacoes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TransacaoDto>>> GetTransacoes()
        {
            var transacoes = await _context.Transacoes
                .OrderByDescending(t => t.DataTransacao)
                .Select(t => new TransacaoDto
                {
                    Id = t.Id,
                    Tipo = t.Tipo,
                    Valor = t.Valor,
                    DataTransacao = t.DataTransacao,
                    Descricao = t.Descricao,
                    ContaBancariaId = t.ContaBancariaId,
                    ContaRelacionadaId = t.ContaRelacionadaId
                })
                .ToListAsync();

            return Ok(transacoes);
        }

        // GET: api/transacoes/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<TransacaoDto>> GetTransacao(int id)
        {
            var t = await _context.Transacoes.FindAsync(id);
            if (t == null)
                return NotFound(new { mensagem = $"Transação com Id {id} não encontrada." });

            return Ok(new TransacaoDto
            {
                Id = t.Id,
                Tipo = t.Tipo,
                Valor = t.Valor,
                DataTransacao = t.DataTransacao,
                Descricao = t.Descricao,
                ContaBancariaId = t.ContaBancariaId,
                ContaRelacionadaId = t.ContaRelacionadaId
            });
        }

        // GET: api/transacoes/conta/5
        [HttpGet("conta/{contaId:int}")]
        public async Task<ActionResult<IEnumerable<TransacaoDto>>> GetTransacoesPorConta(int contaId)
        {
            var contaExiste = await _context.Contas.AnyAsync(c => c.Id == contaId);
            if (!contaExiste)
                return NotFound(new { mensagem = $"Conta com Id {contaId} não encontrada." });

            var transacoes = await _context.Transacoes
                .Where(t => t.ContaBancariaId == contaId)
                .OrderByDescending(t => t.DataTransacao)
                .Select(t => new TransacaoDto
                {
                    Id = t.Id,
                    Tipo = t.Tipo,
                    Valor = t.Valor,
                    DataTransacao = t.DataTransacao,
                    Descricao = t.Descricao,
                    ContaBancariaId = t.ContaBancariaId,
                    ContaRelacionadaId = t.ContaRelacionadaId
                })
                .ToListAsync();

            return Ok(transacoes);
        }

        // OBS: Não há PUT/DELETE de transações propositalmente.
        // Em um sistema financeiro real, transações são imutáveis (auditoria);
        // correções são feitas com novos lançamentos (estorno), nunca alterando o histórico.
    }
}
