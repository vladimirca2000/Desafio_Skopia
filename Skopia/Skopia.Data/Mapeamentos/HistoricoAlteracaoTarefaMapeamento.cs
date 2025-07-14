using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skopia.Domain.Entidades;

namespace Skopia.Data.Mapeamentos;   

public class HistoricoAlteracaoTarefaMapeamento : IEntityTypeConfiguration<HistoricoAlteracaoTarefa>
{
    public void Configure(EntityTypeBuilder<HistoricoAlteracaoTarefa> builder)
    {
        builder.ToTable("HistoricosAlteracaoTarefa");

        builder.HasKey(h => h.Id);
        builder.Property(h => h.Id).ValueGeneratedNever(); 

        builder.Property(h => h.TarefaId).IsRequired();
        builder.Property(h => h.UsuarioId).IsRequired();

        builder.Property(h => h.CampoModificado)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(h => h.ValorAntigo)
            .HasColumnType("TEXT"); 

        builder.Property(h => h.ValorNovo)
            .HasColumnType("TEXT"); 

       
        builder.Property(h => h.DataModificacao)
            .IsRequired();
    }
}