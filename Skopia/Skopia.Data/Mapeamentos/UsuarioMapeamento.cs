using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skopia.Domain.Entidades;
using Skopia.Domain.Enums; 

namespace Skopia.Data.Mapeamentos;


public class UsuarioMapeamento  : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.ToTable("Usuarios"); 

        builder.HasKey(u => u.Id); 
        builder.Property(u => u.Id).ValueGeneratedNever(); 

        builder.Property(u => u.Nome)
            .IsRequired()      
            .HasMaxLength(100); 

        builder.Property(u => u.Email)
            .IsRequired()      
            .HasMaxLength(100); 

        
        builder.Property(u => u.Funcao)
            .IsRequired()
            .HasConversion<int>();

        
        builder.HasIndex(u => u.Email)
               .IsUnique();
    }
}
