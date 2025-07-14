// Program.cs
using Skopia.CrossCutting;
using Skopia.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; 

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();

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

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;

    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        var context = services.GetRequiredService<SkopiaDbContext>();
        context.Database.Migrate();
        logger.LogInformation("Migrações do banco de dados aplicadas com sucesso."); 
    }
    catch (Exception ex)
    {
        logger.LogCritical(ex, "Erro crítico ao aplicar migrações do banco de dados: {Message}", ex.Message); 
        throw;
    }
}

app.Run();