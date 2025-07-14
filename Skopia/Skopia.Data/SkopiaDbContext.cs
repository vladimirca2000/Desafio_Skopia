using Microsoft.EntityFrameworkCore;
using Skopia.Domain.Entidades;
using Skopia.Domain.Enums;
using System.Linq.Expressions;
using System.Reflection; 

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
            
            if (typeof(EntidadeBase).IsAssignableFrom(entityType.ClrType))
            {
                
                modelBuilder.Entity(entityType.ClrType)
                    .HasQueryFilter(GetSoftDeleteFilter(entityType.ClrType));
            }
        }

        SeedData(modelBuilder);
    }

    
    private static LambdaExpression GetSoftDeleteFilter(Type entityType)
    {
        
        var parameter = Expression.Parameter(entityType, "e");

        
        var property = Expression.Property(parameter, "EstaDeletado");

        
        var notProperty = Expression.Not(property);

        
        return Expression.Lambda(notProperty, parameter);
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is EntidadeBase entidadeBase)
            {
               
                if (entry.State == EntityState.Deleted)
                {
                    entry.State = EntityState.Modified;
                    entidadeBase.Deletar();
                }
            }
        }
        return await base.SaveChangesAsync(cancellationToken);
    }

    private void SeedData(ModelBuilder modelBuilder)
    {
        
        Guid usuarioIdRegular = Guid.Parse("A0000000-0000-0000-0000-000000000001");
        Guid usuarioIdGerente = Guid.Parse("A0000000-0000-0000-0000-000000000002");
        Guid projetoIdExemplo = Guid.Parse("B0000000-0000-0000-0000-000000000001");
        Guid tarefaIdExemplo = Guid.Parse("C0000000-0000-0000-0000-000000000001");
        Guid comentarioIdExemplo = Guid.Parse("D0000000-0000-0000-0000-000000000001");

        
        var dataSeed = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        
        modelBuilder.Entity<Usuario>().HasData(
            new Usuario(usuarioIdRegular, "Usuário Comum", "usuario@exemplo.com", FuncaoUsuario.Regular),
            new Usuario(usuarioIdGerente, "Usuário Gerente", "gerente@exemplo.com", FuncaoUsuario.Gerente)
        );

        
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
                DataVencimento = (DateTime?)null, 
                EstaDeletado = false,
                QuandoDeletou = (DateTime?)null
            }
        );

       
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