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

        builder.Property(p => p.Id).ValueGeneratedNever();

        builder.Property(p => p.Nome)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(p => p.Descricao)
            .HasColumnType("TEXT"); 

       
        builder.Property(p => p.DataCriacao)
            .IsRequired();

        builder.Property(p => p.UsuarioId)
            .IsRequired();

        builder.HasMany(p => p.Tarefas)
            .WithOne(t => t.Projeto)
            .HasForeignKey(t => t.ProjetoId)            

            .OnDelete(DeleteBehavior.NoAction);
    }
}