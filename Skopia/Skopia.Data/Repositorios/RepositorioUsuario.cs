using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Skopia.Domain.Entidades; 
using Skopia.Domain.Enums;
using Skopia.Domain.Repositorios.Interfaces; 

namespace Skopia.Data.Repositorios;


public class RepositorioUsuario : IRepositorioUsuario
{
    private readonly SkopiaDbContext _context;
    private readonly ILogger<RepositorioUsuario> _logger;
    private readonly DbSet<Usuario> _dbSet;

    public RepositorioUsuario(SkopiaDbContext context, ILogger<RepositorioUsuario> logger)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context), "O contexto do banco de dados não pode ser nulo para o RepositorioUsuario.");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger), "O logger não pode ser nulo para o RepositorioUsuario.");
        _dbSet = _context.Set<Usuario>();
    }

    /// <inheritdoc/>
    public async Task<Usuario?> ObterPorIdAsync(Guid id)
    {
        _logger.LogInformation("Obtendo usuário com ID: {Id}", id);
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <inheritdoc/>
    public async Task<bool> VerificarSeGerenteAsync(Guid usuarioId)
    {
        _logger.LogInformation("Verificando se o usuário com ID: {Id} é um gerente.", usuarioId);
        return await _dbSet.AnyAsync(u => u.Id == usuarioId && u.Funcao == FuncaoUsuario.Gerente);
    }   
}