using Skopia.CrossCutting; 
using Skopia.Data; 
using Microsoft.EntityFrameworkCore; 

var builder = WebApplication.CreateBuilder(args);

// Adicionar servi�os ao cont�iner.
// Chama o m�todo de extens�o do projeto Skopia.CrossCutting para configurar toda a infraestrutura de DI.
// Isso inclui o DbContext, reposit�rios e servi�os da aplica��o.
builder.Services.AddInfrastructure(builder.Configuration); 

builder.Services.AddControllers();

// Configura��o do Swagger/OpenAPI para documenta��o da API
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "API de Gerenciamento de Tarefas Skopia", 
        Version = "v1",
        Description = "API para gerenciar tarefas e projetos." 
    });
});

var app = builder.Build();

// Configurar o pipeline de requisi��es HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Aplicar migra��es do banco de dados na inicializa��o
// Isso � �til para ambientes de desenvolvimento/teste para garantir que o banco esteja atualizado.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SkopiaDbContext>(); 

        // Aplicar migra��es pendentes
        context.Database.Migrate();
        Console.WriteLine("Migra��es do banco de dados aplicadas com sucesso.");
    }
    catch (Exception ex)
    {
        // Logar o erro (considere usar um framework de log apropriado como Serilog, NLog)
        Console.WriteLine($"Erro ao aplicar migra��es do banco de dados: {ex.Message}");
        // Se a migra��o falhar no startup, geralmente � um erro cr�tico.
        // O 'throw;' aqui pode impedir a aplica��o de iniciar com um banco de dados inconsistente.
        throw;
    }
}

app.Run();