using Microsoft.EntityFrameworkCore; // Necessário para DbContext, DbSet, ToListAsync, FirstOrDefaultAsync, AnyAsync
using Skopia.Domain.Entidades; // Necessário para a entidade Usuario
using Skopia.Domain.Enums;
using Skopia.Domain.Repositorios.Interfaces; // Necessário para a interface IRepositorioUsuario

namespace Skopia.Data.Repositorios;

/// <summary>
/// Implementa as operações de acesso a dados para a entidade <see cref="Usuario"/>,
/// aderindo ao contrato definido pela interface <see cref="IRepositorioUsuario"/>.
///
/// Este repositório é responsável por gerenciar a recuperação de dados do usuário,
/// incluindo a obtenção de um usuário por ID e a verificação de seu status como gerente.
///
/// O soft delete é gerenciado de forma transparente pelo <see cref="SkopiaDbContext"/>,
/// garantindo que apenas usuários ativos (não logicamente deletados) sejam retornados
/// por consultas padrão.
/// </summary>
public class RepositorioUsuario : IRepositorioUsuario
{
    private readonly SkopiaDbContext _context;
    // DbSet específico para a entidade Usuario, otimizando o acesso e garantindo tipagem forte.
    private readonly DbSet<Usuario> _dbSet;

    /// <summary>
    /// Construtor que recebe a instância do <see cref="SkopiaDbContext"/> via Injeção de Dependência.
    /// O DbContext é a unidade de trabalho do Entity Framework Core, responsável por interagir
    /// com o banco de dados e gerenciar o ciclo de vida das entidades.
    /// </summary>
    /// <param name="context">A instância do SkopiaDbContext.</param>
    /// <exception cref="ArgumentNullException">Lançada se o contexto for nulo,
    /// garantindo que a dependência essencial seja fornecida.</exception>
    public RepositorioUsuario(SkopiaDbContext context)
    {
        _context = context ?? throw new ArgumentNullException(nameof(context), "O contexto do banco de dados não pode ser nulo para o RepositorioUsuario.");
        _dbSet = _context.Set<Usuario>(); // Obtém o DbSet para a entidade Usuario.
    }

    /// <inheritdoc/>
    public async Task<Usuario?> ObterPorIdAsync(Guid id)
    {
        // Obtém um usuário pelo seu ID único.
        // FirstOrDefaultAsync é usado para respeitar os filtros globais de consulta (soft delete).
        // Se o usuário não existir ou estiver logicamente deletado, FirstOrDefaultAsync retornará null.
        // AsNoTracking() é aplicado para otimizar a leitura, pois esta consulta é apenas para recuperação de dados
        // e não para modificação subsequente da entidade no mesmo contexto.
        return await _dbSet.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <inheritdoc/>
    public async Task<bool> VerificarSeGerenteAsync(Guid usuarioId)
    {
        // Verifica se um usuário com o ID especificado existe e é um gerente.
        // O filtro global de soft delete do DbContext já garante que apenas usuários ativos sejam considerados.
        // Assumo que a entidade Usuario possui uma propriedade booleana 'IsGerente'.
        // AnyAsync é eficiente porque a consulta será encerrada assim que um usuário que atenda aos critérios for encontrado,
        // sem a necessidade de carregar o objeto completo na memória.
        return await _dbSet.AnyAsync(u => u.Id == usuarioId && u.Funcao == FuncaoUsuario.Gerente); 
    }

    // Se no futuro houver necessidade de métodos de criação, atualização ou exclusão lógica,
    // eles seriam implementados aqui, seguindo o padrão de não chamar SaveChangesAsync() diretamente.
    
    public async Task<Usuario> CriarAsync(Usuario usuario)
    {
        await _dbSet.AddAsync(usuario);
        return usuario;
    }

    /*
    public async Task<Usuario> AtualizarAsync(Usuario usuario)
    {
        _dbSet.Update(usuario);
        return usuario;
    }

    public async Task<bool> ExcluirAsync(Guid id)
    {
        var usuario = await _dbSet.FindAsync(id); // FindAsync não respeita filtro global de soft delete
        if (usuario == null)
            return false;
        _dbSet.Remove(usuario); // Interceptor de soft delete atua aqui
        return true;
    }
    */
}