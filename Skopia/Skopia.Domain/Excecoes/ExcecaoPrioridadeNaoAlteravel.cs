using Skopia.Domain.Excecoes;

namespace Skopia.Domain.Excecoes;

public class ExcecaoPrioridadeNaoAlteravel : ExcecaoDominio
{
    public ExcecaoPrioridadeNaoAlteravel(string mensagem) : base(mensagem)
    {
    }
}
