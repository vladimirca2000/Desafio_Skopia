using Microsoft.EntityFrameworkCore;
using Skopia.Data; // Certifique-se de ter esta referência
using Skopia.CrossCutting; // Adicione esta using

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Adicionar o SkopiaDbContext ao serviço de injeção de dependência.
// Ele lerá a string de conexão da configuração (appsettings.json, appsettings.Development.json, etc.)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SkopiaDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    // Se quiser ver as queries SQL no console (útil para debug):
    // options.LogTo(Console.WriteLine, LogLevel.Information);
    // options.EnableSensitiveDataLogging(); // Use com cautela, expõe dados sensíveis nos logs
});

// Chama o método de extensão do projeto CrossCutting para configurar toda a infraestrutura de DI
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
