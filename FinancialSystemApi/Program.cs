using FinancialSystemApi.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Controllers + JSON (serializa enums como string, fica mais legível na API)
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.Converters.Add(
            new System.Text.Json.Serialization.JsonStringEnumConverter());
    });

// Swagger / OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.OpenApiInfo
    {
        Title = "API Sistema Financeiro",
        Version = "v1",
        Description = "API REST para um sistema financeiro: clientes, contas bancárias e transações."
    });

    // Define a ordem dos métodos dentro de cada grupo de rota (ex: /api/clientes)
    var ordemMetodos = new Dictionary<string, int>
    {
        ["POST"] = 1,
        ["GET"] = 2,
        ["PUT"] = 3,
        ["DELETE"] = 4
    };

    c.OrderActionsBy(apiDesc =>
    {
        var metodo = apiDesc.HttpMethod ?? "GET";
        var ordem = ordemMetodos.TryGetValue(metodo, out var valor) ? valor : 99;
        // Primeiro ordena pelo método (POST, GET, PUT, DELETE...), depois pela rota como desempate
        return $"{ordem:D2}_{apiDesc.RelativePath}";
    });
});


// EF Core + SQL Server
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// CORS liberado (ajuste conforme necessidade em produção)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader();
    });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthorization();
app.MapControllers();

app.Run();