using Microsoft.EntityFrameworkCore;
using Skopia.Domain.Entidades;
using Skopia.Domain.Enums;
using System.Linq.Expressions;
using System.Reflection; // Certifique-se de ter esta referência
// ... outras usings ...

namespace Skopia.Data;

public class SkopiaDbContext : DbContext
{
    public SkopiaDbContext(DbContextOptions<SkopiaDbContext> options) : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            // Verifica se o tipo de entidade herda de EntidadeBase
            // Isso garante que o filtro de soft delete seja aplicado apenas a entidades que possuem a propriedade 'EstaDeletado'.
            if (typeof(EntidadeBase).IsAssignableFrom(entityType.ClrType))
            {
                // Chamar o método auxiliar para construir o filtro.
                // Isso garante que a expressão seja construída de forma que o EF Core possa traduzir
                // para SQL de maneira eficiente, sem problemas com casts implícitos ou explícitos.
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(GetSoftDeleteFilter(entityType.ClrType));
            }
        }

        SeedData(modelBuilder);
    }

    // NOVO MÉTODO AUXILIAR PARA O HASQUERYFILTER
    /// <summary>
    /// Constrói dinamicamente a expressão de filtro para soft delete para o tipo de entidade fornecido.
    /// Isso é necessário para lidar corretamente com a herança e a tradução do EF Core para o filtro global.
    /// </summary>
    /// <param name="entityType">O tipo CLR da entidade (por exemplo, typeof(Projeto), typeof(Tarefa)).</param>
    /// <returns>Uma LambdaExpression que representa o filtro de consulta para 'EstaDeletado = false'.</returns>
    private static LambdaExpression GetSoftDeleteFilter(Type entityType)
    {
        // 1. Define o parâmetro da expressão lambda (ex: 'e' em 'e => ...')
        // O tipo do parâmetro é o tipo da entidade que estamos configurando (ex: Projeto, Tarefa).
        // Isso é crucial porque o EF Core precisa saber o tipo exato para a tradução SQL.
        var parameter = Expression.Parameter(entityType, "e");

        // 2. Acessa a propriedade 'EstaDeletado'.
        // Como 'EstaDeletado' está em EntidadeBase, e 'entityType' é sempre derivado de EntidadeBase,
        // podemos acessar a propriedade diretamente. O EF Core consegue mapear isso para a coluna correta.
        var property = Expression.Property(parameter, "EstaDeletado");

        // 3. Nega a expressão da propriedade (e.g., !e.EstaDeletado).
        // Isso cria a condição 'EstaDeletado == false'.
        var notProperty = Expression.Not(property);

        // 4. Cria a expressão lambda completa (e => !e.EstaDeletado).
        // A LambdaExpression é retornada para ser usada no HasQueryFilter.
        // Esta é a forma que o EF Core consegue traduzir de forma otimizada para SQL.
        return Expression.Lambda(notProperty, parameter);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is EntidadeBase entidadeBase)
            {
                // Lógica de Soft Delete: Intercepta a exclusão e marca a entidade como deletada.
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified; // Muda o estado para Modified
                    entidadeBase.Deletar(); // Chama o método para setar EstaDeletado e QuandoDeletou
                }
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        // Definição de IDs determinísticos para o seeding, garantindo idempotência.
        Guid usuarioIdRegular = Guid.Parse("A0000000-0000-0000-0000-000000000001");
        Guid usuarioIdGerente = Guid.Parse("A0000000-0000-0000-0000-000000000002");
        Guid projetoIdExemplo = Guid.Parse("B0000000-0000-0000-0000-000000000001");
        Guid tarefaIdExemplo = Guid.Parse("C0000000-0000-0000-0000-000000000001");
        Guid comentarioIdExemplo = Guid.Parse("D0000000-0000-0000-0000-000000000001");

        // Data de seed consistente e em UTC para evitar problemas de fuso horário.
        var dataSeed = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        // Seeding para Usuario: Construtores podem ser usados se os IDs forem passados explicitamente.
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario(usuarioIdRegular, "Usuário Comum", "usuario@exemplo.com", FuncaoUsuario.Regular),
            new Usuario(usuarioIdGerente, "Usuário Gerente", "gerente@exemplo.com", FuncaoUsuario.Gerente)
        );

        // CORREÇÃO: Uso de tipo anônimo para o HasData de Projeto para garantir Id determinístico
        // e preencher explicitamente todas as propriedades, incluindo as da EntidadeBase.
        modelBuilder.Entity<Projeto>().HasData(
            new
            {
                Id = projetoIdExemplo,
                Nome = "Projeto Exemplo",
                Descricao = "Este é um projeto de exemplo para testes e demonstração de funcionalidades.",
                DataCriacao = dataSeed,
                UsuarioId = usuarioIdRegular,
                EstaDeletado = false,
                QuandoDeletou = (DateTime?)null
            }
        );

        // Seeding para Tarefa, também usando tipo anônimo para consistência e idempotência.
        modelBuilder.Entity<Tarefa>().HasData(
            new
            {
                Id = tarefaIdExemplo,
                ProjetoId = projetoIdExemplo,
                UsuarioId = usuarioIdRegular,
                Titulo = "Tarefa de Exemplo",
                Descricao = "Descrição detalhada da tarefa de exemplo, demonstrando como o seed pode preencher campos e estados iniciais.",
                DataCriacao = dataSeed,
                Status = StatusTarefa.Pendente,
                Prioridade = PrioridadeTarefa.Media,
                DataVencimento = (DateTime?)null, // Propriedade anulável
                EstaDeletado = false,
                QuandoDeletou = (DateTime?)null
            }
        );

        // Seeding para ComentarioTarefa, seguindo o mesmo padrão.
        modelBuilder.Entity<ComentarioTarefa>().HasData(
            new
            {
                Id = comentarioIdExemplo,
                TarefaId = tarefaIdExemplo,
                UsuarioId = usuarioIdRegular,
                Conteudo = "Este é um comentário de exemplo para a tarefa.",
                DataComentario = dataSeed,
                EstaDeletado = false,
                QuandoDeletou = (DateTime?)null
            }
        );
    }
}