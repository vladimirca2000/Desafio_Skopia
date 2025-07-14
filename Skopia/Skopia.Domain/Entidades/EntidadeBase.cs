using Skopia.Domain.Excecoes; 

namespace Skopia.Domain.Entidades;

public abstract class EntidadeBase
{
    public Guid Id { get; private set; } 
    public bool EstaDeletado { get; private set; }
    public DateTime? QuandoDeletou { get; private set; }

    
    protected EntidadeBase()
    {
        Id = Guid.NewGuid();
        EstaDeletado = false;
        QuandoDeletou = null;
    }

    
    protected EntidadeBase(Guid id)
    {
       
        ExcecaoDominio.Quando(id == Guid.Empty, "O ID da entidade não pode ser vazio.");

        Id = id;
        EstaDeletado = false;
        QuandoDeletou = null;
    }

    
    public void Deletar()
    {
        if (EstaDeletado) return; 
        EstaDeletado = true;
        QuandoDeletou = DateTime.UtcNow; 
    }

    
    public void Restaurar()
    {
        if (!EstaDeletado) return; 
        EstaDeletado = false;
        QuandoDeletou = null; 
    }
}