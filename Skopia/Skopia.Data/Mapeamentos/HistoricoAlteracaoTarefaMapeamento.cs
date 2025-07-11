using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skopia.Domain.Entidades;

namespace Skopia.Data.Mapeamentos   
{
    public class HistoricoAlteracaoTarefaMapeamento : IEntityTypeConfiguration<HistoricoAlteracaoTarefa>
    {
        public void Configure(EntityTypeBuilder<HistoricoAlteracaoTarefa> builder)
        {
            builder.ToTable("HistoricosAlteracaoTarefa");

            builder.HasKey(h => h.Id);
            builder.Property(h => h.Id).ValueGeneratedNever(); // Id gerado pelo domínio.

            builder.Property(h => h.TarefaId).IsRequired();
            builder.Property(h => h.UsuarioId).IsRequired();

            builder.Property(h => h.CampoModificado)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(h => h.ValorAntigo)
                .HasColumnType("TEXT"); // Permite armazenar valores antigos de diferentes tipos como texto.

            builder.Property(h => h.ValorNovo)
                .HasColumnType("TEXT"); // Permite armazenar valores novos de diferentes tipos como texto.

            // REMOVIDO: HasDefaultValueSql e ValueGeneratedOnAdd.
            // DataModificacao é definida pela entidade HistoricoAlteracaoTarefa.
            builder.Property(h => h.DataModificacao)
                .IsRequired();
        }
    }
}