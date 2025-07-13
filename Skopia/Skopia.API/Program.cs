using Skopia.CrossCutting; 
using Skopia.Data; 
using Microsoft.EntityFrameworkCore; 

var builder = WebApplication.CreateBuilder(args);

// Adicionar serviços ao contêiner.
// Chama o método de extensão do projeto Skopia.CrossCutting para configurar toda a infraestrutura de DI.
// Isso inclui o DbContext, repositórios e serviços da aplicação.
builder.Services.AddInfrastructure(builder.Configuration); 

builder.Services.AddControllers();

// Configuração do Swagger/OpenAPI para documentação da API
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

// Configurar o pipeline de requisições HTTP.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

// Aplicar migrações do banco de dados na inicialização
// Isso é útil para ambientes de desenvolvimento/teste para garantir que o banco esteja atualizado.
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<SkopiaDbContext>(); 

        // Aplicar migrações pendentes
        context.Database.Migrate();
        Console.WriteLine("Migrações do banco de dados aplicadas com sucesso.");
    }
    catch (Exception ex)
    {
        // Logar o erro (considere usar um framework de log apropriado como Serilog, NLog)
        Console.WriteLine($"Erro ao aplicar migrações do banco de dados: {ex.Message}");
        // Se a migração falhar no startup, geralmente é um erro crítico.
        // O 'throw;' aqui pode impedir a aplicação de iniciar com um banco de dados inconsistente.
        throw;
    }
}

app.Run();