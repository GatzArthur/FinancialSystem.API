using FinancialSystemApi.Data;
using FinancialSystemApi.DTOs;
using FinancialSystemApi.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FinancialSystemApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ClientesController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/clientes
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ClienteDto>>> GetClientes()
        {
            var clientes = await _context.Clientes
                .Select(c => new ClienteDto
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Cpf = c.Cpf,
                    Email = c.Email,
                    Telefone = c.Telefone,
                    DataCadastro = c.DataCadastro
                })
                .ToListAsync();

            return Ok(clientes);
        }

        // GET: api/clientes/5
        [HttpGet("{id:int}")]
        public async Task<ActionResult<ClienteDto>> GetCliente(int id)
        {
            var cliente = await _context.Clientes.FindAsync(id);

            if (cliente == null)
                return NotFound(new { mensagem = $"Cliente com Id {id} não encontrado." });

            return Ok(new ClienteDto
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Cpf = cliente.Cpf,
                Email = cliente.Email,
                Telefone = cliente.Telefone,
                DataCadastro = cliente.DataCadastro
            });
        }

        // POST: api/clientes
        [HttpPost]
        public async Task<ActionResult<ClienteDto>> PostCliente(ClienteCreateDto dto)
        {
            if (await _context.Clientes.AnyAsync(c => c.Cpf == dto.Cpf))
                return Conflict(new { mensagem = "Já existe um cliente cadastrado com este CPF." });

            if (await _context.Clientes.AnyAsync(c => c.Email == dto.Email))
                return Conflict(new { mensagem = "Já existe um cliente cadastrado com este e-mail." });

            var cliente = new Cliente
            {
                Nome = dto.Nome,
                Cpf = dto.Cpf,
                Email = dto.Email,
                Telefone = dto.Telefone,
                DataCadastro = DateTime.UtcNow
            };

            _context.Clientes.Add(cliente);
            await _context.SaveChangesAsync();

            var resultado = new ClienteDto
            {
                Id = cliente.Id,
                Nome = cliente.Nome,
                Cpf = cliente.Cpf,
                Email = cliente.Email,
                Telefone = cliente.Telefone,
                DataCadastro = cliente.DataCadastro
            };

            return CreatedAtAction(nameof(GetCliente), new { id = cliente.Id }, resultado);
        }

        // PUT: api/clientes/
        [HttpPut("{id:int}")]
        public async Task<IActionResult> PutCliente(int id, ClienteUpdateDto dto)
        {
            var cliente = await _context.Clientes.FindAsync(id);
            if (cliente == null)
                return NotFound(new { mensagem = $"Cliente com Id {id} não encontrado." });

            if (await _context.Clientes.AnyAsync(c => c.Email == dto.Email && c.Id != id))
                return Conflict(new { mensagem = "Já existe outro cliente cadastrado com este e-mail." });

            cliente.Nome = dto.Nome;
            cliente.Email = dto.Email;
            cliente.Telefone = dto.Telefone;

            await _context.SaveChangesAsync();

            return NoContent();
        }

        // DELETE: api/clientes/
        [HttpDelete("{id:int}")]
        public async Task<IActionResult> DeleteCliente(int id)
        {
            var cliente = await _context.Clientes
                .Include(c => c.Contas)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return NotFound(new { mensagem = $"Cliente com Id {id} não encontrado." });

            if (cliente.Contas.Any(c => c.Saldo != 0))
                return BadRequest(new { mensagem = "Não é possível excluir cliente com contas que possuem saldo diferente de zero." });

            _context.Clientes.Remove(cliente);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
