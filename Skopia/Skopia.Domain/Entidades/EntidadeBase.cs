using Skopia.Domain.Excecoes; // Adicionar para usar ExcecaoDominio

namespace Skopia.Domain.Entidades;

public abstract class EntidadeBase
{
    public Guid Id { get; private set; } // Continua com private set
    public bool EstaDeletado { get; private set; }
    public DateTime? QuandoDeletou { get; private set; }

    /// <summary>
    /// Construtor padrão da EntidadeBase. Gera um novo Guid para o Id.
    /// Usado quando o Id é gerado pela própria entidade no momento da criação,
    /// garantindo uma identidade única desde o início.
    /// </summary>
    protected EntidadeBase()
    {
        Id = Guid.NewGuid();
        EstaDeletado = false;
        QuandoDeletou = null;
    }

    /// <summary>
    /// Construtor da EntidadeBase que permite definir o Id externamente.
    /// Este construtor é essencial para situações onde o Id já existe, como:
    /// 1. Ao carregar uma entidade do banco de dados (usado por ORMs como Entity Framework).
    /// 2. Ao reconstruir uma entidade a partir de eventos (em arquiteturas de Event Sourcing).
    /// 3. Quando o Id é gerado por um sistema externo ou distribuído.
    /// </summary>
    /// <param name="id">O Guid a ser atribuído como Id da entidade.</param>
    protected EntidadeBase(Guid id)
    {
        // A validação para Guid.Empty é fundamental para a identidade da entidade.
        // Um Id vazio não representa uma entidade válida.
        ExcecaoDominio.Quando(id == Guid.Empty, "O ID da entidade não pode ser vazio.");

        Id = id;
        EstaDeletado = false;
        QuandoDeletou = null;
    }

    /// <summary>
    /// Marca a entidade como deletada logicamente (soft delete).
    /// Impede a deleção múltipla e registra o momento da ação.
    /// </summary>
    public void Deletar()
    {
        if (EstaDeletado) return; // Evita operações redundantes
        EstaDeletado = true;
        QuandoDeletou = DateTime.UtcNow; // Registra o tempo em UTC para consistência
    }

    /// <summary>
    /// Restaura uma entidade que foi deletada logicamente.
    /// </summary>
    public void Restaurar()
    {
        if (!EstaDeletado) return; // Evita operações redundantes
        EstaDeletado = false;
        QuandoDeletou = null; // Limpa o registro de deleção
    }
}