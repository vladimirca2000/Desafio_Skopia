using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Skopia.Domain.Entidades;
using Skopia.Domain.Enums; // Para FuncaoUsuario

namespace Skopia.Data.Mapeamentos
{
    /// <summary>
    /// Configuração do mapeamento da entidade `Usuario` para o banco de dados.
    /// </summary>
    public class UsuarioMapeamento  : IEntityTypeConfiguration<Usuario>
    {
        public void Configure(EntityTypeBuilder<Usuario> builder)
        {
            builder.ToTable("Usuarios"); // Define o nome da tabela

            builder.HasKey(u => u.Id); // Define 'Id' como chave primária
            builder.Property(u => u.Id).ValueGeneratedNever(); // GUID gerado pela aplicação

            builder.Property(u => u.Nome)
                .IsRequired()      // Nome é obrigatório
                .HasMaxLength(100); // Limite de 100 caracteres

            builder.Property(u => u.Email)
                .IsRequired()      // Email é obrigatório
                .HasMaxLength(100); // Limite de 100 caracteres

            // Mapeamento de Enums para int no DB:
            // 'Funcao' é armazenado como seu valor inteiro subjacente no banco de dados.
            builder.Property(u => u.Funcao)
                .IsRequired()
                .HasConversion<int>();

            // Adicionar um índice único para a propriedade 'Email'.
            // Isso garante que não haverá emails duplicados na tabela de usuários no banco de dados,
            // impondo uma restrição de unicidade e otimizando buscas por email.
            builder.HasIndex(u => u.Email)
                   .IsUnique();
        }
    }
}
