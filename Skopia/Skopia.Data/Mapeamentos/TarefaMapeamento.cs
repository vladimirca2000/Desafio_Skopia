using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skopia.Domain.Entidades;
using Skopia.Domain.Enums;

namespace Skopia.Data.Mapeamentos;

public class TarefaMapeamento : IEntityTypeConfiguration<Tarefa>
{
    public void Configure(EntityTypeBuilder<Tarefa> builder)
    {
        builder.ToTable("Tarefas");

        builder.HasKey(t => t.Id);
        builder.Property(t => t.Id).ValueGeneratedNever();

        builder.Property(t => t.ProjetoId).IsRequired();
        builder.Property(t => t.UsuarioId).IsRequired();

        builder.Property(t => t.Titulo)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(t => t.Descricao)
            .HasColumnType("TEXT");
                
        builder.Property(t => t.DataCriacao)
            .IsRequired();

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<int>(); 

        builder.Property(t => t.Prioridade)
            .IsRequired()
            .HasConversion<int>();

        builder.Property(t => t.DataVencimento)
            .IsRequired(false); 

        builder.HasMany(t => t.Comentarios)
            .WithOne(c => c.Tarefa)
            .HasForeignKey(c => c.TarefaId)
            .OnDelete(DeleteBehavior.NoAction); 

        builder.HasMany(t => t.Historico)
            .WithOne(h => h.Tarefa)
            .HasForeignKey(h => h.TarefaId)
            .OnDelete(DeleteBehavior.NoAction); 
    }
}