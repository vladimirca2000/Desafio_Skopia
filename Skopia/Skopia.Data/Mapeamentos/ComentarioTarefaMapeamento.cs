using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skopia.Domain.Entidades;

namespace Skopia.Data.Mapeamentos;

public class ComentarioTarefaMapeamento : IEntityTypeConfiguration<ComentarioTarefa>
{
    public void Configure(EntityTypeBuilder<ComentarioTarefa> builder)
    {
        builder.ToTable("ComentariosTarefas");

        builder.HasKey(c => c.Id);
        builder.Property(c => c.Id).ValueGeneratedNever(); 

        builder.Property(c => c.TarefaId).IsRequired();
        builder.Property(c => c.UsuarioId).IsRequired();

        builder.Property(c => c.Conteudo)
            .IsRequired()
            .HasColumnType("TEXT");
                
        builder.Property(c => c.DataComentario)
            .IsRequired();
    }
}