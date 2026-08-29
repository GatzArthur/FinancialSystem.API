# Financial System API

API REST em **ASP.NET Core 10 (Web API)** para um sistema financeiro básico, com **Entity Framework Core 10** e **SQL Server**.

## Domínio

- **Cliente**: dados cadastrais (nome, CPF, e-mail, telefone).
- **ContaBancaria**: pertence a um cliente, possui saldo, tipo (Corrente/Poupança) e status (ativa/inativa).
- **Transacao**: histórico imutável de movimentações (depósito, saque, transferência enviada/recebida). Não possui PUT/DELETE propositalmente — em sistemas financeiros reais, o histórico não é alterado; correções viram novos lançamentos.

## Estrutura do projeto

```
FinancialSystemApi/
├── FinancialSystemApi.sln
└── FinancialSystemApi/
    ├── Controllers/
    │   ├── ClientesController.cs      (CRUD)
    │   ├── ContasController.cs        (CRUD + depósito/saque/transferência)
    │   └── TransacoesController.cs    (consulta / extrato)
    ├── Models/
    │   ├── Cliente.cs
    │   ├── ContaBancaria.cs
    │   ├── Transacao.cs
    │   └── Enums/ (TipoConta, TipoTransacao)
    ├── DTOs/
    ├── Data/ApplicationDbContext.cs
    ├── Program.cs
    ├── appsettings.json
    └── FinancialSystemApi.csproj
```

## Pré-requisitos

- [.NET SDK 10.0](https://dotnet.microsoft.com/download) (a versão exata é fixada em `global.json`)
- SQL Server (local, Docker ou Azure SQL)
- (Opcional) `dotnet-ef` tool: `dotnet tool install --global dotnet-ef`

## Configuração

1. Ajuste a connection string em `FinancialSystemApi/appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost,1433;Database=FinancialSystemDb;User Id=sa;Password=SuaSenhaForte123!;TrustServerCertificate=True;"
}
```

Se preferir subir um SQL Server rapidamente via Docker:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=SuaSenhaForte123!" \
  -p 1433:1433 --name sqlserver -d mcr.microsoft.com/mssql/server:2022-latest
```

## Como rodar

```bash
cd FinancialSystemApi/FinancialSystemApi

# Restaurar pacotes
dotnet restore

# Criar a migration inicial (gera as tabelas a partir dos Models)
dotnet ef migrations add InitialCreate

# Aplicar no banco de dados
dotnet ef database update

# Rodar a API
dotnet run
```

A API sobe em `http://localhost:5080` (ver `Properties/launchSettings.json`) e o Swagger fica disponível em `/swagger`.

## Endpoints principais

### Clientes
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/clientes` | Lista todos os clientes |
| GET | `/api/clientes/{id}` | Busca um cliente |
| POST | `/api/clientes` | Cria um cliente |
| PUT | `/api/clientes/{id}` | Atualiza um cliente |
| DELETE | `/api/clientes/{id}` | Remove um cliente (bloqueado se houver contas com saldo) |

### Contas
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/contas` | Lista todas as contas |
| GET | `/api/contas/{id}` | Busca uma conta |
| POST | `/api/contas` | Cria uma conta para um cliente |
| PUT | `/api/contas/{id}` | Atualiza agência/tipo/status |
| DELETE | `/api/contas/{id}` | Remove uma conta (bloqueado se saldo ≠ 0) |
| POST | `/api/contas/{id}/deposito` | Deposita um valor |
| POST | `/api/contas/{id}/saque` | Saca um valor (valida saldo) |
| POST | `/api/contas/transferencia` | Transfere entre duas contas (atômico) |

### Transações
| Método | Rota | Descrição |
|---|---|---|
| GET | `/api/transacoes` | Lista todas as transações |
| GET | `/api/transacoes/{id}` | Busca uma transação |
| GET | `/api/transacoes/conta/{contaId}` | Extrato de uma conta |

## Exemplo de fluxo

```http
POST /api/clientes
{
  "nome": "Maria Silva",
  "cpf": "12345678900",
  "email": "maria@email.com",
  "telefone": "11999998888"
}

POST /api/contas
{
  "numeroConta": "00123-4",
  "agencia": "0001",
  "tipo": "Corrente",
  "saldoInicial": 0,
  "clienteId": 1
}

POST /api/contas/1/deposito
{
  "valor": 500.00,
  "descricao": "Depósito inicial"
}

POST /api/contas/transferencia
{
  "contaOrigemId": 1,
  "contaDestinoId": 2,
  "valor": 100.00,
  "descricao": "Pagamento"
}
```

## Notas de design

- Depósito, saque e transferência rodam dentro de uma transação de banco de dados (`BeginTransactionAsync`), garantindo que o ajuste de saldo e o registro da transação sejam atômicos.
- Enums (`TipoConta`, `TipoTransacao`) são serializados como string no JSON, para facilitar leitura.
- Validações de negócio: saldo insuficiente, conta inativa, CPF/e-mail/número de conta duplicados, exclusão bloqueada quando há saldo.
- DTOs separam o modelo de domínio do que é exposto pela API (evita over-posting e vazamento de dados internos).

## Próximos passos sugeridos

- Autenticação/Autorização (JWT).
- Paginação nas listagens.
- Testes automatizados (xUnit + banco em memória ou Testcontainers).
- Logs estruturados e tratamento global de exceções (middleware).
