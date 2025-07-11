using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skopia.Domain.Entidades;

namespace Skopia.Data.Mapeamentos;

public class ProjetoMapeamento : IEntityTypeConfiguration<Projeto>
{
    public void Configure(EntityTypeBuilder<Projeto> builder)
    {
        builder.ToTable("Projetos");

        builder.HasKey(p => p.Id);
        // O Id é gerado pelo domínio (Guid.NewGuid() no construtor de EntidadeBase),
        // então o banco de dados não deve gerar seu valor.
        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Descricao)
            .HasColumnType("TEXT"); // Mapeia para um tipo de texto longo no banco de dados.

        // REMOVIDO: HasDefaultValueSql e ValueGeneratedOnAdd.
        // A propriedade DataCriacao é definida pelo domínio (no construtor da entidade)
        // e é persistida como está. Não é gerada pelo banco de dados.
        builder.Property(p => p.DataCriacao)
            .IsRequired();

        builder.Property(p => p.UsuarioId)
            .IsRequired();

        builder.HasMany(p => p.Tarefas)
            .WithOne(t => t.Projeto)
            .HasForeignKey(t => t.ProjetoId)
            // DeleteBehavior.NoAction: Previne a exclusão em cascata de tarefas quando um projeto é excluído.
            // Isso é crucial para soft delete ou para um controle mais granular da remoção de dados.
            .OnDelete(DeleteBehavior.NoAction);
    }
}