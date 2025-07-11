using Microsoft.EntityFrameworkCore;
using Skopia.Data; // Certifique-se de ter esta refer�ncia
using Skopia.CrossCutting; // Adicione esta using

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Adicionar o SkopiaDbContext ao servi�o de inje��o de depend�ncia.
// Ele ler� a string de conex�o da configura��o (appsettings.json, appsettings.Development.json, etc.)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddDbContext<SkopiaDbContext>(options =>
{
    options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
    // Se quiser ver as queries SQL no console (�til para debug):
    // options.LogTo(Console.WriteLine, LogLevel.Information);
    // options.EnableSensitiveDataLogging(); // Use com cautela, exp�e dados sens�veis nos logs
});

// Chama o m�todo de extens�o do projeto CrossCutting para configurar toda a infraestrutura de DI
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
